using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class MobileAngleManager : MonoBehaviour
{

    public int targetAngleMin;
    public int targetAngleMax;
    private int avgAngle;
    private bool canCalculate;


    [Header("UI Area")]
    public Sprite[] handleStateSprite;
    public Image handleImage;
    public Slider slider;
    
    [Header("Text Debug Area")]
    public Text angle;
    public Text message;

    [Header("Reference Area")]
    public GameObject m_timerPrefab;
    public Timer coachingTimer;
    private GameObject timerRef;



    private void Start()
    {
        avgAngle = (targetAngleMin + targetAngleMax) / 2;
        slider.minValue = 0;
        slider.maxValue = avgAngle * 2;
        canCalculate = false;


    }
    private void OnEnable()
    {
        Invoke("PlayInstructionSound", 1);
    }

    void PlayInstructionSound()
    {
        float length = VoiceManager.instance.PlayInstructionSound(13, true); // index of instruction sound
        Invoke("StartAngleAdjustment", length);
    }

    void StartAngleAdjustment()
    {
        canCalculate = true;
        Debug.Log("Testing This : "+canCalculate);
    }

    void Update()
    {
        Quaternion referenceRotation = Quaternion.identity;
        Quaternion deviceRotation = DeviceRotation.Get();
        Quaternion eliminationOfXY = Quaternion.Inverse(
            Quaternion.FromToRotation(referenceRotation * Vector3.right,
                                      deviceRotation * Vector3.right)
        );


        Quaternion rotationZ = eliminationOfXY * deviceRotation;
        float phoneAngle = rotationZ.eulerAngles.x;
       

        if (phoneAngle >= 0 && phoneAngle <= 90 && canCalculate)
        {
            if (phoneAngle >= targetAngleMin && phoneAngle <= targetAngleMax)
            {
                handleImage.sprite = handleStateSprite[2];
                handleImage.color = Color.green;
                if (timerRef==null)
                {
                    timerRef = Instantiate(m_timerPrefab, gameObject.transform);
                }

                message.text = "Waiting for Screen to Load!";
                StartCoroutine(LoadNextScreen());
            }

            if (phoneAngle >= targetAngleMax)
            {
                handleImage.sprite = handleStateSprite[1];
                handleImage.color = Color.white;
                if (timerRef!=null)
                {
                    Destroy(timerRef);
                }

                StopAllCoroutines();
                message.text = "Please Adjust phone angle!";
            }

            if (phoneAngle <= targetAngleMin)
            {
                handleImage.sprite = handleStateSprite[0];
                handleImage.color = Color.white;
                if (timerRef != null)
                {
                    Destroy(timerRef);
                }
                StopAllCoroutines();
                message.text = "Please Adjust phone angle!";
            }
            slider.value = (avgAngle * 2) - phoneAngle;

        }
        else
        {
            StopAllCoroutines();
            //message.text = "Please Adjust phone angle!";
        }

        angle.text = "X: " + (int)phoneAngle;

    }

    IEnumerator LoadNextScreen()
    {
        yield return new WaitForSeconds(5);
        canCalculate = false;
        PositionManager.instance.canAdjustPosition = true;
        VoiceManager.instance.PlayInstructionSound(14); // index of middle screen instructions
        coachingTimer.enabled = true;
        Destroy(gameObject);
        message.text = "Next Screen Loaded!";
    }
}
