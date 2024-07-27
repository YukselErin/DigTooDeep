using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmbientSounds : MonoBehaviour
{

    public static AmbientSounds Instance { get; private set; }
    private void Awake()
    {
        // If there is an instance, and it's not me, delete myself.
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;

        }
    }
    public AudioClip[] caveSounds;
    public AudioClip[] creepySounds;

    public AudioClip[] minotaurWarningSound;
    public AudioSource AmbientAudiosource;
    public AudioSource CreepAudiosource;
    public int caveSoundIndex = 0;
    void Start()
    {
        if (!AmbientAudiosource) { AmbientAudiosource = GetComponent<AudioSource>(); }
    }
    float lastTimeCreepySound;
    public float creepyCD;
    public float creepyCDRandom;
    public bool creepySoundPrompt;
    float creepyRandomComponent;

    void Update()
    {
        if (!AmbientAudiosource.isPlaying)
        {
            caveSoundIndex = (caveSoundIndex + 1) % caveSounds.Length;
            AmbientAudiosource.clip = caveSounds[caveSoundIndex];
            AmbientAudiosource.Play();
        }

        if (lastTimeCreepySound + creepyCD + creepyRandomComponent < Time.time || creepySoundPrompt)
        {
            lastTimeCreepySound = Time.time;
            creepySoundPrompt = false;
            CreepAudiosource.PlayOneShot(creepySounds[Random.Range(0, creepySounds.Length)]);
            creepyRandomComponent = Random.Range(0f, creepyCDRandom);
        }
    }
    public float warningCD;
    float lastWarning;

    public void MinotaurWarning()
    {
        if (warningCD + lastWarning < Time.time)
        {
            lastWarning = Time.time;
            CreepAudiosource.PlayOneShot(minotaurWarningSound[Random.Range(0, minotaurWarningSound.Length)]);
        }

    }
}
