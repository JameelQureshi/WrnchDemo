using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using wrnchAI.wrAPI;
namespace wrnchAI.Visualization
{

    public class JointDataDisplay : MonoBehaviour
    {

        public static JointDataDisplay instance;
        /// <summary>
        /// JointData Use to store Joint Data for individaul joint; name and position
        /// </summary>
        [SerializeField]
        public JointData[] jointData;

        public Text positionsText;

        public Text totalRawPose3DPointsText;

        // Person Recived from Skeleton for calculations
        public Person person;

        // Raw Point from RawPose3D
        public float[] positions;


        /// <summary>
        /// The name of joints to extract from raw position.
        /// </summary>
        private static readonly List<string> m_jointsToExtract = new List<string> {
            "RANKLE",
            "RKNEE",
            "RHIP",
            "LHIP",
            "LKNEE",
            "LANKLE",
            "PELV",
            "THRX",
            "NECK",
            "HEAD",
            "RWRIST",
            "RELBOW",
            "RSHOULDER",
            "LSHOULDER",
            "LELBOW",
            "LWRIST",
            "NOSE",
            "REYE",
            "REAR",
            "LEYE",
            "LEAR",
            "RTOE",
            "LTOE",
            "RHEEL",
            "LHEEL"
            };

        public void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(this.gameObject);
            }
            else
            {
                Destroy(gameObject);
            }

        }
       

        public void ShowJointdata()
        {
            if (person.RawPose3D!=null)
            {
                totalRawPose3DPointsText.text = "" + person.RawPose3D.NumJoints;

                positions = person.RawPose3D.Positions;

                foreach (float f in positions)
                {
                    positionsText.text = positionsText.text + " " + f;
                }
            }



        }
    }

    [System.Serializable]
    public class JointData
    {
        public string jointname;
        public Vector3 jointposition;
        public int index;
    }
}