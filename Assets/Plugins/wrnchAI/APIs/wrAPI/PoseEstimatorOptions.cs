/*
 Copyright (c) 2019 Wrnch Inc.
 All rights reserved
*/

using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace wrnchAI.wrAPI
{
    public enum MainPersonId
    {
        NONE = -1,
        LARGEST = -2,
        ALL = -3,
        CENTER = -4
    }

    public class PoseEstimatorOptions
    {
        private IntPtr m_nativeHandle;
        public IntPtr NativeHandle { get { return m_nativeHandle; } }

        [DllImport(Glob.DLLName)]
        private static extern IntPtr wrPoseEstimatorOptions_Create();
        [DllImport(Glob.DLLName)]
        private static extern void wrPoseEstimatorOptions_Destroy(IntPtr nativeHandle);
        [DllImport(Glob.DLLName)]
        private static extern void wrPoseEstimatorOptions_SetEnableJointSmoothing(IntPtr nativeHandle, int yesNo);
        [DllImport(Glob.DLLName)]
        private static extern void wrPoseEstimatorOptions_SetEnableHeadSmoothing(IntPtr nativeHandle, int yesNo);
        [DllImport(Glob.DLLName)]
        private static extern void wrPoseEstimatorOptions_SetEstimateAllHandBoxes(IntPtr nativeHandle, int yesNo);
        [DllImport(Glob.DLLName)]
        private static extern void wrPoseEstimatorOptions_SetEstimatePoseFace(IntPtr nativeHandle, int yesNo);
        [DllImport(Glob.DLLName)]
        private static extern void wrPoseEstimatorOptions_SetEstimateSingle(IntPtr nativeHandle, int yesNo);
        [DllImport(Glob.DLLName)]
        private static extern void wrPoseEstimatorOptions_SetEstimateHands(IntPtr nativeHandle, int yesNo);
        [DllImport(Glob.DLLName)]
        private static extern void wrPoseEstimatorOptions_SetEstimateHead(IntPtr nativeHandle, int yesNo);
        [DllImport(Glob.DLLName)]
        private static extern void wrPoseEstimatorOptions_SetEstimateMask(IntPtr nativeHandle, int yesNo);
        [DllImport(Glob.DLLName)]
        private static extern void wrPoseEstimatorOptions_SetMainPersonId(IntPtr nativeHandle, MainPersonId id);
        [DllImport(Glob.DLLName)]
        private static extern void wrPoseEstimatorOptions_SetUseIK(IntPtr nativeHandle, int yesNo);
        [DllImport(Glob.DLLName)]
        private static extern void wrPoseEstimatorOptions_SetEstimate3d(IntPtr nativeHandle, int yesNo);
        [DllImport(Glob.DLLName)]
        private static extern void wrPoseEstimatorOptions_SetRotationMultipleOf90(IntPtr nativeHandle, int rotationMultipleOf90);
        [DllImport(Glob.DLLName)]
        private static extern int wrPoseEstimatorOptions_GetEnableJointSmoothing(IntPtr nativeHandle);
        [DllImport(Glob.DLLName)]
        private static extern int wrPoseEstimatorOptions_GetEnableHeadSmoothing(IntPtr nativeHandle);
        [DllImport(Glob.DLLName)]
        private static extern int wrPoseEstimatorOptions_GetEstimateAllHandBoxes(IntPtr nativeHandle);
        [DllImport(Glob.DLLName)]
        private static extern int wrPoseEstimatorOptions_GetEstimatePoseFace(IntPtr nativeHandle);
        [DllImport(Glob.DLLName)]
        private static extern int wrPoseEstimatorOptions_GetEstimateSingle(IntPtr nativeHandle);
        [DllImport(Glob.DLLName)]
        private static extern int wrPoseEstimatorOptions_GetEstimateHands(IntPtr nativeHandle);
        [DllImport(Glob.DLLName)]
        private static extern int wrPoseEstimatorOptions_GetEstimateHead(IntPtr nativeHandle);
        [DllImport(Glob.DLLName)]
        private static extern int wrPoseEstimatorOptions_GetEstimateMask(IntPtr nativeHandle);
        [DllImport(Glob.DLLName)]
        private static extern MainPersonId wrPoseEstimatorOptions_GetMainPersonId(IntPtr nativeHandle);
        [DllImport(Glob.DLLName)]
        private static extern int wrPoseEstimatorOptions_GetEstimate3D(IntPtr nativeHandle);
        [DllImport(Glob.DLLName)]
        private static extern int wrPoseEstimatorOptions_GetUseIK(IntPtr nativeHandle);
        [DllImport(Glob.DLLName)]
        private static extern int wrPoseEstimatorOptions_GetRotationMultipleOf90(IntPtr nativeHandle);

        /// <summary>
        /// Constructor with default values for pose estimator options
        /// </summary>
        public PoseEstimatorOptions()
        {
            m_nativeHandle = wrPoseEstimatorOptions_Create();
        }

        ~PoseEstimatorOptions()
        {
            if (m_nativeHandle != IntPtr.Zero)
            {
                wrPoseEstimatorOptions_Destroy(m_nativeHandle);
            }
        }

        /// <summary>
        /// Enable the internal joint smoothing. This smoothing is temporal and will induce input lag
        /// </summary>
        /// <param name="yesNo"></param>
        /// <returns></returns>
        public PoseEstimatorOptions SetEnableJointSmoothing(bool yesNo)
        {
            if (m_nativeHandle == IntPtr.Zero)
            {
                Debug.LogError("Trying to access native object from nullptr");
                return null;
            }
            wrPoseEstimatorOptions_SetEnableJointSmoothing(m_nativeHandle, yesNo ? 1 : 0);
            return this;
        }

        /// <summary>
        /// Enable the internal joint smoothing for the head. This smoothing is temporal and will induce input lag. 
        /// This feature is not yet available on iOS.
        /// </summary>
        /// <param name="yesNo"></param>
        /// <returns></returns>
        public PoseEstimatorOptions SetEnableHeadSmoothing(bool yesNo)
        {
            if (m_nativeHandle == IntPtr.Zero)
            {
                Debug.LogError("Trying to access native object from nullptr");
                return null;
            }
            wrPoseEstimatorOptions_SetEnableHeadSmoothing(m_nativeHandle, yesNo ? 1 : 0);
            return this;
        }

        /// <summary>
        /// Toggle the estimation of hand boxes.
        /// </summary>
        /// <param name="yesNo"></param>
        /// <returns></returns>
        public PoseEstimatorOptions SetEstimateAllHandBoxes(bool yesNo)
        {
            if (m_nativeHandle == IntPtr.Zero)
            {
                Debug.LogError("Trying to access native object from nullptr");
                return null;
            }
            wrPoseEstimatorOptions_SetEstimateAllHandBoxes(m_nativeHandle, yesNo ? 1 : 0);
            return this;
        }

        /// <summary>
        /// Toggle the estimation of facial features. 
        /// </summary>
        /// <param name="yesNo"></param>
        /// <returns></returns>
        public PoseEstimatorOptions SetEstimatePoseFace(bool yesNo)
        {
            if (m_nativeHandle == IntPtr.Zero)
            {
                Debug.LogError("Trying to access native object from nullptr");
                return null;
            }

#if !(UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX || UNITY_IOS)
            wrPoseEstimatorOptions_SetEstimatePoseFace(m_nativeHandle, yesNo ? 1 : 0);
#else
            Debug.LogWarning("No iOS/OSX support for head pose estimator");
#endif
            return this;
        }

        /// <summary>
        /// Toggle single pose estimation. <see cref="MainPersonId"/>
        /// </summary>
        /// <param name="yesNo"></param>
        /// <returns></returns>
        public PoseEstimatorOptions SetEstimateSingle(bool yesNo)
        {
            if (m_nativeHandle == IntPtr.Zero)
            {
                Debug.LogError("Trying to access native object from nullptr");
                return null;
            }
            wrPoseEstimatorOptions_SetEstimateSingle(m_nativeHandle, yesNo ? 1 : 0);
            return this;
        }

        /// <summary>
        /// Toggle the estimation of the finger joints.
        /// </summary>
        /// <param name="yesNo"></param>
        /// <returns></returns>
        public PoseEstimatorOptions SetEstimateHands(bool yesNo)
        {
            if (m_nativeHandle == IntPtr.Zero)
            {
                Debug.LogError("Trying to access native object from nullptr");
                return null;
            }
            wrPoseEstimatorOptions_SetEstimateHands(m_nativeHandle, yesNo ? 1 : 0);
            return this;
        }

        /// <summary>
        /// Toggle the estimation of head rotation.
        /// This feature is not yet available for iOS.
        /// </summary>
        /// <param name="yesNo"></param>
        /// <returns></returns>
        public PoseEstimatorOptions SetEstimateHead(bool yesNo)
        {
            if (m_nativeHandle == IntPtr.Zero)
            {
                Debug.LogError("Trying to access native object from nullptr");
                return null;
            }
#if !(UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX || UNITY_IOS)
            wrPoseEstimatorOptions_SetEstimateHead(m_nativeHandle, yesNo ? 1 : 0);
#else
            Debug.LogWarning("No iOS/OSX support for head pose estimator");
#endif
            return this;
        }

        /// <summary>
        /// Toggle the estimation of the mask for green screening.
        /// </summary>
        /// <param name="yesNo"></param>
        /// <returns></returns>
        public PoseEstimatorOptions SetEstimateMask(bool yesNo)
        {
            if (m_nativeHandle == IntPtr.Zero)
            {
                Debug.LogError("Trying to access native object from nullptr");
                return null;
            }
            wrPoseEstimatorOptions_SetEstimateMask(m_nativeHandle, yesNo ? 1 : 0);
            return this;
        }

        /// <summary>
        /// Define which person is considered as the main id. Use MainPersonId.ALL to enable multi 3D.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public PoseEstimatorOptions SetMainPersonId(MainPersonId id)
        {
            if (m_nativeHandle == IntPtr.Zero)
            {
                Debug.LogError("Trying to access native object from nullptr");
                return null;
            }
            wrPoseEstimatorOptions_SetMainPersonId(m_nativeHandle, id);
            return this;
        }

        /// <summary>
        /// Toggle the IK solving. This is needed to get joint rotations
        /// </summary>
        /// <param name="yesNo"></param>
        /// <returns></returns>
        public PoseEstimatorOptions SetUseIK(bool yesNo)
        {
            if (m_nativeHandle == IntPtr.Zero)
            {
                Debug.LogError("Trying to access native object from nullptr");
                return null;
            }
            wrPoseEstimatorOptions_SetUseIK(m_nativeHandle, yesNo ? 1 : 0);
            return this;
        }

        /// <summary>
        /// Toggle the 3d pose estimation.
        /// </summary>
        /// <param name="yesNo"></param>
        /// <returns></returns>
        public PoseEstimatorOptions SetEstimate3d(bool yesNo)
        {
            if (m_nativeHandle == IntPtr.Zero)
            {
                Debug.LogError("Trying to access native object from nullptr");
                return null;
            }
            wrPoseEstimatorOptions_SetEstimate3d(m_nativeHandle, yesNo ? 1 : 0);
            return this;
        }

        /// <summary>
        /// Add a clockwise rotation of 90 degrees as internal preprocessing step.
        /// This will only affect the internal representation of the frame, and not the visualization
        /// </summary>
        /// <param name="rotationMultipleOf90"></param>
        /// <returns></returns>
        public PoseEstimatorOptions SetRotationMultipleOf90(int rotationMultipleOf90)
        {
            if (m_nativeHandle == IntPtr.Zero)
            {
                Debug.LogError("Trying to access native object from nullptr");
                return null;
            }
            wrPoseEstimatorOptions_SetRotationMultipleOf90(m_nativeHandle, rotationMultipleOf90);
            return this;
        }

        public bool GetEnableJointSmoothing()
        {
            if (m_nativeHandle == IntPtr.Zero)
            {
                Debug.LogError("Trying to access native object from nullptr");
                return false;
            }
            return wrPoseEstimatorOptions_GetEnableJointSmoothing(m_nativeHandle) == 1;
        }

        public bool GetEnableHeadSmoothing()
        {
            if (m_nativeHandle == IntPtr.Zero)
            {
                Debug.LogError("Trying to access native object from nullptr");
                return false;
            }
#if !(UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX || UNITY_IOS)
            return wrPoseEstimatorOptions_GetEnableHeadSmoothing(m_nativeHandle) == 1;
#else
            Debug.LogWarning("No iOS/OSX support for head pose estimator");
            return false;
#endif
        }

        public bool GetEstimateAllHandBoxes()
        {
            if (m_nativeHandle == IntPtr.Zero)
            {
                Debug.LogError("Trying to access native object from nullptr");
                return false;
            }
            return wrPoseEstimatorOptions_GetEstimateAllHandBoxes(m_nativeHandle) == 1;
        }

        public bool GetEstimatePoseFace()
        {
            if (m_nativeHandle == IntPtr.Zero)
            {
                Debug.LogError("Trying to access native object from nullptr");
                return false;
            }
            return wrPoseEstimatorOptions_GetEstimatePoseFace(m_nativeHandle) == 1;
        }

        public bool GetEstimateSingle()
        {
            if (m_nativeHandle == IntPtr.Zero)
            {
                Debug.LogError("Trying to access native object from nullptr");
                return false;
            }
            return wrPoseEstimatorOptions_GetEstimateSingle(m_nativeHandle) == 1;
        }

        public bool GetEstimateHands()
        {
            if (m_nativeHandle == IntPtr.Zero)
            {
                Debug.LogError("Trying to access native object from nullptr");
                return false;
            }
            return wrPoseEstimatorOptions_GetEstimateHands(m_nativeHandle) == 1;
        }

        public bool GetEstimateHead()
        {
            if (m_nativeHandle == IntPtr.Zero)
            {
                Debug.LogError("Trying to access native object from nullptr");
                return false;
            }
            return wrPoseEstimatorOptions_GetEstimateHead(m_nativeHandle) == 1;
        }

        public bool GetEstimateMask()
        {
            if (m_nativeHandle == IntPtr.Zero)
            {
                Debug.LogError("Trying to access native object from nullptr");
                return false;
            }
            return wrPoseEstimatorOptions_GetEstimateMask(m_nativeHandle) == 1;
        }

        /// <summary>
        /// Get which pose 2D will be considered as the main person. <see cref="MainPersonId"/>.
        /// Use ALL for multi 3d.
        /// </summary>
        /// <returns></returns>
        public MainPersonId GetMainPersonId()
        {
            if (m_nativeHandle == IntPtr.Zero)
            {
                Debug.LogError("Trying to access native object from nullptr");
                return MainPersonId.NONE;
            }
            return wrPoseEstimatorOptions_GetMainPersonId(m_nativeHandle);
        }

        public bool GetEstimate3D()
        {
            if (m_nativeHandle == IntPtr.Zero)
            {
                Debug.LogError("Trying to access native object from nullptr");
                return false;
            }
            return wrPoseEstimatorOptions_GetEstimate3D(m_nativeHandle) == 1;
        }

        public bool GetUseIK()
        {
            if (m_nativeHandle == IntPtr.Zero)
            {
                Debug.LogError("Trying to access native object from nullptr");
                return false;
            }
            return wrPoseEstimatorOptions_GetUseIK(m_nativeHandle) == 1;
        }

        public int GetRotationMultipleOf90()
        {
            if (m_nativeHandle == IntPtr.Zero)
            {
                Debug.LogError("Trying to access native object from nullptr");
                return -1;
            }
            return wrPoseEstimatorOptions_GetRotationMultipleOf90(m_nativeHandle);

        }
    }
}