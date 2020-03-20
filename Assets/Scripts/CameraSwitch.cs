using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace wrnchAI.Core
{
    public class CameraSwitch : MonoBehaviour
    {
        public static string message;
        public Text deviceName;

        WebCamTexture webCamTexture;
        WebCamDevice[] devices;

        public PoseManager poseManager;
        // Start is called before the first frame update
        void Start()
        {
            devices = WebCamTexture.devices;
            foreach (WebCamDevice d in devices)
            {
                Debug.Log(d.name);
                deviceName.text = deviceName.text + d.name;
            }


            //Debug.Log( poseManager.VideoController.Config.DeviceName);
        }

        // Update is called once per frame
        void Update()
        {

        }

        public static void DebugLog(string message)
        {
            Debug.Log(message);
        }
    }
}
