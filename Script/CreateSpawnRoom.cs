using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using UnityEngine;

public class CreateSpawnRoom : MonoBehaviour
{
    public Transform startRoomLocation;
    public GameObject startAltar;
    GameObject StartAltarInstance;
    public int xSide = 1;
    public int ySide = 2;
    public float range = 2f;
    public float modification = -500;
    public void BuildStartRoom()
    {
        for (int x = -xSide; x <= xSide; x++)
        {
            for (int y = -ySide; y <= xSide; y++)
            {
                ChunkManagerNetwork.Instance.ModifyChunkData(new Vector3(startRoomLocation.position.x + x, startRoomLocation.position.y, startRoomLocation.position.z + y), range, modification, 9, 2);
            }
        }

    }
    public void placePrefabs()
    {
        if (StartAltarInstance) { Destroy(StartAltarInstance); }
        StartAltarInstance = Instantiate(startAltar);
        StartAltarInstance.transform.position = startRoomLocation.position;
    }
    public void CreateSpawnRoomAtStart()
    {
        Debug.Log("CreateSpawnRoomAtStart");
        BuildStartRoom();
      //  placePrefabs();
    }


}
