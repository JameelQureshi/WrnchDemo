using UnityEngine;
using UnityEditor;

public class BuildPlayeriOS : MonoBehaviour
{
    [MenuItem("Build/Build iOS")]
    public static void Build()
    {
        try
        {
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.scenes = new[] { "Assets/Plugins/Demo/MobileScene.unity" };
            buildPlayerOptions.locationPathName = "iOSBuild";
            buildPlayerOptions.target = BuildTarget.iOS;
            buildPlayerOptions.options = BuildOptions.None;
            PlayerSettings.iOS.targetOSVersionString = "12.0";
            PlayerSettings.iOS.appleEnableAutomaticSigning = false;
            Debug.Log(BuildPipeline.BuildPlayer(buildPlayerOptions));
            PlayerSettings.SetArchitecture(BuildTargetGroup.iOS, 1);
            // Magic string to communicate to Teamcity that the build is successful.
            Debug.Log("Successful build ~0xDEADBEEF");
        }
        catch (System.Exception e)
        {
            Debug.Log("##teamcity[message text='" + e.Message + "'" + "status='ERROR']");
            throw;
        }
    }
}