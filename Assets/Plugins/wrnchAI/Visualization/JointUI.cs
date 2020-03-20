/* 
Copyright (c) 2019 Wrnch Inc.
 All rights reserved
*/

using UnityEngine;
using UnityEngine.UI;

namespace wrnchAI.Visualization
{
    /// <summary>
    /// Component for drawing joints on top of UI components.
    /// </summary>
    public class JointUI : Joint
    {
        RawImage m_rawImage;
        Vector2 m_Offset;

        protected override void Start()
        {
            base.Start();
            m_rawImage = GetComponent<RawImage>();
            ApplyColor();
            if (m_rawImage != null)
            {
                var rectTransform = m_rawImage.rectTransform;
                m_Offset = new Vector2(rectTransform.sizeDelta.x * rectTransform.localScale.x * 0.5f,
                                       rectTransform.sizeDelta.y * rectTransform.localScale.y * 0.5f);
            }
        }

        protected override void ApplyColor()
        {
            if (m_rawImage != null)
            {
                m_rawImage.color = m_color;
            }
        }

        public override void SetPosition(Vector3 inPosition)
        {
            transform.localPosition = new Vector3(inPosition.x - m_Offset.x, inPosition.y, inPosition.z);
        }

        public override Vector3 GetPosition()
        {
            var pos = transform.localPosition;
            pos.x += m_Offset.x;
            return pos;
        }

        protected override void OnDestroy()
        {
            if (m_rawImage != null)
                Destroy(m_rawImage);
        }
    }
}
