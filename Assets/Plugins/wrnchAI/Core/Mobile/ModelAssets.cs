using UnityEngine;

public class ModelAssets : MonoBehaviour
{

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void OnBeforeSceneLoadRuntimeMethod()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            StreamingAssetsToStorage("wrandroid_pose2d_v1.enc");
            StreamingAssetsToStorage("wrandroid_pose3d_v1.enc");
        }
    }

    static void StreamingAssetsToStorage(string filename)
    {
        string modelAssetPath = System.IO.Path.Combine(Application.streamingAssetsPath, filename);

        // Android only use WWW to read file
        WWW reader = new WWW(modelAssetPath);
        while (!reader.isDone) { }

        string modelFilePath = Application.persistentDataPath + "/" + filename;
        System.IO.File.WriteAllBytes(modelFilePath, reader.bytes);
    }
}
