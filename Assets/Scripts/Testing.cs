using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class Testing : MonoBehaviour
{
   public void StartExercise(string exerciseName)
    {
        PlayerPrefs.SetString("ExerciseName", exerciseName);
        SceneManager.LoadScene(1);
    }

}
