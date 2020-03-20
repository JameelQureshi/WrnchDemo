/*
 Copyright (c) 2019 Wrnch Inc.
 All rights reserved
*/

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using System.Linq;

namespace wrnchAI.wrAPI
{
    /// <summary>
    /// Access to wrAPIs joint registry.
    /// </summary>
    class JointDefinitionRegistry
    {
        [DllImport(Glob.DLLName)]
        public static extern IntPtr wrJointDefinition_Get([MarshalAs(UnmanagedType.LPStr)] string name);
        [DllImport(Glob.DLLName)]
        public static extern int wrJointDefinition_GetNumDefinitions();
    }

    /// <summary>
    ///  Class holding the details relative to a joint definition
    /// </summary>
    public class JointDefinition
    {
        private IntPtr m_nativeHandle;
        public IntPtr NativeHandle { get { return m_nativeHandle; } }

        /// <summary>
        /// Return the number of joints in this joint definition
        /// </summary>
        public int NumJoints
        {
            get
            {
                if (m_nativeHandle == IntPtr.Zero)
                {
                    Debug.LogError("Trying to access object from nullptr.");
                    return -1;
                }
                return wrJointDefinition_GetNumJoints(m_nativeHandle);
            }
        }

        /// <summary>
        /// Return the number of bones in this joint definition
        /// </summary>
        public int NumBones
        {
            get
            {
                if (m_nativeHandle == IntPtr.Zero)
                {
                    Debug.LogError("Trying to access object from nullptr.");
                    return -1;
                }
                return wrJointDefinition_GetNumBones(m_nativeHandle);
            }
        }

        [DllImport(Glob.DLLName)]
        private static extern int wrJointDefinition_GetNumJoints(IntPtr nativeHandle);
        [DllImport(Glob.DLLName)]
        private static extern int wrJointDefinition_GetNumBones(IntPtr nativeHandle);
        [DllImport(Glob.DLLName)]
        private static extern IntPtr wrJointDefinition_GetName(IntPtr nativeHandle);
        [DllImport(Glob.DLLName)]
        private static extern void wrJointDefinition_GetJointNames(IntPtr nativeHandle, IntPtr outputNames);
        [DllImport(Glob.DLLName)]
        private static extern IntPtr wrJointDefinition_GetBonePairs(IntPtr nativeHandle, int[] pairs);
        [DllImport(Glob.DLLName)]
        private static extern int wrJointDefinition_GetJointIndex(IntPtr nativeHandle, [MarshalAs(UnmanagedType.LPStr)] string name);

        /// <summary>
        ///  Create a joint definition object from a native handle
        /// </summary>
        /// <param name="nativeHandle"></param>
        public JointDefinition(IntPtr nativeHandle)
        {
            m_nativeHandle = nativeHandle;
        }

        /// <summary>
        /// Create a joint definition by name <see cref="JointDefinitionRegistry"/>
        /// </summary>
        /// <param name="name"></param>
        public JointDefinition(string name)
        {
            m_nativeHandle = JointDefinitionRegistry.wrJointDefinition_Get(name);
        }

        /// <summary>
        ///  Get index of joint in the definition with name
        /// </summary>
        /// <param name="name"> joint index </param>
        /// <returns></returns>
        public int GetJointIndex(string name)
        {
            if (m_nativeHandle == IntPtr.Zero)
            {
                Debug.LogError("Trying to access object from nullptr.");
                return -1;
            }
            return wrJointDefinition_GetJointIndex(m_nativeHandle, name);
        }

        /// <summary>
        /// Get name of joint definition
        /// </summary>
        /// <returns>Returns the name of the joints `definition</returns>
        public string GetName()
        {
            if (m_nativeHandle == IntPtr.Zero)
            {
                Debug.LogError("Trying to access object from nullptr.");
                return "";
            }
            return Marshal.PtrToStringAnsi(wrJointDefinition_GetName(m_nativeHandle));
        }

        /// <summary>
        ///  Return an array of joints composing this joint definition
        /// </summary>
        /// <returns>The names</returns>
        public string[] GetJointNames()
        {
            if (m_nativeHandle == IntPtr.Zero)
            {
                Debug.LogError("Trying to access object from nullptr.");
                return null;
            }

            string[] output = new string[wrJointDefinition_GetNumJoints(m_nativeHandle)];
            IntPtr[] pManagedNames = new IntPtr[output.Length];
            GCHandle pNativeNames = GCHandle.Alloc(pManagedNames, GCHandleType.Pinned);

            wrJointDefinition_GetJointNames(m_nativeHandle, pNativeNames.AddrOfPinnedObject());
            Marshal.Copy(pNativeNames.AddrOfPinnedObject(), pManagedNames, 0, output.Length);
            pNativeNames.Free();

            for (int i = 0; i < output.Length; i++)
            {
                output[i] = Marshal.PtrToStringAnsi(pManagedNames[i]);
            }

            return output;
        }

