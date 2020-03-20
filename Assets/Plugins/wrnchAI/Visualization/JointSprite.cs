/*
 Copyright (c) 2019 Wrnch Inc.
 All rights reserved
*/

using UnityEngine;

namespace wrnchAI.Visualization
{
    /// <summary>
    /// Joint prefab for drawing on top of a component with renderer attached.
    /// </summary>
    public class JointSprite : Joint
    {
        Renderer m_renderer;

        protected override void Start()
        {
            base.Start();
            m_renderer = GetComponent<Renderer>();
            ApplyColor();
        }

        protected override void ApplyColor()
        {
            if (m_renderer != null)
            {
                m_renderer.material.color = m_color;
            }
        }

        public override void SetPosition(Vector3 inPosition)
        {
            transform.localPosition = inPosition;
        }

        public override Vector3 GetPosition()
        {
            return transform.localPosition;
        }

        protected override void OnDestroy()
        {
            if (m_renderer != null && m_renderer.material != null)
                Destroy(this.m_renderer.material);
        }
    }
}