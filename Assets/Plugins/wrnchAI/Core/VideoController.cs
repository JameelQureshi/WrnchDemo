/*
 Copyright (c) 2019 Wrnch Inc.
 All rights reserved
*/

using System;
using System.Diagnostics;
using System.Collections;
using UnityEngine;
using UnityEngine;
using UnityEngine.Video;
using wrnchAI.Config;

namespace wrnchAI.Core
{
    public enum VideoMode
    {
        Webcam,
        ImageFile,
        MovieFile
    }
    /// <summary>
    /// Base class for Video stream handling.
    /// Contains a copy of the latest frame as BGR array for inter thread communication
    /// as well as some metadata.
    /// </summary>
    public abstract class VideoSource : MonoBehaviour
    {
        protected byte[] m_bgrFrame;

        protected int m_width;
        public int Width { get { return m_width; } set { m_width = value; } }

        protected int m_height;
        public int Height { get { return m_height; } set { m_height = value; } }

        protected VideoControllerConfig m_config;
        public VideoControllerConfig Config { get { return m_config; } set { UpdateConfig(value); } }

        protected abstract void UpdateConfig(VideoControllerConfig config);

        public abstract Texture GetTexture();

        public abstract void Stop();
        public abstract void Init(VideoControllerConfig config);

        public delegate void PostProcess(ref byte[] data, int width, int height);
        public static event PostProcess onPostProcess;

        public delegate void FrameReady(byte[] data, int width, int height);
        public static event FrameReady onFrame;

        public delegate void videoDeviceDimensionChanged();
        public static videoDeviceDimensionChanged onVideoDeviceDimensionChanged;

        public delegate void FlipBlock(Color32[] source);
        public static event FlipBlock onFlip;

        /// <summary>
        /// Crop and flip the frame before sending it to the pose estimator.
        /// Takes an array of Color32 as input, and will convert it to an array
        /// of byte stored in m_bgrFrame
        /// </summary>
        protected void PrepareFrame(Color32[] sourceTex, int width, int height)
        {
            if (sourceTex == null)
                return;

            //If no function is bound to onFlip, we still need a dummy one to
            //copy the content of sourceTex to byte[] format for processing outside
            //of the main thread. UpdateFlipAndCopy will take care of binding a function
            if (onFlip != null)
            {
                onFlip(sourceTex);
            }
            else
            {
                UpdateFlipAndCopy();
                onFlip(sourceTex);
            }
        }

        /// <summary>
        /// Sets a flip function that mirrors the video source image along X and/or Y axis as per selected VideoControllerConfig options.
        /// This allows for the image sent to pose estimation to be adjusted for different camera types or simply mirror avatar motion.
        /// The flip function will also convert the input image from Color32 to an array of BGR bytes.
        /// </summary>
        protected void UpdateFlipAndCopy()
        {
            if (m_config.MirrorX)
                if (m_config.MirrorY)
                    onFlip = (Color32[] in_array) =>    // Mirror X and Y
                    {
                        int in_array_idx = 0;
                        for (int j = m_height - 1; j >= 0; j -= 1)
                        {
                            for (int i = 3 * m_width - 3; i >= 0; i -= 3)
                            {
                                m_bgrFrame[j * 3 * m_width + i] = in_array[in_array_idx].b;
                                m_bgrFrame[j * 3 * m_width + i + 1] = in_array[in_array_idx].g;
                                m_bgrFrame[j * 3 * m_width + i + 2] = in_array[in_array_idx].r;
                                ++in_array_idx;
                            }
                        }
                    };
                else
                    onFlip = (Color32[] in_array) =>    // Mirror X only
                    {
                        int in_array_idx = 0;
                        for (int j = 0; j < m_height; j += 1)
                        {
                            for (int i = 3 * m_width - 3; i >= 0; i -= 3)
                            {
                                m_bgrFrame[j * 3 * m_width + i] = in_array[in_array_idx].b;
                                m_bgrFrame[j * 3 * m_width + i + 1] = in_array[in_array_idx].g;
                                m_bgrFrame[j * 3 * m_width + i + 2] = in_array[in_array_idx].r;
                                ++in_array_idx;
                            }
                        }
                    };
            else
                if (m_config.MirrorY)
                onFlip = (Color32[] in_array) =>    // Mirror Y only
                {
                    int in_array_idx = 0;
                    for (int j = m_height - 1; j >= 0; j -= 1)
                    {
                        for (int i = 0; i < 3 * m_width; i += 3)
                        {
                            m_bgrFrame[j * 3 * m_width + i] = in_array[in_array_idx].b;
                            m_bgrFrame[j * 3 * m_width + i + 1] = in_array[in_array_idx].g;
                            m_bgrFrame[j * 3 * m_width + i + 2] = in_array[in_array_idx].r;
                            ++in_array_idx;
                        }
                    }
                };
            else
                onFlip = (Color32[] in_array) =>    // No mirroring
                {
                    int in_array_idx = 0;
                    for (int j = 0; j < m_height; j += 1)
                    {
                        for (int i = 0; i < 3 * m_width; i += 3)
                        {
                            m_bgrFrame[j * 3 * m_width + i] = in_array[in_array_idx].b;
                            m_bgrFrame[j * 3 * m_width + i + 1] = in_array[in_array_idx].g;
                            m_bgrFrame[j * 3 * m_width + i + 2] = in_array[in_array_idx].r;
                            ++in_array_idx;
                        }
                    }
                };
        }

