﻿/*
 Copyright (c) 2019 Wrnch Inc.
 All rights reserved
*/

#if UNITY_IOS
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using UnityEditor.iOS.Xcode.Extensions;
using System.IO;

namespace wrnchAI.Editor
{
    public class wrPostBuildProcessor : MonoBehaviour
    {

        private const string NSCameraUsageDescription = "NSCameraUsageDescription";
        private const string ITSAppUsesNonExemptEncryption = "ITSAppUsesNonExemptEncryption";

        [PostProcessBuild]
        public static void OnPostprocessBuild(BuildTarget buildTarget, string path)
        {
            if (buildTarget == BuildTarget.iOS)
            {

                string projPath = PBXProject.GetPBXProjectPath(path);

                PBXProject proj = new PBXProject();
                proj.ReadFromString(File.ReadAllText(projPath));

                string target = proj.GetUnityMainTargetGuid();

                // Add frameworks that we need
                proj.AddFrameworkToProject(target, "CoreML.framework", true);
                proj.AddFrameworkToProject(target, "Accelerate.framework", true);

                // Disable bitcode
                proj.SetBuildProperty(target, "ENABLE_BITCODE", "NO");

                File.WriteAllText(projPath, proj.WriteToString());

                // Add plist entries
                string plistPath = path + "/Info.plist";
                PlistDocument plist = new PlistDocument();
                plist.ReadFromString(File.ReadAllText(plistPath));

                // Allow camera access
                plist.root.values[NSCameraUsageDescription] = new PlistElementString("Camera is required");
                plist.root.values[ITSAppUsesNonExemptEncryption] = new PlistElementBoolean(false);

                plist.WriteToFile(plistPath);

                AddFramework("wrnch.framework", proj, target, projPath);
                AddFramework("opencv2.framework", proj, target, projPath);
            }
        }

        private static void AddFramework(string frameworkName, PBXProject project, string target, string projectPath)
        {
            string framework = Path.Combine("Plugins/wrnchAI/bin", frameworkName);
            string fileGuid = project.AddFile(framework, "Frameworks/" + framework, PBXSourceTree.Sdk);
            PBXProjectExtensions.AddFileToEmbedFrameworks(project, target, fileGuid);
            project.SetBuildProperty(target, "LD_RUNPATH_SEARCH_PATHS", "$(inherited) @executable_path/Frameworks");
            project.WriteToFile(projectPath);
        }
    }
}

#endif