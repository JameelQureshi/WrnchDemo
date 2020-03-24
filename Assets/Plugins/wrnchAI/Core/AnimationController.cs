/*
 Copyright (c) 2019 Wrnch Inc.
 All rights reserved
*/

using UnityEngine;
using System;
using System.Collections.Generic;
using wrnchAI.wrAPI;
using wrnchAI.Visualization;

namespace wrnchAI.Core
{
    /// <summary>
    /// Sample animation controller that drives an avatar based on pose estimation.
    /// Updates joint rotation accordingly and positions the game object in the scene.
    /// </summary>
    [Serializable]
    public class AnimationController : MonoBehaviour
    {
        public static bool showAvatar = true;


        private JointsFilter m_jointFilter;
        private OneEuroFilter<Vector3> m_rootPositionFilter;
        public static List<Quaternion> WorldTPose;

        protected float m_tPoseAnkleY;
        private float m_tPosePelvisY;

        protected float m_floorY = 0.0F;
        protected float m_scale3DtoMesh = 0.006F;

        private float m_spawnTimer;
        private Dissolving m_dissolver;
        private bool m_setToDestroy = false;

        private float m_halfVideoScale = 1.0f;

        private wrTransform[] m_filteredTransforms;
        private Vector3 m_rootPosition;
        private Vector3 m_filteredRootPosition;
        public Action JointsUpdateFinished;

        private Color m_characterColor;
        public Color CharacterColor
        {
            get { return m_characterColor; }
            set
            {
                m_characterColor = value;
                var cs = gameObject.GetComponent<ColorSpecifier>();
                if (cs != null)
                {
                    cs.Color = value;
                }
            }
        }

        [SerializeField]
        public GameObject[] JointsToRig = new GameObject[30];

        [SerializeField]
        private int m_id;
        public int Id { get { return m_id; } set { m_id = value; } }

        private List<int> m_chain;

        /// <summary>
        /// Adjusts horizontal placement of the avatar in the scene based on position of the pelvis in 2D estimation.
        /// </summary>
        protected virtual void TranslateX(Person p)
        {
            if (p.Pose2d == null)
            {
                return;
            }
            var pos = m_rootPosition;
            var joints = p.Pose2d.Joints;
            float jointValue = joints[2 * PoseManager.Instance.JointDefinition2D.GetJointIndex("PELV")];
            // Valid pelvis position found
            if (jointValue > 0)
            {
                pos.x = m_halfVideoScale * (1.0f - 2.0f * jointValue);
                m_rootPosition = pos;
                foreach (Renderer r in GetComponentsInChildren<Renderer>())
                {
                    if (showAvatar)
                        r.enabled = true;
                }
            }
            else
            {
                // Do not display the Rig
                foreach (Renderer r in GetComponentsInChildren<Renderer>())
                {
                    r.enabled = false;
                }
            }
        }

        private void UpdateVideoQuadScale(Vector3 newScale)
        {
            m_halfVideoScale = newScale.x * 0.5f;
        }

        protected void OnEnable()
        {
            VideoVisualizer.OnVideoScaleUpdated += UpdateVideoQuadScale;
        }

        protected void OnDisable()
        {
            VideoVisualizer.OnVideoScaleUpdated -= UpdateVideoQuadScale;
        }

        /// <summary>
        /// Adjusts vertical placement of the avatar in the scene based on geometry of the legs in 3D estimation.
        /// </summary>
        protected virtual void TranslateY(Person p)
        {
            var pos = m_rootPosition;
            pos.y = m_floorY;//+ m_tPoseAnkleY; // Initial position

            if (p.Pose3D != null)
            {
                float pelvisY = p.Pose3D.Positions[3 * PoseManager.Instance.JointDefinition3D.GetJointIndex("PELV") + 1];
                float lAnkleY = p.Pose3D.Positions[3 * PoseManager.Instance.JointDefinition3D.GetJointIndex("LANKLE") + 1];
                float rAnkleY = p.Pose3D.Positions[3 * PoseManager.Instance.JointDefinition3D.GetJointIndex("RANKLE") + 1];
                float ankleY = Mathf.Min(lAnkleY, rAnkleY);

                float tPosePelvisHeight = m_tPosePelvisY - m_tPoseAnkleY;
                float currentPelvisHeight = (pelvisY - ankleY) * m_scale3DtoMesh;

                // Offset position to keep feet on the floor when squatting
                pos.y -= (tPosePelvisHeight - currentPelvisHeight);
            }
            if (!float.IsNaN(pos.x) && !float.IsNaN(pos.y) && !float.IsNaN(pos.z))
            {
                m_rootPosition = pos;
            }
        }

