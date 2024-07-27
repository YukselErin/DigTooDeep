using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class EnemySpawnController : NetworkBehaviour
{
    public static EnemySpawnController Instance { get; private set; }
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
        players = new List<PlayerEnemyTarget>();
        enemyInstances = new List<GameObject>();
    }
    void Start()
    {
        Debug.Log("Enemy Spawn Start" + IsOwnedByServer);

        NetworkManager.Singleton.OnConnectionEvent += OnConnectionEvent;

    }
    bool listeningToReset = false;
    void OnConnectionEvent(NetworkManager networkManager, ConnectionEventData connectionEventData)
    {
        if (IsOwnedByServer && !listeningToReset)
        {
            listeningToReset = true;
            GameStateController.Instance.GameRestartEvent.AddListener(Reset);
            Debug.Log("Listening");
        }

        if (connectionEventData.EventType == ConnectionEvent.ClientDisconnected)
        {
            Reset();
        }
    }
    public void destroyEnemy(GameObject enemy)
    {
        if (enemy == null)
            return;
        if (enemyInstances.Contains(enemy))
        {
            enemyInstances.Remove(enemy);
        }

        enemy.GetComponentInChildren<NetworkObject>().Despawn();
        Destroy(enemy);
    }
    void Reset()
    {
        StopAllCoroutines();
        checkDisconnectedPlayers();
        spawnFront = false;
        spawnNow = false;
        waveRank = 0;
        Debug.Log("resetting enemies: " + enemyInstances.Count);
        foreach (GameObject enemyinst in enemyInstances)
        {
            if (enemyinst == null)
                return;
            enemyinst.GetComponentInChildren<NetworkObject>().Despawn();
            Destroy(enemyinst);
        }
        enemyInstances.Clear();

    }
    void checkDisconnectedPlayers()
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i] == null)
            {
                players.Remove(players[i]);
            }
        }
    }
    List<GameObject> enemyInstances;
    public void addPlayer(PlayerEnemyTarget playerEnemyTarget)
    {
        players.Add(playerEnemyTarget);
    }
    public GameObject spiderEnemyPrefab;

    public List<PlayerEnemyTarget> players;
    public float GobbospawnDensity = 0.1f;
    public float DarkSpawnspawnDensity = 0.1f;


    public float assesCD = 30f;
    float lastAsses;
    public int maximumEnemyAmount = 30;
    void Update()
    {
        if (!IsHost)
            return;
        if (lastAsses + assesCD < Time.time)
        {
            lastAsses = Time.time;
            AssesDangerLevels();
        }
        if (debugSpawnOnKeyPress)
        {
            AssesDangerLevels();
        }
        if (enemyInstances.Count > maximumEnemyAmount)
        {
            destroyEnemy(enemyInstances[0]);
        }

    }
    public bool debugSpawnOnKeyPress = false;
    bool spawnFront = false;
    void AssesDangerLevels()
    {
        if (players.Count == 0) return;
        foreach (PlayerEnemyTarget playerEnemyTarget in players)
        {
            if (playerEnemyTarget == null)
            {
                players.Remove(playerEnemyTarget);
                continue;
            }
            if (debugSpawnOnKeyPress && Input.GetKeyDown(KeyCode.Z))
            {
                spawnScoutCreepOnPlayer(playerEnemyTarget.transform);
                return;
            }
            if (debugSpawnOnKeyPress && Input.GetKeyDown(KeyCode.X))
            {
                spawnMinotaurAtPlace(playerEnemyTarget.transform.position);
                return;
            }
            if (debugSpawnOnKeyPress)
                return;
            if (playerEnemyTarget.spawnScoutCreepOnPlayer())
            {
                spawnScoutCreepOnPlayer(playerEnemyTarget.transform);
            }
            if (playerEnemyTarget.spawnMinotaurAroundPlayer())
            {
                spawnMinotaurNearPlayerInRock(playerEnemyTarget.transform);
            }
            if (playerEnemyTarget.spawnCreepInFront() || spawnFront) { }
            {
                spawnFront = true;
                if (spawnCreepInFront(playerEnemyTarget.transform))
                    spawnFront = false;


            }

        }
    }
    public bool spawnNow = false;
    public GameObject creepScoutPrefab;
    public float scoutSpawnDistance = 10f;
    IEnumerator layingInWait(Transform givenTransform)
    {
        Transform temp = givenTransform;
        Vector3 spawnpos = temp.position;
        yield return null;
        while (Vector3.Distance(spawnpos, temp.position) < scoutSpawnDistance || PlayerInLOS(spawnpos, .3f))
        {
            yield return null;
        }
        GameObject tempCreep = Instantiate(creepScoutPrefab, spawnpos, quaternion.identity);
        enemyInstances.Add(tempCreep);

        tempCreep.GetComponent<NetworkObject>().Spawn();
        tempCreep.GetComponent<scoutCreepAI>().initiateFirstPath(temp);
        tempCreep.GetComponent<scoutCreepAI>().isScout = true;
    }
    public GameObject hitmark;
    bool PlayerInLOS(Vector3 position, float radius)
    {
        Vector3[] posTocheck =  {
            position + new Vector3(1,0,0) * radius,
            position + new Vector3(-1,0,0) * radius,
            position + new Vector3(0,1,0) * radius,
            position + new Vector3(0,-1,0) * radius,
            position + new Vector3(0,0,1) * radius,
            position + new Vector3(0,0,-1) * radius,
            position
        };
        RaycastHit raycastout;
        foreach (PlayerEnemyTarget playerEnemyTarget in players)
        {
            foreach (Vector3 origin in posTocheck)
            {
                if (Physics.Raycast(origin, (playerEnemyTarget.transform.position + new Vector3(0f, 0.5f, 0f)) - (origin + new Vector3(0f, 0.5f, 0f)), out raycastout))
                {
                    if (raycastout.collider.tag == "Player")
                    {
                        return true;
                    }


                }

            }
        }
        return false;
    }
    public void spawnScoutCreepOnPlayer(Transform spawnTransform)
    {
        if (spawnTransform == null) spawnTransform = GameObject.FindGameObjectWithTag("Player").transform;
        StartCoroutine(layingInWait(spawnTransform));
    }
    [SerializeField]
    int waveRank = 0;
    public void spawnWaveCreepOnPlayer(Vector3 scoutSpawnPos, Transform scoutDeathPos)
    {

        StartCoroutine(waveCoroutine(scoutSpawnPos, scoutDeathPos));

    }
    public void spawnCreepOnPoint(Vector3 scoutSpawnPos, Transform scoutDeathPos)
    {
        StartCoroutine(creepOnPoint(scoutSpawnPos, scoutDeathPos));

    }
    IEnumerator creepOnPoint(Vector3 scoutSpawnPos, Transform scoutDeathPos)
    {
        int amount = 1;
        while (amount > 0)
        {

            if (!PlayerInLOS(scoutSpawnPos, 0.1f))
            {
                amount--;
                GameObject tempCreep = Instantiate(creepScoutPrefab, scoutSpawnPos, quaternion.identity);
                enemyInstances.Add(tempCreep);
                tempCreep.GetComponent<NetworkObject>().Spawn();
                tempCreep.GetComponent<scoutCreepAI>().initiateFirstPath(scoutDeathPos);
                yield return new WaitForSeconds(1f);
            }
            else
            {
                yield return null;
            }


        }
    }
    IEnumerator waveCoroutine(Vector3 scoutSpawnPos, Transform scoutDeathPos)
    {
        if (debugSpawnOnKeyPress)
        {
        }
        else
        {
            yield return new WaitForSeconds(120f);

        }
        int amount = waveRank + 2;
        while (amount > 0)
        {

            if (!PlayerInLOS(scoutSpawnPos, 0.1f))
            {
                amount--;
                GameObject tempCreep = Instantiate(creepScoutPrefab, scoutSpawnPos, quaternion.identity);
                enemyInstances.Add(tempCreep);
                tempCreep.GetComponent<NetworkObject>().Spawn();
                tempCreep.GetComponent<scoutCreepAI>().initiateFirstPath(scoutDeathPos);
                yield return new WaitForSeconds(1f);
            }
            else
            {
                yield return null;
            }


        }
        waveRank++;
    }
    void spawnMinotaurAtPlace(Vector3 pos)
    {
        minotaurSpawnPoint = pos;
        spawnMinotaurDelayed();
    }
    [SerializeField] GameObject minotaurPrefab;
    public float minotaurSpawnDistance = 10f;
    Vector3 minotaurSpawnPoint;
    void spawnMinotaurNearPlayerInRock(Transform playerTransform)
    {
        Vector3 position = playerTransform.position;
        float radius = minotaurSpawnDistance;
        Vector3[] posTocheck =  {
            position + new Vector3(1,0,0) * radius,
            position + new Vector3(-1,0,0) * radius,
            position + new Vector3(0,1,0) * radius,
            position + new Vector3(0,-1,0) * radius,
            position + new Vector3(0,0,1) * radius,
            position + new Vector3(0,0,-1) * radius
        };

        foreach (Vector3 point in posTocheck)
        {
            if (checkIfSphereFilled(point))
            {
                ChunkManagerNetwork.Instance.ModifyChunkData(point, 3f, -255f, 0, Constants.NUMBER_MATERIALS, 2);
                minotaurSpawnPoint = point;
                Invoke("spawnMinotaurDelayed", 1f);
                return;

            }
            else
            {
                Debug.Log("nowhere to spawn");
            }
        }
    }

    void spawnMinotaurDelayed()
    {
        GameObject temp = Instantiate(minotaurPrefab, minotaurSpawnPoint, quaternion.identity);
        enemyInstances.Add(temp);
        temp.GetComponent<NetworkObject>().Spawn();
    }
    bool checkIfSphereFilled(Vector3 position)
    {

        float radius = 5f;
        Vector3[] posTocheck =  {
            position + new Vector3(1,0,0) * radius,
            position + new Vector3(-1,0,0) * radius,
            position + new Vector3(0,1,0) * radius,
            position + new Vector3(0,-1,0) * radius,
            position + new Vector3(0,0,1) * radius,
            position + new Vector3(0,0,-1) * radius,
            position
        };
        foreach (Vector3 point in posTocheck)
        {
            if (ChunkManager.Instance.GetMaterialFromPoint(point) == Constants.NUMBER_MATERIALS)
                return false;

        }

        return true;
    }
    bool spawnCreepInFront(Transform playerTransform)
    {
        float radius = 15f;
        Vector3 position = players[0].transform.position;
        Vector3[] posTocheck =  {
            position + new Vector3(1,0,0) * radius,
            position + new Vector3(-1,0,0) * radius,
            position + new Vector3(0,0,1) * radius,
            position + new Vector3(0,0,-1) * radius,
        };
        foreach (Vector3 point in posTocheck)
        {
            //Debug.Log("Checkingpoint: " + (point));
            foreach (RaycastHit hit in Physics.RaycastAll(point + new Vector3(0f, 10f, 0f), Vector3.down, 15f))
            {
                // Debug.DrawRay(hit.point, hit.normal);
                //Debug.Log("Normal: " + hit.normal);
                float dot = Vector3.Dot(hit.normal, Vector3.down);
                if (dot < 0)
                {
                    // Debug.Log("opposing");
                    spawnWaveCreepOnPlayer(hit.point, players[0].transform);
                    GameObject tempCreep = Instantiate(creepScoutPrefab, hit.point, quaternion.identity);
                    enemyInstances.Add(tempCreep);
                    tempCreep.GetComponent<NetworkObject>().Spawn();
                    return true;
                }
            }
        }
        return false;

    }
    public void StartEndingRitualAssault()
    {
        if (!IsServer)
            return;
        spawnMinotaurNearPlayerInRock(players[0].transform);
        float radius = 10f;
        Vector3 position = players[0].transform.position;
        Vector3[] posTocheck =  {
            position + new Vector3(1,0,0) * radius,
            position + new Vector3(-1,0,0) * radius,
            position + new Vector3(0,0,1) * radius,
            position + new Vector3(0,0,-1) * radius,
        };
        foreach (Vector3 point in posTocheck)
        {
            Debug.Log("Checkingpoint: " + (point));
            foreach (RaycastHit hit in Physics.RaycastAll(point + new Vector3(0f, 10f, 0f), Vector3.down))
            {
                Debug.DrawRay(hit.point, hit.normal);
                Debug.Log("Normal: " + hit.normal);
                float dot = Vector3.Dot(hit.normal, Vector3.down);
                if (dot < 0)
                {
                    Debug.Log("opposing");
                    spawnWaveCreepOnPlayer(hit.point, players[0].transform);
                    return;
                }
            }
        }
    }
}
