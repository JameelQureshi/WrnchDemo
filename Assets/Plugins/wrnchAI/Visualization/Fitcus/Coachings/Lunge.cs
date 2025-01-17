﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using wrnchAI.Core;
using wrnchAI.Visualization;

public class Lunge : Coaching
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

    //def __init__(self, depth1= 120, depth2= 140, kneeAngleCutoff= 120, torsoAngleCutoff= 130):

    public List<float> torso_angles_of_current_rep = new List<float>();
    // public List<float> l_knee_x_coord_of_current_rep = new List<float>();
    // public List<float> r_knee_x_coord_of_current_rep = new List<float>();
    // public List<float> pelvis_x_coord_of_current_rep = new List<float>();
    public List<float> l_knee_angles_of_current_rep = new List<float>();
    public List<float> r_knee_angles_of_current_rep = new List<float>();
    public List<float> user_rotations_of_current_rep = new List<float>();


    private float depth1 = 140;
    private float depth2 = 150;
    private float kneeAngleCutoff = 120f;
    private float torsoAngleCutoff = 130f;

    /// Used for rep counting
    public bool trackingBegan = false;
    public bool thresholdReached = false;

    public int frame_no = 0;

    public CoachingOneEuroFilter one_euro_filter_right ;
    public CoachingOneEuroFilter one_euro_filter_left ;
    public CoachingOneEuroFilter one_euro_filter_torso ;
    public CoachingOneEuroFilter one_euro_filter_r_heel ;
    public CoachingOneEuroFilter one_euro_filter_r_hip ;
    public CoachingOneEuroFilter one_euro_filter_l_heel ;
    public CoachingOneEuroFilter one_euro_filter_l_hip ;
    public CoachingOneEuroFilter one_euro_filter_userRotation;
    private bool stride_is_long = false;
    private int start_of_rep;
    private int end_of_rep;

    private bool feetAreShoulderWidth = false;
    private bool turnedToSide = false;
    private bool isfeetAreShoulderWidthInstructionRunning = false;
    private System.DateTime sideTimer1;
    private System.DateTime sideTimer2;
    private bool isTimerRunning = false;
    public int frame_no_stance_check = 0;
    public int frame_no_shoulder_check = 0;
    /*
        Analyses lunge mechanics given a single frame of joints.

        This function will trigger count reps functionality and detect correct movement patterns.

        Parameters
        ----------
        frame: list
           A list of 25 joints that have been extracted from a video frame.
           Joints are(x, y, z) coordinates */

    public override void AnalyseFrame( JointData[] frame)
    {
         if (feetAreShoulderWidth == false) {
             // Debug.Log("---------- FEET -------------");
            GuideFeetToShoulderWidth(frame);
            frame_no_stance_check += 1;
        } else if (feetAreShoulderWidth && turnedToSide == false) {
           // Debug.Log("---------- SHOULDER -------------");
            GuideBodyToSidePosition(frame);
            frame_no_shoulder_check += 1;
        } else {
            //Debug.Log("---------- COACHING -------------");
            DoCoaching(frame);
        }

    }

    public void GuideBodyToSidePosition( JointData[] frame) {
        
        Vector3 r_heel = frame[23].jointposition;
        Vector3 r_ankle = frame[0].jointposition;
        Vector3 r_knee = frame[1].jointposition;
        Vector3 r_hip = frame[2].jointposition;
        Vector3 r_shoulder = frame[12].jointposition;

        Vector3 l_heel = frame[24].jointposition;
        Vector3 l_ankle = frame[5].jointposition;
        Vector3 l_knee = frame[4].jointposition;
        Vector3 l_hip = frame[3].jointposition;
        Vector3 l_shoulder = frame[13].jointposition;

        int right = 0;
        int left = 0;

        float userRotation = MathHelper.instance.GetUserOrientation(r_shoulder, l_shoulder);

        if(sideTimer1 == null ) {
            // Init times
            sideTimer1 = System.DateTime.Now;
            sideTimer2 = System.DateTime.Now;
        }

        if (frame_no_shoulder_check == 0) {
            // Init Euro Filters(
            one_euro_filter_userRotation = new CoachingOneEuroFilter(frame_no_shoulder_check, userRotation);
                      
        } else {
            l_shoulder.x = one_euro_filter_userRotation.ApplyFilter(frame_no_shoulder_check, userRotation);  
        }

        if (!float.IsNaN(userRotation)) {

            if (r_heel.z > l_heel.z) { right += 1;  } else { left += 1; }
            if (r_ankle.z > l_ankle.z) { right += 1;  } else { left += 1; }
            if (r_knee.z > l_knee.z) { right += 1;  } else { left += 1; }
            if (r_hip.z > l_hip.z) { right += 1;  } else { left += 1; }
            if (r_shoulder.z > l_shoulder.z) { right += 1;  } else { left += 1; }


            if ( right < left && Mathf.Abs(userRotation) >= 60 )  {
                DataManager.currentSkeleton.ResetGlowValues();
                Debug.Log("--------------- Correct ---------- " + userRotation);
                double diffInSeconds = (sideTimer2 - sideTimer1).TotalSeconds;
                DataManager.currentSkeleton.SetBoneGlowValues(new int[] { 0,1,2,3,4,6,7,8,9,10,11 } , GlowColor.Green);
                if (diffInSeconds >= 3) {

                    VoiceManager.instance.PlayInstructionSound(14);  
                    turnedToSide = true;
                    DataManager.currentSkeleton.ResetGlowValues();
                    // Reset timers
                    sideTimer1 = System.DateTime.Now;
                    sideTimer2 = System.DateTime.Now;
                } else {
                    sideTimer2 = System.DateTime.Now;
                }
            } else {
                DataManager.currentSkeleton.SetBoneGlowValues(new int[] { 0,1,2,3,4,6,7,8,9,10,11 } , GlowColor.Red);
                // Reset timers
                sideTimer1 = System.DateTime.Now;
                sideTimer2 = System.DateTime.Now;
                Debug.Log("--------------- Incorrect ---------- " + userRotation);
            }
            
        } else {
            // DataManager.currentSkeleton.SetRedGlowValues(new int[] { 6 });
        }

    }

        public void GuideFeetToShoulderWidth( JointData[] frame) {

        Vector3 r_heel = frame[23].jointposition;
        Vector3 r_hip = frame[2].jointposition;
        Vector3 l_heel = frame[24].jointposition;
        Vector3 l_hip = frame[3].jointposition;

        if (frame_no_stance_check == 0) {
            // Init Euro Filters(
            one_euro_filter_r_heel = new CoachingOneEuroFilter(frame_no_stance_check, r_heel.x, 0.0f, 0.01f, 0.0f, 1.0f);
            one_euro_filter_r_hip = new CoachingOneEuroFilter(frame_no_stance_check, r_hip.x, 0.0f, 0.01f, 0.0f, 1.0f);
            one_euro_filter_l_heel = new CoachingOneEuroFilter(frame_no_stance_check, l_heel.x, 0.0f, 0.01f, 0.0f, 1.0f);
            one_euro_filter_l_hip = new CoachingOneEuroFilter(frame_no_stance_check, l_hip.x, 0.0f, 0.01f, 0.0f, 1.0f);
                      
        } else {
            r_heel.x = one_euro_filter_r_heel.ApplyFilter(frame_no_stance_check, r_heel.x);
            r_hip.x = one_euro_filter_r_hip.ApplyFilter(frame_no_stance_check, r_hip.x);
            l_heel.x = one_euro_filter_l_heel.ApplyFilter(frame_no_stance_check, l_heel.x);
            l_hip.x = one_euro_filter_l_hip.ApplyFilter(frame_no_stance_check, l_hip.x);  
        }

            

        Feetdata feetdata = MathHelper.instance.FeetAreShoulderWidth(r_hip, l_hip, r_heel, l_heel); // Actually Hip width
        // If feet are hip width
        if (feetdata.state)
        {
            if (!isTimerRunning)
            {
                StartCoroutine(ShoulderWidthCompleteTimer());
                Debug.Log("Timer Started");
                isTimerRunning = true;
            }
            //ShoulderWidthComplete();
            DataManager.currentSkeleton.SetBoneGlowValues(new int[] { 0,2 }, GlowColor.Green);
            // Debug.Log("--------------- FEET ARE SHOULDER WIDTH ---------");

        // If feet are NOT shoulder width
        } else 
        {

            isTimerRunning = false;
            StopAllCoroutines();
            Debug.Log("Timer Stoped");

            if (!isfeetAreShoulderWidthInstructionRunning)
            {
                // Please place your feet shoulder width apart
                VoiceManager.instance.PlayInstructionSound(12);
                isfeetAreShoulderWidthInstructionRunning = true;
                Invoke("MakeFeetAreShoulderWidthInstructionRunningInstructionFalse", 5);
            }


            if (feetdata.leftFoot != 1) {
                DataManager.currentSkeleton.SetBoneGlowValues(new int[] { 0 }, GlowColor.Red);
            } else {
                DataManager.currentSkeleton.SetBoneGlowValues(new int[] { 2 }, GlowColor.Green);
            }
            
            if (feetdata.rightFoot != 1) {
                DataManager.currentSkeleton.SetBoneGlowValues(new int[] { 2 }, GlowColor.Red);
            } else {
                DataManager.currentSkeleton.SetBoneGlowValues(new int[] { 2 }, GlowColor.Green);
            }
            

        }

    }
     void MakeFeetAreShoulderWidthInstructionRunningInstructionFalse()
    {
        isfeetAreShoulderWidthInstructionRunning = false;
    }

    IEnumerator ShoulderWidthCompleteTimer()
    {
        yield return new WaitForSeconds(3);
        float length = VoiceManager.instance.PlayInstructionSound(13,true);
        Debug.Log("ShoulderWidthComplete");
        // StartCoroutine(MoveToSidePosition(length));
        DataManager.currentSkeleton.ResetGlowValues();
        feetAreShoulderWidth = true;


    }


    public void DoCoaching(JointData[] frame)
    {

        Vector3 r_heel = frame[23].jointposition;
        Vector3 r_ankle = frame[0].jointposition;
        Vector3 r_knee = frame[1].jointposition;
        Vector3 r_hip = frame[2].jointposition;
        Vector3 r_shoulder = frame[12].jointposition;

        Vector3 l_heel = frame[24].jointposition;
        Vector3 l_ankle = frame[5].jointposition;
        Vector3 l_knee = frame[4].jointposition;
        Vector3 l_hip = frame[3].jointposition;
        Vector3 l_shoulder = frame[13].jointposition;

        if ( r_heel.x <= 0 || r_ankle.x <= 0 || r_knee.x < 0 || r_hip.x <= 0 || r_shoulder.x <= 0 || l_heel.x <= 0 || l_ankle.x <= 0 || l_knee.x <= 0 || l_hip.x <= 0 || l_shoulder.x <= 0)
        {
            return;
        }
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
        // l_knee_x_coord_of_current_rep.Add(l_knee.x);
        // r_knee_x_coord_of_current_rep.Add(r_knee.x);
        // float pelvis_x = (r_hip.x + l_hip.x)/2;
        // pelvis_x_coord_of_current_rep.Add(pelvis_x);
        user_rotations_of_current_rep.Add(userRotation);

        float left_shin_length = MathHelper.instance.GetEuclideanDistance2D(l_ankle, l_knee);
        float right_shin_length = MathHelper.instance.GetEuclideanDistance2D(r_ankle, r_knee);
        float shin_length = (left_shin_length + right_shin_length) / 2; // Avg 

        float stride_length = MathHelper.instance.GetEuclideanDistance2D(l_ankle, r_ankle);

        if (stride_length > shin_length * 1.3) {
            stride_is_long = true;
        }

        bool audioPlayed = false;

        if (RepCounter(knee_angle, depth1, depth2))
        {

            float avgUserRoation = MathHelper.instance.GetPositiveMean(user_rotations_of_current_rep);
            if (!stride_is_long) {
                //Debug.Log("Sound On: Try to take a longer step forward");
                if (!audioPlayed)
                {
                    Debug.Log("Step Forward");
                    VoiceManager.instance.PlayInstructionSound(15);
                    audioPlayed = true;

                    // Make Shins Red
                    if (DataManager.currentSkeleton != null)
                    {
                        DataManager.currentSkeleton.ResetGlowValues();
                        DataManager.currentSkeleton.SetBoneGlowValues(new int[] { 0,2 } , GlowColor.Red);
                    }
                }
            }
            else if (torso_angles_of_current_rep.Min() <  torsoAngleCutoff)
            {
                //Debug.Log("Sound on: Keep your chest up!");
                if (!audioPlayed)
                {
                    VoiceManager.instance.PlayInstructionSound(10);
                    audioPlayed = true;

                    // Make Spine Red
                    if (DataManager.currentSkeleton != null)
                    {
                        DataManager.currentSkeleton.ResetGlowValues();
                        DataManager.currentSkeleton.SetBoneGlowValues(new int[] { 6 } , GlowColor.Red);
                    }
                }
            }

            else if( r_knee_angles_of_current_rep.Min() > kneeAngleCutoff && l_knee_angles_of_current_rep.Min() > kneeAngleCutoff)
            {
                // Debug.Log("Sound on: Try to get a bit lower!");
                if (!audioPlayed)
                {
                    VoiceManager.instance.PlayInstructionSound(4);
                    audioPlayed = true;

                    // Make Thighs Red
                    if (DataManager.currentSkeleton != null)
                    {
                        DataManager.currentSkeleton.ResetGlowValues();
                        DataManager.currentSkeleton.SetBoneGlowValues(new int[] { 1,3 } , GlowColor.Red);
                    }
                }
                
            }

            // Play Audio Count
            else if (!audioPlayed)
            {
                reps += 1;
                VoiceManager.instance.PlayInstructionSound(1); // index of rep sound 
                audioPlayed = true;
                if (DataManager.currentSkeleton != null)
                {
                    DataManager.currentSkeleton.ResetGlowValues();
                }
            }

            stride_is_long = false;
            torso_angles_of_current_rep.Clear();
            // l_knee_x_coord_of_current_rep.Clear();
            // r_knee_x_coord_of_current_rep.Clear();
            // pelvis_x_coord_of_current_rep.Clear();
            l_knee_angles_of_current_rep.Clear();
            r_knee_angles_of_current_rep.Clear();
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

        // // Check if hips are between knees
        // if (thresholdReached && hip_is_between_knees == false) {
        //     float left = l_knee_x_coord_of_current_rep.Last();
        //     float right = r_knee_x_coord_of_current_rep.Last();
        //     float pelvis = pelvis_x_coord_of_current_rep.Last();

        //     if (left < right) {
        //         if (pelvis >= left && pelvis <= right) {
        //             hip_is_between_knees = true;
        //         } else {
        //             hip_is_between_knees = false;
        //         }
        //     } else {
        //         if (pelvis >= right && pelvis <= left) {
        //             hip_is_between_knees = true;
        //         } else {
        //             hip_is_between_knees = false;
        //         }
        //     }
        // }

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
