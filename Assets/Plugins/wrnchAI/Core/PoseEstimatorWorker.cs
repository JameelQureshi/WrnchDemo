/*
 Copyright (c) 2019 Wrnch Inc.
 All rights reserved
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using UnityEngine;
using wrnchAI.wrAPI;
using wrnchAI.Config;

namespace wrnchAI.Core
{
    /// <summary>
    ///  Class running a <see cref="PoseEstimator"/> in a separate thread.
    ///  The pose worker receive an image as a byte array of BGR pixels with top left origin
    ///  from the videocontroller and sends it to the backend for pose estimation.
    ///  On mobile platform, two pose estimators are instantiated for portrait and landscape mode.
    ///  The PoseWorker will receive sets of <see cref="Pose3D"/>, <see cref="Pose2D"/>and <see cref="PoseHead"/>objects and merge them
    ///  by id into <see cref="Person"/> objects before sending it to registered objects through onNewPersons.
    /// </summary>
    public class PoseEstimatorWorker
    {
        // Thread
        private bool m_isRunning;
        private Thread m_thread;

        // Data sync
        private EventWaitHandle m_frameWaitHandle;
        private bool frameEnqueued = false;

        // Pre allocated resources to reduce framewise allocation and GC overhead.
        private byte[] m_frame;
        private int m_width, m_height;

        //
        private int m_numHumans;
        private Pose3D[] m_latestResp3d;
        private Pose3D[] m_latestRespRaw3d;
        private Pose2D[] m_latestResp2d;
        private PoseHead[] m_latestRespHead;
        private PoseFace[] m_latestRespFaces;

        private List<Person> m_persons;

        // Pose estimator
        private PoseEstimator m_poseEstimator;
        private PoseEstimator m_poseEstimatorHorizontal;
        private PoseEstimator m_poseEstimatorVertical;
        private PoseEstimatorOptions m_poseOptions;

        public event Action<List<Person>> onNewPersons;
        public static event Action<float> OnFPSUpdated;
        public static event Action<byte[], int, int> onMask;

        // Mask
        private byte[] m_mask;
        private int m_maskWidth = 0;
        private int m_maskHeight = 0;
        public int MaskWidth { get { return m_maskWidth; } set { m_width = m_maskWidth; } }
        public int MaskHeight { get { return m_maskHeight; } set { m_width = m_maskHeight; } }

        // Timing
        private long m_timeAccumulator = 0;
        private uint m_frameAccumulator = 0;
        private float m_currentFPS;
        public float CurrentFPS { get { return m_currentFPS; } }
        private Stopwatch m_stopwatch;

        private PoseWorkerConfig m_config;
        public PoseWorkerConfig Config { get { return m_config; } set { UpdateConfig(value); } }

        /// <summary>
        /// Update the PoseEstimator configuration
        /// </summary>
        /// <param name="config"></param>
        private void UpdateConfig(PoseWorkerConfig config)
        {
            m_config = config;
            if (m_poseOptions == null)
                m_poseOptions = new PoseEstimatorOptions();

            m_poseOptions.SetUseIK(true)
            .SetEstimate3d(true)
            .SetEnableJointSmoothing(config.JointSmoothing)
            .SetEstimateMask(config.EstimateMask)
            .SetEstimateHead(config.EstimateHead) // This feature is not yet supported on iOS
            .SetEstimatePoseFace(config.EstimateHead)
            .SetEnableHeadSmoothing(config.HeadSmoothing) // This feature is not yet supported on iOS
            .SetRotationMultipleOf90(config.RotationMultipleOf90)
            .SetMainPersonId(config.Multi3D ? MainPersonId.ALL : MainPersonId.CENTER);
        }

        private void ReceiveRotation(int rotation)
        {
            this.Config.RotationMultipleOf90 = rotation;
        }

        /// <summary>
        ///  Create and run a pose estimator in a separate thread.
        /// </summary>
        /// <exception cref="InvalidOperationException">Throw if initialization of pose estimator fails.</exception>
        public PoseEstimatorWorker(PoseWorkerConfig config)
        {
            PoseWorkerConfig.onConfigChanged += UpdateConfig;
            VideoControllerConfig.OnRotationChanged += ReceiveRotation;

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            string[] res = System.IO.Directory.GetFiles(Application.dataPath, "wrAPI.dll", System.IO.SearchOption.AllDirectories);
            if (res.Length == 0)
            {
                throw new Exception("Failed to find wrAPI.dll");
            }
            string path = (config.OverrideModelPath != null && config.OverrideModelPath != "") ? config.OverrideModelPath : res[0].Replace("wrAPI.dll", "wrModels");
#elif UNITY_EDITOR_LINUX || UNITY_STANDALONE_LINUX
            string[] res = System.IO.Directory.GetFiles(Application.dataPath, "libwrAPI.so", System.IO.SearchOption.AllDirectories);
            if (res.Length == 0)
            {
                throw new Exception("Failed to find libwrAPI.so");
            }
            string path = Application.streamingAssetsPath;
#elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX  // Not yet supported
            string[] res = System.IO.Directory.GetFiles(Application.dataPath, "wrAPI.bundle", System.IO.SearchOption.AllDirectories);
            if (res.Length == 0)
            {
                throw new Exception("Failed to find wrAPI.bundle");
            }
            string path = Application.streamingAssetsPath;

#elif UNITY_IOS
            string path = "/Data/Raw";
#elif UNITY_ANDROID
            string path = Application.persistentDataPath;
#endif

            m_poseEstimatorHorizontal = new PoseEstimator();
            if (config.SerializedModelPath.Length > 0)
            {
                m_poseEstimatorHorizontal.InitFromSerialized(config.SerializedModelPath, config.DeviceId);
            }
            else
            {
                m_poseEstimatorHorizontal.Init(path, config, "^((?!portrait).)*$");
            }

#if UNITY_IOS && !UNITY_EDITOR
            m_poseEstimatorVertical = new PoseEstimator();
            m_poseEstimatorVertical.Init(path, config, "portrait");
            changePoseEstimator(Screen.orientation == ScreenOrientation.Landscape);
#else
            changePoseEstimator();
#endif
            UpdateConfig(config);

            var maxPersons = m_poseEstimator.GetMaxPersons();
            //Initialize pose containers.
            m_latestResp2d = Enumerable.Range(0, maxPersons).Select(i => new Pose2D()).ToArray();
            m_latestResp3d = Enumerable.Range(0, maxPersons).Select(i => new Pose3D()).ToArray();
            m_latestRespRaw3d = Enumerable.Range(0, maxPersons).Select(i => new Pose3D()).ToArray();
            m_latestRespHead = Enumerable.Range(0, maxPersons).Select(i => new PoseHead()).ToArray();
            m_latestRespFaces = Enumerable.Range(0, maxPersons).Select(i => new PoseFace()).ToArray();

            m_persons = new List<Person>();

            m_isRunning = false;
            m_thread = new Thread(Run);
            m_frameWaitHandle = new EventWaitHandle(true, EventResetMode.AutoReset);

            VideoSource.onFrame += enqueueFrame;

            m_stopwatch = new Stopwatch();
            m_currentFPS = 0.0f;
        }

        public void Start()
        {
            m_thread.Start();
            m_isRunning = true;
        }

        /// <summary>
        ///   Threaded function
        /// </summary>
        void Run()
        {
            while (m_isRunning)
            {
                if (!frameEnqueued)
                    continue;

                // Wait for a frame to be ready to process
                m_frameWaitHandle.WaitOne();
                m_stopwatch.Start();
                m_poseEstimator.ProcessFrame(m_frame, m_width, m_height, m_poseOptions);
                m_stopwatch.Stop();

                m_timeAccumulator += m_stopwatch.ElapsedMilliseconds;
                m_frameAccumulator++;
                if (m_timeAccumulator >= 1000)
                {
                    m_currentFPS = (1000.0f * m_frameAccumulator) / m_timeAccumulator;
                    m_timeAccumulator = 0;
                    m_frameAccumulator = 0;
                    if (OnFPSUpdated != null)
                        OnFPSUpdated(m_currentFPS);
                }
                m_stopwatch.Reset();

                m_numHumans = m_poseEstimator.GetAllHumans2D(ref m_latestResp2d);
                m_poseEstimator.GetAllRawHumans3D(ref m_latestRespRaw3d);
                m_poseEstimator.GetAllHumans3D(ref m_latestResp3d);
                m_poseEstimator.GetAllHeads(ref m_latestRespHead);
                m_poseEstimator.GetAllFaces(ref m_latestRespFaces);

                if (Config.EstimateMask)
                {
                    m_poseEstimator.GetMaskView(ref m_mask, ref m_maskWidth, ref m_maskHeight);
                    if (onMask != null && m_mask != null)
                        onMask(m_mask, m_maskWidth, m_maskHeight);
                }

                MergePoses();
                frameEnqueued = false;
                m_frameWaitHandle.Set();

                if (onNewPersons != null)
                    onNewPersons(m_persons);
            }

            PoseWorkerConfig.onConfigChanged -= UpdateConfig;
            VideoControllerConfig.OnRotationChanged -= ReceiveRotation;

            // Kill everybody
            m_poseEstimator.Release();
            m_frameWaitHandle.Close();
        }

        /// <summary>
        /// Perform an id based merge of the poses returned by the pose estimator to inflate an array of Persons. <see cref="Person"/>
        /// </summary>
        void MergePoses()
        {
            if (m_numHumans == 0)
            {
                m_persons.Clear();
                return;
            }
            if (m_numHumans > m_persons.Count)
            {
                for (int i = m_persons.Count; i < m_numHumans; ++i)
                {
                    m_persons.Add(new Person());
                }
            }
            else if (m_numHumans < m_persons.Count)
            {
                for (int i = m_numHumans; i < m_persons.Count; ++i)
                {
                    m_persons.RemoveAt(i);
                }
            }

            for (int idx = 0; idx < m_numHumans; idx++)
            {
                int id = m_latestResp2d[idx].Id;
                m_persons[idx].Id = m_latestResp2d[idx].Id;
                m_persons[idx].IdState = m_poseEstimator.GetIDState(id);
                m_persons[idx].IsMain = m_latestResp2d[idx].IsMain;
                m_persons[idx].Pose2d = m_latestResp2d[idx];

                m_persons[idx].RawPose3D = (m_latestRespRaw3d == null ? null : (from p in m_latestRespRaw3d where p.Id == id select p).FirstOrDefault());
                m_persons[idx].Pose3D = (m_latestResp3d == null ? null : (from p in m_latestResp3d where p.Id == id select p).FirstOrDefault());
                m_persons[idx].PoseHead = (m_latestRespHead == null ? null : (from p in m_latestRespHead where p.Id == id select p).FirstOrDefault());
                m_persons[idx].PoseFace = (m_latestRespFaces == null ? null : (from p in m_latestRespFaces where p.Id == id select p).FirstOrDefault());
            }
        }

        public bool isRunning() { return m_isRunning; }
        public void Kill() { m_isRunning = false; }
        public void Join() { m_thread.Join(); }


        /// <summary>
        ///   Send a frame to be processed by the pose estimator.
        /// </summary>
        /// <param name="frame"> linear array of BGR8 pixels.</param>
        /// <param name="width"> width of the frame </param>
        /// <param name="height"> height of the frame </param>
        public void enqueueFrame(byte[] frame, int width, int height)
        {
            if (frame == null)
                return;
            m_frameWaitHandle.WaitOne();
            if (m_frame == null || m_frame.Length != frame.Length)
            {
                m_frame = new byte[frame.Length];
                m_width = width;
                m_height = height;
            }

            frame.CopyTo(m_frame, 0);
            frameEnqueued = true;
            m_frameWaitHandle.Set();
        }

        /// <summary>
        ///  Change between horizontal and vertical models.
        /// </summary>
        /// <param name="horizontal">If set to <c>true</c> horizontal.</param>
        public void changePoseEstimator(bool horizontal = true)
        {
#if UNITY_IOS && !UNITY_EDITOR
            m_poseEstimator = horizontal? m_poseEstimatorHorizontal: m_poseEstimatorVertical;
#else
            m_poseEstimator = m_poseEstimatorHorizontal;
#endif
        }

        public void SetIKProperty(int prop, float val)
        {
            m_poseEstimator.SetIKProperty(prop, val);
        }

        public JointDefinition GetJointDef2D() { return m_poseEstimator.JointDefinition2d; }
        public JointDefinition GetJointDef3D() { return m_poseEstimator.JointDefinition3d; }
        public Pose3D GetDefaultTPose3D() { return m_poseEstimator.GetTPose3D(0); }
    }
}
