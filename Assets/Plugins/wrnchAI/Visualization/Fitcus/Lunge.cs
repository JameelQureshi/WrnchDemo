using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using wrnchAI.Core;
using wrnchAI.Visualization;

public class Lunge : MonoBehaviour
{

    public static Lunge instance;
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
    /* 
    The class used to track a normal Lunge

    Attributes
    ----------
    depth1 : Int
        How deep should the user lunge for a rep to be counted.
        Value represents the angle of knee(assuming a straight shin).
    depth2 : Int
        How high should the user rise out of the lunge for a rep to be counted.
        Value represents the angle of knee(assuming a straight shin).
    kneeAngleCutoff: Int
        If the knee angle doesnt get below this point, the coach is triggered.
        Knee angle is measured between the thigh and the verticle plane(or straight shin)
    torsoAngleCutoff: Int
        How low of an angle can the torso get before triggering the coach.
        Torso angle is measured between torso and the verticle plane */

    public List<float> knee_angle_list = new List<float>();
    public List<float> knee_angle_list1 = new List<float>();
    public List<float> torso_angle_list = new List<float>();
    public List<float> l_knee_list = new List<float>();
    public List<float> r_knee_list = new List<float>();
    public List<float> rep_list = new List<float>();


    //def __init__(self, depth1= 120, depth2= 140, kneeAngleCutoff= 120, torsoAngleCutoff= 130):

    public List<float> torso_angles_of_current_rep = new List<float>();
    public List<float> l_knee_y_coord_of_current_rep = new List<float>();
    public List<float> r_knee_y_coord_of_current_rep = new List<float>();
    public List<float> l_knee_angles_of_current_rep = new List<float>();
    public List<float> r_knee_angles_of_current_rep = new List<float>();
    public List<float> user_rotations_of_current_rep = new List<float>();


    public float depth1 = 129;
    public float depth2 = 140f;
    public float kneeAngleCutoff = 120f;
    public float torsoAngleCutoff = 130f;

    /// Used for rep counting
    public bool trackingBegan = false;
    public bool thresholdReached = false;

    public int frame_no = 0;
    public int reps = 0;

    public CoachingOneEuroFilter one_euro_filter_right ;
    public CoachingOneEuroFilter one_euro_filter_left ;
    public CoachingOneEuroFilter one_euro_filter_torso ;


    /*
        Analyses lunge mechanics given a single frame of joints.

        This function will trigger count reps functionality and detect correct movement patterns.

        Parameters
        ----------
        frame: list
           A list of 25 joints that have been extracted from a video frame.
           Joints are(x, y, z) coordinates */


