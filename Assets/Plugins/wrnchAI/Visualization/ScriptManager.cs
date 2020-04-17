
using UnityEngine;
using UnityEngine.UI;
using wrnchAI.Visualization;

namespace wrnchAI.Core
{
    public class ScriptManager : MonoBehaviour
    {

        public bool showAvatar = true;

        // Update is called once per frame
        void Update()
        {

        }
       public void ShowJointdata()
        {


            foreach (JointData joint in Skeleton.jointData)
            {
                Debug.Log("JointName: " + joint.jointname + "JointPosition: " + joint.jointposition);
            }

        }


        public void CharacterState(bool state)
        {

            AnimationController.showAvatar = state;
            Debug.Log("Avatar Display: " + AnimationController.showAvatar);
            if (!state)
            {
                GameObject renderobject;
                renderobject = GameObject.FindGameObjectWithTag("Avatar");
                if (renderobject != null)
                {
                    foreach (Renderer r in renderobject.GetComponentsInChildren<Renderer>())
                    {
                        r.enabled = false;
                    }
                }
               
            }

        }

    }


}
