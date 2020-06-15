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


    public float PlayInstructionSound(int index , bool skipPrevious = false)
    {
        if (skipPrevious)
        {
            audioSource.Stop();
            audioSource.PlayOneShot(instructionClips[index]);
        }
        else
        {
            if (!audioSource.isPlaying)
            {
                audioSource.PlayOneShot(instructionClips[index]);
                float length = instructionClips[index].length;
                return length;
            }
        }

        return 0;
    }

}
