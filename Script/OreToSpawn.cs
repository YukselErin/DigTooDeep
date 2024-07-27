using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/OreToSpawnScriptableObject", order = 1)]
public class OreToSpawn : ScriptableObject
{
    public int cutoff = 60;
    public int orescale = 6;
    public int oreInt = 6;
    public byte weight = 255;
    float[] oreNoise;

    public void createOreNoiseMap(Vector2Int vecPos)
    {

        oreNoise = NoiseManager.GenenerateSimple3DNoiseMap(orescale, vecPos, oreInt);

    }
    public bool createThisOre(int index)
    {

        return (oreNoise[index / 2] * 255f < cutoff);
    }
}
