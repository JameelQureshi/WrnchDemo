/*
 Copyright (c) 2019 Wrnch Inc.
 All rights reserved
*/

using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System;
using System.IO;

namespace wrnchAI.wrAPI
{

    public static class Glob
    {
#if UNITY_IOS && !UNITY_EDITOR_OSX
        public const string DLLName = "__Internal";
#else
        public const string DLLName = "wrAPI";
#endif
    }

    /// <summary>
    /// Class wrapping the wrnch::PoseEstimator present in wrAPI.
    /// </summary>
    public class PoseEstimator
    {
        protected IntPtr m_nativeHandle = System.IntPtr.Zero;
        public IntPtr nativeHandle { get { return m_nativeHandle; } }

        JointDefinition m_jointDefinition2d = new JointDefinition("j25");
        public JointDefinition JointDefinition2d { get { return m_jointDefinition2d; } }

        JointDefinition m_jointDefinition3d;
        public JointDefinition JointDefinition3d { get { return m_jointDefinition3d; } }

        JointDefinition m_jointDefinitionRaw3d;
        public JointDefinition JointDefinitionRaw3d { get { return m_jointDefinitionRaw3d; } }

        [DllImport(Glob.DLLName)]
        private static extern int wrPoseEstimator_Create(ref IntPtr nativeHandle, [MarshalAs(UnmanagedType.LPStr)] string modelsPath, int deviceId, IntPtr nativeParams, IntPtr jointDefParams);
        [DllImport(Glob.DLLName)]
        private static extern int wrPoseEstimator_CreateDefault(ref IntPtr nativeHandle, [MarshalAs(UnmanagedType.LPStr)] string modelsPath);
        [DllImport(Glob.DLLName)]
        private static extern int wrPoseEstimator_CreateFromConfig(ref IntPtr nativeHandle, IntPtr nativeConfig);
        [DllImport(Glob.DLLName)]
        private static extern void wrPoseEstimator_Destroy(IntPtr nativeHandle);
        [DllImport(Glob.DLLName)]
        private static extern IntPtr wrPoseEstimator_GetHuman2DOutputFormat(IntPtr nativeHandle);
        [DllImport(Glob.DLLName)]
        private static extern IntPtr wrPoseEstimator_GetHuman3DOutputFormat(IntPtr nativeHandle);
        [DllImport(Glob.DLLName)]
        private static extern IntPtr wrPoseEstimator_GetHuman3DRawOutputFormat(IntPtr nativeHandle);
        [DllImport(Glob.DLLName)]
        private static extern IntPtr wrPoseEstimator_GetFaceOutputFormat(IntPtr nativeHandle);
        [DllImport(Glob.DLLName)]
        private static extern void wrPoseEstimator_GetDefaultTPose3D(IntPtr nativeHandle, IntPtr poseInOut);
        [DllImport(Glob.DLLName)]
        private static extern int wrPoseEstimator_ProcessFrame(IntPtr nativeHandle, IntPtr bgrData, int width, int height, IntPtr options);
        [DllImport(Glob.DLLName)]
        private static extern int wrPoseEstimator_InitializeHeadDefault(IntPtr nativeHandle, [MarshalAs(UnmanagedType.LPStr)] string modelPath);
        [DllImport(Glob.DLLName)]
        private static extern IntPtr wrPoseEstimator_GetHeadPosesBegin(IntPtr nativeHandle);
        [DllImport(Glob.DLLName)]
        protected static extern IntPtr wrPoseEstimator_GetHeadPosesNext(IntPtr currHeadPose);
        [DllImport(Glob.DLLName)]
        protected static extern IntPtr wrPoseEstimator_GetHeadPosesEnd(IntPtr nativeHandle);
        [DllImport(Glob.DLLName)]
        protected static extern void wrPoseEstimator_GetAllHumans2D(IntPtr nativeHandle, IntPtr[] posesOut);
        [DllImport(Glob.DLLName)]
        protected static extern int wrPoseEstimator_GetNumHumans2D(IntPtr nativeHandle);
        [DllImport(Glob.DLLName)]
        protected static extern IntPtr wrPoseEstimator_GetHumans2DBegin(IntPtr nativeHandle);
        [DllImport(Glob.DLLName)]
        protected static extern IntPtr wrPoseEstimator_GetPose2DNext(IntPtr currPose2D);
        [DllImport(Glob.DLLName)]
        protected static extern IntPtr wrPoseEstimator_GetHumans2DEnd(IntPtr nativeHandle);
        [DllImport(Glob.DLLName)]
        protected static extern int wrPoseEstimator_GetMaxPersons(IntPtr nativeHandle);
        [DllImport(Glob.DLLName)]
        protected static extern IntPtr wrPoseEstimator_GetHumans3DBegin(IntPtr nativeHandle);
        [DllImport(Glob.DLLName)]
        protected static extern IntPtr wrPoseEstimator_GetPose3DNext(IntPtr currPose3D);
        [DllImport(Glob.DLLName)]
        protected static extern IntPtr wrPoseEstimator_GetHumans3DEnd(IntPtr nativeHandle);
        [DllImport(Glob.DLLName)]
        protected static extern IntPtr wrPoseEstimator_GetRawHumans3DBegin(IntPtr nativeHandle);
        [DllImport(Glob.DLLName)]
        protected static extern IntPtr wrPoseEstimator_GetRawHumans3DEnd(IntPtr nativeHandle);
        [DllImport(Glob.DLLName)]
        protected static extern int wrPoseEstimator_GetNumHumans3D(IntPtr nativeHandle);
        [DllImport(Glob.DLLName)]
        protected static extern int wrPoseEstimator_GetNumRawHumans3D(IntPtr nativeHandle);
        [DllImport(Glob.DLLName)]
        protected static extern int wrPoseEstimator_Initialize3D(IntPtr nativeHandle, IntPtr ikParams, [MarshalAs(UnmanagedType.LPStr)] string modelPath);
        [DllImport(Glob.DLLName)]
        protected static extern int wrPoseEstimator_HasIK(IntPtr nativeHandle);
        [DllImport(Glob.DLLName)]
        protected static extern void wrPoseEstimator_GetMaskDims(IntPtr nativeHandle, ref int outMaskWidth, ref int outMaskHeight, ref int outMaskDepth);
        [DllImport(Glob.DLLName)]
        protected static extern IntPtr wrPoseEstimator_GetMaskView(IntPtr nativeHandle);
        [DllImport(Glob.DLLName)]
        private static extern int wrPoseEstimator_Deserialize(IntPtr data, int size, int deviceId, ref IntPtr nativeHandle);

