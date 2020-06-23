using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
public class UnityController : MonoBehaviour
{
  
    public void UnloadUnity() {

        #if UNITY_IOS
            NativeLoadAPI.UnloadUnity();
        #endif

    }

}

