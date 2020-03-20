using System;
using UnityEngine;
using wrnchAI.Core;

namespace wrnchAI.Config
{
    [Serializable]
    public class VideoControllerConfig : ISerializationCallbackReceiver, ICloneable, IEquatable<VideoControllerConfig>
    {
        private VideoControllerConfig m_cached;
        public static event Action<VideoControllerConfig> onConfigChanged;
        public static event Action<int> OnRotationChanged;

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        #region ISerializationCallbackReceiver implementation
        public void OnAfterDeserialize()
        {

        }

        public void OnBeforeSerialize()
        {
            if (this != m_cached)
                this.OnValidate();
        }
        #endregion

        #region IEquatable and comparison implementation
        public static bool operator ==(VideoControllerConfig lhs, VideoControllerConfig rhs)
        {
            if (object.ReferenceEquals(lhs, null))
                return object.ReferenceEquals(rhs, null);

            return lhs.Equals(rhs);
        }

        public static bool operator !=(VideoControllerConfig lhs, VideoControllerConfig rhs)
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
            return this.Equals(obj as VideoControllerConfig);
        }

        public bool Equals(VideoControllerConfig rhs)
        {
            return this.m_deviceName == rhs.m_deviceName &&
                   this.m_desiredHeight == rhs.m_desiredHeight &&
                   this.m_desiredWidth == rhs.m_desiredWidth &&
                   this.m_mirrorX == rhs.m_mirrorX &&
                   this.m_mirrorY == rhs.m_mirrorY &&
                   this.m_rotationMultipleOf90 == rhs.m_rotationMultipleOf90;
        }
        #endregion

        public VideoControllerConfig()
        {
            m_cached = this.Clone() as VideoControllerConfig;
        }

        [SerializeField]
        protected string m_deviceName = "";
        public string DeviceName
        {
            get { return m_deviceName; }
            set
            {
                m_deviceName = value;
                if (onConfigChanged != null)
                    onConfigChanged(this);
            }
        }

        //TODO: Implement update
        [SerializeField]
        protected VideoMode m_videoMode = VideoMode.Webcam;
        public VideoMode Videomode { get { return m_videoMode; } set { m_videoMode = value; } }

        [SerializeField]
        private int m_desiredWidth = -1;
        public int DesiredWidth
        {
            get { return m_desiredWidth; }
            set
            {
                m_desiredWidth = value;
                this.OnValidate();
            }
        }

        [SerializeField]
        private int m_desiredHeight = -1;
        public int DesiredHeight
        {
            get { return m_desiredHeight; }
            set
            {
                m_desiredHeight = value;
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
        protected string m_videoURL = "";
        public string VideoURL
        {
            get { return m_videoURL; }
            set
            {
                m_videoURL = value;
                if (onConfigChanged != null)
                    onConfigChanged(this);
            }
        }

        public void OnValidate()
        {
            if (m_cached.m_rotationMultipleOf90 != m_rotationMultipleOf90)
                if (OnRotationChanged != null)
                    OnRotationChanged(m_rotationMultipleOf90);

            m_cached = this.Clone() as VideoControllerConfig;
            if (onConfigChanged != null)
                onConfigChanged(this);
        }
    }
}