        [DllImport(Glob.DLLName)]
        protected static extern uint wrPoseEstimator_GetNumPoseFace(IntPtr nativeHandle);
        [DllImport(Glob.DLLName)]
        protected static extern IntPtr wrPoseEstimator_GetFacePosesBegin(IntPtr nativeHandle);
        [DllImport(Glob.DLLName)]
        protected static extern IntPtr wrPoseEstimator_GetFacePosesNext(IntPtr nativeHandle);
        [DllImport(Glob.DLLName)]
        protected static extern IntPtr wrPoseEstimator_GetFacePosesEnd(IntPtr nativeHandle);
        [DllImport(Glob.DLLName)]
        protected static extern int wrPoseEstimator_GetIdState(IntPtr nativeHandle, int id);

        /// <summary>
        ///  Initialization of the pose estimator and structures
        /// </summary>
        /// <param name="modelsDir">Path to the folder containing the network models</param>
        /// <param name="modelPath2d">Full path to the 2D model (optional)</param>
        public void Init(string modelsDir, Config.PoseWorkerConfig config, string modelPath2d = "")
        {
            PoseParams poseParams = new PoseParams();
            poseParams.SetTrackerKind(config.TrackerKind);

            Debug.Log($"modelsDir={modelsDir}");
            PoseEstimatorConfigParams configParams = new PoseEstimatorConfigParams(modelsDir);

            if (modelPath2d != "")
            {
                configParams.Set2dModelPath(modelPath2d);
            }

#if UNITY_IOS || UNITY_ANDROID
            string license = @"{
            ""vendor"": ""wrnch Inc."",
            ""license"": {
            ""product"": ""wrnchAI engine"",
            ""version"": """",
            ""expiry"": """",
            ""hostid"": """",
            ""customer"": """",
            ""signature"": """"
            }
            }";

            configParams.SetLicenseString(license);
#else
            poseParams.SetPreferredNetWidth2d(config.NetRes.x);
            poseParams.SetPreferredNetHeight2d(config.NetRes.y);
#endif

            if (config.LicenseString.Length > 0)
            {
                configParams.SetLicenseString(config.LicenseString);
            }

            configParams.SetOutputFormat(JointDefinition2d.NativeHandle).
            SetPoseParams(poseParams.NativeHandle).
            SetPoseParams(poseParams.NativeHandle);
            int code = wrPoseEstimator_CreateFromConfig(ref m_nativeHandle, configParams.NativeHandle);
            configParams.Destroy();
            Debug.Log("Checking return code for 2D");
            CheckReturnCodeOk(code);
            Debug.Log("2D model OK");

            IKParams ikParams = new IKParams(); // default ctor - default params
            ikParams.SetMaxAngularVelocity(Mathf.Deg2Rad * 750.0F);
            ikParams.SetFPS(60.0f); // nominal FPS. Doesn't have to be exact.

            code = wrPoseEstimator_Initialize3D(m_nativeHandle, ikParams.NativeHandle, modelsDir);
            Debug.Log("Checking return code for 3D");
            CheckReturnCodeOk(code);
            Debug.Log("3D model OK");

#if !(UNITY_IOS || UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX || UNITY_ANDROID)
            code = wrPoseEstimator_InitializeHeadDefault(m_nativeHandle, modelsDir);
            try
            {
                CheckReturnCodeOk(code);
            }
            catch (Exception e)
            {
                Debug.Log("Head Model is not available in this configuration.");
            }
#endif

            m_jointDefinition3d = new JointDefinition(wrPoseEstimator_GetHuman3DOutputFormat(m_nativeHandle));
            m_jointDefinitionRaw3d = new JointDefinition(wrPoseEstimator_GetHuman3DRawOutputFormat(m_nativeHandle));
        }

        public void InitFromSerialized(string serializedEstimatorPath, int deviceId = 0)
        {
            byte[] fileBytes = File.ReadAllBytes(serializedEstimatorPath);

            GCHandle pinnedBytes = GCHandle.Alloc(fileBytes, GCHandleType.Pinned);
            int code = wrPoseEstimator_Deserialize(pinnedBytes.AddrOfPinnedObject(),
                                                          fileBytes.Length,
                                                          deviceId,
                                                          ref m_nativeHandle);
            pinnedBytes.Free();

            m_jointDefinition2d = new JointDefinition(wrPoseEstimator_GetHuman2DOutputFormat(m_nativeHandle));
            m_jointDefinition3d = new JointDefinition(wrPoseEstimator_GetHuman3DOutputFormat(m_nativeHandle));
            m_jointDefinitionRaw3d = new JointDefinition(wrPoseEstimator_GetHuman3DRawOutputFormat(m_nativeHandle));

            CheckReturnCodeOk(code);
        }

        private void CheckReturnCodeOk(int code)
        {
            if (code != 0)
            {
                throw new Exception(ReturncodeDefinition.ToString(code));
            }
        }

        /// <summary>
        ///   Core function of the pose estimator. Feed a given frame to the wrnchAI engine and update internal result structures.
        /// </summary>
        /// <param name="img"> Image to process as a linear array of BGR8 pixels</param>
        /// <param name="width"> Width in pixels of input image</param>
        /// <param name="height"> Height in pixels of input image</param>
        /// <param name="poseOptions"> Set of options to toggle internal modules when processing the frame. See PoseEstimatorOptions </param>
        public void ProcessFrame(byte[] img, int width, int height, PoseEstimatorOptions poseOptions)
        {
            if (m_nativeHandle == IntPtr.Zero)
            {
                Debug.LogError("PoseEstimator::ProcessFrame -Trying to call an uninitialized pose estimator");
                return;
            }

            GCHandle pinnedImg = GCHandle.Alloc(img, GCHandleType.Pinned);
            int code = wrPoseEstimator_ProcessFrame(m_nativeHandle, pinnedImg.AddrOfPinnedObject(), width, height, poseOptions.NativeHandle);
            CheckReturnCodeOk(code);
            pinnedImg.Free();
        }

        public int GetAllFaces(ref PoseFace[] posesOut)
        {
            if (m_nativeHandle == IntPtr.Zero)
            {
                Debug.LogError("PoseEstimator::GetAllFaces-Trying to call an uninitialized pose estimator");
                return -1;
            }

            IntPtr currPose = wrPoseEstimator_GetFacePosesBegin(m_nativeHandle);
            IntPtr endPose = wrPoseEstimator_GetFacePosesEnd(m_nativeHandle);

            int idx = 0;
            while (currPose != endPose)
            {
                posesOut[idx++].Update(currPose);
                currPose = wrPoseEstimator_GetFacePosesNext(currPose);
            }

            return idx;
        }

        /// <summary>
        ///  Return an array of processed heads positions and orientations found during the latest call to ProcessFrame.
        ///  Orientation will be returned as a 3 component rotation (in radians) relative to a TPose style head pose.
        ///  For more informations about PoseHead, see PoseTypes.cs
        /// </summary>
        /// <param name="posesOut">Output array with enough structures allocated to store the result</param>
        /// <returns> The number of heads found </returns>
        public int GetAllHeads(ref PoseHead[] posesOut)
        {
            if (m_nativeHandle == IntPtr.Zero)
            {
                Debug.LogError("PoseEstimator::GetAllHeads- Trying to call an uninitialized pose estimator");
                return -1;
            }

            IntPtr currPose = wrPoseEstimator_GetHeadPosesBegin(m_nativeHandle);
            IntPtr endPose = wrPoseEstimator_GetHeadPosesEnd(m_nativeHandle);

            int idx = 0;
            while (currPose != endPose)
            {
                posesOut[idx++].Update(currPose);
                currPose = wrPoseEstimator_GetHeadPosesNext(currPose);
            }

            return idx;
        }

        /// <summary>
        ///  Return an array of processed 2D human poses found during the latest call to ProcessFrame.
        ///  The resulting joints will be stored as a float array in normalized frame coordinates.
        ///  For more informations about Pose2D, see PoseTypes.cs
        /// </summary>
        /// <param name="posesOut"> Output array with enough structures allocated to store the result</param>
        /// <returns> The number of humans found </returns>
        public int GetAllHumans2D(ref Pose2D[] posesOut)
        {
            if (m_nativeHandle == IntPtr.Zero)
            {
                Debug.LogError("PoseEstimator::GetAllHumans2D - Trying to call an uninitialized pose estimator");
                return -1;
            }

            IntPtr currPose = wrPoseEstimator_GetHumans2DBegin(m_nativeHandle);
            IntPtr endPose = wrPoseEstimator_GetHumans2DEnd(m_nativeHandle);

            int idx = 0;
            while (currPose != endPose)
            {
                posesOut[idx++].Update(currPose, JointDefinition2d);
                currPose = wrPoseEstimator_GetPose2DNext(currPose);
            }

            return idx;
        }

        /// <summary>
        ///  Grab the number of 2D humans found during the last call to ProcessFrame without returning the poses.
        /// </summary>
        /// <returns> The number of humans found </returns>
        public int GetNumHumans2D()
        {
            if (m_nativeHandle == IntPtr.Zero)
            {
                Debug.LogError("PoseEstimator::GetNumHumans2D - Trying to call an uninitialized pose estimator");
                return -1;
            }
            return wrPoseEstimator_GetNumHumans2D(m_nativeHandle);
        }

        /// <summary>
        ///  Grab the maximum number of humans the wrnchAI engine can estimate in a single frame. A nice way to preallocate resources.
        /// </summary>
        /// <returns> Max number of 2D humans</returns>
        public int GetMaxPersons()
        {
            if (m_nativeHandle == IntPtr.Zero)
            {
                Debug.LogError("PoseEstimator::GetMaxPersons - Trying to call an uninitialized pose estimator");
                return -1;
            }
            return wrPoseEstimator_GetMaxPersons(m_nativeHandle);
        }

        /// <summary>
        ///  Returns the number of 3D humans found during the last call to ProcessFrame and copies their data into the passed array.
        ///  The joint data comprises a set of quaternions (x, y, z, w) and positions (x, y, z) in world space.
        ///  For more informations about Pose3D, see PoseTypes.cs
        /// </summary>
        /// <param name="posesOut"> Output array with enough structures allocated to store the result </param>
        /// <returns> The number of 3D humans found </returns>
        public int GetAllHumans3D(ref Pose3D[] posesOut)
        {
            if (m_nativeHandle == IntPtr.Zero)
            {
                Debug.LogError("PoseEstimator::GetAllHumans3D - Trying to call an uninitialized pose estimator");
                return -1;
            }

            IntPtr currPose = wrPoseEstimator_GetHumans3DBegin(m_nativeHandle);
            IntPtr endPose = wrPoseEstimator_GetHumans3DEnd(m_nativeHandle);

            int idx = 0;
            while (currPose != endPose)
            {
                posesOut[idx++].Update(currPose, JointDefinition3d);
                currPose = wrPoseEstimator_GetPose3DNext(currPose);
            }
            return idx;
        }

        /// <summary>
        /// Returns the number of 3D humans found during the last call to ProcessFrame without returning the poses.
        /// </summary>
        /// <returns> The number of humans found </returns>
        public int GetNumHumans3D()
        {
            if (m_nativeHandle == IntPtr.Zero)
            {
                Debug.LogError("PoseEstimator::GetNumHumans3D - Trying to call an uninitialized pose estimator");
                return -1;
            }
            return wrPoseEstimator_GetNumRawHumans3D(m_nativeHandle);
        }

        /// <summary>
        /// Returns the number of raw 3D humans found during the last call to ProcessFrame without returning the poses.
        /// </summary>
        /// <returns></returns>
        public int GetNumRawHumans3d()
        {
            if (m_nativeHandle == IntPtr.Zero)
            {
                Debug.LogError("PoseEstimator::GetNumRawHumans3D - Trying to call an uninitialized pose estimator");
                return -1;
            }
            return wrPoseEstimator_GetNumRawHumans3D(m_nativeHandle);
        }

        /// <summary>
        /// Returns the number of raw 3D humans found during the last call to ProcessFrame and copies their data into the passed array.
        /// </summary>
        /// <param name="posesOut">Array of Pose3D found on the last frame </param>
        /// <returns>Number of 3D humans estimated</returns>
        public int GetAllRawHumans3D(ref Pose3D[] posesOut)
        {
            if (m_nativeHandle == IntPtr.Zero)
            {
                Debug.LogError("PoseEstimator::GetNumHumans3D - Trying to call an uninitialized pose estimator");
                return -1;
            }

            IntPtr currPose = wrPoseEstimator_GetRawHumans3DBegin(m_nativeHandle);
            IntPtr endPose = wrPoseEstimator_GetRawHumans3DEnd(m_nativeHandle);

            int idx = 0;
            while (currPose != endPose)
            {
                posesOut[idx++].Update(currPose, m_jointDefinitionRaw3d);
                currPose = wrPoseEstimator_GetPose3DNext(currPose);
            }

            return idx;
        }

        /// <summary>
        ///  Query if the pose estimator supports inverse kinematics.  Since version 1.8.0, the pose estimator will always support IK
        ///  Thus, this function is deprecated and will always return 1
        /// </summary>
        /// <returns> 1 if wrIK is enabled in the PoseEstimator</returns>
        public int HasIK()
        {
            if (m_nativeHandle == IntPtr.Zero)
            {
                Debug.LogError("PoseEstimator::HasIK - Trying to call an uninitialized pose estimator");
                return -1;
            }

            return wrPoseEstimator_HasIK(m_nativeHandle);
        }

        /// <summary>
        ///  Returns a JointDefinition containing informations about the 2D joint definition. For 3D joints definition, see wrExtended
        /// </summary>
        /// <returns>2D pose joint definition </returns>
        public JointDefinition GetHuman2DOutputFormat()
        {
            if (m_nativeHandle == IntPtr.Zero)
            {
                Debug.LogError("PoseEstimator::GetHuman2DOutputFormat - Trying to call an uninitialized pose estimator");
                return null;
            }

            return new JointDefinition(wrPoseEstimator_GetHuman2DOutputFormat(m_nativeHandle));
        }

        /// <summary>
        ///  Returns a JointDefinition containing informations about the 3D joint definition.
        /// </summary>
        /// <returns>3D pose joint definition </returns>
        public JointDefinition GetHuman3DOutputFormat()
        {
            if (m_nativeHandle == IntPtr.Zero)
            {
                Debug.LogError("PoseEstimator::GetHuman3DOutputFormat - Trying to call an uninitialized pose estimator");
                return null;
            }

            return new JointDefinition(wrPoseEstimator_GetHuman3DOutputFormat(m_nativeHandle));
        }

        /// <summary>
        ///  Returns a JointDefinition containing informations about the raw 3D joint definition
        /// </summary>
        /// <returns>Raw 3D pose joint definition </returns>
        public JointDefinition GetHuman3DRawOutputFormat()
        {
            if (m_nativeHandle == IntPtr.Zero)
            {
                Debug.LogError("PoseEstimator::GetHuman3DRawOutputFormat - Trying to call an uninitialized pose estimator");
                return null;
            }

            return new JointDefinition(wrPoseEstimator_GetHuman3DRawOutputFormat(m_nativeHandle));
        }

        /// <summary>
        ///  Returns a JointDefinition containing informations about the face joint definition
        /// </summary>
        /// <returns>Face joint definition </returns>
        public JointDefinition GetFaceOutputFormat()
        {
            if (m_nativeHandle == IntPtr.Zero)
            {
                Debug.LogError("PoseEstimator::GetFaceOutputFormat - Trying to call an uninitialized pose estimator");
                return null;
            }

            return new JointDefinition(wrPoseEstimator_GetFaceOutputFormat(m_nativeHandle));
        }

        public void GetMaskDims(ref int outMaskWidth, ref int outMaskHeight, ref int outMaskDepth)
        {
            if (m_nativeHandle == IntPtr.Zero)
            {
                Debug.LogError("PoseEstimator::GetMaskDims - Trying to call an uninitialized pose estimator");
                return;
            }

            wrPoseEstimator_GetMaskDims(m_nativeHandle, ref outMaskWidth, ref outMaskHeight, ref outMaskDepth);
        }

        public void GetMaskView(ref byte[] maskView, ref int maskWidth, ref int maskHeight)
        {
            if (m_nativeHandle == IntPtr.Zero)
            {
                Debug.LogError("PoseEstimator::GetMaskView - Trying to call an uninitialized pose estimator");
                return;
            }

            int maskDepth = 0;

            wrPoseEstimator_GetMaskDims(m_nativeHandle, ref maskWidth, ref maskHeight, ref maskDepth);

            if (maskView == null || maskView.Length != maskWidth * maskHeight)
            {
                maskView = new byte[maskWidth * maskHeight];
            }

            var maskHandle = wrPoseEstimator_GetMaskView(m_nativeHandle);
            if (maskHandle != IntPtr.Zero)
                Marshal.Copy(maskHandle, maskView, 0, maskView.Length);

        }

        public IdState GetIDState(int id)
        {
            if (m_nativeHandle == IntPtr.Zero)
            {
                Debug.LogError("PoseEstimator::GetIdState -Trying to call an uninitialized pose estimator");
                return IdState.Untracked;
            }

            return (IdState)wrPoseEstimator_GetIdState(m_nativeHandle, id);

        }

        /// <summary>
        ///  Deallocate CPU/GPU resources and destroy the pose estimator.
        /// </summary>
        public void Release()
        {
            if (m_nativeHandle == IntPtr.Zero)
            {
                Debug.LogError("PoseEstimator::Release - Trying to call an uninitialized pose estimator");
                return;
            }

            wrPoseEstimator_Destroy(m_nativeHandle);
            m_nativeHandle = IntPtr.Zero;
        }

        /// <summary>
        /// Return codes definition can be found in the wrnchAI-engine package
        /// in include/wrnch/returnCodes.h
        /// </summary>
        protected static class ReturncodeDefinition
        {
            [DllImport(Glob.DLLName)]
            private static extern IntPtr wrReturnCode_Translate(int code);

            public static string ToString(int c)
            {
                return Marshal.PtrToStringAnsi(wrReturnCode_Translate(c));
            }
        }
    }
};
