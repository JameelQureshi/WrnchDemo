using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using UnityEngine;

public class PostBuild : MonoBehaviour
{
    [PostProcessBuild(999)]
    public static void OnPostProcessBuild(BuildTarget buildTarget, string path)
    {
        if (buildTarget == BuildTarget.iOS)
        {
            string projectPath = path + "/Unity-iPhone.xcodeproj/project.pbxproj";

            PBXProject pbxProject = new PBXProject();
            pbxProject.ReadFromFile(projectPath);

            string target = pbxProject.GetUnityMainTargetGuid();
            pbxProject.SetBuildProperty(target, "ENABLE_BITCODE", "NO");
            pbxProject.AddBuildProperty(target, "CLANG_ENABLE_MODULES", "YES");
            pbxProject.WriteToFile(projectPath);

            PBXProject pbxProject1 = new PBXProject();
            pbxProject1.ReadFromFile(projectPath);

            string target1 = pbxProject1.GetUnityFrameworkTargetGuid();
            pbxProject1.SetBuildProperty(target1, "ENABLE_BITCODE", "NO");
            pbxProject1.AddBuildProperty(target1, "CLANG_ENABLE_MODULES", "YES");
            pbxProject1.WriteToFile(projectPath);
        }
    }
}
