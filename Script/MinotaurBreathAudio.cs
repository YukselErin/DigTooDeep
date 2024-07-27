using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinotaurBreathAudio : MonoBehaviour
{
    public AudioClip bullgettingangry;
    AudioSource audioSource;
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            audioSource.Play();
        }
    }
}
