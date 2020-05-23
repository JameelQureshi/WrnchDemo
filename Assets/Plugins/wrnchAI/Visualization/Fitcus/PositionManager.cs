using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionManager : MonoBehaviour
{
    public static PositionManager instance;
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

    public bool canAdjustPosition = false;
    public static bool isTimerRuning = false;
    [Header("Reference Area")]
    public GameObject m_timerPrefab;
    private GameObject timerRef;


    public PositionCalculator head       = new PositionCalculator(); // index 9
    public PositionCalculator r_shoulder = new PositionCalculator(); // index 12
    public PositionCalculator l_shoulder = new PositionCalculator(); //index 13
    public PositionCalculator pelv       = new PositionCalculator(); // index 6
    public PositionCalculator r_knee     = new PositionCalculator();// index 1
    public PositionCalculator l_knee     = new PositionCalculator();// index 4
    public PositionCalculator r_ankle    = new PositionCalculator();// index 0
    public PositionCalculator l_ankle    = new PositionCalculator();// index 5



    public void CalculatePosition( JointData[] jointData2D )
    {

        if (!canAdjustPosition)
        {
            return;
        }
        Debug.Log("Code comes here!");

        if (    head.CheackPointInPosition(jointData2D[9].jointposition)
             && r_shoulder.CheackPointInPosition(jointData2D[12].jointposition)
             && l_shoulder.CheackPointInPosition(jointData2D[13].jointposition)
             && pelv.CheackPointInPosition(jointData2D[6].jointposition)
             && r_knee.CheackPointInPosition(jointData2D[1].jointposition)
             && l_knee.CheackPointInPosition(jointData2D[4].jointposition)
             && r_ankle.CheackPointInPosition(jointData2D[0].jointposition)
             && l_ankle.CheackPointInPosition(jointData2D[5].jointposition) )
        {
            Debug.Log("Body in position");

            if (timerRef == null)
            {
                timerRef = Instantiate(m_timerPrefab, gameObject.transform);
                isTimerRuning = true;
                StartCoroutine(LoadNextScreen());
            }


           
        }
        else
        {
            if (timerRef != null)
            {
                Destroy(timerRef);
                isTimerRuning = false;
            }

            StopAllCoroutines();
        }

    }

    IEnumerator LoadNextScreen()
    {
        yield return new WaitForSeconds(3);
        JointDataManager.instance.canDoCoaching = true;
        Destroy(gameObject);
    }

}


public class PositionCalculator
{
    public float minX ;
    public float maxX ;
    public float minY ;
    public float maxY ;


    int negativeValueFixCounter;
    public PositionCalculator()
    {   
        minX = 0.43f;
        maxX = 0.5f;
        minY = 0.05f;
        maxY = 0.95f;
        negativeValueFixCounter = 0;
    }

    public bool CheackPointInPosition(Vector3 position )
    {
        if ( position.x >= minX && position.x <= maxX && position.y >= minY && position.y <= maxY )
        {
            negativeValueFixCounter = 0;
            return true;
        }
        else
        {
            if ( position.x <= 0 && PositionManager.isTimerRuning)
            {
                negativeValueFixCounter++;
                if (negativeValueFixCounter >= 10)
                {
                    negativeValueFixCounter = 0;
                    return false;
                }
                else
                {
                    return true;
                }
            }

            return false;
        }
    }
}
