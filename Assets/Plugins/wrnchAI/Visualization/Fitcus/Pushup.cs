using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public List<float> hip_angles_of_current_rep = new List<float>(); 
    public List<float> slope_torso_of_current_rep = new List<float>();
    public List<float> slope_thigh_of_current_rep = new List<float>();

    private float depth1 = 130.0f;
    private float depth2 = 140.0f;
    private float elbowAngleCutoff = 120.0f;
    private string side = "RIGHT";

    //Used for rep counting
    public bool trackingBegan = false;
    public bool thresholdReached = false;

    public float frame_no = 0;
    public int reps = 0;
    public CoachingOneEuroFilter one_euro_filter_elbow ;
    public CoachingOneEuroFilter one_euro_filter_hip;


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

        bool r_arm_detected; 
        bool l_arm_detected;
        bool hips_knee_detected;
        float elbow_angle;
        float hip_angle;
        float slope_torso;
        float slope_thigh;

        if (r_wrist.x <= 0 || r_elbow.x <= 0 || r_shoulder.x <= 0 || r_knee.x <= 0 || r_hip.x <= 0)
        {
            r_arm_detected = false;
        } else {
            r_arm_detected = true;
        }

        if (l_wrist.x <= 0 || l_elbow.x <= 0 || l_shoulder.x <= 0 || l_knee.x <= 0 || l_hip.x <= 0)
        {
            l_arm_detected = false;
        } else {
            l_arm_detected = true;
        }


        // RIGHT SIDE OF BODY FACING CAMERA
        if (l_arm_detected && side == "RIGHT") {

            elbow_angle = MathHelper.instance.GetAngle(l_wrist, l_elbow, l_shoulder);

            hip_angle = Mathf.Abs(MathHelper.instance.GetAngle2D(l_shoulder, l_hip, l_knee));


            // if (hips_knee_detected) {
            //     slope_torso = MathHelper.instance.CalculateSlope2D(l_hip.x, l_hip.y, l_shoulder.x, l_shoulder.y);
            //     slope_thigh = MathHelper.instance.CalculateSlope2D(l_hip.x, l_hip.y, l_knee.x, l_knee.y);
            //     slope_thigh_of_current_rep.Add(slope_thigh);
            //     slope_torso_of_current_rep.Add(slope_torso);
            // }


        }
        // LEFT SIDE OF BODY FACING CAMERA
        else if (r_arm_detected && side == "LEFT") {

            elbow_angle = MathHelper.instance.GetAngle(r_wrist, r_elbow, r_shoulder);

            hip_angle =  Mathf.Abs(MathHelper.instance.GetAngle2D(r_shoulder, r_hip, r_knee));


            // if (hips_knee_detected) {
            //     slope_torso = MathHelper.instance.CalculateSlope2D(r_hip.x, r_hip.y, r_shoulder.x, r_shoulder.y);
            //     slope_thigh = MathHelper.instance.CalculateSlope2D(r_hip.x, r_hip.y, r_knee.x, r_knee.y);
            //     slope_thigh_of_current_rep.Add(slope_thigh);
            //     slope_torso_of_current_rep.Add(slope_torso);
            // }

        } else {
            return;
        }

        if (frame_no == 0) // Initialise Eurofilter
        {   
            // Reduce min_cutoff to 0.05 (from 0.1)
            one_euro_filter_elbow = new CoachingOneEuroFilter(frame_no, elbow_angle, 0.0f, 0.05f, 0.0f, 1.0f);
            one_euro_filter_hip = new CoachingOneEuroFilter(frame_no, hip_angle);
        }
        else
        {
            elbow_angle = one_euro_filter_elbow.ApplyFilter(frame_no, elbow_angle);
            hip_angle = one_euro_filter_hip.ApplyFilter(frame_no, hip_angle);

        }

        elbow_angles_of_current_rep.Add(elbow_angle);
        hip_angles_of_current_rep.Add(hip_angle);

        bool audioPlayed = false;
        
        print("----------- Elbow Angle: " + elbow_angle);
        print("----------- Hip Angle: " + hip_angle);

        if (RepCounter(elbow_angle, depth1, depth2))
        {
            reps += 1;

            print("--------- Reps on frame: " + frame_no);

            if (elbow_angles_of_current_rep.Min() > elbowAngleCutoff) {
                print("Sound on: Try to get a bit lower!");
                VoiceManager.instance.PlayInstructionSound(7);
                audioPlayed = true;

            }

            // Get the slope of the torso and thigh and check for similarity
            // The smaller the number the more similarity, which means more straight body
            // float sad = MathHelper.instance.SumOfAbsoluteDifferences(slope_torso_of_current_rep, slope_thigh_of_current_rep);
            // if (sad >= 40) {
            //     print("Sound on: Try to keep your back and thighs in a straight line");
            //     VoiceManager.instance.PlayInstructionSound(9);
            //     audioPlayed = true;
            // }

            if (hip_angles_of_current_rep.Min() < 140){
                print("Sound on: Try to keep your back and thighs in a straight line");
                VoiceManager.instance.PlayInstructionSound(9);
                audioPlayed = true;
            }

            // Play Audio Count
            if (!audioPlayed)
            {
                VoiceManager.instance.PlayCountingSound(reps - 1);
                audioPlayed = true;
            }

            elbow_angles_of_current_rep.Clear();
            hip_angles_of_current_rep.Clear();
            slope_thigh_of_current_rep.Clear();
            slope_torso_of_current_rep.Clear();
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