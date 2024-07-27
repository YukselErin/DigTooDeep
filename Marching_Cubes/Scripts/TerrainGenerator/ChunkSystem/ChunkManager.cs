using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Unity.Netcode;
using Unity.Mathematics;
using System.Diagnostics;
using System;

public class ChunkManager : Singleton<ChunkManager>
{


    [Tooltip("Material used by all the terrain.")]
    public Material terrainMaterial;
    [Range(3, Constants.REGION_SIZE / 2)]
    [Tooltip("Chunks load and visible for the player,radius distance.")]
    public int chunkViewDistance = 10;
    [Range(0.1f, 0.6f)]
    [Tooltip("Distance extra for destroy inactive chunks, this chunks consume ram, but load faster.")]
    public float chunkMantainDistance = 0.3f;
    [Tooltip("Use the camera position to calculate the player position. True-> use Camera.main tag / False-> use Player tag")]
    public bool useCameraPosition = true;
    [Tooltip("F4 to active. Show the current chunk and the data of the voxel you are looking. Important: You need activate gizmos in Game tab!!")]
    public bool debugMode = false;

    public Dictionary<Vector2Int, Chunk> chunkDict = new Dictionary<Vector2Int, Chunk>();
    private Dictionary<Vector2Int, Region> regionDict = new Dictionary<Vector2Int, Region>();
    private List<Vector2Int> chunkLoadList = new List<Vector2Int>();

    private NoiseManager noiseManager;
    private Transform player;
    private Vector3 lastPlayerPos;
    private int lastChunkViewDistance;
    private float hideDistance;
    private float removeDistance;
    private float loadRegionDistance;
    public bool alwaysSaveChunk = false;
    Stopwatch stopWatch;
    //Search the player start position and the start the chunk load (Called from NoiseManager start)
    NetworkObject networkObject;
    public void Initialize()
    {
        allChunksLoaded = false;
        RemoveAll();
        chunkDict = new Dictionary<Vector2Int, Chunk>();
        regionDict = new Dictionary<Vector2Int, Region>();
        chunkLoadList = new List<Vector2Int>();
        orders = new Dictionary<Vector2Int, Queue<(Vector3, int, ulong, int, int)>>();
        stopWatch = new Stopwatch();
        stopWatch.Start();
        noiseManager = NoiseManager.Instance;

        if (useCameraPosition)
            player = Camera.main.transform;//Use the Camera.main as player pos
        else
        {
            foreach (GameObject tempPlayer in GameObject.FindGameObjectsWithTag("Player"))
            {
                UnityEngine.Debug.Log("player name: " + tempPlayer.name);
                if (tempPlayer.transform.parent.TryGetComponent<NetworkObject>(out networkObject))
                {
                    UnityEngine.Debug.Log("player has network object, owner: " + networkObject.IsOwner);

                    if (networkObject.IsOwner)
                        player = tempPlayer.transform;
                }
            }

        }

        if (player == null)
        {
            return;
        }
        else
        {
            loadRegionDistance = Constants.CHUNK_SIDE * Constants.REGION_SIZE * Constants.VOXEL_SIDE * 0.9f;
            lastPlayerPos.x = Mathf.FloorToInt(player.position.x / loadRegionDistance) * loadRegionDistance + loadRegionDistance / 2;
            lastPlayerPos.z = Mathf.FloorToInt(player.position.z / loadRegionDistance) * loadRegionDistance + loadRegionDistance / 2;
            initRegion(Mathf.FloorToInt(player.position.x / loadRegionDistance), Mathf.FloorToInt(player.position.z / loadRegionDistance));
        }
    }

    /// <summary>
    /// Load surrounding regions of the player when first load
    /// </summary>
    void initRegion(int initX, int initZ)
    {
        regionDict.Clear();
        for (int x = initX - 1; x < initX + 2; x++)
        {
            for (int z = initZ - 1; z < initZ + 2; z++)
            {
                regionDict.Add(new Vector2Int(x, z), new Region(x, z));
            }
        }
    }

