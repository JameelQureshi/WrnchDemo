/*
 Copyright (c) 2019 Wrnch Inc.
 All rights reserved
*/

using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace wrnchAI.wrAPI
{
    class PoseIK
    {
        private IntPtr m_nativeHandle = IntPtr.Zero;
        public IntPtr nativeHandle { get { return m_nativeHandle; } }

        private JointDefinition m_inputFormat;
        public JointDefinition InputFormat { get { return m_inputFormat; } }

        private JointDefinition m_outputFormat = new JointDefinition("extended");
        public JointDefinition outputFormat { get { return m_outputFormat; } }

        [DllImport(Glob.DLLName)]
        private static extern int wrPoseIK_CreateDefault(ref IntPtr handle, IntPtr inputFormat);
        [DllImport(Glob.DLLName)]
        private static extern int wrPoseIK_Create(ref IntPtr handle, IntPtr inputFormat, IntPtr initialPose, uint numJoints);
        //This shouldn't be used from here, Pose3D is not holding it's native counterpart because of risks of data corruption and performance issues.
        //[DllImport(Glob.DLLName)]
        //private static extern PoseEstimator.ReturnCode wrPoseIK_CreateFromPose(ref IntPtr handle, IntPtr inputFormat, IntPtr initialPose);
        [DllImport(Glob.DLLName)]
        private static extern void wrPoseIK_ResetDefault(IntPtr handle);
        [DllImport(Glob.DLLName)]
        private static extern void wrPoseIK_Reset(IntPtr handle, IntPtr initialPose, int numJoints);
        [DllImport(Glob.DLLName)]
        private static extern void wrPoseIK_ResetFromPose(IntPtr handle, IntPtr initialPose);
        [DllImport(Glob.DLLName)]
        private static extern void wrPoseIK_Destroy(IntPtr handle);
        [DllImport(Glob.DLLName)]
        private static extern IntPtr wrPoseIK_Solve(IntPtr handle, IntPtr pose, IntPtr visibilities, IntPtr ikParams);
        [DllImport(Glob.DLLName)]
        private static extern IntPtr wrPoseIK_GetOutputFormat(IntPtr handle);
        [DllImport(Glob.DLLName)]
        private static extern IntPtr wrPoseIK_GetInputFormat(IntPtr handle);
        [DllImport(Glob.DLLName)]
        private static extern void wrPoseIK_SetInputFormat(IntPtr handle, IntPtr inputFormat);
        [DllImport(Glob.DLLName)]
        private static extern float wrPoseIK_GetIKProperty(IntPtr handle, int property);
        [DllImport(Glob.DLLName)]
        private static extern void wrPoseIK_SetIKProperty(IntPtr handle, int property, float value);
        [DllImport(Glob.DLLName)]
        private static extern IntPtr wrPoseIK_GetTPose(IntPtr handle);

        public PoseIK(JointDefinition inputFormat)
        {
            if (inputFormat.NativeHandle == IntPtr.Zero)
            {
                throw new NullReferenceException("Trying to build a PoseIK Object with null joint definition");
            }

            wrPoseIK_CreateDefault(ref m_nativeHandle, inputFormat.NativeHandle);
            m_inputFormat = inputFormat;
        }

        public PoseIK(JointDefinition inputFormat, float[] initialPose)
        {
            if (initialPose.Length != 90)
            {
                throw new InvalidOperationException("initialPose should contain 30 3d joints");
            }
            if (inputFormat.NativeHandle == IntPtr.Zero)
            {
                throw new NullReferenceException("Trying to build a PoseIK Object with null joint definition");
            }

            GCHandle initialPoseHandle = GCHandle.Alloc(initialPose, GCHandleType.Pinned);
            wrPoseIK_Create(ref m_nativeHandle, inputFormat.NativeHandle, initialPoseHandle.AddrOfPinnedObject(), 30);
            initialPoseHandle.Free();
        }

        public Pose3D Solve(float[] pose, int[] visibilities, IKParams ikParams)
        {
            GCHandle pinnedPose = GCHandle.Alloc(pose, GCHandleType.Pinned);
            GCHandle pinnedVisibilities = GCHandle.Alloc(visibilities, GCHandleType.Pinned);

            var result = wrPoseIK_Solve(m_nativeHandle, pinnedPose.AddrOfPinnedObject(), pinnedVisibilities.AddrOfPinnedObject(), ikParams.NativeHandle);

            pinnedVisibilities.Free();
            pinnedPose.Free();

            var output = new Pose3D();
            output.Update(result, m_outputFormat);
            return output;
        }

        public void Reset()
        {
            if (m_nativeHandle == IntPtr.Zero)
            {
                Debug.LogError("PoseIK::Reset called on a null native object.");
                return;
            }

            wrPoseIK_ResetDefault(m_nativeHandle);
        }

        public void Reset(float[] initialPose)
        {
            if (m_nativeHandle == IntPtr.Zero)
            {
                Debug.LogError("PoseIK::Reset called on a null native object.");
                return;
            }
            if (initialPose.Length != 90)
            {
                throw new InvalidOperationException("initialPose should contain 30 3d joints");
            }

            GCHandle initialPoseHandle = GCHandle.Alloc(initialPose, GCHandleType.Pinned);
            wrPoseIK_Reset(m_nativeHandle, initialPoseHandle.AddrOfPinnedObject(), wrExtended.GetNumJoints());
            initialPoseHandle.Free();
        }

        public float GetIKProperty(int property)
        {
            return wrPoseIK_GetIKProperty(m_nativeHandle, property);
        }

        public void SetIKProperty(int property, float value)
        {
            wrPoseIK_SetIKProperty(m_nativeHandle, property, value);
        }

        JointDefinition GetInputFormat()
        {
            return new JointDefinition(wrPoseIK_GetInputFormat(m_nativeHandle));
        }

        JointDefinition GetOutputFormat()
        {
            return new JointDefinition(wrPoseIK_GetOutputFormat(m_nativeHandle));
        }

        Pose3D GetTPose()
        {
            var result = wrPoseIK_GetTPose(m_nativeHandle);

            var output = new Pose3D();
            output.Update(result, m_outputFormat);

            return output;
        }

    }
}