using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WormSegmentAlign : MonoBehaviour
{
    public GameObject AttachmentPoint;
    // Start is called before the first frame update
    void Start()
    {

    }
    public void updateOrientation()
    {
        transform.LookAt(AttachmentPoint.transform);
    }
    // Update is called once per frame
    void Update()
    {

    }
}
