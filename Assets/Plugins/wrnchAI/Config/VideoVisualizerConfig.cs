using System;
using UnityEngine;
using wrnchAI.Core;
using wrnchAI.Visualization;

namespace wrnchAI.Config
{
    [Serializable]
    public class VideoVisualizerConfig : ISerializationCallbackReceiver, ICloneable, IEquatable<VideoVisualizerConfig>
    {
        private VideoVisualizerConfig m_cached;
        public static event Action<VideoVisualizerConfig> onConfigChanged;

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        #region ISerializationCallbackReceiver implementation
        public void OnAfterDeserialize() { }

        public void OnBeforeSerialize()
        {
            if (this != m_cached)
                this.OnValidate();
        }
        #endregion

        #region IEquatable and comparison implementation
        public static bool operator ==(VideoVisualizerConfig lhs, VideoVisualizerConfig rhs)
        {
            if (object.ReferenceEquals(lhs, null))
                return object.ReferenceEquals(rhs, null);

            return lhs.Equals(rhs);
        }

        public static bool operator !=(VideoVisualizerConfig lhs, VideoVisualizerConfig rhs)
        {
            return !(lhs == rhs);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            return this.Equals(obj as VideoVisualizerConfig);
        }

        public bool Equals(VideoVisualizerConfig rhs)
        {
            return this.m_mirrorX == rhs.m_mirrorX &&
                   this.m_mirrorY == rhs.m_mirrorY &&
                   this.m_rotationMultipleOf90 == rhs.m_rotationMultipleOf90;
        }
        #endregion

        public VideoVisualizerConfig()
        {
            m_cached = this.Clone() as VideoVisualizerConfig;
        }

        [SerializeField]
        private GameObject m_visualizer;
        public GameObject Visualizer
        {
            get { return m_visualizer; }
            set
            {
                m_visualizer = value;
                this.OnValidate();
            }
        }

        [SerializeField]
        private bool m_mirrorX;
        public bool MirrorX
        {
            get { return m_mirrorX; }
            set
            {
                m_mirrorX = value;
                this.OnValidate();
            }
        }

        [SerializeField]
        private bool m_mirrorY;
        public bool MirrorY
        {
            get { return m_mirrorY; }
            set
            {
                m_mirrorY = value;
                this.OnValidate();
            }
        }

        [SerializeField]
        private int m_rotationMultipleOf90;
        public int RotationMultipleOf90
        {
            get { return m_rotationMultipleOf90; }
            set
            {
                m_rotationMultipleOf90 = value;
                this.OnValidate();
            }
        }

        [SerializeField]
        private bool m_displaySkeletons;
        public bool DisplaySkeletons
        {
            get { return m_displaySkeletons; }
            set
            {
                m_displaySkeletons = value;
                this.OnValidate();
            }
        }

        public void OnValidate()
        {
            if (m_cached.m_visualizer != m_visualizer)
            {
                if (m_cached.m_visualizer != null)
                {
                    var videoComponent = m_cached.m_visualizer.GetComponent<VideoVisualizer>();
                    if (videoComponent != null)
                        GameObject.Destroy(videoComponent);

                    PoseManager.Instance.VisualHandler.UnregisterSkeletonVisualizer(m_cached.m_visualizer);
                }
                if (Application.isPlaying && m_visualizer != null) //Init will try to setup the material, which is forbidden via script in edit mode
                {
                    var visualizer = m_visualizer.AddComponent<VideoVisualizer>();
                    visualizer.Init(this);
                }
            }

            m_cached = this.Clone() as VideoVisualizerConfig;
            if (onConfigChanged != null)
                onConfigChanged(this);
        }
    }
}