    /// <summary>
    /// Load new regions and unload the older.
    /// </summary>
    void LoadRegion(int initX, int initZ)
    {
        Dictionary<Vector2Int, Region> newRegionDict = new Dictionary<Vector2Int, Region>();

        for (int x = initX - 1; x < initX + 2; x++)
        {
            for (int z = initZ - 1; z < initZ + 2; z++)
            {
                if (regionDict.ContainsKey(new Vector2Int(x, z)))
                {
                    newRegionDict.Add(new Vector2Int(x, z), regionDict[new Vector2Int(x, z)]);
                    regionDict.Remove(new Vector2Int(x, z));
                }
                else
                    newRegionDict.Add(new Vector2Int(x, z), new Region(x, z));
            }
        }
        //save old regions
        foreach (Region region in regionDict.Values)
            region.SaveRegionData();

        //Assign new region area
        regionDict = newRegionDict;
    }
    public bool allowInit = false;
    //Called each frame
    void Update()
    {
        if (player == null)
        {
            foreach (GameObject tempPlayer in GameObject.FindGameObjectsWithTag("Player"))
            {
                if (tempPlayer.transform.parent.TryGetComponent<NetworkObject>(out networkObject))
                {
                    if (networkObject.IsOwner)
                        player = tempPlayer.transform;
                }
            }
            if (player && allowInit)
            {
                Initialize();
            }
            else
            {
                return;
            }
        }
        if (lastChunkViewDistance != chunkViewDistance)
        {
            CalculateDistances();
            if (useCameraPosition)
                player = Camera.main.transform;//Use the Camera.main as player pos
            else
                foreach (GameObject tempPlayer in GameObject.FindGameObjectsWithTag("Player"))
                {
                    if (tempPlayer.transform.parent.TryGetComponent<NetworkObject>(out networkObject))
                    {
                        if (networkObject.IsOwner)
                            player = tempPlayer.transform;
                    }
                }
        }
        HiddeRemoveChunk();
        CheckNewChunks();
        LoadChunkFromList();
        CheckRegion();
        if (Input.GetKeyDown(KeyCode.F4))
        {
            debugMode = !debugMode;
        }
        //Debug.Log("Regions: " + regionDict.Count + "   / Chunks: " + chunkDict.Count);
    }

    void RemoveAll()
    {
        List<Vector2Int> removeList = new List<Vector2Int>(); ;
        foreach (KeyValuePair<Vector2Int, Chunk> chunk in chunkDict)
        {

            chunk.Value.saveChunkInRegion();//Save chunk only in case that get some modifications
            Destroy(chunk.Value.gameObject);
            removeList.Add(chunk.Key);

        }

        //remove chunks
        if (removeList.Count != 0)
        {
            foreach (Vector2Int key in removeList)
            {
                //Debug.Log("chunk deleted: " + key);
                chunkDict.Remove(key);
            }
        }
    }
    /// <summary>
    /// Check the distance to the player for inactive or remove the chunk.  
    /// </summary>
    void HiddeRemoveChunk()
    {
        List<Vector2Int> removeList = new List<Vector2Int>(); ;
        foreach (KeyValuePair<Vector2Int, Chunk> chunk in chunkDict)
        {
            float distance = Mathf.Sqrt(Mathf.Pow((player.position.x - chunk.Value.transform.position.x), 2) + Mathf.Pow((player.position.z - chunk.Value.transform.position.z), 2));
            if (distance > removeDistance)
            {
                chunk.Value.saveChunkInRegion();//Save chunk only in case that get some modifications
                Destroy(chunk.Value.gameObject);
                removeList.Add(chunk.Key);
            }
            else if (distance > hideDistance && chunk.Value.gameObject.activeSelf)
            {
                chunk.Value.gameObject.SetActive(false);
            }
        }

        //remove chunks
        if (removeList.Count != 0)
        {
            foreach (Vector2Int key in removeList)
            {
                //Debug.Log("chunk deleted: " + key);
                chunkDict.Remove(key);
            }
        }
    }

