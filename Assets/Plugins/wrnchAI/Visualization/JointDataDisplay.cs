using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

            foreach (JointData joint in jointData)
            {
                if (joint.jointname!="") 
                {
                    Debug.Log("JointName: " + joint.jointname + "JointPosition: " + joint.jointposition);
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