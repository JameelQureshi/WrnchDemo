using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using wrnchAI.Core;
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
                int jointIdx = PoseManager.Instance.JointDefinition2D.GetJointIndex("RKNEE");
                totalRawPose3DPointsText.text = totalRawPose3DPointsText.text + jointIdx;


                int index = 0;
                foreach (JointData jd in jointData)
                {
                    positionsText.text = positionsText.text + " " + jointData[index].jointname + ":"+ jointData[index].jointposition.x+","+ jointData[index].jointposition.y;
                    index++;
                }
            }

        }


        private void Update()
        {
            if (person.RawPose3D != null)
            {
                UpdateJointData();
            }
        }

        private void UpdateJointData()
        {

            positions = person.RawPose3D.Positions;

            int positionIndexX = 0;
            int positionIndexY = 1;
            int positionIndexZ = 2;

            for (int i = 0 ; i<25 ; i++)
            {
                jointData[i].jointname = m_jointsToExtract[i];
                jointData[i].jointposition = new Vector3(positions[positionIndexX], positions[positionIndexY], positions[positionIndexZ]);
                positionIndexX = positionIndexX + 3;
                positionIndexY = positionIndexY + 3;
                positionIndexZ = positionIndexZ + 3;
                jointData[i].index =i;
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