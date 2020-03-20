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
        private wrTransform[] m_previousTransforms;

        private Vector3 m_prevPelvisLocation = Vector3.negativeInfinity;

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

            m_previousTransforms = Enumerable.Range(0, initialPose.Length).Select(i => new wrTransform(Quaternion.identity, Vector3.zero)).ToArray();
            m_worldTPose = new Quaternion[m_numJoints];

            for (int i = 0; i < m_numJoints; i++)
            {
                m_worldTPose[i] = initialPose[i] == null ? Quaternion.identity : initialPose[i].transform.rotation;
            }

            m_prevPelvisLocation = Vector3.one * -1.0f;
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
            m_previousTransforms[idx].q = FilterRotation(m_previousTransforms[idx].q, newRotation);
            return m_previousTransforms[idx].q * m_worldTPose[idx];
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
                if(headLess && (i == m_neckIdx || i == m_headIdx))
                    continue;

                transforms[i] = new wrTransform(ApplyRotation(i, QuatFromWrnchAI(rotations, i)), Vector3.zero);
            }

            if(headLess)
            {
                var spineQuat = QuatFromWrnchAI(rotations, wrExtended.GetIdByName("SPINE2"));
                transforms[m_headIdx] = new wrTransform(ApplyRotation(m_headIdx, spineQuat), Vector3.zero);
                transforms[m_neckIdx] = new wrTransform(ApplyRotation(m_neckIdx, spineQuat), Vector3.zero);
            }

            var positions = person.Pose3D.Positions;
            var scaleHints = person.Pose3D.ScaleHint;

            var pelvLocation = new Vector3(
                positions[3 * m_pelvIdx] * scaleHints[0],
                positions[3 * m_pelvIdx + 1] * scaleHints[1],
                positions[3 * m_pelvIdx + 2] * scaleHints[2]);

            if(m_prevPelvisLocation.z < 0)
            {
                m_prevPelvisLocation = pelvLocation;
            }

            transforms[m_pelvIdx].p = m_prevPelvisLocation = Vector3.Lerp(m_prevPelvisLocation, pelvLocation, m_filterAlpha);

            return transforms;
        }
    };
}
