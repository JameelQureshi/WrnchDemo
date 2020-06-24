
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Squat : Coaching
{

    public static Squat instance;
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
    /// <summary>
    ///  """
    //The class used to track a normal Squat

    //Attributes
    //----------
    //  depth1 : Int
    //       How deep should the user squat for a rep to be counted.
    //        Value represents the angle of knee(assuming a straight shin).
    //  depth2 : Int
    //        How high should the user rise out of the squat for a rep to be counted.
    //       Value represents the angle of knee(assuming a straight shin).
    //  kneeAngleCutoff: Int
    //      If the knee angle doesnt get below this point, the coach is triggered.
    //    Knee angle is measured between the thigh and the verticle plane(or straight shin)
    //  torsoAngleCutoff: Int
    //How low of an angle can the torso get before triggering the coach.
    //Torso angle is measured between torso and the verticle plane

    /// </summary>

    public List<float> knee_angle_list = new List<float>();
    public List<float> torso_angle_list = new List<float>();

    public List<float> torso_angles_of_current_rep = new List<float>();
    public List<float> knee_angles_of_current_rep = new List<float>();
    public List<float> user_rotations_of_current_rep = new List<float>();

    private float depth1 = 130f;
    private float depth2 = 140f;
    private float kneeAngleCutoff = 110f;
    private float torsoAngleCutoff = 115f;
    private bool feetAreShoulderWidth = false;
    private System.DateTime wrongTimer1;
    private System.DateTime wrongTimer2;
    private System.DateTime rightTimer1;
    private System.DateTime rightTimer2;

    /// Used for rep counting
    public bool trackingBegan = false;
    public bool thresholdReached = false;

    public CoachingOneEuroFilter one_euro_filter_r_heel ;
    public CoachingOneEuroFilter one_euro_filter_r_shoulder ;
    public CoachingOneEuroFilter one_euro_filter_l_heel ;
    public CoachingOneEuroFilter one_euro_filter_l_shoulder ;
    public int frame_no = 0;
    public int frame_no_stance_check = 0;
    public int test = 0;




        //Analyses squat mechanics given a single frame of joints.

        //This function will trigger count reps functionality and detect correct movement patterns.

        //Parameters
        //----------
        //frame : list
        //    A list of 25 joints that have been extracted from a video frame.
        //    Joints are (x, y, z) coordinates



    public override void AnalyseFrame( JointData[] frame)
    {

        if (feetAreShoulderWidth == false) {
            GuideFeetToShoulderWidth(frame);
            frame_no_stance_check += 1;
        } else {
            DoCoaching(frame);
        }

    }

    public void GuideFeetToShoulderWidth( JointData[] frame) {

        Vector3 r_heel = frame[23].jointposition;
        Vector3 r_shoulder = frame[12].jointposition;
        Vector3 l_heel = frame[24].jointposition;
        Vector3 l_shoulder = frame[13].jointposition;

        if (frame_no_stance_check == 0) {
            // Init Euro Filters(
            Debug.Log("-------------------- Im In ---------------------");
            one_euro_filter_r_heel = new CoachingOneEuroFilter(frame_no_stance_check, r_heel.x, 0.0f, 0.01f, 0.0f, 1.0f);
            one_euro_filter_r_shoulder = new CoachingOneEuroFilter(frame_no_stance_check, r_shoulder.x, 0.0f, 0.01f, 0.0f, 1.0f);
            one_euro_filter_l_heel = new CoachingOneEuroFilter(frame_no_stance_check, l_heel.x, 0.0f, 0.01f, 0.0f, 1.0f);
            one_euro_filter_l_shoulder = new CoachingOneEuroFilter(frame_no_stance_check, l_shoulder.x, 0.0f, 0.01f, 0.0f, 1.0f);
                      
        } else {
            r_heel.x = one_euro_filter_r_heel.ApplyFilter(frame_no_stance_check, r_heel.x);
            r_shoulder.x = one_euro_filter_r_shoulder.ApplyFilter(frame_no_stance_check, r_shoulder.x);
            l_heel.x = one_euro_filter_l_heel.ApplyFilter(frame_no_stance_check, l_heel.x);
            l_shoulder.x = one_euro_filter_l_shoulder.ApplyFilter(frame_no_stance_check, l_shoulder.x);  
        }

            

        Feetdata feetdata = MathHelper.instance.FeetAreShoulderWidth(r_shoulder, l_shoulder, r_heel, l_heel);
        // If feet are shoulder width
        if (feetdata.state)
        {
            
            // Reset the wrong timer
            if(wrongTimer1 != null && wrongTimer2 != null) {
                wrongTimer1 = System.DateTime.Now;
                wrongTimer2 = System.DateTime.Now;
            }

            if(rightTimer1 == null) {
                // Init times
                rightTimer1 = System.DateTime.Now;
                rightTimer2 = System.DateTime.Now;
            } else {
                double diffInSeconds = (rightTimer2 - rightTimer1).TotalSeconds;
                double time = 5.0;
                if (diffInSeconds > time) {
                    // Great work, now put right shoulder forward
                    // VoiceManager.instance.PlayInstructionSound(12); // index of rep sound 
                    VoiceManager.instance.PlayInstructionSound(20);
                    // feetAreShoulderWidth = true;
                    // Reset timer
                    rightTimer1 = System.DateTime.Now;
                    rightTimer2 = System.DateTime.Now;
                } else {
                    // Update timer
                    rightTimer2 = System.DateTime.Now;
                    Debug.Log("--------------- FEET ARE SHOULDER WIDTH ---------" + diffInSeconds);
                }
            }

            DataManager.currentSkeleton.ResetGlowValues();
            Debug.Log("--------------- FEET ARE SHOULDER WIDTH ---------");

        // If feet are NOT shoulder width
        } else {
            
            // Reset the right timer
            if(rightTimer1 != null && rightTimer2 != null) {
                rightTimer1 = System.DateTime.Now;
                rightTimer2 = System.DateTime.Now;
            }

            if(wrongTimer1 == null ) {
                // Init times
                wrongTimer1 = System.DateTime.Now;
                wrongTimer2 = System.DateTime.Now;
            } else {
                double diffInSeconds = (wrongTimer2 - wrongTimer1).TotalSeconds;
                double time = 5;
                if (diffInSeconds >= time) {
                    // Please place your feet shoulder width apart
                    VoiceManager.instance.PlayInstructionSound(19);

                    // Reset timer
                    wrongTimer1 = System.DateTime.Now;
                    wrongTimer2 = System.DateTime.Now;

                    test += 1;
                } else {
                    // Update timer
                    wrongTimer2 = System.DateTime.Now;
                    DataManager.currentSkeleton.SetRedGlowValues(new int[] { 0,2 });
                    Debug.Log("-------------------------------------------------------" + diffInSeconds + (diffInSeconds > time) + diffInSeconds.GetType() + time.GetType());
                }
            }

        }

    }

    public void DoCoaching( JointData[] frame) {
        
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

        if ( r_heel.x <= 0 || r_ankle.x <= 0 || r_knee.x <= 0 || r_hip.x <= 0 || r_shoulder.x <= 0 || l_heel.x <= 0 || l_ankle.x <= 0 || l_knee.x <= 0 || l_hip.x <= 0 || l_shoulder.x <= 0)
        {
            return;
        }
       

        float torso_angle  = MathHelper.instance.GetTorsoAngleWithStraightLeg(r_shoulder, r_hip, r_ankle);
        float knee_angle   = MathHelper.instance.GetKneeAngleWithStraightShin(r_hip, r_knee, r_ankle);
        float userRotation = MathHelper.instance.GetUserOrientation(r_hip, l_hip);

        torso_angles_of_current_rep.Add(torso_angle);
        knee_angles_of_current_rep.Add(knee_angle);
        user_rotations_of_current_rep.Add(userRotation);

        bool audioPlayed = false;
        if (RepCounter(knee_angle,depth1, depth2))
        {
            Debug.Log("Rep on frame: " + frame_no);
            
            reps += 1;



            if ( knee_angles_of_current_rep.Min() > kneeAngleCutoff)
            {
                //Debug.Log("Sound on: Try to get a bit lower!");
                VoiceManager.instance.PlayInstructionSound(7);
                audioPlayed = true;

                // Make Bodybone Red
                if (DataManager.currentSkeleton != null)
                {
                    DataManager.currentSkeleton.SetRedGlowValues(new int[] { 6 });
                }

            }

            float avgUserRoation = MathHelper.instance.GetPositiveMean(user_rotations_of_current_rep);
            print("Depth: " + knee_angles_of_current_rep.Min() + " TorsoAngle: " + torso_angles_of_current_rep.Min());


            if (Mathf.Abs(userRotation) <= 45) {
                // If the user is facing the screen, check feet
                Feetdata feetdata = MathHelper.instance.FeetAreShoulderWidth(r_shoulder, l_shoulder, r_heel, l_heel);
                if (feetdata.state)
                {
                    Debug.Log("Feet are correct");
                }
                else
                {
                    //Debug.Log("Sound On: Make sure your feet are shoulder width apart");
                    if (!audioPlayed)
                    {
                        VoiceManager.instance.PlayInstructionSound(9);
                        audioPlayed = true;

                        // Make Bodybone Red
                        if (DataManager.currentSkeleton != null)
                        {
                            DataManager.currentSkeleton.SetRedGlowValues(new int[] { 6 });
                        }
                    }

                }

            } else {
                // If the user is facing the side

                // Check Torso angle
                if ( torso_angles_of_current_rep.Min() <   torsoAngleCutoff)
                {
                    //Debug.Log("Sound on: Keep your chest up!");
                    if (!audioPlayed)
                    {
                        VoiceManager.instance.PlayInstructionSound(10);
                        audioPlayed = true;
                    }
                
                }
                else
                {
                    Debug.Log("Torso is correct");
                }
            }

            // Play Audio Count
            if (!audioPlayed)
            {
                VoiceManager.instance.PlayInstructionSound(12); // index of rep sound 
                audioPlayed = true;
            }
                
            //Debug.Log("Rep " + reps);


            //// Empty All the list for new data

            torso_angles_of_current_rep.Clear();
            knee_angles_of_current_rep.Clear();
            user_rotations_of_current_rep.Clear();


            print("Array Size: " 
            + knee_angles_of_current_rep.Count
            + torso_angles_of_current_rep.Count
            + user_rotations_of_current_rep.Count
            );

        }
        frame_no += 1;
    }

        
        //Counts reps based on the datapoint and 2 thresholds.

        //The datapoint must hit both thresholds to count the rep.

        //Parameters
        //----------
        //datapoint : Int
        //    A list of 25 joints that have been extracted from a video frame.
        //    Joints are (x, y, z) coordinates

        //threshold1 : Int
        //    First point the datapoint must hit.

        //threshold2 : Int
            //Second point the datapoint must hit.
        
    public bool RepCounter(float datapoint ,float threshold1,float threshold2)
    {

         // This rep counter waits for the user to get below the threshold, then above again
        if ( datapoint > threshold1  && trackingBegan == false) {
            // Possible improvement: Is last 5 points linear and pointing down?

            // Tracking has began
            trackingBegan = true;
            return false;

        }
        else if( datapoint < threshold1 && trackingBegan && thresholdReached == false)
        {

            // Threshold is reach
            thresholdReached = true;
            return false;

        }
        else if ( datapoint > threshold2 && trackingBegan && thresholdReached){
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
