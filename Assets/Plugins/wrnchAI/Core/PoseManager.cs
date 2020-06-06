/*
 Copyright (c) 2019 Wrnch Inc.
 All rights reserved
*/

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading;
using UnityEngine.Video;
using wrnchAI.wrAPI;
using wrnchAI.Config;
using wrnchAI.Visualization;

namespace wrnchAI.Core
{
    /// <summary>
    /// Communication interface between the PoseWorker thread and Unity's main thread.
    /// PoseManager contains and exposes all configuration classes through fields and inspector
    /// for video stream, pose estimation, and visualizers configuration.
    /// The pose estimation is done on a separate thread, which will send received poses
    /// to this object, during it's update loop it will broadcast the poses on the main thread to
    /// every registered Component through onPoseReceived. Once the poses are processed by every
    /// GameObject, the onPoseProcessed will notify the video controller to unlock the next frame.
    /// PoseManager is a singleton and should be the main interface to communicate with every related subsystem such
    /// as visualization, video stream, animation controllers.
    /// </summary>
    [Serializable]
    public class PoseManager : MonoBehaviour
    {



        public VideoClip m_videoClip;
        private Pose3D defaultIkTPose;

        private static PoseManager m_instance;
        public static PoseManager Instance { get { return m_instance; } }
        protected PoseManager() { }

        public JointDefinition JointDefinition2D { get; set; }
        public JointDefinition JointDefinition3D { get; set; }

        protected PoseEstimatorWorker m_poseWorker;

        [SerializeField]
        private VideoControllerConfig m_videoControllerConfig;
        public VideoControllerConfig videoControllerConfig
        {
            get { return m_videoControllerConfig; }
            set
            {
                m_videoControllerConfig = value;
                m_videoControllerConfig.OnValidate();
            }
        }

        [SerializeField]
        private PoseWorkerConfig m_poseWorkerConfig;
        public PoseWorkerConfig poseWorkerConfig
        {
            get { return m_poseWorkerConfig; }
            set
            {
                m_poseWorkerConfig = value;
                m_poseWorkerConfig.OnValidate();
            }
        }

        [SerializeField]
        private VideoVisualizerConfig m_visualizerConfig;
        public VideoVisualizerConfig VisualizerConfig
        {
            get { return m_visualizerConfig; }
            set
            {
                m_visualizerConfig = value;
                m_visualizerConfig.OnValidate();
            }
        }

        [SerializeField]
        private bool m_displayAvatars = true;

        protected VideoSource m_videoController;
        public VideoSource VideoController { get { return m_videoController; } }
        protected VisualHandler  m_visualHandler;
        public VisualHandler VisualHandler { get { return m_visualHandler; } }


        private List<Person> m_latestPoses;
        private bool m_hasFreshData = false;
        private Mutex m_poseMutex;

        private Text m_fpsText;
        private float m_fps;
        private bool m_hasFreshFPS = false;

        public delegate void PoseProcessed();
        public static event PoseProcessed onPoseProcessed;
        public static event Action<List<Person>> onPoseReceived;
        public static event Action<float> OnFPSUpdated;

        //Default values.
        [SerializeField]
        private GameObject m_defaultCharacter;
        public GameObject DefaultCharacter
        {
            get { return m_defaultCharacter; }
            set
            {
                m_visualHandler.UnregisterAvatarVisualizer(m_defaultCharacter);
                m_defaultCharacter = value;
                m_visualHandler.RegisterAvatarVisualizer(m_defaultCharacter);
            }
        }

        [SerializeField]
        private GameObject m_defaultVisualizer;
        public GameObject DefaultVisualizer { get { return m_defaultVisualizer; } set { m_defaultVisualizer = value; } }