        /// <summary>
        ///  Returns an array of bones composing this joint definition
        /// </summary>
        /// <returns>The bones as a list of pair of ints </returns>
        public List<int[]> GetBonePairs()
        {
            var numBones = NumBones;    // Getter is just wrapping the C API call. 

            List<int[]> output = new List<int[]>(numBones);
            int[] nativeBonePairs = new int[numBones * 2];

            wrJointDefinition_GetBonePairs(m_nativeHandle, nativeBonePairs);

            for (int i = 0; i < numBones; i++)
            {
                output.Add(new int[2] { nativeBonePairs[2 * i], nativeBonePairs[2 * i + 1] });
            }
            return output;
        }
    }

    /// <summary>
    ///  Joint format used to represent 3d poses out of wrIK
    /// </summary>
    public enum wrExtendedJoints
    {
        PELV,
        LHIP, LKNEE, LANKLE,
        RHIP, RKNEE, RANKLE,
        SPINE0,
        LSHOULDER, LELBOW, LWRIST,
        RSHOULDER, RELBOW, RWRIST,
        HEAD,
        LTOE,
        RTOE,
        LCOLLAR,
        RCOLLAR,
        NECK, SPINE1, SPINE2,
        LHIP_ROLL, LKNEE_ROLL,
        RHIP_ROLL, RKNEE_ROLL,
        LSHOULDER_ROLL, LELBOW_ROLL,
        RSHOULDER_ROLL, RELBOW_ROLL,
        NONE = -1
    };

    public class wrExtended
    {
        private static Dictionary<string, int> wrExtendedJointDict = new Dictionary<string, int>
        {
            { "PELV", 0 },
            { "LHIP", 1 },
            { "LKNEE", 2 },
            { "LANKLE", 3 },
            { "RHIP", 4 },
            { "RKNEE", 5 },
            { "RANKLE", 6 },
            { "SPINE0", 7 },
            { "LSHOULDER", 8 },
            { "LELBOW", 9 },
            { "LWRIST", 10 },
            { "RSHOULDER", 11 },
            { "RELBOW", 12 },
            { "RWRIST", 13 },
            { "HEAD", 14 },
            { "LTOE", 15 },
            { "RTOE", 16 },
            { "LCOLLAR", 17 },
            { "RCOLLAR", 18 },
            { "NECK", 19 },
            { "SPINE1", 20 },
            { "SPINE2", 21 },
            { "LHIP_ROLL", 22 },
            { "LKNEE_ROLL", 23 },
            { "RHIP_ROLL", 24 },
            { "RKNEE_ROLL", 25 },
            { "LSHOULDER_ROLL", 26 },
            { "LELBOW_ROLL", 27 },
            { "RSHOULDER_ROLL", 28 },
            { "RELBOW_ROLL", 29 }
        };

        public static string GetNameById(int id) { return wrExtendedJointDict.FirstOrDefault(x => x.Value == id).Key; }
        public static int GetIdByName(string name) { return wrExtendedJointDict[name]; }
        public static string[] ToStringArray() { return wrExtendedJointDict.Keys.ToArray(); }
        public static int GetNumJoints() { return wrExtendedJointDict.Count; }
    };

    /// <summary>
    /// Conversion structure for wrJ20 to wrExtended joints for partial matching of 2d/3d skeletons.
    /// </summary>
    public struct j23ToExtendedJoints
    {
        public static List<wrExtendedJoints> translator = new List<wrExtendedJoints>
        {
            wrExtendedJoints.RANKLE,
            wrExtendedJoints.RKNEE,
            wrExtendedJoints.RHIP,
            wrExtendedJoints.LHIP,
            wrExtendedJoints.LKNEE,
            wrExtendedJoints.LANKLE,
            wrExtendedJoints.PELV,
            wrExtendedJoints.SPINE2,
            wrExtendedJoints.NECK,
            wrExtendedJoints.HEAD,
            wrExtendedJoints.RWRIST,
            wrExtendedJoints.RELBOW,
            wrExtendedJoints.RSHOULDER,
            wrExtendedJoints.LSHOULDER,
            wrExtendedJoints.LELBOW,
            wrExtendedJoints.LWRIST,
            wrExtendedJoints.NONE, //NOSE
            wrExtendedJoints.NONE, //REYE
            wrExtendedJoints.NONE, //REAR
            wrExtendedJoints.NONE, //LEYE
            wrExtendedJoints.NONE, //LEAR
            wrExtendedJoints.RTOE,
            wrExtendedJoints.LTOE
        };
    }
}