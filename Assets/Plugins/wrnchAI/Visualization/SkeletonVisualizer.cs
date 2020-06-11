/*
Copyright (c) 2019 Wrnch Inc.
All rights reserved
*/

using System;
using System.Collections.Generic;
using UnityEngine;
using wrnchAI.Core;
using wrnchAI.wrAPI;

namespace wrnchAI.Visualization
{
    /// <summary>
    ///  Sample visualizer that displays a skeleton overlay for each person based on 2D pose estimation.
    /// </summary>
    class SkeletonVisualizer : BaseVisualizer
    {
        //Transformation matrix sending joints in normalized framespace (topleft origin). 
        //To the required texture space (centered origin, y up, x right).
        private Matrix4x4 m_jointToVideoQuad;
        private List<int[]> m_boneMap;
        private List<Skeleton> m_skeletons;

        //Texture space will be different if the visualizer component is bound to 
        //a Renderer or RawImage
        private bool m_isUI;
        public bool IsUI
        {
            get { return m_isUI; }
            set { m_isUI = value; BuildJointToVideoquadMatrix(); }
        }

        private Vector2 m_jointScaleOffset = new Vector2(1f, 1f);

        /// <summary>
        /// Build the transformation matrix sending joints from framespace to texture space.
        /// </summary>
        private void BuildJointToVideoquadMatrix()
        {
            if (m_isUI)
            {
                var rectTransform = gameObject.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    m_jointToVideoQuad.SetTRS(new Vector3(-0.5f * rectTransform.sizeDelta.x, 0.5f * rectTransform.sizeDelta.y, 0.0f), Quaternion.identity, new Vector3(1.0f * rectTransform.sizeDelta.x, -1.0f * rectTransform.sizeDelta.y));
                    m_jointScaleOffset = new Vector2(1f, 1f); //new Vector2(rectTransform.sizeDelta.x, rectTransform.sizeDelta.y);
                }
            }
            else
            {
                m_jointToVideoQuad.SetTRS(new Vector3(-0.5f, 0.5f, 0.0f), Quaternion.identity, new Vector3(1.0f, -1.0f, 0.0f));
                m_jointScaleOffset = new Vector2(1f, 1f);
            }
            foreach (var s in m_skeletons)
            {
                s.JointToVideoQuad = m_jointToVideoQuad;
                s.JointScaleOffset = m_jointScaleOffset;
            }
        }

        private void Awake()
        {
            m_jointToVideoQuad = new Matrix4x4();
            m_skeletons = new List<Skeleton>();
            BuildJointToVideoquadMatrix();
            PoseManager.onPoseReceived += UpdatePersons;
        }

        private void Start()
        {
            m_boneMap = PoseManager.Instance.JointDefinition2D.GetBonePairs();
        }

        /// <summary>
        /// Spawn a new skeleton on the visualizer with a color and a Person structure.
        /// </summary>
        /// <param name="personToAdd"></param>
        /// <param name="visualColor"></param>
        public override void AddNewPerson(Person personToAdd, Color visualColor)
        {
            Skeleton skeleton;


            if (m_isUI)
                skeleton = Instantiate(Resources.Load("PF_SkeletonUI") as GameObject).GetComponent<Skeleton>();
            else
                skeleton = Instantiate(Resources.Load("PF_Skeleton") as GameObject).GetComponent<Skeleton>();

            Debug.Log($"personToAdd: {personToAdd}");
            Debug.Log($"skeleton: {skeleton}");
            Debug.Log($"m_boneMap: {m_boneMap}");

            skeleton.color = visualColor;
            skeleton.JointToVideoQuad = m_jointToVideoQuad;


            skeleton.Id = personToAdd.Id;

           

            try
            {
                skeleton.JointScaleOffset = new Vector2(gameObject.transform.localScale.y, gameObject.transform.localScale.x);
                Debug.Log("After Assigning JointScaleOffset");
            }
            catch (Exception e)
            {
                Debug.Log("Error Caught");
                Debug.Log(e);
            }


            skeleton.Init(m_boneMap);
            skeleton.transform.SetParent(gameObject.transform, false);
            m_skeletons.Add(skeleton);

        }

        /// <summary>
        /// Remove an active skeleton based on its Id.
        /// </summary>
        /// <param name="personToRemove"></param>
        public override void RemovePerson(Person personToRemove)
        {
            Skeleton toRemove = m_skeletons.Find(x => x.Id == personToRemove.Id);
            if (toRemove != null)
            {
                m_skeletons.Remove(toRemove);
                GameObject.Destroy(toRemove.gameObject);
            }
        }

        /// <summary>
        ///  Update 2d skeletons with incoming poses, will automatically resize internal lists
        /// </summary>
        /// <param name="skeletons"></param>
        public override void UpdatePersons(List<Person> skeletons)
        {
            foreach (Person p in skeletons)
            {
                for (int i = 0; i < m_skeletons.Count; i++)
                {
                    if (m_skeletons[i] != null)
                    {
                        if (m_skeletons[i].Id == p.Id)
                        {
                            m_skeletons[i].UpdateSkeleton(p);
                        }
                    }
                }
            }
        }

        public int GetNumBones()
        {
            return m_boneMap.Count;
        }

        private void OnDestroy()
        {
            for (int i = 0; i < m_skeletons.Count; i++)
            {
                GameObject.Destroy(m_skeletons[i]);
            }
        }
    }
}