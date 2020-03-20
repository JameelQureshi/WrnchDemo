using System;
using UnityEngine;

namespace wrnchAI.Config
{
    [Serializable]
    public class PoseWorkerConfig : ISerializationCallbackReceiver, ICloneable, IEquatable<PoseWorkerConfig>
    {
        public static event Action<PoseWorkerConfig> onConfigChanged;
        public static event Action<bool> onToggleMask;

        private PoseWorkerConfig m_cached;

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
        public static bool operator ==(PoseWorkerConfig lhs, PoseWorkerConfig rhs)
        {
            if (object.ReferenceEquals(lhs, null))
                return object.ReferenceEquals(rhs, null);

            return lhs.Equals(rhs);
        }

        public static bool operator !=(PoseWorkerConfig lhs, PoseWorkerConfig rhs)
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
            return this.Equals(obj as PoseWorkerConfig);
        }

        public bool Equals(PoseWorkerConfig rhs)
        {
            return m_multi3D == rhs.m_multi3D &&
                   m_estimateMask == rhs.m_estimateMask &&
                   m_jointSmoothing == rhs.m_jointSmoothing &&
                   m_headSmoothing == rhs.m_headSmoothing &&
                   m_estimateHead == rhs.m_estimateHead &&
                   m_rotationMultipleOf90 == rhs.m_rotationMultipleOf90 &&
                   m_overrideModelPath == rhs.m_overrideModelPath &&
                   m_serializedModelPath == rhs.m_serializedModelPath &&
                   m_trackerKind == rhs.m_trackerKind &&
                   m_licenseString == rhs.m_licenseString;
        }
        #endregion

        public PoseWorkerConfig()
        {
            m_cached = this.Clone() as PoseWorkerConfig;
        }

        [SerializeField]
        private bool m_multi3D;
        public bool Multi3D
        {
            get { return m_multi3D; }
            set
            {
                m_multi3D = value;
                this.OnValidate();
            }
        }

        [SerializeField]
        private bool m_estimateMask;
        public bool EstimateMask
        {
            get { return m_estimateMask; }
            set
            {
                m_estimateMask = value;
                this.OnValidate();
            }
        }

        [SerializeField]
        private bool m_jointSmoothing;
        public bool JointSmoothing
        {
            get { return m_jointSmoothing; }
            set
            {
                m_jointSmoothing = value;
                this.OnValidate();
            }
        }

        [SerializeField]
        private bool m_headSmoothing;
        public bool HeadSmoothing
        {
            get { return m_headSmoothing; }
            set
            {
                m_headSmoothing = value;
                this.OnValidate();
            }
        }

        [SerializeField]
        private bool m_estimateHead;
        public bool EstimateHead
        {
            get { return m_estimateHead; }
            set
            {
                m_estimateHead = value;
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
        private string m_overrideModelPath;
        public string OverrideModelPath
        {
            get { return m_overrideModelPath; }
            set
            {
                m_overrideModelPath = value;
                this.OnValidate();
            }
        }

        [SerializeField]
        private int m_deviceId = 0;
        public int DeviceId
        {
            get { return m_deviceId; }
            set
            {
                m_deviceId = value;
                this.OnValidate();
            }
        }

        [SerializeField]
        private string m_serializedModelPath;
        public string SerializedModelPath
        {
            get { return m_serializedModelPath; }
            set
            {
                m_serializedModelPath = value;
                this.OnValidate();
            }
        }

        [SerializeField]
        private string m_licenseString;
        public string LicenseString
        {
            get { return m_licenseString; }
            set
            {
                m_licenseString = value;
                this.OnValidate();
            }
        }

        [SerializeField]
        private wrnchAI.wrAPI.TrackerKind m_trackerKind;
        public wrnchAI.wrAPI.TrackerKind TrackerKind
        {
            get { return m_trackerKind; }
            set
            {
                m_trackerKind = value;
                this.OnValidate();
            }
        }

        [SerializeField]
        private Vector2Int m_netRes = new Vector2Int(456, 256);
        public Vector2Int NetRes
        {
            get { return m_netRes; }
            set
            {
                m_netRes = value;
                this.OnValidate();
            }
        }

        public void OnValidate()
        {
            if (m_cached.m_estimateMask != m_estimateMask)
                if (onToggleMask != null)
                    onToggleMask(m_estimateMask);

            m_cached = this.Clone() as PoseWorkerConfig;
            if (onConfigChanged != null)
                onConfigChanged(this);
        }
    }
}
