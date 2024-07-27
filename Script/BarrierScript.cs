using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class BarrierScript : NetworkBehaviour
{
    // Start is called before the first frame update
    RaycastHit raycastHit;
    float dist1;
    float dist2;
    Transform playerTransform;
    float createTime = 0f;

    float createCD = 20f;
    public void spawnBarrier(Transform player)
    {
        if (IsOwner)
        {
            playerTransform = player;
            createTime = Time.time;
        }
    }
    void Update()
    {
        if (playerTransform != null)
        {
            transform.position = playerTransform.position + playerTransform.forward * 2f;
            transform.rotation = playerTransform.rotation;
        }
        if (createTime != 0f)
        {
            if (createTime + createCD < Time.time)
            {
                endCast();
                transform.SetParent(null, true);
            }
        }
    }
    public void endCast()
    {
        if (IsOwner)
        {
            playerTransform = null;
            createTime = 0f;
        }
    }
    // Update is called once per frame

}
