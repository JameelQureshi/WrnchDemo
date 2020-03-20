/*
 Copyright (c) 2019 Wrnch Inc.
 All rights reserved
*/

using System;
using System.Runtime.InteropServices;
using System.Linq;
using UnityEngine;

namespace wrnchAI.wrAPI
{
    public class PoseEstimatorConfigParams
    {
        private IntPtr m_nativeHandle;
        public IntPtr NativeHandle { get { return m_nativeHandle; } }

        [DllImport(Glob.DLLName)]
        private static extern IntPtr wrPoseEstimatorConfigParams_Create([MarshalAs(UnmanagedType.LPStr)] string modelsPath);
        [DllImport(Glob.DLLName)]
        private static extern void wrPoseEstimatorConfigParams_SetLicenseString(IntPtr nativeHandle, [MarshalAs(UnmanagedType.LPStr)] string licenseString);
        [DllImport(Glob.DLLName)]
        private static extern void wrPoseEstimatorConfigParams_SetLicensePath(IntPtr nativeHandle, [MarshalAs(UnmanagedType.LPStr)] string licensePath);
        [DllImport(Glob.DLLName)]
        private static extern void wrPoseEstimatorConfigParams_SetDeviceFingerprint(IntPtr nativeHandle, [MarshalAs(UnmanagedType.LPStr)] string deviceFingerprint);
        [DllImport(Glob.DLLName)]
        private static extern void wrPoseEstimatorConfigParams_SetPoseParams(IntPtr nativeHandle, IntPtr nativePoseParams);
        [DllImport(Glob.DLLName)]
        private static extern void wrPoseEstimatorConfigParams_SetOutputFormat(IntPtr nativeHandle, IntPtr nativeJointDefParams);
        [DllImport(Glob.DLLName)]
        private static extern void wrPoseEstimatorConfigParams_Set2dModelRegex(IntPtr nativeHandle, [MarshalAs(UnmanagedType.LPStr)] string regex);
        [DllImport(Glob.DLLName)]
        private static extern string wrPoseEstimatorConfigParams_GetModelsDirectory(IntPtr nativeHandle);
        [DllImport(Glob.DLLName)]
        private static extern void wrPoseEstimatorConfigParams_SetDeviceId(IntPtr nativeHandle, int deviceId);
        [DllImport(Glob.DLLName)]
        private static extern void wrPoseEstimatorConfigParams_Destroy(IntPtr nativeHandle);

        public PoseEstimatorConfigParams(string modelsPath)
        {
            m_nativeHandle = wrPoseEstimatorConfigParams_Create(modelsPath);
        }

        public PoseEstimatorConfigParams SetLicenseString(string licenseString)
        {
            if (m_nativeHandle == IntPtr.Zero)
            {
                Debug.LogError("Trying to access native object from nullptr");
                return null;
            }

            wrPoseEstimatorConfigParams_SetLicenseString(m_nativeHandle, licenseString);
#if UNITY_IOS
            var fingerPrint = string.Concat(UnityEngine.iOS.Device.vendorIdentifier, Enumerable.Repeat("0", 65 - UnityEngine.iOS.Device.vendorIdentifier.Length));
            wrPoseEstimatorConfigParams_SetDeviceFingerprint(m_nativeHandle, fingerPrint);
            wrPoseEstimatorConfigParams_SetLicensePath(m_nativeHandle, ".");
#endif

            return this;
        }

        public PoseEstimatorConfigParams SetDeviceId(int deviceId)
        {
            if (m_nativeHandle == IntPtr.Zero)
            {
                Debug.LogError("Trying to access native object from nullptr");
                return null;
            }

            wrPoseEstimatorConfigParams_SetDeviceId(m_nativeHandle, deviceId);

            return this;
        }

        public PoseEstimatorConfigParams SetPoseParams(IntPtr nativePoseParams)
        {
            if (m_nativeHandle == IntPtr.Zero)
            {
                Debug.LogError("Trying to access native object from nullptr");
                return null;
            }

            wrPoseEstimatorConfigParams_SetPoseParams(m_nativeHandle, nativePoseParams);

            return this;
        }

        public PoseEstimatorConfigParams SetOutputFormat(IntPtr nativeJointDefParams)
        {
            if (m_nativeHandle == IntPtr.Zero)
            {
                Debug.LogError("Trying to access native object from nullptr");
                return null;
            }

            wrPoseEstimatorConfigParams_SetOutputFormat(m_nativeHandle, nativeJointDefParams);

            return this;
        }

        public PoseEstimatorConfigParams Set2dModelRegex(string regex)
        {
            if(m_nativeHandle == IntPtr.Zero)
            {
                Debug.LogError("Trying to access native object from nullptr");
                return null;
            }

            wrPoseEstimatorConfigParams_Set2dModelRegex(m_nativeHandle, regex);
            return this;
        }

        public string GetModelsDir()
        {
            if (m_nativeHandle == IntPtr.Zero)
            {
                Debug.LogError("Trying to access native object from nullptr");
                return null;
            }

            return wrPoseEstimatorConfigParams_GetModelsDirectory(m_nativeHandle);
        }

        public PoseEstimatorConfigParams Destroy()
        {
            if (m_nativeHandle == IntPtr.Zero)
            {
                Debug.LogError("Trying to access native object from nullptr");
                return null;
            }

            wrPoseEstimatorConfigParams_Destroy(m_nativeHandle);

            return this;
        }

    }
}
