using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
public class UnityController : MonoBehaviour
{
   

    public void UnloadUnity() {
        Debug.Log("Unload Unity");
        SceneManager.LoadScene(0);
//#if UNITY_IOS
//        NativeLoadAPI.UnloadUnity();
//#endif

    }



    public void SwitchToNative() {
#if UNITY_IOS
        NativeLoadAPI.SwitchToNativeWindow(ColorUtility.ToHtmlStringRGB(Color.red));
#endif
    }
}

