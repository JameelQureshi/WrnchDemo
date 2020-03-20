/*
 Copyright (c) 2019 Wrnch Inc.
 All rights reserved
*/

using System;
using UnityEngine;

namespace wrnchAI.Visualization
{
    /// <summary>
    /// Dissolving component to apply some fancy dissolving effect on a character.
    /// </summary>
    public class Dissolving : MonoBehaviour
    {
        //Must contain the dissolve shader
        [SerializeField]
        private GameObject[] m_objectsToDissolve;

        private Action DissolveAction;

        public Action OnDissolveAppearComplete;
        public Action OnDissolveDisappearComplete;

        public float SecondsToDissolve = 0.5f;
        private float m_dissolveStatus = 0.0f;

        void Awake()
        {
            DissolveAction = EmptyAction;
            m_dissolveStatus = SecondsToDissolve;
        }

        void Update()
        {
            if (DissolveAction != null)
                DissolveAction();
        }

        private void EmptyAction() { }

        //Used to force the seeing / unseeing of the asset based on the shader
        public void SetAppears(bool shouldAppear)
        {
            if (shouldAppear)
            {
                m_dissolveStatus = SecondsToDissolve;
            }
            else
            {
                m_dissolveStatus = 0.0f;
            }
            ApplyDissolution(m_dissolveStatus / SecondsToDissolve);
        }

        public void StartAppearing()
        {
            DissolveAction = DissolveAppear;
        }

        public void StartDisappearing()
        {
            DissolveAction = DissolveDisappear;
        }

        private void DissolveAppear()
        {
            m_dissolveStatus -= Time.deltaTime;
            if (m_dissolveStatus > 0.0f)
            {
                ApplyDissolution(m_dissolveStatus / SecondsToDissolve);
            }
            else
            {
                ApplyDissolution(0.0f);
                DissolveAction = EmptyAction;
                if (OnDissolveAppearComplete != null)
                {
                    OnDissolveAppearComplete();
                }
            }
        }

        private void DissolveDisappear()
        {
            m_dissolveStatus += Time.deltaTime;
            if (m_dissolveStatus < SecondsToDissolve)
            {
                ApplyDissolution(m_dissolveStatus / SecondsToDissolve);
            }
            else
            {
                ApplyDissolution(SecondsToDissolve);
                DissolveAction = EmptyAction;
                if (OnDissolveDisappearComplete != null)
                {
                    OnDissolveDisappearComplete();
                }
            }
        }

        private void ApplyDissolution(float amount)
        {
            foreach (GameObject go in m_objectsToDissolve)
            {
                go.GetComponent<Renderer>().material.SetFloat("_Level", amount);
            }
        }
    }
}
