using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerEnemyTarget : NetworkBehaviour
{
    float creepInFrontSpawnCD = 240f;
    float timeSinceLastFrontCreepSpawn;
    bool spawnedFirstFrontCreep = false;
    public bool spawnCreepInFront()
    {
        if (!spawnedFirstFrontCreep)
        {
            spawnedFirstFrontCreep = true;
            return true;
        }
        if (timeSinceLastFrontCreepSpawn + creepInFrontSpawnCD < Time.time + spellsUsedSince * 3)
        {
            timeSinceLastScoutCreepSpawn = Time.time;
            spellsUsedSince = 0;
            return true;
        }
        return false;
    }
    public float regionDanger = 1f;
    public float timeSinceLastSpawn = 0f;
    public float timeSinceGameStart = 0f;
    public float timeSinceLastScoutCreepSpawn = 0f;
    public int scoutCreepSpawnedTimes = 0;
    public int creepWaveSpawnedTimes = 0;
    public int spellsUsedSince = 0;
    public float scoutCd = 240f;
    public bool spawnScoutCreepOnPlayer()
    {
        if (timeSinceLastScoutCreepSpawn + scoutCd < spellsUsedSince * 3 + Time.time)
        {
            timeSinceLastScoutCreepSpawn = Time.time;
            spellsUsedSince = 0;
            return true;
        }
        return false;
    }
    float timeSinceMinataurSpawn = 0;
    public float minotaurSpawnCD = 240f;
    public bool spawnMinotaurAroundPlayer()
    {
        if (timeSinceMinataurSpawn + minotaurSpawnCD < Time.time + spellsUsedSince * 3)
        {
            timeSinceMinataurSpawn = Time.time;
            spellsUsedSince = 0;
            return true;
        }
        return false;
    }
    public override void OnNetworkSpawn()
    {
        EnemySpawnController.Instance.addPlayer(this);
    }
    public CastSpell castSpellComponent;
    void Start()
    {
        castSpellComponent.OnChannellingSpell.AddListener(castingSpell);
    }
    void castingSpell()
    {
        spellsUsedSince++;
    }
}
