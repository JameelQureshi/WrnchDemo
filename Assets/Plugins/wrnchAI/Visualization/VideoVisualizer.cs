using System;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using wrnchAI.Config;
using wrnchAI.Core;

namespace wrnchAI.Visualization
{
    /// <summary>
    /// Component for visualization of a video stream and/or 2D skeletons.
    /// This component must be attached to GameObject containing either a Renderer component
    /// or a RawImage. The TexCombineShader.shader is required for mask application and rotation/mirroring.
    /// </summary>
    /// <see cref="TexCombineShader.shader"/>
    public class VideoVisualizer : MonoBehaviour
    {
        private VideoVisualizerConfig m_config;

        //Store shader bindings to avoid lookups
        private static int m_rotationShaderId = Shader.PropertyToID("_Rotation");
        private static int m_maskRotationShaderId = Shader.PropertyToID("_MaskRotation");
        private static int m_blendTexShaderId = Shader.PropertyToID("_BlendTex");
        private static int m_mainTextureId = Shader.PropertyToID("_MainTex");

        //Mask handling for greenscreening.
        private Mutex m_maskMutex;
        private bool m_maskReady = false;
        private byte[] m_maskData;
        private Resolution m_maskResolution;
        private Texture2D m_maskTexture;

        //If true, the visualizer will be applied to a RawImage object
        //instead of mesh renderer.
        private bool m_isUIComponent = false;

        public static event Action<Vector3> OnVideoScaleUpdated;

        //Reference to the material on which visualization will happen.
        private Material m_videoMaterial;
        public Material VideoMaterial
        {
            get { return m_videoMaterial; }
            set
            {
                m_videoMaterial = value;
                UpdateConfig(m_config); //Make sure we send the right uniforms to the new instance.
            }
        }

        /// <summary>
        /// Initialize the video visualizer component and associated material.
        /// </summary>
        /// <param name="config"></param>
        /// <returns>true if initialization succeeded</returns>
        public bool Init(VideoVisualizerConfig config)
        {
            if (config.Visualizer == null)
            {
                Debug.LogWarning("Visualizer initialized without gameObject");
                return false;
            }

            m_maskMutex = new Mutex();

            //If there is no renderer component on the gameObject, fallback to UI component.
            var renderer = gameObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                m_videoMaterial = renderer.material;
                m_isUIComponent = false;
            }
            else
            {
                var rawImage = gameObject.GetComponent<RawImage>();
                if (rawImage != null)
                {
                    m_videoMaterial = rawImage.material;
                    m_isUIComponent = true;
                }
                else
                {
                    Debug.LogWarning("Unsupported visualizer format");
                    return false;
                }
            }

            if (m_videoMaterial != null)
                m_videoMaterial.mainTexture = PoseManager.Instance.VideoController.GetTexture();

            //Setup the video material with current config.
            UpdateConfig(config);

            //Register skeleton visualizer if needed.
            if (config.DisplaySkeletons)
                PoseManager.Instance.VisualHandler.RegisterSkeletonVisualizer(config.Visualizer, m_isUIComponent);

            ToggleMask(PoseManager.Instance.poseWorkerConfig.EstimateMask);

            return true;
        }

        private void OnEnable()
        {
            VideoVisualizerConfig.onConfigChanged += UpdateConfig;
            VideoSource.onVideoDeviceDimensionChanged += AdjustAspectRatio;
            PoseWorkerConfig.onToggleMask += ToggleMask;
        }

        private void OnDisable()
        {
            VideoVisualizerConfig.onConfigChanged -= UpdateConfig;
            VideoSource.onVideoDeviceDimensionChanged -= AdjustAspectRatio;
            PoseWorkerConfig.onToggleMask -= ToggleMask;
        }

        /// <summary>
        /// Toggle the mask reception and application on the texture.
        /// </summary>
        /// <param name="state"></param>
        private void ToggleMask(bool state)
        {
            if (state)
            {
                Shader.EnableKeyword("MASK_ON");
                Shader.DisableKeyword("MASK_OFF");
                PoseEstimatorWorker.onMask += ReceiveMask;
            }
            else
            {
                Shader.DisableKeyword("MASK_ON");
                Shader.EnableKeyword("MASK_OFF");
                PoseEstimatorWorker.onMask -= ReceiveMask;
            }
        }

