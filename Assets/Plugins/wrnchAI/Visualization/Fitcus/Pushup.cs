using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using wrnchAI.Core;
using wrnchAI.Visualization;

public class Pushup : MonoBehaviour
{
    public static Pushup instance;
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
    The class used to track a normal Pushup

    Attributes
    ----------
    depth1 : Int
        How deep should the user go in the pushup for a rep to be counted.
        Value represents the angle of elbow.
    depth2 : Int
        How high should the user rise out of the pushup for a rep to be counted.
        Value represents the angle of elbow.
    elbowAngleCutoff: Int
        If the elbow angle doesnt get below this point, the coach is triggered.
    */



    public List<float> elbow_angle_list = new List<float>();
    public List<float> slope_torso_list = new List<float>();
    public List<float> slope_thigh_list = new List<float>();


    public List<float> elbow_angles_of_current_rep = new List<float>();
    public List<float> slope_torso_of_current_rep = new List<float>();
    public List<float> slope_thigh_of_current_rep = new List<float>();

    public float depth1 = 100.0f;
    public float depth2 = 130.0f;
    public float elbowAngleCutoff = 110.0f;

    //Used for rep counting
    public bool trackingBegan = false;
    public bool thresholdReached = false;

    public float frame_no = 0;
    public float reps = 0;



   /*
        Analyses pushup mechanics given a single frame of joints.

        This function will trigger count reps functionality and detect correct movement patterns.

        Parameters
        ----------
        frame : list
            A list of 25 joints that have been extracted from a video frame.
            Joints are (x, y, z) coordinates

        debug : bool
            Optional parameter to print extra information.
        */

    public void AnalyseFrame(JointData[] frame)
    {
        Vector3 r_wrist = frame[PoseManager.Instance.JointDefinition2D.GetJointIndex("RWRIST")].jointposition;
        Vector3 r_elbow = frame[PoseManager.Instance.JointDefinition2D.GetJointIndex("RELBOW")].jointposition;
        Vector3 r_shoulder = frame[PoseManager.Instance.JointDefinition2D.GetJointIndex("RSHOULDER")].jointposition;
        Vector3 r_hip = frame[PoseManager.Instance.JointDefinition2D.GetJointIndex("RHIP")].jointposition;
        Vector3 r_knee = frame[PoseManager.Instance.JointDefinition2D.GetJointIndex("RKNEE")].jointposition;

        Vector3 l_wrist = frame[PoseManager.Instance.JointDefinition2D.GetJointIndex("LWRIST")].jointposition;
        Vector3 l_elbow = frame[PoseManager.Instance.JointDefinition2D.GetJointIndex("LELBOW")].jointposition;
        Vector3 l_shoulder = frame[PoseManager.Instance.JointDefinition2D.GetJointIndex("LSHOULDER")].jointposition;
        Vector3 l_hip = frame[PoseManager.Instance.JointDefinition2D.GetJointIndex("LHIP")].jointposition;
        Vector3 l_knee = frame[PoseManager.Instance.JointDefinition2D.GetJointIndex("LKNEE")].jointposition;


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
        if (datapoint > threshold1 && trackingBegan == false){
            //Possible improvement: Is last 5 points linear and pointing down?
            //Tracking has began
            trackingBegan = true;
            return false;
        }

        else if(datapoint < threshold1 && trackingBegan && thresholdReached == false){
            //Threshold is reach
            thresholdReached = true;
            return false;
        }
       
        else if(datapoint > threshold2 && trackingBegan & thresholdReached)
        {
            //Rep is counted now that threshold has returned above threshold2
            trackingBegan = false;
            thresholdReached = false;
            return true;
        }

        else
        {
            return false;
        }

    }
}