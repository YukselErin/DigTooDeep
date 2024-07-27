using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteFacePlayer : MonoBehaviour
{
    public AudioClip sonarPing;
    int frame = 0;

    public Transform faceTransform;

    bool up = true;
    public float max = 0.4f;
    public float min = 0.15f;
    public float speed = 0.05f;
    public float destroytime = 1.5f;
    public int period = 30;
    void Update()
    {
        if (faceTransform)
        { transform.LookAt(faceTransform); }
        if (up)
        {
            transform.localScale = transform.localScale + (new Vector3(1f, 1f, 1f)) * speed * Time.deltaTime;
        }
        else
        {
            transform.localScale = transform.localScale - (new Vector3(1f, 1f, 1f)) * speed * Time.deltaTime;

        }
        if (max <= transform.localScale.x)
        {
            up = false;
            frame++;
            GetComponent<AudioSource>().PlayOneShot(sonarPing);
        }
        else if (min >= transform.localScale.x)
        {
            up = true;
        }
        if (frame == 2)
        {
            Destroy(transform.parent.parent.gameObject, destroytime);
        }
    }
}
