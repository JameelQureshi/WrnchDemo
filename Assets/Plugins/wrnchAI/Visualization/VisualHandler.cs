/*
 Copyright (c) 2019 Wrnch Inc.
 All rights reserved
*/
using System.Collections.Generic;
using UnityEngine;
using wrnchAI.Core;
using wrnchAI.wrAPI;

class Triple<T, U, I>
{
    public Triple(T _key, U _val, I _color)
    {
        Key = _key;
        Val = _val;
        Color = _color;
    }
    public T Key;
    public U Val;
    public I Color;
}

namespace wrnchAI.Visualization
{
    /// <summary>
    ///  Class responsible for managing and updating visualizations with new pose estimation results.
    /// </summary>
    public class VisualHandler : MonoBehaviour
    {

        private List<KeyValuePair<AnimationController, Color>> m_rigsNdColors;
        private ColorCollection m_colorCollection;

        private List<BaseVisualizer> m_visualizers = new List<BaseVisualizer>();

        private List<Person> m_lastPersons;

        private List<Triple<Person, float, Color>> m_knownPersons = new List<Triple<Person, float, Color>>();
        private float m_visualTimeout = 0.1f;

        public bool IsMultiPerson = true; // try to turn off multiperson

        void Start()
        {
            AnimationController[] wrA = FindObjectsOfType<AnimationController>();
            m_rigsNdColors = new List<KeyValuePair<AnimationController, Color>>();
            foreach (AnimationController wac in wrA)
            {
                m_rigsNdColors.Add(new KeyValuePair<AnimationController, Color>(wac, wac.CharacterColor));
            }
            m_colorCollection = new ColorCollection();
        }

        public void RegisterSkeletonVisualizer(GameObject videoQuad, bool isUI = false)
        {
            var visualizer = videoQuad.GetComponent<SkeletonVisualizer>();
            if (visualizer == null)
            {
                visualizer = videoQuad.AddComponent<SkeletonVisualizer>();
                visualizer.IsUI = isUI;
                m_visualizers.Add(visualizer);
            }
            else
            {
                if (!m_visualizers.Contains(visualizer))
                {
                    m_visualizers.Add(visualizer);
                }
            }
        }

        public void RegisterAvatarVisualizer(GameObject avatarPrefab)
        {
            var visualizer = gameObject.GetComponent<AvatarHandler>();
            if (visualizer == null)
            {
                visualizer = gameObject.AddComponent<AvatarHandler>();
                visualizer.Init(avatarPrefab);
                m_visualizers.Add(visualizer);
            }
            else
            {
                if (!m_visualizers.Contains(visualizer))
                {
                    m_visualizers.Add(visualizer);
                }
            }
        }

        public void UnregisterAvatarVisualizer(GameObject avatarPrefab)
        {
            for (int i = m_visualizers.Count - 1; i >= 0; --i)
            {
                var v = m_visualizers[i];
                if (v.GetType() == typeof(AvatarHandler) && (v as AvatarHandler).AvatarPrefab == avatarPrefab)
                {
                    Destroy(v);
                    m_visualizers.RemoveAt(i);
                }
            }
        }

        public void UnregisterAllAvatarVisualizers(GameObject avatarPrefab)
        {
            for (int i = m_visualizers.Count - 1; i >= 0; --i)
            {
                if (m_visualizers[i] != null &&
                    m_visualizers[i].GetType() == typeof(AvatarHandler) &&
                    (avatarPrefab != null && (m_visualizers[i] as AvatarHandler).AvatarPrefab == avatarPrefab))
                {
                    Destroy(m_visualizers[i]);
                    m_visualizers.RemoveAt(i);
                }
            }
            m_knownPersons.Clear();
            m_colorCollection.Reset();
        }

        public void AddVisualizer(BaseVisualizer visualizer)
        {
            m_visualizers.Add(visualizer);
            for (int i = 0; i < m_knownPersons.Count; i++)
            {
                if (m_knownPersons[i].Key.IdState == IdState.Mature)
                {

                    //// Add white color to all skeletons
                    visualizer.AddNewPerson(m_knownPersons[i].Key, Color.white);
                }
            }
        }

        public void UnregisterSkeletonVisualizer(GameObject visualizer)
        {
            for (int i = m_visualizers.Count - 1; i >= 0; --i)
            {
                var v = m_visualizers[i];
                if (v != null && v.GetType() == typeof(SkeletonVisualizer) && v.gameObject == visualizer)
                {
                    Destroy(v);
                    m_visualizers.RemoveAt(i);
                }
            }
            m_knownPersons.Clear();
        }

        public void UnregisterAllSkeletonVisualizers()
        {
            foreach (var v in m_visualizers)
            {
                if (v != null)
                    Destroy(v);
            }
            m_visualizers.Clear();
            m_knownPersons.Clear();
            m_colorCollection.Reset();
        }

        public void DeactivateAllVisualizers()
        {
            for (int i = 0; i < m_visualizers.Count; ++i)
            {
                GameObject.Destroy(m_visualizers[i]);
            }
            m_visualizers.Clear();
            m_knownPersons.Clear();
            m_colorCollection.Reset();
        }

        public void CreateVisualizer<T>() where T : BaseVisualizer
        {
            new GameObject().AddComponent<T>();
        }

        private void UpdateAll(List<Person> persons)
        {
            List<Person> personsToAdd = new List<Person>();
            foreach (Person p in persons)
            {
                if (!m_knownPersons.Exists(x => x.Key.Id == p.Id))
                {
                    personsToAdd.Add(p);
                }
            }

            List<Person> personToRemove = new List<Person>();
            foreach (var p in m_knownPersons)
            {
                if (persons.Exists(x => x.Id == p.Key.Id))
                {
                    p.Val = 0.0f;
                }
                else
                {
                    p.Val += Time.deltaTime;
                    if (p.Val > m_visualTimeout)
                    {
                        personToRemove.Add(p.Key);
                    }
                }
            }

            foreach (var p in personToRemove)
            {
                var rm = m_knownPersons.Find(x => x.Key.Id == p.Id);
                m_colorCollection.DisposeColor(rm.Color);
                foreach (var bv in m_visualizers)
                {
                    bv.RemovePerson(p);
                }
                m_knownPersons.Remove(rm);
            }

            foreach (var p in personsToAdd)
            {
                Color chosenColor = m_colorCollection.GetNextColor();
                if (p.IdState != IdState.Mature)
                {
                    continue;
                }
                foreach (var bv in m_visualizers)
                {
                    //// Add white color to all skeletons
                    if (bv == null)
                    {
                       
                        Debug.LogError("visualizer is null" + m_visualizers.Count);
                    }
                    else
                    {
                        bv.AddNewPerson(p, Color.white);
                    }

                }
                m_knownPersons.Add(new Triple<Person, float, Color>(p, 0.0f, chosenColor));
            }

            foreach (BaseVisualizer bv in m_visualizers)
            {
                bv.UpdatePersons(persons);
            }
        }

        public void UpdatePersons(List<Person> persons)
        {
            if (IsMultiPerson)
            {
                UpdateAll(persons);
            }
            else
            {
                if (persons.Count > 0)
                {
                    List<Person> p = new List<Person>();
                    Person per = persons.Find(x => x.Id == 0);
                    if (per != null)
                    {
                        p.Add(per);
                    }
                    UpdateAll(p);
                }
            }
        }
    }
}
