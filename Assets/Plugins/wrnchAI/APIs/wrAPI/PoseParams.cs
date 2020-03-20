/*
 Copyright (c) 2019 Wrnch Inc.
 All rights reserved
*/

using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace wrnchAI.wrAPI
{
    public enum Sensivity
    {
        LOW,
        MEDIUM,
        HIGH
    };

    public enum TrackerKind
    {
        V1,
        V2,
        NONE
    }

    public enum IdState
    {
        Mature,
        Immature,
        Untracked
    }

    class PoseParams
    {
        private IntPtr m_nativeHandle;
        public IntPtr NativeHandle { get { return m_nativeHandle; } }

        [DllImport(Glob.DLLName)]
        private static extern IntPtr wrPoseParams_Create();
        [DllImport(Glob.DLLName)]
        private static extern void wrPoseParams_Destroy(IntPtr nativeHandle);
        [DllImport(Glob.DLLName)]
        private static extern void wrPoseParams_SetBoneSensitivity(IntPtr nativeHandle, Sensivity s);
        [DllImport(Glob.DLLName)]
        private static extern void wrPoseParams_SetJointSensitivity(IntPtr nativeHandle, Sensivity s);
        [DllImport(Glob.DLLName)]
        private static extern void wrPoseParams_SetPreferredNetWidth2d(IntPtr nativeHandle, int width);
        [DllImport(Glob.DLLName)]
        private static extern void wrPoseParams_SetPreferredNetHeight2d(IntPtr nativeHandle, int height);
        [DllImport(Glob.DLLName)]
        private static extern void wrPoseParams_SetPreferredNetWidth3d(IntPtr nativeHandle, int width);
        [DllImport(Glob.DLLName)]
        private static extern void wrPoseParams_SetPreferredNetHeight3d(IntPtr nativeHandle, int height);
        [DllImport(Glob.DLLName)]
        private static extern void wrPoseParams_SetSmoothingBetaX(IntPtr nativeHandle, float betaX);
        [DllImport(Glob.DLLName)]
        private static extern void wrPoseParams_SetSmoothingBetaY(IntPtr nativeHandle, float betaY);
        [DllImport(Glob.DLLName)]
        private static extern void wrPoseParams_SetSmoothingBetaZ(IntPtr nativeHandle, float betaZ);
        [DllImport(Glob.DLLName)]
        private static extern void wrPoseParams_SetMinValidJoints(IntPtr nativeHandle, int minJoints);
        [DllImport(Glob.DLLName)]
        private static extern void wrPoseParams_SetTrackerKind(IntPtr nativeHandle, int trackerKind);
        [DllImport(Glob.DLLName)]
        private static extern Sensivity wrPoseParams_GetBoneSensitivity(IntPtr nativeHandle);
        [DllImport(Glob.DLLName)]
        private static extern Sensivity wrPoseParams_GetJointSensitivity(IntPtr nativeHandle);
        [DllImport(Glob.DLLName)]
        private static extern int wrPoseParams_GetPreferredNetWidth2d(IntPtr nativeHandle);
        [DllImport(Glob.DLLName)]
        private static extern int wrPoseParams_GetPreferredNetHeight2d(IntPtr nativeHandle);
        [DllImport(Glob.DLLName)]
        private static extern float wrPoseParams_GetSmoothingBetaX(IntPtr nativeHandle);
        [DllImport(Glob.DLLName)]
        private static extern float wrPoseParams_GetSmoothingBetaY(IntPtr nativeHandle);
        [DllImport(Glob.DLLName)]
        private static extern float wrPoseParams_GetSmoothingBetaZ(IntPtr nativeHandle);
        [DllImport(Glob.DLLName)]
        private static extern int wrPoseParams_GetMinValidJoints(IntPtr nativeHandle);
        [DllImport(Glob.DLLName)]
        private static extern int wrPoseParams_GetTrackerKind(IntPtr nativeHandle);

        public PoseParams()
        {
            m_nativeHandle = wrPoseParams_Create();
        }

        ~PoseParams()
        {
            if (m_nativeHandle != IntPtr.Zero)
            {
                wrPoseParams_Destroy(m_nativeHandle);
            }
        }

        /// <summary>
        /// Bone sensitivity can be lowered to reduce false positives in 2D skeleton bone clustering (when not using estimateSingle).
        /// This is an advanced parameter and it is not recommended to change it. High (default) generally gives best detection results.
        /// </summary>
        /// <param name="sensitivity"></param>
        public void SetBoneSensitivity(Sensivity sensitivity)
        {
            if (m_nativeHandle == IntPtr.Zero)
            {
                Debug.LogError("Trying to access native object from nullptr");
                return;
            }
            wrPoseParams_SetBoneSensitivity(m_nativeHandle, sensitivity);
        }

        /// <summary>
        /// Joint sensitivity can be lowered to suppress noisy joint detection.
        /// This is an advanced parameter and it is not recommended to change it. High (default) generally gives best detection results.
        /// </summary>
        /// <param name="sensitivity"></param>
        public void SetJointSensitivity(Sensivity sensitivity)
        {
            if (m_nativeHandle == IntPtr.Zero)
            {
                Debug.LogError("Trying to access native object from nullptr");
                return;
            }
            wrPoseParams_SetJointSensitivity(m_nativeHandle, sensitivity);
        }

        /// <summary>
        /// Set a required width for the neural network running the pose estimation.
        /// This parameter requires resetting the pose estimator in order to have an effect.
        /// Note: For iOS builds, this function has no effect on a resulting estimator --
        ///       the net dimensions are fixed on disk.
        /// </summary>
        /// <param name="width"></param>
        public void SetPreferredNetWidth2d(int width)
        {
            if (m_nativeHandle == IntPtr.Zero)
            {
                Debug.LogError("Trying to access native object from nullptr");
                return;
            }
            wrPoseParams_SetPreferredNetWidth2d(m_nativeHandle, width);
        }

        /// <summary>
        /// Set a required height for the neural network running the pose estimation.
        /// This parameter requires resetting the pose estimator in order to have an effect.
        /// Note: For iOS builds, this function has no effect on a resulting estimator --
        ///       the net dimensions are fixed on disk.
        /// </summary>
        /// <param name="width"></param>
        public void SetPreferredNetHeight2d(int height)
        {
            if (m_nativeHandle == IntPtr.Zero)
            {
                Debug.LogError("Trying to access native object from nullptr");
                return;
            }
            wrPoseParams_SetPreferredNetHeight2d(m_nativeHandle, height);
        }

        public void SetSmoothingBetaX(float betaX)
        {
            if (m_nativeHandle == IntPtr.Zero)
            {
                Debug.LogError("Trying to access native object from nullptr");
                return;
            }
            wrPoseParams_SetSmoothingBetaX(m_nativeHandle, betaX);
        }

        public void SetSmoothingBetaY(float betaY)
        {
            if (m_nativeHandle == IntPtr.Zero)
            {
                Debug.LogError("Trying to access native object from nullptr");
                return;
            }
            wrPoseParams_SetSmoothingBetaY(m_nativeHandle, betaY);
        }

        public void SetSmoothingBetaZ(float betaZ)
        {
            if (m_nativeHandle == IntPtr.Zero)
            {
                Debug.LogError("Trying to access native object from nullptr");
                return;
            }
            wrPoseParams_SetSmoothingBetaZ(m_nativeHandle, betaZ);
        }

        public void SetMinValidJoints(int minJoints)
        {
            if (m_nativeHandle == IntPtr.Zero)
            {
                Debug.LogError("Trying to access native object from nullptr");
                return;
            }
            wrPoseParams_SetMinValidJoints(m_nativeHandle, minJoints);
        }

        public void SetTrackerKind(TrackerKind kind)
        {
            if (m_nativeHandle == IntPtr.Zero)
            {
                Debug.LogError("Trying to access native object from nullptr");
                return;
            }
            wrPoseParams_SetTrackerKind(m_nativeHandle, (int)kind);
        }

        public Sensivity GetBoneSensitivity()
        {
            if (m_nativeHandle == IntPtr.Zero)
            {
                Debug.LogError("Trying to access native object from nullptr");
                return 0;
            }
            return wrPoseParams_GetBoneSensitivity(m_nativeHandle);
        }

        public int GetPreferredNetWidth2d()
        {
            if (m_nativeHandle == IntPtr.Zero)
            {
                Debug.LogError("Trying to access native object from nullptr");
                return -1;
            }
            return wrPoseParams_GetPreferredNetWidth2d(m_nativeHandle);
        }

        public int GetPreferredNetHeight2d()
        {
            if (m_nativeHandle == IntPtr.Zero)
            {
                Debug.LogError("Trying to access native object from nullptr");
                return -1;
            }
            return wrPoseParams_GetPreferredNetHeight2d(m_nativeHandle);
        }

        public float GetSmoothingBetaX()
        {
            if (m_nativeHandle == IntPtr.Zero)
            {
                Debug.LogError("Trying to access native object from nullptr");
                return -1;
            }
            return wrPoseParams_GetSmoothingBetaX(m_nativeHandle);
        }

        public float GetSmoothingBetaY()
        {
            if (m_nativeHandle == IntPtr.Zero)
            {
                Debug.LogError("Trying to access native object from nullptr");
                return -1;
            }
            return wrPoseParams_GetSmoothingBetaY(m_nativeHandle);
        }

        public float GetSmoothingBetaZ()
        {
            if (m_nativeHandle == IntPtr.Zero)
            {
                Debug.LogError("Trying to access native object from nullptr");
                return -1;
            }
            return wrPoseParams_GetSmoothingBetaZ(m_nativeHandle);
        }

        public int GetMinValidJoints()
        {
            if (m_nativeHandle == IntPtr.Zero)
            {
                Debug.LogError("Trying to access native object from nullptr");
                return -1;
            }
            return wrPoseParams_GetMinValidJoints(m_nativeHandle);
        }

        public TrackerKind GetTrackerKind()
        {
            if (m_nativeHandle == IntPtr.Zero)
            {
                Debug.LogError("Trying to access native object from nullptr");
                return TrackerKind.NONE;
            }
            return (TrackerKind)wrPoseParams_GetTrackerKind(m_nativeHandle);
        }
    }

    /// <summary>
    /// Class exposing a set of internal params driving the IK solving
    /// </summary>
    public class IKParams
    {
        private IntPtr m_nativeHandle;
        public IntPtr NativeHandle { get { return m_nativeHandle; } }

        [DllImport(Glob.DLLName)]
        private static extern IntPtr wrIKParams_Create();
        [DllImport(Glob.DLLName)]
        private static extern void wrIKParams_Destroy(IntPtr nativeHandle);
        [DllImport(Glob.DLLName)]
        private static extern void wrIKParams_SetTransReach(IntPtr nativeHandle, float transReach);
        [DllImport(Glob.DLLName)]
        private static extern void wrIKParams_SetRotReach(IntPtr nativeHandle, float rotReach);
        [DllImport(Glob.DLLName)]
        private static extern void wrIKParams_SetPull(IntPtr nativeHandle, float pull);
        [DllImport(Glob.DLLName)]
        private static extern void wrIKParams_SetResist(IntPtr nativeHandle, float resist);
        [DllImport(Glob.DLLName)]
        private static extern void wrIKParams_SetMaxAngularVelocity(IntPtr nativeHandle, float maxAngularVelocity);
        [DllImport(Glob.DLLName)]
        private static extern void wrIKParams_SetFPS(IntPtr nativeHandle, float fps);
        [DllImport(Glob.DLLName)]
        private static extern void wrIKParams_SetJointVisibilityThresh(IntPtr nativeHandle, float thresh);
        [DllImport(Glob.DLLName)]
        private static extern float wrIKParams_GetTransReach(IntPtr nativeHandle);
        [DllImport(Glob.DLLName)]
        private static extern float wrIKParams_GetRotReach(IntPtr nativeHandle);
        [DllImport(Glob.DLLName)]
        private static extern float wrIKParams_GetPull(IntPtr nativeHandle);
        [DllImport(Glob.DLLName)]
        private static extern float wrIKParams_GetResist(IntPtr nativeHandle);
        [DllImport(Glob.DLLName)]
        private static extern float wrIKParams_GetMaxAngularVelocity(IntPtr nativeHandle);
        [DllImport(Glob.DLLName)]
        private static extern float wrIKParams_GetFPS(IntPtr nativeHandle);
        [DllImport(Glob.DLLName)]
        private static extern float wrIKParams_GetJointVisibilityThresh(IntPtr nativeHandle);

        public IKParams()
        {
            m_nativeHandle = wrIKParams_Create();
        }

        ~IKParams()
        {
            if (m_nativeHandle != IntPtr.Zero)
            {
                wrIKParams_Destroy(m_nativeHandle);
            }
        }

        public IKParams SetTransReach(float transReach)
        {
            if (m_nativeHandle == IntPtr.Zero)
            {
                Debug.LogError("Trying to access native object from nullptr");
                return null;
            }
            wrIKParams_SetTransReach(m_nativeHandle, transReach);
            return this;
        }

        public IKParams SetRotReach(float rotReach)
        {
            if (m_nativeHandle == IntPtr.Zero)
            {
                Debug.LogError("Trying to access native object from nullptr");
                return null;
            }
            wrIKParams_SetRotReach(m_nativeHandle, rotReach);
            return this;
        }

        public IKParams SetPull(float pull)
        {
            if (m_nativeHandle == IntPtr.Zero)
            {
                Debug.LogError("Trying to access native object from nullptr");
                return null;
            }
            wrIKParams_SetRotReach(m_nativeHandle, pull);
            return this;
        }

        public IKParams SetResist(float resist)
        {
            if (m_nativeHandle == IntPtr.Zero)
            {
                Debug.LogError("Trying to access native object from nullptr");
                return null;
            }
            wrIKParams_SetRotReach(m_nativeHandle, resist);
            return this;
        }

        public IKParams SetMaxAngularVelocity(float maxAngularVelocity)
        {
            if (m_nativeHandle == IntPtr.Zero)
            {
                Debug.LogError("Trying to access native object from nullptr");
                return null;
            }
            wrIKParams_SetMaxAngularVelocity(m_nativeHandle, maxAngularVelocity);
            return this;
        }

        public IKParams SetFPS(float fps)
        {
            if (m_nativeHandle == IntPtr.Zero)
            {
                Debug.LogError("Trying to access native object from nullptr");
                return null;
            }
            wrIKParams_SetFPS(m_nativeHandle, fps);
            return this;
        }

        public IKParams SetJointVisibilityThresh(float thresh)
        {
            if (m_nativeHandle == IntPtr.Zero)
            {
                Debug.LogError("Trying to access native object from nullptr");
                return null;
            }
            wrIKParams_SetJointVisibilityThresh(m_nativeHandle, thresh);
            return this;
        }

        public float GetTransReach()
        {
            if (m_nativeHandle == IntPtr.Zero)
            {
                Debug.LogError("Trying to access native object from nullptr");
                return 0.0F;
            }
            return wrIKParams_GetTransReach(m_nativeHandle);
        }

        public float GetRotReach()
        {
            if (m_nativeHandle == IntPtr.Zero)
            {
                Debug.LogError("Trying to access native object from nullptr");
                return 0.0F;
            }
            return wrIKParams_GetRotReach(m_nativeHandle);
        }

        public float GetPull()
        {
            if (m_nativeHandle == IntPtr.Zero)
            {
                Debug.LogError("Trying to access native object from nullptr");
                return 0.0F;
            }
            return wrIKParams_GetPull(m_nativeHandle);
        }

        public float GetResist()
        {
            if (m_nativeHandle == IntPtr.Zero)
            {
                Debug.LogError("Trying to access native object from nullptr");
                return 0.0F;
            }
            return wrIKParams_GetResist(m_nativeHandle);
        }

        public float GetMaxAngularVelocity()
        {
            if (m_nativeHandle == IntPtr.Zero)
            {
                Debug.LogError("Trying to access native object from nullptr");
                return 0.0F;
            }
            return wrIKParams_GetMaxAngularVelocity(m_nativeHandle);
        }

        public float GetFPS()
        {
            if (m_nativeHandle == IntPtr.Zero)
            {
                Debug.LogError("Trying to access native object from nullptr");
                return 0.0F;
            }
            return wrIKParams_GetFPS(m_nativeHandle);
        }

        public float GetJointVisibilityThresh()
        {
            if (m_nativeHandle == IntPtr.Zero)
            {
                Debug.LogError("Trying to access native object from nullptr");
                return 0.0F;
            }
            return wrIKParams_GetJointVisibilityThresh(m_nativeHandle);
        }
    }
}