    /// <summary>
    /// Load in chunkLoadList or active Gameobject chunks at the chunkViewDistance radius of the player
    /// </summary>
    void CheckNewChunks()
    {
        Vector2Int actualChunk = new Vector2Int(Mathf.CeilToInt((player.position.x - Constants.CHUNK_SIDE / 2) / Constants.CHUNK_SIDE),
                                                Mathf.CeilToInt((player.position.z - Constants.CHUNK_SIDE / 2) / Constants.CHUNK_SIDE));
        //Debug.Log("Actual chunk: " + actualChunk);
        for (int x = actualChunk.x - chunkViewDistance; x < actualChunk.x + chunkViewDistance; x++)
        {
            for (int z = actualChunk.y - chunkViewDistance; z < actualChunk.y + chunkViewDistance; z++)
            {
                if (Mathf.Pow((actualChunk.x - x), 2) + Mathf.Pow((actualChunk.y - z), 2) > chunkViewDistance * chunkViewDistance)
                {
                    continue;
                }
                Vector2Int key = new Vector2Int(x, z);
                if (!chunkDict.ContainsKey(key))
                {
                    if (!chunkLoadList.Contains(key))
                    {
                        chunkLoadList.Add(key);
                    }
                }
                else
                {
                    if (!chunkDict[key].gameObject.activeSelf)
                        chunkDict[key].gameObject.SetActive(true);
                }
            }
        }
    }

    /// <summary>
    /// Load one chunk per frame from the chunkLoadList
    /// </summary>
    /// boo
    public bool allChunksLoaded = false;
    public int runmultiple = 0;
    int totalLoop = 0;

    void LoadChunkFromList()
    {
        if (chunkLoadList.Count == 0)
        {
            //  stopWatch.Stop();
            //  TimeSpan ts = stopWatch.Elapsed;

            // Format and display the TimeSpan value.
            /*  string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                  ts.Hours, ts.Minutes, ts.Seconds,
                  ts.Milliseconds / 10);
              UnityEngine.Debug.Log("RunTime " + elapsedTime);*/

            return;
        }


        int looptimes = runmultiple;

        while (looptimes > 0)
        {
            totalLoop++;
            looptimes--;
            Vector2Int key = chunkLoadList[0];

            Vector2Int regionPos = new Vector2Int(Mathf.FloorToInt(((float)key.x) / Constants.REGION_SIZE), Mathf.FloorToInt(((float)key.y) / Constants.REGION_SIZE));
            if (!regionDict.ContainsKey(regionPos))//In case that the chunk isn't in the loaded regions we remove it, tp or too fast movement.
            {
                chunkLoadList.RemoveAt(0);
                return;
            }
            GameObject chunkObj = new GameObject("Chunk_" + key.x + "|" + key.y, typeof(MeshFilter), typeof(MeshRenderer));
            chunkObj.transform.parent = transform;
            chunkObj.transform.position = new Vector3(key.x * Constants.CHUNK_SIDE, 0, key.y * Constants.CHUNK_SIDE);
            //Debug.Log("Try load: "+x+"|"+z +" in "+regionPos);

            Vector2Int keyInsideChunk = new Vector2Int(key.x - regionPos.x * Constants.REGION_SIZE, key.y - regionPos.y * Constants.REGION_SIZE);
            //We get X and Y in the world position, we need calculate the x and y in the region.
            int chunkIndexInRegion = regionDict[regionPos].GetChunkIndex(keyInsideChunk.x, keyInsideChunk.y);
            if (chunkIndexInRegion != 0)//Load chunk from a region data
                chunkDict.Add(key, chunkObj.AddComponent<Chunk>().ChunkInit(regionDict[regionPos].GetChunkData(chunkIndexInRegion), keyInsideChunk.x, keyInsideChunk.y, regionDict[regionPos], false));
            else //Generate chunk with the noise generator
                chunkDict.Add(key, chunkObj.AddComponent<Chunk>().ChunkInit(noiseManager.GenerateChunkData(key), keyInsideChunk.x, keyInsideChunk.y, regionDict[regionPos], Constants.SAVE_GENERATED_CHUNKS));

            chunkLoadList.RemoveAt(0);
            fullfillRequest(key);
            if (chunkLoadList.Count == 0)
            {
                allChunksLoaded = true;
                break;
            }
        }

    }

