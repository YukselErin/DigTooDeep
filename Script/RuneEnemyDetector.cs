using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RuneEnemyDetector : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }
    public Transform faceTransform;
    SkinnedMeshRenderer skinnedMeshRenderer;
    // Update is called once per frame
    AudioSource audioSource;
    public AudioClip triggerSound;
    void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.TryGetComponent(out skinnedMeshRenderer))
        {
            // transform.GetChild(0).gameObject.layer = LayerMask.GetMask("XRAY");
            transform.GetChild(0).gameObject.GetComponentInChildren<SpriteFacePlayer>().faceTransform = faceTransform;
            transform.GetChild(0).gameObject.SetActive(true);
        }

    }
}
