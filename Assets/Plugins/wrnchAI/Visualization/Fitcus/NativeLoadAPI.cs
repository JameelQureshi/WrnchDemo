using System.Runtime.InteropServices;
public class NativeLoadAPI
{
#if UNITY_IOS || UNITY_TVOS

        [DllImport("__Internal")]
        public static extern void SwitchToNativeWindow(string color);

        [DllImport("__Internal")]
        public static extern void UnloadUnity();

#endif
}