    /// <summary>
    /// Check chunk manager need load a new regions area
    /// </summary>
    void CheckRegion()
    {
        if (Mathf.Abs(lastPlayerPos.x - player.position.x) > loadRegionDistance || Mathf.Abs(lastPlayerPos.z - player.position.z) > loadRegionDistance)
        {
            int actualX = Mathf.FloorToInt(player.position.x / loadRegionDistance);
            lastPlayerPos.x = actualX * loadRegionDistance + loadRegionDistance / 2;
            int actualZ = Mathf.FloorToInt(player.position.z / loadRegionDistance);
            lastPlayerPos.z = actualZ * loadRegionDistance + loadRegionDistance / 2;
            LoadRegion(actualX, actualZ);
        }
    }



    /// <summary>
    /// Calculate the distances of hide, remove and load chunks.
    /// </summary>
    void CalculateDistances()
    {
        lastChunkViewDistance = chunkViewDistance;
        hideDistance = Constants.CHUNK_SIDE * chunkViewDistance;
        removeDistance = hideDistance + hideDistance * chunkMantainDistance;
    }

    /// <summary>
    /// Modify voxels in a specific point of a chunk.
    /// </summary>

    public void ModifyChunkData(Vector3 modificationPoint, float range, float modification, ulong clientID, int mat = -1, int power = 1)
    {
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

                    int chunkModification = (int)(modification * (1 - distance / range));
                    //Debug.Log( vertexPoint + " | chunk: "+ hitChunk+ " / " + vertexChunk);//Debug Vertex point to chunk and vertexChunk
                    modifyRequest(hitChunk, vertexChunk, chunkModification, clientID, mat, power);

                    //Functions for change last vertex of chunk (vertex that touch others chunk)
                    if (vertexChunk.x == 0 && vertexChunk.z == 0)//Interact with chunk(-1,-1), chunk(-1,0) and chunk(0,-1)
                    {
                        //Vertex of chunk (-1,0)
                        hitChunk.x -= 1;//Chunk -1
                        vertexChunk.x = Constants.CHUNK_SIZE; //Vertex of a chunk -1, last vertex
                        modifyRequest(hitChunk, vertexChunk, chunkModification, clientID, mat, power);
                        //Vertex of chunk (-1,-1)
                        hitChunk.y -= 1;
                        vertexChunk.z = Constants.CHUNK_SIZE;
                        modifyRequest(hitChunk, vertexChunk, chunkModification, clientID, mat, power);
                        //Vertex of chunk (0,-1)
                        hitChunk.x += 1;
                        vertexChunk.x = 0;
                        modifyRequest(hitChunk, vertexChunk, chunkModification, clientID, mat, power);
                    }
                    else if (vertexChunk.x == 0)//Interact with vertex of chunk(-1,0)
                    {
                        hitChunk.x -= 1;
                        vertexChunk.x = Constants.CHUNK_SIZE;
                        modifyRequest(hitChunk, vertexChunk, chunkModification, clientID, mat, power);
                    }
                    else if (vertexChunk.z == 0)//Interact with vertex of chunk(0,-1)
                    {
                        hitChunk.y -= 1;
                        vertexChunk.z = Constants.CHUNK_SIZE;
                        modifyRequest(hitChunk, vertexChunk, chunkModification, clientID, mat, power);
                    }

                    //Debug.Log(distance / range);


                }
            }
        }
    }
    Dictionary<Vector2Int, Queue<(Vector3, int, ulong, int, int)>> orders;
    void modifyRequest(Vector2Int hitChunk, Vector3 vertexChunk, int chunkModification, ulong clientID, int mat = 0, int power = 1)
    {
        if (chunkDict.ContainsKey(hitChunk))
        {
            chunkDict[hitChunk].modifyTerrain(vertexChunk, chunkModification, clientID, mat, power);
        }
        else
        {
            if (orders.ContainsKey(hitChunk))
            {
                orders[hitChunk].Enqueue((vertexChunk, chunkModification, clientID, mat, power));
            }
            else
            {
                orders.Add(hitChunk, new Queue<(Vector3, int, ulong, int, int)>());
                orders[hitChunk].Enqueue((vertexChunk, chunkModification, clientID, mat, power));
            }
        }
    }
    void fullfillRequest(Vector2Int hitChunk)
    {
        if (orders.ContainsKey(hitChunk))
        {
            while (orders[hitChunk].Count > 0)
            {
                (Vector3, int, ulong, int, int) order = orders[hitChunk].Dequeue();
                chunkDict[hitChunk].modifyTerrain(order.Item1, order.Item2, order.Item3, order.Item4, order.Item5);
            }

        }
    }
    /// <summary>
    /// Get the material(byte) from a specific point in the world
    /// </summary>
    public byte GetMaterialFromPoint(Vector3 point)
    {
        point = new Vector3(point.x / Constants.VOXEL_SIDE, point.y / Constants.VOXEL_SIDE, point.z / Constants.VOXEL_SIDE);

        Vector3 vertexOrigin = new Vector3((int)point.x, (int)point.y, (int)point.z);

        //Chunk containing the point
        Vector2Int hitChunk = new Vector2Int(Mathf.CeilToInt((vertexOrigin.x + 1 - Constants.CHUNK_SIDE / 2) / Constants.CHUNK_SIDE),
                                        Mathf.CeilToInt((vertexOrigin.z + 1 - Constants.CHUNK_SIDE / 2) / Constants.CHUNK_SIDE));
        //VertexPoint of the point in the chunk (x,y,z)
        Vector3Int vertexChunk = new Vector3Int((int)(vertexOrigin.x - hitChunk.x * Constants.CHUNK_SIZE + Constants.CHUNK_VERTEX_SIZE / 2),
            (int)(vertexOrigin.y + Constants.CHUNK_VERTEX_HEIGHT / 2),
            (int)(vertexOrigin.z - hitChunk.y * Constants.CHUNK_SIZE + Constants.CHUNK_VERTEX_SIZE / 2));

        if (chunkDict[hitChunk].GetMaterial(vertexChunk) != Constants.NUMBER_MATERIALS)//not air material, we return it
        {
            return chunkDict[hitChunk].GetMaterial(vertexChunk);
        }
        else//Loop next vertex for get a other material different to air
        {
            //we check six next vertex 
            Vector3[] nextVertexPoints = new Vector3[6];
            nextVertexPoints[0] = new Vector3(vertexOrigin.x + 0, vertexOrigin.y - 1, vertexOrigin.z + 0);
            nextVertexPoints[1] = new Vector3(vertexOrigin.x + 1, vertexOrigin.y + 0, vertexOrigin.z + 0);
            nextVertexPoints[2] = new Vector3(vertexOrigin.x - 1, vertexOrigin.y + 0, vertexOrigin.z + 0);
            nextVertexPoints[3] = new Vector3(vertexOrigin.x + 0, vertexOrigin.y + 0, vertexOrigin.z + 1);
            nextVertexPoints[4] = new Vector3(vertexOrigin.x + 0, vertexOrigin.y + 0, vertexOrigin.z + 1);
            nextVertexPoints[5] = new Vector3(vertexOrigin.x + 0, vertexOrigin.y + 1, vertexOrigin.z + 0);
            List<byte> mats = new List<byte>();
            for (int i = 0; i < nextVertexPoints.Length; i++)
            {
                //Chunk of the vertexPoint
                hitChunk = new Vector2Int(Mathf.CeilToInt((nextVertexPoints[i].x + 1 - Constants.CHUNK_SIDE / 2) / Constants.CHUNK_SIDE),
                                                Mathf.CeilToInt((nextVertexPoints[i].z + 1 - Constants.CHUNK_SIDE / 2) / Constants.CHUNK_SIDE));
                //Position of the vertexPoint in the chunk (x,y,z)
                vertexChunk = new Vector3Int((int)(nextVertexPoints[i].x - hitChunk.x * Constants.CHUNK_SIZE + Constants.CHUNK_VERTEX_SIZE / 2),
                    (int)(nextVertexPoints[i].y + Constants.CHUNK_VERTEX_HEIGHT / 2),
                    (int)(nextVertexPoints[i].z - hitChunk.y * Constants.CHUNK_SIZE + Constants.CHUNK_VERTEX_SIZE / 2));

                if (chunkDict[hitChunk].GetMaterial(vertexChunk) != Constants.NUMBER_MATERIALS)//not air material, we return it
                {
                    return chunkDict[hitChunk].GetMaterial(vertexChunk);
                }
            }
        }

        return Constants.NUMBER_MATERIALS;//only air material in that point.
    }


    /// <summary>
    /// Save all chunk and regions data when user close the game.
    /// </summary>
    void OnApplicationQuit()
    {
        //save chunks
        foreach (Chunk chunk in chunkDict.Values)
            chunk.saveChunkInRegion();

        //save regions
        foreach (Region region in regionDict.Values)
            region.SaveRegionData();
    }



    #region DebugMode
    //The code of the region is used for the Debug system, allow you to check your current chunk and see data from the voxels.

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (debugMode && Application.isPlaying)
        {
            //Show chunk
            Vector2Int actualChunk = new Vector2Int(Mathf.CeilToInt((player.position.x - Constants.CHUNK_SIDE / 2) / Constants.CHUNK_SIDE),
                                        Mathf.CeilToInt((player.position.z - Constants.CHUNK_SIDE / 2) / Constants.CHUNK_SIDE));
            Vector3 chunkCenter = new Vector3(actualChunk.x * Constants.CHUNK_SIDE, 0, actualChunk.y * Constants.CHUNK_SIDE);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(chunkCenter, new Vector3(Constants.CHUNK_SIDE, Constants.MAX_HEIGHT * Constants.VOXEL_SIDE, Constants.CHUNK_SIDE));

            //Show voxel
            RaycastHit hitInfo;
            if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hitInfo, 100.0f))
            {
                Vector2Int chunkHit = new Vector2Int(Mathf.CeilToInt((hitInfo.point.x - Constants.CHUNK_SIDE / 2) / Constants.CHUNK_SIDE),
                            Mathf.CeilToInt((hitInfo.point.z - Constants.CHUNK_SIDE / 2) / Constants.CHUNK_SIDE));
                Vector3Int vertexChunk = new Vector3Int((int)(hitInfo.point.x - chunkHit.x * Constants.CHUNK_SIZE + Constants.CHUNK_VERTEX_SIZE / 2),
                            (int)(hitInfo.point.y + Constants.CHUNK_VERTEX_HEIGHT / 2),
                            (int)(hitInfo.point.z - chunkHit.y * Constants.CHUNK_SIZE + Constants.CHUNK_VERTEX_SIZE / 2));
                Vector3 voxelRealPosition = new Vector3((Mathf.FloorToInt(hitInfo.point.x / Constants.VOXEL_SIDE)) * Constants.VOXEL_SIDE + Constants.VOXEL_SIDE / 2,
                            (Mathf.FloorToInt(hitInfo.point.y / Constants.VOXEL_SIDE)) * Constants.VOXEL_SIDE + Constants.VOXEL_SIDE / 2,
                            (Mathf.FloorToInt(hitInfo.point.z / Constants.VOXEL_SIDE)) * Constants.VOXEL_SIDE + Constants.VOXEL_SIDE / 2);

                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(voxelRealPosition, Vector3.one * Constants.VOXEL_SIDE);
            }
        }
    }

#endif
    #endregion
}