        /// <summary>
        /// Call the post processing blocks if they exist
        /// </summary>
        protected void CallPostProcess()
        {
            if (onPostProcess != null)
                onPostProcess(ref m_bgrFrame, m_width, m_height);
        }

        /// <summary>
        /// Call every callback listening to new frames
        /// </summary>
        protected void CallOnframe()
        {
            if (onFrame != null)
                onFrame(m_bgrFrame, m_width, m_height);
        }
    }

    /// <summary>
    /// Class handling a webcam input for video stream.
    /// The WebcamTexture object being an opaque container running at
    /// the webcam framerate, we need to handle the synchronization between
    /// pose estimation and frame.
    /// The Update loop will grab a frame from the webcam, and send it to every
    /// object registered with the onFrame event. It will not be able to send
    /// a new frame until UnlockFrame() is called. This allows to ensure the
    /// frame shown by the visualization and sent to the pose estimator are the same.
    /// </summary>
    public class WebcamSource : VideoSource
    {
        public WebCamTexture videoDevice { get; private set; }

        private Texture2D m_renderTexture;
        public Texture2D RenderTexture { get; private set; }
        private Color32[] frame;

        //Frame lock to ensure synchronization
        private bool m_frameLocked = false;

        /// <summary>
        /// Unlock a frame once it's been processed
        /// </summary>
        public void UnlockFrame()
        {
            m_frameLocked = false;
        }

        /// <summary>
        /// Apply a VideoControllerConfig to this video controller
        /// </summary>
        /// <param name="config"></param>
        protected override void UpdateConfig(VideoControllerConfig config)
        {
            m_config = config;
            //Ensure we don't lock the video device in editor.
            if (Application.isPlaying)
            {
                //Set the right conversion function according to the new configuration
                UpdateFlipAndCopy();
                //Video device change requested
                if (videoDevice != null)
                {
                    if (config.DeviceName != "Auto" && videoDevice.deviceName != config.DeviceName)
                    {
                        videoDevice.Stop();
                        videoDevice = null;
                    }
                }
                //Initialize (new) video device
                if (videoDevice == null)
                {
                    //Default width and height
                    if (config.DesiredHeight == -1 || config.DesiredWidth == -1)
                    {
                        videoDevice = new WebCamTexture(config.DeviceName == "Auto" ? "" : config.DeviceName);
                    }
                    else
                    {
                        videoDevice = new WebCamTexture(config.DeviceName == "Auto" ? "" : config.DeviceName, config.DesiredWidth, config.DesiredHeight);
                    }
                   
                    UnityEngine.Debug.Log(videoDevice.name);
                    videoDevice.Play();
                    videoDevice.Stop();
                    WebCamDevice[] devices = WebCamTexture.devices;
                    foreach (WebCamDevice d in devices)
                    {
                        if (d.isFrontFacing)
                        {
                            videoDevice.deviceName = d.name;
                            break;
                        }
                    }
                    videoDevice.Play();
                }
            }
        }

        /// <summary>
        /// Initialization of the video stream
        /// </summary>
        /// <param name="config"></param>
        public override void Init(VideoControllerConfig config)
        {
            VideoControllerConfig.onConfigChanged += UpdateConfig;
            UpdateConfig(config);

            //This would be the effective resolution, can be diferent from requested
            m_width = videoDevice.width;
            m_height = videoDevice.height;

            m_bgrFrame = new byte[m_width * m_height * 3];

            m_renderTexture = new Texture2D(videoDevice.width, videoDevice.height, TextureFormat.RGBA32, false);
            PoseManager.onPoseProcessed += UnlockFrame;
        }

        /// <summary>
        /// Returns the current render texture to allow binding it to a material for example.
        /// </summary>
        /// <returns>The texture</returns>
        public override Texture GetTexture()
        {
            if (m_renderTexture != null)
                return m_renderTexture as Texture;
            else
                throw new InvalidOperationException("Trying to access texture of an uninitialized video device");
        }

        /// <summary>
        /// Stop the video device.
        /// </summary>
        public override void Stop()
        {
            videoDevice.Stop();
            Destroy(videoDevice);
        }

        /// <summary>
        /// Update function called by the unity main loop on main thread
        /// </summary>
        private void Update()
        {
            //No need to call onVideoDeviceDimension changed here, it will be called when reszing the render texture.
            if (m_width != videoDevice.width || m_height != videoDevice.height)
            {
                m_width = videoDevice.width;
                m_height = videoDevice.height;

                m_bgrFrame = new byte[m_width * m_height * 3];
            }

            //If there is a new frame available, and the previous one has been processed
            if (videoDevice.didUpdateThisFrame && !m_frameLocked)
            {
                if ((videoDevice.width != m_renderTexture.width || videoDevice.height != m_renderTexture.height))
                {
                    m_renderTexture.Resize(videoDevice.width, videoDevice.height);
                    if (onVideoDeviceDimensionChanged != null)
                    {
                        onVideoDeviceDimensionChanged();
                    }
                }

                if (frame != null)
                {
                    m_renderTexture.SetPixels32(frame);
                    m_renderTexture.Apply();
                }

                frame = videoDevice.GetPixels32();
                PrepareFrame(frame, videoDevice.width, videoDevice.height);
                m_frameLocked = true;
                CallPostProcess();
                CallOnframe();
            }
        }
    }

