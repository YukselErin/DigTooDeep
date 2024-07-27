using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.Mathematics;
using UnityEngine;

public class WormHead : MonoBehaviour
{
    List<Vector3> path;
    public GameObject segmentPrefab;
    List<GameObject> wormSegments;
    GameObject instantited;
    public GameObject attachmentPoint;
    public int spacing = 5;

    void Start()
    {
        path = new List<Vector3>();

        wormSegments = new List<GameObject>();
        slitherCycle = new Vector3();
        slitherCycle = (randomTurnMultiplier * new Vector3(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f)));
    }
    void CreateSegment()
    {
        instantited = Instantiate(segmentPrefab);
        //WormSegmentAlign temp = instantited.GetComponentInChildren<WormSegmentAlign>();
        //temp.AttachmentPoint = attachmentPoint;
        wormSegments.Add(instantited);
        instantited.transform.position = path[path.Count - spacing * (wormSegments.Count)];
        instantited.transform.LookAt(path[path.Count - (spacing * wormSegments.Count) + 1]);

    }
    public float stepsize = 1f;
    public int pathlen = 0;
    public int wormsize = 10;
    public float randomTurnMultiplier = 1f;
    int effectiveSpacing;
    public Transform[] waypoints;
    int currentWaypoint = 0;
    Transform temp;
    public int cycle = 5;
    public int cycleperiod = 5;
    Vector3 slitherCycle;
    public float slitherModifier = 1f;
    bool rise = true;
    bool startNew = false;
    void wormMove()
    {
        if (Vector3.Distance(transform.position, waypoints[currentWaypoint].position) < 10f)
        {
            currentWaypoint = (currentWaypoint + 1) % waypoints.Length;
        }
        Vector3 temppos = transform.position;
        /* 

                         if (cycle == cycleperiod)
                         {
                             rise = !rise;
                             cycle = 1;
                             if (startNew)
                             {
                                 startNew = false;
                             }
                             else
                             {
                                 startNew = true;
                             }
                         }
                         if (rise)
                         {
                             Debug.Log((float)cycle * slitherModifier);
                             Debug.Log(1 / ((float)cycle * slitherModifier));
                             temppos = temppos + slitherCycle * (1 / ((float)cycle * slitherModifier));
                         }
                         else
                         {
                             temppos = temppos - slitherCycle * (1 / ((float)cycle * slitherModifier));
                         }*/
        Vector3 direction = (waypoints[currentWaypoint].position) - transform.position;
        direction = direction.normalized;
        transform.LookAt(temppos + direction * stepsize);
        transform.position = (temppos + direction * stepsize + (randomTurnMultiplier * new Vector3(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f))));

        //+ transform.right * randomTurnMultiplier * UnityEngine.Random.Range(-.5f, 1f);
        //new Vector3(Random.Range(-.5f, 1f), Random.Range(-.5f, 1f), Random.Range(-.5f, 1f));

        path.Add(transform.position);
        if (path.Count > (wormSegments.Count + 1) * effectiveSpacing)
        {
            path.RemoveAt(0);
        }
        for (int i = 1; i < wormSegments.Count + 1; i++)
        {
            //  Debug.Log("i" + i + " worms:" + wormSegments.Count);
            // Debug.Log(path.Count - (spacing * i));
            //  Debug.Log(path.Count - (spacing * i) + 1);
            //Debug.Log(path.Count);
            wormSegments[i - 1].transform.position = path[path.Count - (effectiveSpacing * i)];
            wormSegments[i - 1].transform.LookAt(path[path.Count - (effectiveSpacing * i) + effectiveSpacing - 1]);
        }
    }
    public bool step = false;
    public bool create = false;
    public float cd = .2f;
    float lasttime = 0f;
    // Update is called once per frame
    void Update()
    {
        effectiveSpacing = (int)Mathf.Round(spacing * stepsize);
        if (cd + lasttime < Time.time)
        {
            lasttime = Time.time;
            step = false;
            wormMove();
            pathlen = path.Count;
        }
        if ((path.Count / effectiveSpacing) - wormSegments.Count > 0 && wormSegments.Count <= wormsize)
        {
            create = false;
            CreateSegment();
        }
    }
}
