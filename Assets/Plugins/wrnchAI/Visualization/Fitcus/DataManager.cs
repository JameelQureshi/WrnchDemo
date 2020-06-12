using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using wrnchAI.Core;
using wrnchAI.wrAPI;

    public class DataManager : MonoBehaviour
    {

        public static DataManager instance;
        public static Exercise currentExercise;

        /// <summary>
        /// JointData Use to store Joint Data for individaul joint; name and position
        /// </summary>
        [SerializeField]
        public JointData[] jointData;
        public JointData[] jointData2D;

       

        public Text totalRepsText;


        public Text All2DJointsValues;
        public Text All3DRawValues;

        // Person Recived from Skeleton for calculations
        public Person person;

        // Raw Point from RawPose3D
        public float[] positions;
        public float[] positions2D;

        [Header("Silhouettes Images")]
        public Image silhouetteImage;
        public Sprite[] silhouette;

        [Header("Avatar References")]
        public GameObject rootAvatar;
        public GameObject[] avatar;

        public bool canDoCoaching = false;
        Coaching coaching;

        /// <summary>
        /// The name of joints to extract from raw position.
        /// </summary>
        private static readonly List<string> m_jointsToExtract = new List<string> {
            "RANKLE", //0
            "RKNEE", //1
            "RHIP", //2
            "LHIP", //3
            "LKNEE", //4
            "LANKLE",  //5
            "PELV",  //6
            "THRX", //7
            "NECK", //8
            "HEAD", //9
            "RWRIST", //10
            "RELBOW", //11
            "RSHOULDER", //12 
            "LSHOULDER", //13
            "LELBOW", //14
            "LWRIST", //15
            "NOSE", //16
            "REYE", // 17
            "REAR", //18
            "LEYE", //19
            "LEAR", //20
            "RTOE", //21
            "LTOE", //22
            "RHEEL", //23
            "LHEEL" //24
            };
            

    public void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(gameObject);
            }

        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }

    public void Init()
    {
        string exerciseName = PlayerPrefs.GetString("ExerciseName");

       
        switch (exerciseName) {
            case "Squat":
                currentExercise = Exercise.Squat;
                break;
            case "Lunge":
                currentExercise = Exercise.Lunge;
                break;
            case "Pushup":
                currentExercise = Exercise.Pushup;
                break;
        }


        // select data according to current Exercise
        GameObject avatarRef;
        switch (currentExercise)
        {
            case Exercise.Squat:
                silhouetteImage.sprite = silhouette[0];
                avatarRef = Instantiate(avatar[0],rootAvatar.transform);
                avatarRef.transform.localPosition = Vector3.zero;
                coaching = new Squat();
                break;
            case Exercise.Lunge:
                silhouetteImage.sprite = silhouette[1];
                avatarRef = Instantiate(avatar[1], rootAvatar.transform);
                avatarRef.transform.localPosition = Vector3.zero;
                coaching = new Lunge();
                break;
            case Exercise.Pushup:
                silhouetteImage.sprite = silhouette[2];
                avatarRef = Instantiate(avatar[2], rootAvatar.transform);
                avatarRef.transform.localPosition = Vector3.zero;
                coaching = new Pushup();
                break;
        }

    }


    public void GreatWorkTodayWeWillLearn()
    {
        float length = VoiceManager.instance.PlayInstructionSound(17, true); // index of GreatWorkTodayWeWillLearn sound 17
        StartCoroutine(PlaySoundOK(length));
    }

    IEnumerator PlaySoundOK(float delay)
    {
        yield return new WaitForSeconds(delay + 1);
        float length = VoiceManager.instance.PlayInstructionSound(11,true); // index of ok sound 11   
        StartCoroutine(PlaySoundAreYouReady(length));
    }

    IEnumerator PlaySoundAreYouReady(float delay)
    {
        yield return new WaitForSeconds(delay+1);
        float length = VoiceManager.instance.PlayInstructionSound(15, true); // index of are you ready sound 15   
        StartCoroutine(PlaySoundThreeTwoOne(length));
    }

    IEnumerator PlaySoundThreeTwoOne(float delay)
    {
        yield return new WaitForSeconds(delay+1);
        float length = VoiceManager.instance.PlayInstructionSound(16, true); // index of three two one sound 16   
        StartCoroutine(PlaySoundLetsGo(length));
    }
    IEnumerator PlaySoundLetsGo(float delay)
    {
        yield return new WaitForSeconds(delay+1);
        float length = VoiceManager.instance.PlayInstructionSound(4, true); // index of three two one sound 4   
        StartCoroutine(StartCoaching(length));
    }
    IEnumerator StartCoaching(float delay)
    {
        yield return new WaitForSeconds(delay);
        canDoCoaching = true;
    }

    private IEnumerator ShowJointdata()
        {
        yield return (new WaitForEndOfFrame());

        All2DJointsValues.text = "";

           
                
            All2DJointsValues.text = All2DJointsValues.text + " PELV-X: " + jointData2D[6].jointposition.x + "\n";
            All2DJointsValues.text = All2DJointsValues.text + " PELV-Y: " + jointData2D[6].jointposition.y + "\n";
            All2DJointsValues.text = All2DJointsValues.text + " NECK-X: " + jointData2D[8].jointposition.x + "\n";
            All2DJointsValues.text = All2DJointsValues.text + " NECK-Y: " + jointData2D[8].jointposition.y + "\n";
            All2DJointsValues.text = All2DJointsValues.text + " RSHOULDER-X: " + jointData2D[12].jointposition.x + "\n";
            All2DJointsValues.text = All2DJointsValues.text + " RSHOULDER-Y: " + jointData2D[12].jointposition.y + "\n";
            All2DJointsValues.text = All2DJointsValues.text + " LSHOULDER-X: " + jointData2D[13].jointposition.x + "\n";
            All2DJointsValues.text = All2DJointsValues.text + " LSHOULDER-Y: " + jointData2D[13].jointposition.y + "\n";

        All2DJointsValues.text = All2DJointsValues.text + " RANKLE-X: " + jointData2D[0].jointposition.x + "\n";
        All2DJointsValues.text = All2DJointsValues.text + " RANKLE-Y: " + jointData2D[0].jointposition.y + "\n";
        All2DJointsValues.text = All2DJointsValues.text + " LANKLE-X: " + jointData2D[5].jointposition.x + "\n";
        All2DJointsValues.text = All2DJointsValues.text + " LANKLE-Y: " + jointData2D[5].jointposition.y + "\n";

        All2DJointsValues.text = All2DJointsValues.text + " RKNEE-X: " + jointData2D[1].jointposition.x + "\n";
        All2DJointsValues.text = All2DJointsValues.text + " RKNEE-Y: " + jointData2D[1].jointposition.y + "\n";
        All2DJointsValues.text = All2DJointsValues.text + " LKNEE-X: " + jointData2D[4].jointposition.x + "\n";
        All2DJointsValues.text = All2DJointsValues.text + " LKNEE-Y: " + jointData2D[4].jointposition.y + "\n";

        }


      
        void Update()
        {
            for (int i = 0; i < positions2D.Length; i++)
            {
                positions2D[i] = -0.2f;
            }

            for (int i = 0; i < positions.Length; i++)
            {
                positions[i] = -0.2f;
            }



            UpdateJointData();
            UpdateJointData2D();


            if (canDoCoaching)
            {
                coaching.AnalyseFrame(jointData);
            }

            totalRepsText.text = "" + Coaching.reps;
            StartCoroutine(ShowJointdata());
            
        }


    private void UpdateJointData()
        {
            if (person!=null)
            {
                if (person.RawPose3D != null)
                {
                    positions = person.RawPose3D.Positions;
                }
            }



            int positionIndexX = 0;
            int positionIndexY = 1;
            int positionIndexZ = 2;

            // print("-------------- Positions Length 3D: " + positions);

            // Debug.Log("-------------- Positions Length 3D: " + System.String.Join("", new List<float>(positions).ConvertAll(i => i.ToString()).ToArray()));

            for (int i = 0 ; i<25 ; i++)
            {
                /// Fill name of the joint
                jointData[i].jointname = m_jointsToExtract[i];


                //fill joint positions
                jointData[i].jointposition = new Vector3(positions[positionIndexX], positions[positionIndexY], positions[positionIndexZ]);

                positionIndexX = positionIndexX + 3;
                positionIndexY = positionIndexY + 3;
                positionIndexZ = positionIndexZ + 3;

                //fill index of the joint
                jointData[i].index =i;

            }
           
        }

        private void UpdateJointData2D()
        {


            if (person != null)
            {
                positions2D = person.Pose2d.Joints;
            }
            

            int positionIndexX = 0;
            int positionIndexY = 1;

            // print("-------------- Positions Length 2D: " + positions2D);
            // Debug.Log("-------------- Positions Length 2D: " + System.String.Join("", new List<float>(positions2D).ConvertAll(i => i.ToString()).ToArray()));



            for (int i = 0; i < 25; i++)
            {
                /// Fill name of the joint
                jointData2D[i].jointname = m_jointsToExtract[i];

                //fill joint positions
                jointData2D[i].jointposition = new Vector3(positions2D[positionIndexX], positions2D[positionIndexY], 0);
                positionIndexX = positionIndexX + 2;
                positionIndexY = positionIndexY + 2;


                //fill index of the joint
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

public enum Exercise
{
    Squat,
    Lunge,
    Pushup
}