    public void AnalyseFrame(JointData[] frame)
    {
        Vector3 r_heel = frame[PoseManager.Instance.JointDefinition2D.GetJointIndex("RHEEL")].jointposition;
        Vector3 r_ankle = frame[PoseManager.Instance.JointDefinition2D.GetJointIndex("RANKLE")].jointposition;
        Vector3 r_knee = frame[PoseManager.Instance.JointDefinition2D.GetJointIndex("RKNEE")].jointposition;
        Vector3 r_hip = frame[PoseManager.Instance.JointDefinition2D.GetJointIndex("RHIP")].jointposition;
        Vector3 r_shoulder = frame[PoseManager.Instance.JointDefinition2D.GetJointIndex("RSHOULDER")].jointposition;

        Vector3 l_heel = frame[PoseManager.Instance.JointDefinition2D.GetJointIndex("LHEEL")].jointposition;
        Vector3 l_ankle = frame[PoseManager.Instance.JointDefinition2D.GetJointIndex("LANKLE")].jointposition;
        Vector3 l_knee = frame[PoseManager.Instance.JointDefinition2D.GetJointIndex("LKNEE")].jointposition;
        Vector3 l_hip = frame[PoseManager.Instance.JointDefinition2D.GetJointIndex("LHIP")].jointposition;
        Vector3 l_shoulder = frame[PoseManager.Instance.JointDefinition2D.GetJointIndex("LSHOULDER")].jointposition;

        float torso_angle  = MathHelper.instance.GetTorsoAngleWithStraightLeg(r_shoulder, r_hip, r_ankle);
        float r_knee_angle = MathHelper.instance.GetAngle(r_hip, r_knee, r_ankle);
        float l_knee_angle = MathHelper.instance.GetAngle(l_hip, l_knee, l_ankle);
        float userRotation = MathHelper.instance.GetUserOrientation(r_hip, l_hip);

        float knee_angle;


        if (frame_no == 0)
        {   
            one_euro_filter_right = new CoachingOneEuroFilter(frame_no, r_knee_angle);
            one_euro_filter_left = new CoachingOneEuroFilter(frame_no, l_knee_angle);
            one_euro_filter_torso = new CoachingOneEuroFilter(frame_no, torso_angle);

            if (r_knee_angle < l_knee_angle)
            {
                knee_angle = r_knee_angle;
            }

            else
            {
                knee_angle = l_knee_angle;
            }

        }

        else
        {
            r_knee_angle = one_euro_filter_right.ApplyFilter(frame_no, r_knee_angle);
            l_knee_angle = one_euro_filter_left.ApplyFilter(frame_no, l_knee_angle);
            torso_angle = one_euro_filter_torso.ApplyFilter(frame_no, torso_angle);

            if (r_knee_angle < l_knee_angle)
            {
                knee_angle = r_knee_angle;
            }

            else
            {
                knee_angle = l_knee_angle;
            }

        }

        torso_angles_of_current_rep.Add(torso_angle);
        l_knee_angles_of_current_rep.Add(l_knee_angle);
        r_knee_angles_of_current_rep.Add(r_knee_angle);
        l_knee_y_coord_of_current_rep.Add(l_knee.y);
        r_knee_y_coord_of_current_rep.Add(r_knee.y);
        user_rotations_of_current_rep.Add(userRotation);

        if (RepCounter(knee_angle, depth1, depth2))
        {

            Debug.Log("Rep on frame: " + frame_no);
            rep_list.Add(frame_no);
            reps += 1;
            Debug.Log("Rep " + reps);

            if( r_knee_angles_of_current_rep.Min() > kneeAngleCutoff && l_knee_angles_of_current_rep.Min() > kneeAngleCutoff)
            {
                Debug.Log(r_knee_angles_of_current_rep.Min() + l_knee_angles_of_current_rep.Min());
                Debug.Log("Sound on: Try to get a bit lower!");
            }

            float avgUserRoation = MathHelper.instance.GetPositiveMean(user_rotations_of_current_rep);



            if (Mathf.Abs(avgUserRoation) <= 45) {
                // If the user is facing the screen, check feet
                Feetdata feetdata = MathHelper.instance.FeetAreHipWidth(r_hip, l_hip, r_heel, l_heel);

                if (feetdata.state)
                {
                    Debug.Log("Feet are correct");
                }
                else
                {
                    Debug.Log("Sound On: Make sure your feet are hip width apart");
                }

            }

            if ( torso_angles_of_current_rep.Min() <  torsoAngleCutoff)
            {
                Debug.Log("Sound on: Keep your chest up! on frame: ");
            }

            else
            {
                Debug.Log("Torso is correct");
            }



            //// Empt All the list for new data
            for (int i = 0; i < torso_angles_of_current_rep.Count; i++)
            {
                torso_angles_of_current_rep.RemoveAt(i);
            }
            for (int i = 0; i < l_knee_y_coord_of_current_rep.Count; i++)
            {
                l_knee_y_coord_of_current_rep.RemoveAt(i);
            }
            for (int i = 0; i < r_knee_y_coord_of_current_rep.Count; i++)
            {
                r_knee_y_coord_of_current_rep.RemoveAt(i);
            }
            for (int i = 0; i < l_knee_angles_of_current_rep.Count; i++)
            {
                l_knee_angles_of_current_rep.RemoveAt(i);
            }
            for (int i = 0; i < r_knee_angles_of_current_rep.Count; i++)
            {
                r_knee_angles_of_current_rep.RemoveAt(i);
            }


        }

       frame_no += 1;
    }



    /*
        Counts reps based on the datapoint and 2 thresholds.

        The datapoint must hit both thresholds to count the rep.

        Parameters
        ----------
        datapoint : Int
            A list of 25 joints that have been extracted from a video frame.
            Joints are (x, y, z) coordinates

        threshold1 : Int
            First point the datapoint must hit.

        threshold2 : Int
            Second point the datapoint must hit.
        */

    public bool RepCounter(float datapoint, float threshold1, float threshold2)
    {
        // This rep counter waits for the user to get below the threshold, then above again
        if (datapoint > threshold1 && trackingBegan == false)
        { 
            // Possible improvement: Is last 5 points linear and pointing down?

            //Tracking has began
            trackingBegan = true;
            return false; 
        }

        else if(datapoint < threshold1 && trackingBegan && thresholdReached == false)
        {
            // Threshold is reach
            thresholdReached = true;
            return false;
        }

        else if(datapoint > threshold2 && trackingBegan & thresholdReached)
        {
            // Rep is counted now that threshold has returned above threshold2
            trackingBegan = false;
            thresholdReached = false;
            return true;
        }

        else
        {
            return false;
        }
       
    }
        
    // Start is called before the first frame update
        void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
