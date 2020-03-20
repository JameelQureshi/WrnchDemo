/*
 Copyright (c) 2019 Wrnch Inc.
 All rights reserved
*/

using System.Linq;
using UnityEngine;
using System;

namespace wrnchAI.wrAPI
{
    public class wrTransform : ICloneable
    {
        public Quaternion q { get; set; }
        public Vector3 p { get; set; }
        public wrTransform() { }
        public wrTransform(Quaternion quat, Vector3 pos)
        {
            q = quat;
            p = pos;
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }

    /// <summary>
    ///  Class providing temporal joints filtering and analysis for jitter reduction
    /// </summary>
    public class JointsFilter
    {
        private OneEuroFilter<Quaternion>[] m_rotationFilters;
        private OneEuroFilter<Vector3> m_rootPositionFilter;

        public float filterFrequency = 60.0f;
        public float filterMinCutoff = 5.0f;
        public float filterBeta = 0.01f;
        public float filterDcutoff = 1.0f;

        private Quaternion[] m_worldTPose;

        private bool m_useHeadPoseEstimator;

        private float m_filterAlpha = 1.0f; //Default to no smoothing (Native smoothing is faster).
        private int m_numJoints;
        private float m_headRotPart = 0.70f;

        private int m_headIdx = wrExtended.GetIdByName("HEAD");
        private int m_neckIdx = wrExtended.GetIdByName("NECK");
        private int m_pelvIdx = wrExtended.GetIdByName("PELV");

        public JointsFilter()
        {
        }

        private static bool ContainsNaN(Quaternion q)
        {
            return float.IsNaN(q.x) || float.IsNaN(q.y) || float.IsNaN(q.z) || float.IsNaN(q.w);
        }

        private static Quaternion QuatFromWrnchAI(float[] rotations, int idx)
        {
            return new Quaternion(rotations[4 * idx + 0],
                                  -rotations[4 * idx + 1],
                                  -rotations[4 * idx + 2],
                                  rotations[4 * idx + 3]);
        }

        private Quaternion QuatFromEuler(Vector3 euler)
        {
            return Quaternion.Euler(Mathf.Rad2Deg * euler.x,
                            Mathf.Rad2Deg * euler.y,
                            Mathf.Rad2Deg * euler.z);
        }

        /// <summary>
        ///  Initialize the joint filter and underlying structures.
        /// </summary>
        /// <param name="transforms"> Initial set of transform to initialize the filter. Identity transforms could do the trick and the filter will be stabilized after 2-3 iterations</param>
        /// <param name="eulerTPose"> Euler angles (in degrees) representing the character TPose in world coordinates. The joint order is the wrExtended joints order (see JointDefinitions)</param>
        public void Init(GameObject[] initialPose)
        {
            m_numJoints = initialPose.Length;
            m_rotationFilters = new OneEuroFilter<Quaternion>[m_numJoints];
            for (int i = 0; i < m_numJoints; i++)
            {
                m_rotationFilters[i] = new OneEuroFilter<Quaternion>(filterFrequency, filterMinCutoff, filterBeta, filterDcutoff);
                m_rotationFilters[i].Filter(Quaternion.identity);
            }

            m_rootPositionFilter =
                new OneEuroFilter<Vector3>(filterFrequency, filterMinCutoff, filterBeta, filterDcutoff);

            m_rootPositionFilter.Filter(new Vector3());

            m_worldTPose = new Quaternion[m_numJoints];

            for (int i = 0; i < m_numJoints; i++)
            {
                m_worldTPose[i] = initialPose[i] == null ? Quaternion.identity : initialPose[i].transform.rotation;
            }
        }
        private Quaternion FilterRotation(Quaternion previousRotation, Quaternion newRotation)
        {
            if (ContainsNaN(newRotation))
            {
                return ContainsNaN(previousRotation) ? Quaternion.identity : previousRotation;
            }

            newRotation.Normalize();

            if (ContainsNaN(previousRotation))
            {
                return newRotation;
            }

            return Quaternion.Slerp(previousRotation, newRotation, m_filterAlpha);
        }

        private Quaternion ApplyRotation(int idx, Quaternion newRotation)
        {
            var previousRotation = m_rotationFilters[idx].currValue;
            if (ContainsNaN(newRotation))
            {
                // If we don't have a valid new rotation, reuse the previous rotation
                newRotation = previousRotation;
            }

            var filteredRotation = m_rotationFilters[idx].Filter(newRotation);
            return filteredRotation * m_worldTPose[idx];
        }

        public Vector3 FilterPosition(Vector3 pos)
        {
            return m_rootPositionFilter.Filter(pos);
        }

        public wrTransform[] JointsToTransform(Person person)
        {
            if (person.Pose3D == null)
            {
                return null;
            }

            var rotations = person.Pose3D.Rotations;

            var numJoints = person.Pose3D.NumJoints;
            var transforms = new wrTransform[numJoints];

            bool headLess = person.PoseHead == null || ContainsNaN(QuatFromWrnchAI(rotations, m_headIdx));

            for (int i = 0; i < person.Pose3D.NumJoints; ++i)
            {
                if (headLess && (i == m_neckIdx || i == m_headIdx))
                    continue;

                transforms[i] = new wrTransform(ApplyRotation(i, QuatFromWrnchAI(rotations, i)), Vector3.zero);
            }

            if (headLess)
            {
                var spineQuat = QuatFromWrnchAI(rotations, wrExtended.GetIdByName("SPINE2"));
                transforms[m_headIdx] = new wrTransform(ApplyRotation(m_headIdx, spineQuat), Vector3.zero);
                transforms[m_neckIdx] = new wrTransform(ApplyRotation(m_neckIdx, spineQuat), Vector3.zero);
            }

            return transforms;
        }
    };
}
