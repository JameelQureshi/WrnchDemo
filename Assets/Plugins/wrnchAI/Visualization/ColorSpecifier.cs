/*
 Copyright (c) 2019 Wrnch Inc.
 All rights reserved
*/

using UnityEngine;

namespace wrnchAI.Visualization
{
    /// <summary>
    ///  This class is used to specify the color when using the Dissolving shader.
    /// </summary>
    public class ColorSpecifier : MonoBehaviour
    {
        private Color m_currentColor = Color.clear;
        public Color Color
        {
            get { return m_currentColor; }
            set
            {
                m_currentColor = value;
                Transform[] cic = GetComponentsInChildren<Transform>();
                foreach (Transform go in cic)
                {
                    Renderer renderer = go.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        renderer.material.SetColor("_EdgeColor", value);
                    }
                }
            }
        }
    }
}

