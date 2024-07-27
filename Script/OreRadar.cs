using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OreRadar : MonoBehaviour
{
    public GameObject oreMarker;
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R)) { RadarPulse(); }
    }
    public float range = 10f;
    public int detectMatInt = 1;
    public void RadarPulse()
    {
        Vector3 modificationPoint = transform.position;
        Vector3 originalPint = modificationPoint;
        modificationPoint = new Vector3(modificationPoint.x / Constants.VOXEL_SIDE, modificationPoint.y / Constants.VOXEL_SIDE, modificationPoint.z / Constants.VOXEL_SIDE);

        //Chunk voxel position (based on the chunk system)
        Vector3 vertexOrigin = new Vector3((int)modificationPoint.x, (int)modificationPoint.y, (int)modificationPoint.z);

        //intRange (convert Vector3 real world range to the voxel size range)
        int intRange = (int)(range / 2 * Constants.VOXEL_SIDE);//range /2 because the for is from -intRange to +intRange

        for (int y = -intRange; y <= intRange; y++)
        {
            for (int z = -intRange; z <= intRange; z++)
            {
                for (int x = -intRange; x <= intRange; x++)
                {
                    //Avoid edit the first and last height vertex of the chunk, for avoid non-faces in that heights
                    if (vertexOrigin.y + y >= Constants.MAX_HEIGHT / 2 || vertexOrigin.y + y <= -Constants.MAX_HEIGHT / 2)
                        continue;

                    //Edit vertex of the chunk
                    Vector3 vertexPoint = new Vector3(vertexOrigin.x + x, vertexOrigin.y + y, vertexOrigin.z + z);

                    float distance = Vector3.Distance(vertexPoint, modificationPoint);
                    if (distance > range)//Not in range of modification, we check other vertexs
                    {
                        //Debug.Log("no Rango: "+ distance + " > " + range+ " |  "+ vertexPoint +" / " + modificationPoint);
                        continue;
                    }

                    //Chunk of the vertexPoint
                    Vector2Int hitChunk = new Vector2Int(Mathf.CeilToInt((vertexPoint.x + 1 - Constants.CHUNK_SIZE / 2) / Constants.CHUNK_SIZE),
                                                    Mathf.CeilToInt((vertexPoint.z + 1 - Constants.CHUNK_SIZE / 2) / Constants.CHUNK_SIZE));
                    //Position of the vertexPoint in the chunk (x,y,z)
                    Vector3Int vertexChunk = new Vector3Int((int)(vertexPoint.x - hitChunk.x * Constants.CHUNK_SIZE + Constants.CHUNK_VERTEX_SIZE / 2),
                        (int)(vertexPoint.y + Constants.CHUNK_VERTEX_HEIGHT / 2),
                        (int)(vertexPoint.z - hitChunk.y * Constants.CHUNK_SIZE + Constants.CHUNK_VERTEX_SIZE / 2));

                    //Debug.Log( vertexPoint + " | chunk: "+ hitChunk+ " / " + vertexChunk);//Debug Vertex point to chunk and vertexChunk

                    if (detectMatInt != ChunkManager.Instance.chunkDict[hitChunk].GetMaterial(vertexChunk) && Constants.NUMBER_MATERIALS != ChunkManager.Instance.chunkDict[hitChunk].GetMaterial(vertexChunk))
                    {
                        Instantiate(oreMarker, new Vector3(vertexPoint.x * Constants.VOXEL_SIDE, vertexPoint.y * Constants.VOXEL_SIDE, vertexPoint.z * Constants.VOXEL_SIDE), Unity.Mathematics.quaternion.identity);

                    }

                    // modifyTerrain(vertexChunk, chunkModification, clientID, mat, power);

                    //Debug.Log(distance / range);


                }
            }
        }
    }
}
