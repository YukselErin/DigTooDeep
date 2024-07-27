using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerFootstep : NetworkBehaviour
{
    CharacterController characterController;
    AudioSource audioSource;
    public AudioClip[] audioClips;
    void Start()
    {
        characterController = transform.parent.GetComponent<CharacterController>();
        audioSource = GetComponent<AudioSource>();

    }

    Vector2 lateralVelocity;
    public float footstepPeriod = 0.1f;
    float lastTimeFootstep;
    void Update()
    {
        lateralVelocity = new Vector2(characterController.velocity.x, characterController.velocity.z);
        if (lateralVelocity.sqrMagnitude > 0)
        {
            if (Time.time > lastTimeFootstep + footstepPeriod)
            {
                if (characterController.isGrounded)
                {
                    PlayerFootstepRPC();
                }
                lastTimeFootstep = Time.time;
            }
        }
        else
        {
            lastTimeFootstep = Time.time;
        }

    }


    [Rpc(SendTo.Everyone)]
    void PlayerFootstepRPC()
    {
        audioSource.PlayOneShot(audioClips[Random.Range(0, audioClips.Length)]);
    }
}