        /// <summary>
        /// Function called by the PoseWorker thread to update the local mask.
        /// Update of textures from any thread other that the main one is forbidden,
        /// This function will only update a local byte array.
        /// </summary>
        /// <param name="data">incoming data (8bit, 1 channel)</param>
        /// <param name="width">width of the received mask</param>
        /// <param name="height">height of the received mask</param>
        private void ReceiveMask(byte[] data, int width, int height)
        {
            //Timeout after 10ms is the mask is locked by the main thread
            if (m_maskMutex.WaitOne(10))
            {
                if (m_maskData == null || m_maskData.Length != data.Length)
                {
                    m_maskData = new byte[data.Length];
                }
                if (m_maskResolution.width != width || m_maskResolution.height != height)
                {
                    m_maskResolution.width = width;
                    m_maskResolution.height = height;
                }
                Array.Copy(data, m_maskData, data.Length);
                m_maskReady = true;
                m_maskMutex.ReleaseMutex();
            }
        }

        /// <summary>
        /// Send the latest mask received to the texture for application.
        /// This function can only be called from the main thread.
        /// </summary>
        private void MaskToTexture()
        {
            if (m_maskData != null && m_maskMutex.WaitOne(10))
            {
                if (m_maskData == null || m_maskData.Length == 0)
                {
                    m_maskMutex.ReleaseMutex();
                    return;
                }
                if (m_maskTexture == null || m_maskTexture.width != m_maskResolution.width || m_maskTexture.height != m_maskResolution.height)
                {
                    m_maskTexture = new Texture2D(m_maskResolution.width, m_maskResolution.height, TextureFormat.Alpha8, false);
                    m_videoMaterial.SetTexture(m_blendTexShaderId, m_maskTexture);
                    m_videoMaterial.SetTextureScale(m_blendTexShaderId, new Vector2(1, -1)); //Mask is returned upside down.
                }
                m_maskTexture.LoadRawTextureData(m_maskData);
                m_maskTexture.Apply();
                m_maskReady = false;
                m_maskMutex.ReleaseMutex();
            }
        }

        /// <summary>
        /// Change the aspect ratio of  the visualization to match the video source.
        /// </summary>
        private void AdjustAspectRatio()
        {
            if (m_config.Visualizer == null)
                return;

            var w = PoseManager.Instance.VideoController.Width;
            var h = PoseManager.Instance.VideoController.Height;
            var objectScale = m_config.Visualizer.transform.localScale;

            //Are we in portrait or landscape mode ?
            if (m_config.RotationMultipleOf90 % 2 == 1)
                objectScale.x = objectScale.y * ((float)h / w);
            else
                objectScale.x = objectScale.y * ((float)w / h);

            m_config.Visualizer.transform.localScale = objectScale;

            if (OnVideoScaleUpdated != null)
                OnVideoScaleUpdated(objectScale);
        }

        /// <summary>
        /// Apply and store a new configuration set for this object.
        /// </summary>
        /// <param name="config"></param>
        private void UpdateConfig(VideoVisualizerConfig config)
        {
            m_config = config;
            if (m_config.Visualizer == null)
                return;

            m_videoMaterial.SetMatrix(m_rotationShaderId, Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, 0, -90 * config.RotationMultipleOf90), Vector3.one));
            m_videoMaterial.SetTextureScale(m_mainTextureId, new Vector2(config.MirrorX ? -1 : 1, config.MirrorY ? -1 : 1));
            var angle = Mathf.PI * config.RotationMultipleOf90 / 2.0f;
            var rotVector = new Vector4(Mathf.Cos(angle), -Mathf.Sin(angle), Mathf.Sin(angle), Mathf.Cos(angle));
            m_videoMaterial.SetVector(m_maskRotationShaderId, rotVector);
            AdjustAspectRatio();
        }

        /// <summary>
        /// Update loop called by the main thread.
        /// </summary>
        private void Update()
        {
            if (m_maskReady)
            {
                MaskToTexture();
            }
        }
    }
}
