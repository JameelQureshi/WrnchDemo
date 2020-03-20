/*
 Copyright (c) 2019 Wrnch Inc.
 All rights reserved
*/

using UnityEngine;

namespace wrnchAI.Visualization
{
    /// <summary>
    /// Base class for drawing Skeleton joints
    /// </summary>
    public abstract class Joint : MonoBehaviour
    {
        protected Color m_color = new Color(0.15f, 0.66f, 0.89f);

        public Color Color
        {
            get { return m_color; }
            set
            {
                m_color = value;
                ApplyColor();
            }
        }

        protected int m_jointId;
        public int JointId { get { return m_jointId; } set { m_jointId = value; } }

        protected virtual void Start()
        {
        }

        protected abstract void ApplyColor();
        public abstract void SetPosition(Vector3 inPosition);
        public abstract Vector3 GetPosition();
        public void ScaleJoint(Vector2 scaleFactor)
        {
            var baseScale = gameObject.transform.localScale;
            baseScale.x *= scaleFactor.x;
            baseScale.y *= scaleFactor.y;
            gameObject.transform.localScale = baseScale;
        }

        protected virtual void OnDestroy() { }
    }
}
