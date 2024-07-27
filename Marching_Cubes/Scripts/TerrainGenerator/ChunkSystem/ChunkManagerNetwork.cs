using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ChunkManagerNetwork : NetworkBehaviour
{
    public static ChunkManagerNetwork Instance { get; private set; }

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
    public void ModifyChunkData(Vector3 modificationPoint, float range, float modification, ulong clientID, int mat = -1, int power = 1)
    {
        ModifyChunkDataMultiplayerRPC(modificationPoint, range, modification, clientID, mat, power);
    }

    [Rpc(SendTo.Everyone)]
    public void ModifyChunkDataMultiplayerRPC(Vector3 modificationPoint, float range, float modification, ulong clientID, int mat = -1, int power = 1)
    {
        //Debug.Log("modificationPoint" + modificationPoint);
        ChunkManager.Instance.ModifyChunkData(modificationPoint, range, modification, clientID, mat, power);
    }
    void Update()
    {

    }
}
