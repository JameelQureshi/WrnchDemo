/*
 Copyright (c) 2019 Wrnch Inc.
 All rights reserved
*/

using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace wrnchAI.wrAPI
{
    /// <summary>
    ///  Class representing a 2d bounding box as defined in wrAPI
    /// </summary>
    public class Box2d : ICloneable
    {
        [DllImport(Glob.DLLName)]
        private static extern float wrBox2d_GetMinX(IntPtr nativeHandle);
        [DllImport(Glob.DLLName)]
        private static extern float wrBox2d_GetMinY(IntPtr nativeHandle);
        [DllImport(Glob.DLLName)]
        private static extern float wrBox2d_GetWidth(IntPtr nativeHandle);
        [DllImport(Glob.DLLName)]
        private static extern float wrBox2d_GetHeight(IntPtr nativeHandle);

        public Box2d() { m_isValid = false; }

        public Box2d(float x, float y, float w, float h)
        {
            m_minX = x;
            m_minY = y;
            m_width = w;
            m_height = h;
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        /// <summary>
        ///  Fill the class from a native pointer returned by Pose Estimator.
        /// </summary>
        /// <param name="nativeHandle">The native pointer</param>
        public void Update(IntPtr nativeHandle)
        {
            if (nativeHandle == IntPtr.Zero)
            {
                Debug.LogError("Trying to initialize Box2d from nullptr");
                return;
            }
            m_minX = wrBox2d_GetMinX(nativeHandle);
            m_minY = wrBox2d_GetMinY(nativeHandle);
            m_width = wrBox2d_GetWidth(nativeHandle);
            m_height = wrBox2d_GetHeight(nativeHandle);
        }

        private float m_minX;
        public float MinX { get { return m_minX; } }

        private float m_minY;
        public float MinY { get { return m_minY; } }

        private float m_width;
        public float Width { get { return m_width; } }

        private float m_height;
        public float Height { get { return m_height; } }

        private bool m_isValid = false;
        public bool IsValid { get { return m_isValid; } }
    }

    /// <summary>
    ///  Class representing a 3d bounding box as defined in wrAPI
    /// </summary>
    public class Box3d : ICloneable
    {
        [DllImport(Glob.DLLName)]
        private static extern float wrBox3d_GetMinX(IntPtr nativeHandle);
        [DllImport(Glob.DLLName)]
        private static extern float wrBox3d_GetMinY(IntPtr nativeHandle);
        [DllImport(Glob.DLLName)]
        private static extern float wrBox3d_GetMinZ(IntPtr nativeHandle);
        [DllImport(Glob.DLLName)]
        private static extern float wrBox3d_GetWidth(IntPtr nativeHandle);
        [DllImport(Glob.DLLName)]
        private static extern float wrBox3d_GetHeight(IntPtr nativeHandle);
        [DllImport(Glob.DLLName)]
        private static extern float wrBox3d_GetDepth(IntPtr nativeHandle);

        public Box3d() { m_isValid = false; }

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        /// <summary>
        ///  Fill the class from a native pointer returned by Pose Estimator.
        /// </summary>
        /// <param name="nativeHandle">The native pointer</param>
        public void Update(IntPtr nativeHandle)
        {
            if (nativeHandle == IntPtr.Zero)
            {
                Debug.LogError("Trying to update a Box3d object from nullptr");
                return;
            }

            m_minX = wrBox3d_GetMinX(nativeHandle);
            m_minY = wrBox3d_GetMinY(nativeHandle);
            m_minZ = wrBox3d_GetMinZ(nativeHandle);
            m_width = wrBox3d_GetWidth(nativeHandle);
            m_height = wrBox3d_GetHeight(nativeHandle);
            m_depth = wrBox3d_GetDepth(nativeHandle);

            m_isValid = true;
        }

        private float m_minX;
        public float MinX { get { return m_minX; } }

        private float m_minY;
        public float MinY { get { return m_minY; } }

        private float m_minZ;
        public float MinZ { get { return m_minZ; } }

        private float m_width;
        public float Width { get { return m_width; } }

        private float m_height;
        public float Height { get { return m_height; } }

        private float m_depth;
        public float Depth { get { return m_depth; } }

        private bool m_isValid = false;
        public bool IsValid { get { return m_isValid; } }
    }

    public class PoseFace : ICloneable
    {
        [DllImport(Glob.DLLName)]
        private static extern int wrPoseFace_GetId(IntPtr nativeHandle);
        [DllImport(Glob.DLLName)]
        private static extern uint wrPoseFace_GetNumLandmarks(IntPtr nativeHandle);
        [DllImport(Glob.DLLName)]
        private static extern IntPtr wrPoseFace_GetLandmarks(IntPtr nativeHandle);
        [DllImport(Glob.DLLName)]
        private static extern IntPtr wrPoseFace_GetFaceArrow(IntPtr nativeHandle);
        [DllImport(Glob.DLLName)]
        private static extern IntPtr wrPoseFace_GetBoundingBox(IntPtr nativeHandle);
        [DllImport(Glob.DLLName)]
        private static extern float wrArrow_GetTipX(IntPtr nativeHandle);
        [DllImport(Glob.DLLName)]
        private static extern float wrArrow_GetTipY(IntPtr nativeHandle);
        [DllImport(Glob.DLLName)]
        private static extern float wrArrow_GetBaseX(IntPtr nativeHandle);
        [DllImport(Glob.DLLName)]
        private static extern float wrArrow_GetBaseY(IntPtr nativeHandle);

        public PoseFace() { m_isValid = false; }

        public object Clone()
        {
            PoseFace newPoseface = MemberwiseClone() as PoseFace;
            newPoseface.m_landmarks = m_landmarks == null ? null : m_landmarks.Clone() as float[];
            newPoseface.m_bbox = m_bbox == null ? null : m_bbox.Clone() as Box2d;
            return newPoseface;
        }

        public void Update(IntPtr nativeHandle)
        {
            if (nativeHandle == IntPtr.Zero)
            {
                Debug.LogError(" Trying to update a PoseFace object from nullptr");
                return;
            }

            m_id = wrPoseFace_GetId(nativeHandle);

            IntPtr arrowHandle = wrPoseFace_GetFaceArrow(nativeHandle);
            if (arrowHandle != IntPtr.Zero)
            {
                m_arrowBase = new Vector2(wrArrow_GetBaseX(arrowHandle), wrArrow_GetBaseY(arrowHandle));
                m_arrowTip = new Vector2(wrArrow_GetTipX(arrowHandle), wrArrow_GetTipY(arrowHandle));
            }

            m_numJoints = wrPoseFace_GetNumLandmarks(nativeHandle);
            IntPtr landmarksHandle = wrPoseFace_GetLandmarks(nativeHandle);
            if (m_landmarks == null || m_landmarks.Length != m_numJoints * 2)
            {
                m_landmarks = new float[m_numJoints * 2];
            }
            Marshal.Copy(landmarksHandle, m_landmarks, 0, m_landmarks.Length);

            IntPtr bboxHandle = wrPoseFace_GetBoundingBox(nativeHandle);
            if (m_bbox == null)
            {
                m_bbox = new Box2d();
            }
            m_bbox.Update(bboxHandle);

            m_isValid = true;
        }

        private int m_id = -1;
        public int Id
        {
            get { return m_id; }
            set
            {
                m_id = value;
            }
        }

        private Vector2 m_arrowBase;
        public Vector2 ArrowBase
        {
            get { return m_arrowBase; }
            set { m_arrowBase = value; }
        }

        private Vector2 m_arrowTip;
        public Vector2 ArrowTip
        {
            get { return m_arrowTip; }
            set { m_arrowBase = value; }
        }

        private uint m_numJoints;
        public uint NumJoints
        {
            get { return m_numJoints; }
        }

        private float[] m_landmarks;
        public float[] Landmarks
        {
            get { return m_landmarks; }
            set { m_landmarks = value; }
        }

        private Box2d m_bbox;
        public Box2d Bbox
        {
            get { return m_bbox; }
            set
            {
                m_bbox = value;
            }
        }

        private bool m_isValid = false;
        public bool IsValid { get { return m_isValid; } }
    }

    /// <summary>
    ///  Class representing a 2d Pose as defined in wrAPI
    /// </summary>
    public class Pose2D : ICloneable
    {
        [DllImport(Glob.DLLName)]
        private static extern int wrPose2d_GetId(IntPtr nativeHandle);
        [DllImport(Glob.DLLName)]
        private static extern int wrPose2d_GetIsMain(IntPtr nativeHandle);
        [DllImport(Glob.DLLName)]
        private static extern IntPtr wrPose2d_GetJoints(IntPtr nativeHandle);
        [DllImport(Glob.DLLName)]
        private static extern IntPtr wrPose2d_GetScores(IntPtr nativeHandle);
        [DllImport(Glob.DLLName)]
        private static extern IntPtr wrPose2d_GetBoundingBox(IntPtr nativeHandle);

        public object Clone()
        {
            Pose2D newPose2d = MemberwiseClone() as Pose2D;
            newPose2d.m_bbox = m_bbox == null ? null : m_bbox.Clone() as Box2d;
            newPose2d.m_joints = m_joints == null ? null : m_joints.Clone() as float[];
            newPose2d.m_scores = m_scores == null ? null : m_scores.Clone() as float[];

            return newPose2d;
        }

        /// <summary>
        ///  Fill the class from a native pointer returned by Pose Estimator.
        /// </summary>
        /// <param name="nativeHandle">The native pointer</param>
        public void Update(IntPtr nativeHandle, JointDefinition jointDefinition)
        {
            if (nativeHandle == IntPtr.Zero)
            {
                Debug.LogError("Trying to update a Pose2d object from nullptr");
                return;
            }

            m_id = wrPose2d_GetId(nativeHandle);
            m_isMain = wrPose2d_GetIsMain(nativeHandle) != 0;

            m_numJoints = jointDefinition.NumJoints;
            IntPtr nativePtr = wrPose2d_GetJoints(nativeHandle);
            if (m_joints == null || m_joints.Length != m_numJoints * 2)
            {
                m_joints = new float[m_numJoints * 2];
            }
            Marshal.Copy(nativePtr, m_joints, 0, m_joints.Length);

            nativePtr = wrPose2d_GetScores(nativeHandle);
            if (m_scores == null || m_scores.Length != m_numJoints)
            {
                m_scores = new float[m_numJoints];
            }
            Marshal.Copy(nativePtr, m_scores, 0, m_scores.Length);

            if (m_bbox == null)
            {
                m_bbox = new Box2d();
            }
            m_bbox.Update(wrPose2d_GetBoundingBox(nativeHandle));

            m_isValid = true;
        }

        private int m_id = -1;
        public int Id
        {
            get { return m_id; }
            set
            {
                m_id = value;
            }
        }

        private bool m_isMain;
        public bool IsMain
        {
            get { return m_isMain; }
            set
            {
                m_isMain = value;
            }
        }

        private float[] m_joints;
        public float[] Joints
        {
            get { return m_joints; }
            set
            {
                m_joints = value;
            }
        }

        private float[] m_scores;
        public float[] Scores
        {
            get { return m_scores; }
            set
            {
                m_scores = value;
            }
        }

        private int m_numJoints;
        public int NumJoints
        {
            get { return m_numJoints; }
            set
            {
                m_numJoints = value;
            }
        }

        private Box2d m_bbox;
        public Box2d Bbox
        {
            get { return m_bbox; }
            set
            {
                m_bbox = value;
            }
        }

        private bool m_isValid = false;
        public bool IsValid { get { return m_isValid; } }
    }


    /// <summary>
    ///  Class representing a 3d pose as defined in wrAPI
    /// </summary>
    public class Pose3D
    {
        [DllImport(Glob.DLLName)]
        private static extern int wrPose3d_GetId(IntPtr nativeHandle);
        [DllImport(Glob.DLLName)]
        private static extern void wrPose3d_SetId(IntPtr nativeHandle, int id);
        [DllImport(Glob.DLLName)]
        private static extern IntPtr wrPose3d_GetPositions(IntPtr nativeHandle);
        [DllImport(Glob.DLLName)]
        private static extern IntPtr wrPose3d_GetPositionsMutable(IntPtr nativeHandle);
        [DllImport(Glob.DLLName)]
        private static extern IntPtr wrPose3d_GetRotations(IntPtr nativeHandle);
        [DllImport(Glob.DLLName)]
        private static extern IntPtr wrPose3d_GetRotationsMutable(IntPtr nativeHandle);
        [DllImport(Glob.DLLName)]
        private static extern uint wrPose3d_GetScaleHintLength();
        [DllImport(Glob.DLLName)]
        private static extern IntPtr wrPose3d_GetScaleHint(IntPtr nativeHandle);

        public Pose3D() { m_isValid = false; }

        public object Clone()
        {
            Pose3D newPose3D = this.MemberwiseClone() as Pose3D;
            newPose3D.m_positions = this.m_positions == null ? null : this.m_positions.Clone() as float[];
            newPose3D.m_rotations = this.m_rotations == null ? null : this.m_rotations.Clone() as float[];
            newPose3D.m_scaleHint = this.m_scaleHint == null ? null : this.m_scaleHint.Clone() as float[];

            return newPose3D;
        }

        /// <summary>
        ///  Fill a Pose3D class from a native pointer returned by a PoseEstimator. 
        ///  This class can hold a raw Pose3D retrieved with <see cref="PoseEstimator.GetAllRawHumans3D(ref Pose3D[])"/> 
        ///  or a IK'd Pose3D <see cref="PoseEstimator.GetAllHumans3D(ref Pose3D[])"/>
        /// </summary>
        /// <param name="nativeHandle"></param>
        /// <param name="jointDefinition"></param>
        public void Update(IntPtr nativeHandle, JointDefinition jointDefinition)
        {
            if (nativeHandle == IntPtr.Zero)
            {
                Debug.LogError("Trying to update a Pose3D object from nullptr");
                return;
            }

            m_id = wrPose3d_GetId(nativeHandle);
            m_numJoints = jointDefinition.NumJoints;

            IntPtr nativePtr = wrPose3d_GetPositionsMutable(nativeHandle);
            if (m_positions == null || m_positions.Length != m_numJoints * 3) //Positions (x, y, z)
            {
                m_positions = new float[m_numJoints * 3];
            }
            Marshal.Copy(nativePtr, m_positions, 0, m_positions.Length);

            nativePtr = wrPose3d_GetRotationsMutable(nativeHandle);
            if (m_rotations == null || m_rotations.Length != m_numJoints * 4)   //Quaternions (x, y, z, w)
            {
                m_rotations = new float[m_numJoints * 4];
            }
            Marshal.Copy(nativePtr, m_rotations, 0, m_rotations.Length);

            nativePtr = wrPose3d_GetScaleHint(nativeHandle);
            if (m_scaleHint == null || m_scaleHint.Length != wrPose3d_GetScaleHintLength())
            {
                m_scaleHint = new float[wrPose3d_GetScaleHintLength()];
            }
            Marshal.Copy(nativePtr, m_scaleHint, 0, m_scaleHint.Length);

            m_isValid = true;
        }

        private int m_id = -1;
        public int Id
        {
            get { return m_id; }
            set { m_id = value; }
        }

        private int m_numJoints;
        public int NumJoints { get { return m_numJoints; } }

        private float[] m_positions;
        public float[] Positions
        {
            get { return m_positions; }
            set
            {
                m_positions = value;
            }
        }

        private float[] m_rotations;
        public float[] Rotations
        {
            get { return m_rotations; }
            set
            {
                m_rotations = value;
            }
        }

        private float[] m_scaleHint;
        public float[] ScaleHint
        {
            get { return m_scaleHint; }
            set
            {
                m_scaleHint = value;
            }
        }

        private bool m_isValid = false;
        public bool IsValid { get { return m_isValid; } }
    }


    /// <summary>
    ///  Class representing a head pose as defined in wrAPI
    /// </summary>
    public class PoseHead : ICloneable
    {
        [DllImport(Glob.DLLName)]
        private static extern int wrPoseHead_GetId(IntPtr nativeHandle);
        [DllImport(Glob.DLLName)]
        private static extern IntPtr wrPoseHead_GetHeadRotationEuler(IntPtr nativeHandle);
        [DllImport(Glob.DLLName)]
        private static extern IntPtr wrPoseHead_GetHeadRotationQuat(IntPtr nativeHandle);
        [DllImport(Glob.DLLName)]
        private static extern IntPtr wrPoseHead_GetFaceBoundingBox(IntPtr nativeHandle);
        [DllImport(Glob.DLLName)]
        private static extern int wrPoseHead_GetEstimationSuccess(IntPtr nativeHandle);
        [DllImport(Glob.DLLName)]
        private static extern uint wrPoseHead_GetHeadRotationEulerLength();
        [DllImport(Glob.DLLName)]
        private static extern uint wrPoseHead_GetHeadRotationQuatLength();
        public PoseHead() { m_isValid = false; }

        public object Clone()
        {
            PoseHead newPoseHead = this.MemberwiseClone() as PoseHead;
            newPoseHead.m_bbox = this.m_bbox.Clone() as Box3d;
             return newPoseHead;
        }

        /// <summary>
        ///  Fill the class from a native pointer returned by Pose Estimator.
        /// </summary>
        /// <param name="nativeHandle">The native pointer</param>
        public void Update(IntPtr nativeHandle)
        {
            if (nativeHandle == IntPtr.Zero)
            {
                Debug.LogError("Trying to update a PoseHead object from nullptr");
            }

            m_id = wrPoseHead_GetId(nativeHandle);

            var values = new float[wrPoseHead_GetHeadRotationEulerLength()];
            IntPtr eulerPtr = wrPoseHead_GetHeadRotationEuler(nativeHandle);
            Marshal.Copy(eulerPtr, values, 0, values.Length);
            m_euler = new Vector3(values[0], values[1], values[2]);

            values = new float[wrPoseHead_GetHeadRotationQuatLength()];
            IntPtr quatPtr = wrPoseHead_GetHeadRotationQuat(nativeHandle);
            Marshal.Copy(quatPtr, values, 0, values.Length);
            m_quat = new Quaternion(values[0], values[1], values[2], values[3]);

            if (m_bbox == null)
            {
                m_bbox = new Box3d();
            }
            else
            {
                m_bbox.Update(wrPoseHead_GetFaceBoundingBox(nativeHandle));
            }
            m_success = wrPoseHead_GetEstimationSuccess(nativeHandle) != 0;

            m_isValid = true;
        }

        private int m_id = -1;
        public int Id { get { return m_id; } }

        private Vector3 m_euler;
        public Vector3 Euler { get { return m_euler; } }

        private Quaternion m_quat;
        private Quaternion Quat { get { return m_quat; } }

        private Box3d m_bbox;
        public Box3d Bbox { get { return m_bbox; } }

        private bool m_success;
        public bool Success { get { return m_success; } }

        private bool m_isValid = false;
        public bool IsValid { get { return m_isValid; } }
    }
}