        /// <summary>
        /// Receive persons from the worker thread in order to broadcast them from the update loop
        /// </summary>
        /// <param name="persons"> A list of detected persons on the latest frame </param>
        protected void ReceivePersons(List<Person> persons)
        {
            if (m_poseMutex.WaitOne(10))
            {
                m_latestPoses.Clear();
                foreach (var p in persons)
                {
                    m_latestPoses.Add((Person)p.Clone());
                }
                m_hasFreshData = true;
                m_poseMutex.ReleaseMutex();
            }
        }

        /// <summary>
        /// Receive the inference framerate from the worker thread in order to broacast it from the update loop
        /// </summary>
        /// <param name="value"> The number of frame processed by the pose estimator per seconds </param>
        protected void ReceiveFPS(float value)
        {
            m_fps = value;
            m_hasFreshFPS = true;
        }

        /// <summary>
        /// Pause the worker for scene changes
        /// </summary>
        public void Pause()
        {
            VideoSource.onFrame -= m_poseWorker.enqueueFrame;
        }

        /// <summary>
        /// Resume the worker once the scene has changed
        /// </summary>
        public void Resume()
        {
            VideoSource.onFrame += m_poseWorker.enqueueFrame;
        }

        /// <summary>
        /// Early initialization of the objects. Every Component needing informations about
        /// video or pose estimator should be initialized in the Start() to ensure proper
        /// data availability. This call will freeze the main thread for a few minutes while the
        /// pose estimator is loading the models.
        /// </summary>
        private void Awake()
        {
            if (m_instance != null)
                throw new Exception(string.Format("{0} is already running !", this.GetType()));

            m_instance = this;

            switch (m_videoControllerConfig.Videomode)
            {
                case VideoMode.Webcam:
                    m_videoController = gameObject.AddComponent<WebcamSource>();
#if UNITY_IOS || UNITY_ANDROID
                    m_videoControllerConfig.DesiredWidth = Screen.orientation == ScreenOrientation.Portrait ? Screen.height : Screen.width;
                    m_videoControllerConfig.DesiredHeight = Screen.orientation == ScreenOrientation.Portrait ? Screen.width : Screen.height;
#endif
                    break;
                case VideoMode.ImageFile:
                    m_videoController = gameObject.AddComponent<ImageSource>();
                    break;
                case VideoMode.MovieFile:
                    m_videoController = gameObject.AddComponent<MovieSource>();
                    break;
            }

            m_videoController.Init(m_videoControllerConfig);

            m_poseWorker = new PoseEstimatorWorker(m_poseWorkerConfig);
            m_poseWorker.onNewPersons += ReceivePersons;

            m_visualHandler = gameObject.AddComponent<VisualHandler>();

            //onPoseReceived += m_visualHandler.UpdatePersons;
            onPoseReceived += OnPersonFound;

            m_fps = 0f;
            PoseEstimatorWorker.OnFPSUpdated += ReceiveFPS;

            if (m_visualizerConfig.Visualizer != null)
            {
                var visualizer = m_visualizerConfig.Visualizer.AddComponent<VideoVisualizer>();
                visualizer.Init(m_visualizerConfig);
            }

            GameObject fpsTextGo = GameObject.Find("FPS");
            if (fpsTextGo != null)
            {
                m_fpsText = fpsTextGo.GetComponent<Text>();
            }

            if (m_defaultCharacter != null && m_displayAvatars)
            {
                m_visualHandler.RegisterAvatarVisualizer(m_defaultCharacter);
            }
        }

        public void OnPersonFound(List<Person> persons)
        {
            foreach (Person p in persons)
            {
                // Send The Person to JointDataDisplay So we can use it for further calculations ;
                DataManager.instance.person = p;
                return;
            }
        }

            /// <summary>
            /// Starts the pose worker thread.
            /// </summary>
            private void Start()
        {
            m_latestPoses = new List<Person>();

            m_poseMutex = new Mutex();
            JointDefinition2D = m_poseWorker.GetJointDef2D();
            JointDefinition3D = m_poseWorker.GetJointDef3D();

            if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.Android)
            {
                DeviceChange.init();
                DeviceChange.OrientationChange += OnOrientationChange;
                OnOrientationChange();  // set default rotation because might be different than editor
            }

