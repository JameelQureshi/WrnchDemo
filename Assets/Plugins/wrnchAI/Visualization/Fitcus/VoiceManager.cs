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
    public AudioClip[] countingClips;
    public AudioClip[] instructionClips;

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void PlayCountingSound(int index)
    {
        audioSource.PlayOneShot(countingClips[index]);
    }

    public void PlayInstructionSound(int index)
    {
        audioSource.PlayOneShot(instructionClips[index]);
    }
}
