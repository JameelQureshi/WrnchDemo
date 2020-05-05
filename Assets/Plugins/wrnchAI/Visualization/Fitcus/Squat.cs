using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using wrnchAI.Core;
using wrnchAI.Visualization;
using System.Linq;
public class Squat : MonoBehaviour
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


    /// Used for rep counting
    public bool trackingBegan = false;
    public bool thresholdReached = false;


    public int frame_no = 0;
    public int reps = 0;




        //Analyses squat mechanics given a single frame of joints.

        //This function will trigger count reps functionality and detect correct movement patterns.

        //Parameters
        //----------
        //frame : list
        //    A list of 25 joints that have been extracted from a video frame.
        //    Joints are (x, y, z) coordinates



    public void AnalyseFrame( JointData[] frame)
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

        if ( r_heel.x <0 || r_ankle.x < 0 || r_knee.x < 0 || r_hip.x < 0 || r_shoulder.x < 0 || l_heel.x < 0 || l_ankle.x < 0 || l_knee.x < 0 || l_hip.x < 0 || l_shoulder.x < 0)
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
                VoiceManager.instance.PlayCountingSound(reps - 1);
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
