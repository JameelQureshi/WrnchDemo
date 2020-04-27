using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using wrnchAI.Core;
using wrnchAI.wrAPI;
namespace wrnchAI.Visualization
{

    public class JointDataManager : MonoBehaviour
    {

        public static JointDataManager instance;
        /// <summary>
        /// JointData Use to store Joint Data for individaul joint; name and position
        /// </summary>
        [SerializeField]
        public JointData[] jointData;
        public JointData[] jointData2D;

        public Text totalRawPose3DPointsText;


        public Text All2DJointsValues;
        public Text All3DRawValues;

        // Person Recived from Skeleton for calculations
        public Person person;

        // Raw Point from RawPose3D
        public float[] positions;
        public float[] positions2D;


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
        private static readonly List<BonePair> bonePairs = new List<BonePair>
        {

            new BonePair("RANKLE","RKNEE"),
            new BonePair("RKNEE", "RHIP"),
            new BonePair("LANKLE", "LKNEE"),
            new BonePair("LKNEE", "LHIP"),
            new BonePair("RHIP", "PELV"),
            new BonePair("LHIP", "PELV"),
            new BonePair("PELV", "NECK"),
            new BonePair("RSHOULDER", "LSHOULDER"),
            new BonePair("LSHOULDER", "LELBOW"),
            new BonePair("LELBOW", "LWRIST"),
            new BonePair("RSHOULDER", "RELBOW"),
            new BonePair("RELBOW", "RWRIST"),
            new BonePair("LANKLE", "LTOE"),
            new BonePair("RANKLE", "RTOE"),
            new BonePair("LANKLE", "LHEEL"),
            new BonePair("RANKLE", "RHEEL")
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

            //All3DRawValues.text = "";
            //for (int i=0;i<15;i++)
            //{
            //    All3DRawValues.text = All3DRawValues.text+" " + positions[i];
            //}

            //All2DJointsValues.text = "";

            //if (person!=null)
            //{
            //    positions2D = person.Pose2d.Joints;
            //    for (int i = 0; i < 10; i++)
            //    {
            //        All2DJointsValues.text = All2DJointsValues.text + " " + positions2D[i];
            //    }

            //}

        }

      

        private void Update()
        {
            if (person.RawPose3D != null)
            {
                UpdateJointData();
                Squat.instance.AnalyseFrame(jointData);
            }

            totalRawPose3DPointsText.text = "Squats: " + Squat.instance.reps;
            ShowJointdata();
        }


        private void UpdateJointData()
        {

            positions = person.RawPose3D.Positions;
            positions2D = person.Pose2d.Joints;

            int positionIndexX = 0;
            int positionIndexY = 1;
            int positionIndexZ = 2;

            for (int i = 0 ; i<25 ; i++)
            {
                /// Fill name of the joint
                jointData[i].jointname = m_jointsToExtract[i];
                jointData2D[i].jointname = m_jointsToExtract[i];

                //fill joint positions
                jointData[i].jointposition = new Vector3(positions[positionIndexX], positions[positionIndexY], positions[positionIndexZ]);
                jointData2D[i].jointposition = new Vector3(positions2D[positionIndexX], positions2D[positionIndexY], 0);
                positionIndexX = positionIndexX + 3;
                positionIndexY = positionIndexY + 3;
                positionIndexZ = positionIndexZ + 3;

                //fill index of the joint
                jointData[i].index =i;
                jointData2D[i].index = i;
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

    public class BonePair
    {
        public string bone1;
        public string bone2;

        public BonePair(string b1, string b2)
        {
            bone1 = b1;
            bone2 = b2;
        }
    }
}