    /// <summary>
    /// Use a still image for pose estimation.
    /// </summary>
    public class ImageSource : VideoSource
    {
        protected WWW m_image;
        private Texture2D m_renderTexture;
        public Texture2D RenderTexture { get; private set; }

        //Frame lock to ensure synchronization
        private bool m_frameLocked = false;

        /// <summary>
        /// Unlock a frame once it's been processed
        /// </summary>
        public void UnlockFrame()
        {
            m_frameLocked = false;
        }

        public override void Init(VideoControllerConfig config)
        {
            m_config = config;
            m_image = new WWW("file:///" + config.DeviceName);

            while (!m_image.isDone) ;

            m_width = m_image.texture.width;
            m_height = m_image.texture.height;

            m_renderTexture = new Texture2D(m_width, m_height, TextureFormat.RGBA32, false);
            m_bgrFrame = new byte[m_width * m_height * 3];
            m_renderTexture.SetPixels32(m_image.texture.GetPixels32());
            PoseManager.onPoseProcessed += UnlockFrame;
        }

        public override Texture GetTexture()
        {
            if (m_image != null)
                return m_renderTexture as Texture;
            else
                throw new InvalidOperationException("Trying to access texture of an uninitialized video device");
        }

        protected override void UpdateConfig(VideoControllerConfig config)
        {
            m_config = config;
            UpdateFlipAndCopy();
        }

        public void Update()
        {
            PrepareFrame(m_renderTexture.GetPixels32(), m_width, m_height);
            m_frameLocked = true;
            CallPostProcess();
            CallOnframe();
        }

        public override void Stop()
        {
            m_image.Dispose();
        }
    }

    public class MovieSource : VideoSource
    {
        private Texture2D m_renderTexture;
        public Texture2D RenderTexture { get; private set; }

        private VideoPlayer m_player;
        private Texture m_texture;
        private bool m_frameLocked = false;

        public void UnlockFrame()
        {
            m_frameLocked = false;
        }

        private bool m_ready = false;

        public override void Init(VideoControllerConfig config)
        {
            m_config = config;

            m_renderTexture = new Texture2D(2, 2, TextureFormat.RGBA32, true);
            Application.runInBackground = true;

            var url = config.VideoURL;

            StartCoroutine(PlayVideo(url));
        }

        IEnumerator PlayVideo(string url)
        {
            m_player = gameObject.AddComponent<VideoPlayer>();

            m_player.playOnAwake = false;

            m_player.source = UnityEngine.Video.VideoSource.Url;
            m_player.isLooping = true;
            m_player.audioOutputMode = VideoAudioOutputMode.Direct;

            m_player.url = url;
            m_player.Prepare();

            while (!m_player.isPrepared)
            {
                yield return null;
            }

            m_texture = m_player.texture;
            m_player.sendFrameReadyEvents = true;
            m_player.frameReady += OnNewFrame;

            m_player.Play();

            StartCoroutine(SetVideoQuad(m_player));

            m_width = m_texture.width;
            m_height = m_texture.height;

            m_bgrFrame = new byte[m_width * m_height * 3];

            PoseManager.onPoseProcessed += UnlockFrame;

            m_ready = true;

            while (m_player.isPlaying)
            {
                yield return null;
            }
        }

        IEnumerator SetVideoQuad(VideoPlayer videoPlayer )
        {
            yield return new WaitForSeconds(2);
            videoPlayer.targetTexture = DataManager.instance.videoQuadTexture;

        }

        protected override void UpdateConfig(VideoControllerConfig config)
        {
            m_config = config;
            UpdateFlipAndCopy();
        }

        public override Texture GetTexture()
        {
            return m_renderTexture as Texture;
        }

        public override void Stop()
        {
            m_player.Stop();
        }

        private void Update()
        {
        }

        private void OnNewFrame(VideoPlayer source, long frameIdx)
        {
            if (!m_frameLocked)
            {
                RenderTexture texture = m_player.texture as RenderTexture;

                if (m_renderTexture.width != texture.width ||
                    m_renderTexture.height != texture.height)
                {
                    m_renderTexture.Resize(texture.width, texture.height);
                }

                UnityEngine.RenderTexture.active = texture;
                m_renderTexture.ReadPixels(new Rect(0, 0, m_width, m_height), 0, 0);
                m_renderTexture.Apply();
                UnityEngine.RenderTexture.active = null;

                PrepareFrame(m_renderTexture.GetPixels32(), texture.width, texture.height);
                m_frameLocked = true;
                CallPostProcess();
                CallOnframe();
            }
        }
    }
}