        public void Awake()
        {
            m_dissolver = GetComponent<Dissolving>();
            m_jointFilter = new JointsFilter();
            foreach (Renderer r in GetComponentsInChildren<Renderer>())
            {
                r.enabled = false;
            }
            List<Quaternion> initialPose = new List<Quaternion>();
            Dictionary<GameObject, int> reverseJointMap = new Dictionary<GameObject, int>();
            for (int i = 0; i < JointsToRig.Length; i++)
            {
                var jointObj = JointsToRig[i];
                initialPose.Add(jointObj == null ? Quaternion.identity : jointObj.transform.rotation);
                reverseJointMap[jointObj] = i;
            }

            var pelvisTransform = JointsToRig[wrExtended.GetIdByName("PELV")].transform;
            m_tPoseAnkleY = JointsToRig[wrExtended.GetIdByName("LANKLE")].transform.position.y;
            m_tPosePelvisY = pelvisTransform.position.y;

            Pose3D ikTPose = PoseManager.Instance.GetDefaultTPose3D();
            if (ikTPose != null)
            {
                float ikPelvisY = ikTPose.Positions[3 * PoseManager.Instance.JointDefinition3D.GetJointIndex("PELV") + 1];
                float ikAnkleY = ikTPose.Positions[3 * PoseManager.Instance.JointDefinition3D.GetJointIndex("LANKLE") + 1];

                m_scale3DtoMesh = (m_tPosePelvisY - m_tPoseAnkleY) / (ikPelvisY - ikAnkleY);
            }

            Vector3 initialPosition = gameObject.transform.position;
            initialPosition.y = m_floorY + m_tPoseAnkleY; // TPose ankle height is about 0
            gameObject.transform.position = initialPosition;

            m_jointFilter.Init(JointsToRig);

            UpdateVideoQuadScale(PoseManager.Instance.VisualizerConfig.Visualizer.transform.localScale);

            // GetComponentsInChildren does a depth-first traversal of the hierarchy
            // We can use this to determine the kinematic chain
            m_chain = new List<int>();
            foreach (var joint in pelvisTransform.GetComponentsInChildren<Transform>())
            {
                if (reverseJointMap.ContainsKey(joint.gameObject))
                {
                    var jointIdx = reverseJointMap[joint.gameObject];
                    m_chain.Add(jointIdx);
                }
            }
        }

        public void Appear(bool fromStart = true)
        {
            foreach (Renderer r in GetComponentsInChildren<Renderer>())
            {
                if(showAvatar)
                r.enabled = true;
            }
           // if (m_dissolver != null)
            //{
              //  m_dissolver.SetAppears(true);
               // m_dissolver.StartAppearing();
            //}
        }

        public void Disappear()
        {
            foreach (Renderer r in GetComponentsInChildren<Renderer>())
            {
                r.enabled = false;
            }

            //if (m_dissolver != null)
            //{
            //    m_dissolver.StartDisappearing();
            //}
        }

        public void StartDeleting()
        {
            if (m_dissolver != null)
            {
                if (!m_setToDestroy)
                {
                    m_dissolver.StartDisappearing();
                    m_dissolver.OnDissolveDisappearComplete += SelfDestroyer;
                    m_setToDestroy = true;
                }
            }
            else
            {
                SelfDestroyer();
            }
        }

        private void SelfDestroyer()
        {
            if (m_dissolver)
            {
                m_dissolver.OnDissolveDisappearComplete -= SelfDestroyer;
                foreach (Renderer r in GetComponentsInChildren<Renderer>())
                {
                    Destroy(r.material);
                }
            }
            if (this != null)
                Destroy(this.gameObject);
        }

        /// <summary>
        /// Sets each joint's rotation relative to TPose based on smoothed estimation results.
        /// </summary>
        public virtual void UpdateJoints(Person person)
        {
            if (person == null)
                return;

            TranslateX(person);
            TranslateY(person);

            m_filteredTransforms = m_jointFilter.JointsToTransform(person);
            m_filteredRootPosition = m_jointFilter.FilterPosition(m_rootPosition);
        }

        void LateUpdate()
        {
            if (m_filteredTransforms != null)
            {
                foreach (int jointIdx in m_chain)
                {
                    if (JointsToRig[jointIdx] != null && m_filteredTransforms[jointIdx] != null)
                    {
                        JointsToRig[jointIdx].transform.rotation = m_filteredTransforms[jointIdx].q;
                    }
                }

                if (JointsUpdateFinished != null)
                {
                    JointsUpdateFinished();
                }
            }
            transform.position = m_filteredRootPosition;
        }
    }
}
