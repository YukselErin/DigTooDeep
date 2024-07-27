using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using Unity.Burst.Intrinsics;
using Unity.Mathematics;
using UnityEngine;

public class B_Caverns : Biome
{
    [Tooltip("The highest point of the surface")]
    [Range(0, Constants.MAX_HEIGHT - 1)]
    public int maxSurfaceheight = Constants.MAX_HEIGHT - 1;

    [Header("Texture generation")]
    [Tooltip("Increase the effect of the hightMatMult")]
    [Range(1, 20f)]
    public float heightMatOffset = 10;
    [Tooltip("Multiplier of the slope in dependence of the height")]
    public AnimationCurve hightMatMult;
    [Tooltip("Height where the grass change to snow")]
    [Range(0, Constants.MAX_HEIGHT)]
    public int snowHeight = 35;
    [Tooltip("Slope vale where terrain start to be rock")]
    [Range(0, 1f)]
    public float rockLevel = 0.6f;
    [Tooltip("Slope vale where terrain start to be dirt")]
    [Range(0, 1f)]
    public float dirtLevel = 0.25f;
    public GameObject orePrefab;
    long scnoise = 0;
    long noisefloat = 0;
    public override byte[] GenerateChunkData(Vector2Int vecPos, float[] biomeMerge)
    {
        int surfaceStart = NoiseManager.Instance.worldConfig.surfaceLevel;//Avoid too high value that generate bad mesh
        byte[] chunkData = new byte[Constants.CHUNK_BYTES];
        float[] noise = NoiseManager.GenerateExtendedNoiseMap(scale, octaves, persistance, lacunarity, vecPos);
        foreach (OreToSpawn ore in oresToSpawn)
        {
            ore.createOreNoiseMap(vecPos);
        }
        for (int z = 0; z < Constants.CHUNK_VERTEX_SIZE; z++)//start a 1 because the noise start at -1 of the chunk vertex
        {
            for (int x = 0; x < Constants.CHUNK_VERTEX_SIZE; x++)//start a 1 because the noise start at -1 of the chunk vertex
            {

                // Get surface height of the x,z position 1276120704
                float height = Mathf.Lerp(
                    NoiseManager.Instance.worldConfig.surfaceLevel,//Biome merge height
                    (terrainHeightCurve.Evaluate(noise[(x + 1) + (z + 1) * (Constants.CHUNK_VERTEX_SIZE + 2)]) * (maxSurfaceheight - surfaceStart) + surfaceStart),//Desired biome height
                    biomeMerge[x + z * Constants.CHUNK_VERTEX_SIZE]);//Merge value,0 = full merge, 1 = no merge

                //557164096
                int heightY = Mathf.CeilToInt(height);//Vertex Y where surface start
                int lastVertexWeigh = (int)((255 - isoLevel) * (height % 1) + isoLevel);//Weigh of the last vertex
                float slope = CalculateSlope((x + 1), (z + 1), noise);

                for (int y = 0; y < Constants.CHUNK_VERTEX_HEIGHT; y++)
                {
                    int index = (x + z * Constants.CHUNK_VERTEX_SIZE + y * Constants.CHUNK_VERTEX_AREA) * Constants.CHUNK_POINT_BYTE;//apply x-1 and z-1 for get the correct index

                    if (y <= Constants.MAX_HEIGHT - 1)
                    {
                        chunkData[index] = 255;
                        chunkData[index + 1] = 0;//Rock
                    }
                    else
                    {
                        chunkData[index] = 0;
                        chunkData[index + 1] = Constants.NUMBER_MATERIALS;
                    }

                    if (heightY > 50 && y == 70)
                    {
                        chunkData[index] = 255;
                        chunkData[index + 1] = 10;
                    }
                    if (heightY < 52 && heightY > 50 && y > 70 && y < 79)
                    {
                        chunkData[index] = 255;
                        chunkData[index + 1] = 10;
                    }
                    /*Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();
                    if (Unity.Mathematics.noise.snoise(new float3(x, y, z)) > 0.95 - (vecPos.magnitude * 0.01f) && (x != Constants.CHUNK_VERTEX_SIZE && x != 0 && y != Constants.CHUNK_VERTEX_SIZE && y != 0))
                    {
                        chunkData[index] = 0;
                        chunkData[index + 1] = 10;
                    }
                    stopwatch.Stop();
                    scnoise += stopwatch.ElapsedTicks;
                    stopwatch.Restart();*/
                    foreach (OreToSpawn ore in oresToSpawn)
                    {
                        if (ore.createThisOre(index))
                        {

                            chunkData[index] = ore.weight;
                            chunkData[index + 1] = (byte)ore.oreInt;
                        }


                    }
                    if (y == Constants.MAX_HEIGHT)
                    {
                        chunkData[index] = 255;
                        chunkData[index + 1] = 10;
                    }
                    if ((x != Constants.CHUNK_VERTEX_SIZE && x != 0 && y != Constants.CHUNK_VERTEX_SIZE && y != 0))
                    {

                    }

                }
            }
        }

        return chunkData;
    }
    public OreToSpawn[] oresToSpawn;
    /// <summary>
    /// Function that calculate the slope of the terrain
    /// </summary>

    private float CalculateSlope(int x, int z, float[] noise)
    {
        float minValue = 1000;
        for (int xOffset = x - 1; xOffset <= x + 1; xOffset += 1)
        {
            for (int zOffset = z - 1; zOffset <= z + 1; zOffset += 1)
            {
                float value = terrainHeightCurve.Evaluate(noise[xOffset + zOffset * (Constants.CHUNK_VERTEX_SIZE + 2)]);
                if (value < minValue)
                    minValue = value;
            }
        }
        float pointValue = terrainHeightCurve.Evaluate(noise[x + z * (Constants.CHUNK_VERTEX_SIZE + 2)]);
        return (1 - (minValue / pointValue)) * (hightMatMult.Evaluate(pointValue) * heightMatOffset); ;
    }
}
