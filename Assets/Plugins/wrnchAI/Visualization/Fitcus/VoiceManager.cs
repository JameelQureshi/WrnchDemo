using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoiceManager : MonoBehaviour
{
    public static VoiceManager instance;
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

    private AudioSource audioSource;
    public AudioClip[] instructionClips;

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }


    public void PlayInstructionSound(int index)
    {
        if (!audioSource.isPlaying)
        audioSource.PlayOneShot(instructionClips[index]);
    }
    public float PlayInstructionSound(int index,bool getLength)
    {
        if (!audioSource.isPlaying)
        {
            audioSource.PlayOneShot(instructionClips[index]);
            float length = instructionClips[index].length;
            return length;
        }
       
        return 0;
    }


}