            m_poseWorker.Start();
        }

        /// <summary>
        /// Broadcast the data from the PoseWorker from the main thread.
        /// </summary>
        private void Update()
        {
            if (onPoseReceived != null && m_hasFreshData)
            {
                if (m_poseMutex.WaitOne(10))
                {
                    onPoseReceived(m_latestPoses);
                    m_hasFreshData = false;
                    if (onPoseProcessed != null)
                        onPoseProcessed();
                    m_poseMutex.ReleaseMutex();
                }
            }
            if (m_hasFreshFPS)
            {
                if (OnFPSUpdated != null)
                {
                    OnFPSUpdated(m_fps);
                    m_hasFreshFPS = false;
                }
                if (m_fpsText != null)
                {
                    m_fpsText.text = m_fps.ToString("F0") + " FPS";
                }
            }

        }

        /// <summary>
        /// Detect an orientation change for mobile devices.
        /// </summary>
        private void OnOrientationChange()
        {
            //Those configurations should be stored somewhere else.
            switch (DeviceChange.deviceOrientation)
            {
                case ScreenOrientation.LandscapeLeft:
                    m_videoControllerConfig.RotationMultipleOf90 = 0;
                    m_visualizerConfig.RotationMultipleOf90 = 0;
                    m_videoControllerConfig.MirrorX = false;
                    m_videoControllerConfig.MirrorY = false;

                    m_visualizerConfig.MirrorX = false;
                    m_visualizerConfig.MirrorY = true;
                    break;
                case ScreenOrientation.LandscapeRight:
                    m_videoControllerConfig.RotationMultipleOf90 = 0;
                    m_videoControllerConfig.MirrorX = false;
                    m_videoControllerConfig.MirrorY = true;

                    m_visualizerConfig.RotationMultipleOf90 = 0;
                    m_visualizerConfig.MirrorX = false;
                    m_visualizerConfig.MirrorY = false;
                    break;

                case ScreenOrientation.Portrait:
                    m_videoControllerConfig.RotationMultipleOf90 = 3;
                    m_videoControllerConfig.MirrorX = true;
                    m_videoControllerConfig.MirrorY = false;

                    m_visualizerConfig.RotationMultipleOf90 = 1;
                    m_visualizerConfig.MirrorX = false;
                    m_visualizerConfig.MirrorY = false;
                    break;
            }
            // On mobile, we have separate pose estimators running for portrait and landscape mode.
            // This takes care of switching to the correct pose estimator for a given mode.
            m_poseWorker.changePoseEstimator(m_videoControllerConfig.RotationMultipleOf90 % 2 == 0);
        }

        /// <summary>
        /// Returns the internal TPose from wrIK. This is needed for rig adjustments in the Animation controller
        /// </summary>
        /// <returns>A Pose3D Object containing the default TPose </returns>
        public Pose3D GetDefaultTPose3D()
        {
            if (defaultIkTPose == null)
            {
                defaultIkTPose = m_poseWorker != null ? m_poseWorker.GetDefaultTPose3D() : null;
            }
            return defaultIkTPose;
        }

        /// <summary>
        /// Destroy resources.
        /// </summary>

        private void OnDisable()
        {
            if (m_instance!=null)
            {
                Destroy(m_instance);
            }
        }

        private void OnApplicationQuit()
        {
            if (m_visualizerConfig.Visualizer != null)
                m_visualizerConfig.Visualizer = null; //Will unregister and destroy the visualizers
            if (m_poseWorker != null)
            {
                m_poseWorker.Kill();
                m_poseWorker.Join();
            }
            if (m_videoController != null)
            {
                m_videoController.Stop();
                Destroy(m_videoController);
            }
        }
    }
}
