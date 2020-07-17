using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using wrnchAI.Core;
using wrnchAI.wrAPI;

public class MultipersonHandler : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        PoseManager.onPoseReceived += OnPersonFound;
    }

    private void OnDisable()
    {
        PoseManager.onPoseReceived -= OnPersonFound;
    }
    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnPersonFound(List<Person> persons)
    {
        if (persons!=null)
        {
            Debug.Log("Person Found");
        }

    }
}
