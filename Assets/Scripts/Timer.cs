using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.Events;

public class Timer : MonoBehaviour {

    [Header("DisplayArea")]
	public Text timerText;
    public Image timerFill;

    public float startingTime;
	public UnityEvent onTimeEnd;

	private float currentMaxTime;
	private float currentTime;
    private int secondsSpent;
    private int milisecondsSpent;
    bool isCountingDown;
	DateTime startTime;
	
	

	void Update()
	{
		if (isCountingDown)
			UpdateCounter();
	}

	public void StartTimer(float time)
	{
		startTime = DateTime.Now;
		currentTime = time;
		currentMaxTime = time;
		isCountingDown = true;

	}
    private void OnEnable()
    {
        StartTimer(startingTime);
    }

    void UpdateCounter()
	{

		DateTime newTime = System.DateTime.Now;
		TimeSpan difference = newTime.Subtract(startTime);

		secondsSpent = difference.Seconds;
        milisecondsSpent = (secondsSpent*1000)+difference.Milliseconds;

        currentTime = currentMaxTime - secondsSpent;
		if (currentTime <= 0)
		{
				isCountingDown = false;
				currentTime = 0;
            onTimeEnd.Invoke();
		}
		DisplayTime();
       

	}

	

	void DisplayTime()
	{	int min;
		float sec;
		float displayTimer = Mathf.Round (currentTime);
		min = (int)(displayTimer / 60);
		sec = (int)(displayTimer % 60);
        if (sec < 10)
        {
            if(timerText!=null)
            timerText.text = "0" + sec;
        }

        else
        {
            timerText.text = "" + sec;
        }


        if (timerFill!=null)
        {
            timerFill.fillAmount =  milisecondsSpent / (currentMaxTime*1000);
        }
    }
	public void Pause(){
		currentMaxTime = currentTime;
		isCountingDown = false;

	}
    public void Stop()
    {
        isCountingDown = false;
    }
    public void Resume(){
		isCountingDown = true;
		startTime=System.DateTime.Now;
	}
}
