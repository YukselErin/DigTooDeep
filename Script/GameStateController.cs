using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using Cinemachine;
using UnityEngine.Events;

public class GameStateController : NetworkBehaviour
{
    public static GameStateController Instance { get; private set; }
    public UnityEvent GameRestartEvent;
    public UnityEvent PlayerDeathEvent;
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
        if (GameRestartEvent == null)
            GameRestartEvent = new UnityEvent();

        if (PlayerDeathEvent == null)
            PlayerDeathEvent = new UnityEvent();

        waystones = new List<GameObject>();
    }
    public void startingNetworkLoading()
    {
        enabled = true;
        loadScreen.SetActive(true);
        if (mainMenu)
        {
            mainMenu.SetActive(false);
        }
    }
    GameObject player;
    public Transform spawnPos;
    NetworkTransform networkTransform;
    public GameObject loadScreen;
    public GameObject mainMenu;
    public List<GameObject> players = new List<GameObject>();
    void getPlayers()
    {
        PlayerHealth playerHealth;
        players.Clear();
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("Player"))
        {
            if (go.transform.parent.gameObject.TryGetComponent(out playerHealth))
                players.Add(go.transform.parent.gameObject);
        }
    }
    public void listenPlayerDeaths()
    {
        getPlayers();
        Debug.Log("players count:" + players.Count);
        foreach (GameObject playerGO in players)
        {
            playerGO.transform.parent.GetComponentInChildren<PlayerHealth>().PlayerDeathEvent.RemoveListener(HandlePlayerDeathRPC);
            playerGO.transform.parent.GetComponentInChildren<PlayerHealth>().PlayerDeathEvent.AddListener(HandlePlayerDeathRPC);
        }

    }
    [Rpc(SendTo.Everyone)]
    void HandlePlayerDeathRPC()
    {

        PlayerDeathEvent.Invoke();
        Debug.Log("player death");

        bool allDead = true;
        foreach (GameObject playerGO in players)
        {
            allDead = allDead && playerGO.GetComponentInChildren<PlayerHealth>().GetPlayerIsDead();
        }

        if (allDead)
        {
            if (!IsOwner)
                return;
            Debug.Log("reload");

            ReloadNewWorldRPC(Random.Range(0, amountOfSavedWorlds + 1));

        }
    }
    void OnServerStopped(bool state)
    {
        Debug.Log("OnServerStopped");
        DisconnectedFromGame();
    }
    void OnClientStopped(bool state)
    {
        Debug.Log("OnClientStopped");
        DisconnectedFromGame();
    }
    void ListenHooks()
    {
        NetworkManager.OnConnectionEvent += OnConnectionEvent;
        NetworkManager.OnServerStopped += OnServerStopped;
        NetworkManager.OnClientStopped += OnClientStopped;
        listenPlayerDeaths();
        /*
        foreach (var go in GameObject.FindGameObjectsWithTag("Player"))
        {
            if (go.TryGetComponent<NetworkTransform>(out networkTransform))
            {
                player.GetComponent<CharacterController>().enabled = false;
                
                if (networkTransform.IsOwner)
                {
                    networkTransform.SyncPositionX = false;
                    networkTransform.SyncPositionY = false;
                    networkTransform.SyncPositionZ = false;
                    player = go;

                    break;
                }

                }
            }
            enabled = false;
            enabled = true;
            player.transform.position = spawnPos.position; */
    }
    public myPerlinWorm[] myPerlinWorms;
    void activateWorms()
    {
        foreach (myPerlinWorm mp in myPerlinWorms)
        {
            mp.runWorm2();
        }
    }
    public CreateSpawnRoom createSpawnRoom;
    public string worldname = "";
    public int amountOfSavedWorlds = 4;
    public GameObject deathScreen;
    public GameObject cycleWonCanvas;

    public void CycleWonReload()
    {
        cycleWonCanvas.SetActive(true);
        ReloadNewWorldRPC(Random.Range(0, amountOfSavedWorlds + 1));
    }

    [Rpc(SendTo.Everyone)]
    public void ReloadNewWorldRPC(int worldname)
    {

        this.worldname = worldname.ToString();
        deathScreen.SetActive(true);
        Invoke("reloadWorld", 4f);
    }
    public void quitLobby()
    {
        GameRestartEvent.Invoke();
    }
    public void reloadWorld()
    {
        GameRestartEvent.Invoke();
        setupWorld();
        deathScreen.SetActive(false);
        cycleWonCanvas.SetActive(false);

    }
    void setupWorld()
    {
        listenPlayerDeaths();

        WorldManager.Instance.world = worldname;

        NoiseManager.Instance.StartNoiseManager();

        StartCoroutine(waitForChunksAndSpawnPlayer());

        createSpawnRoom.CreateSpawnRoomAtStart();
        activateWorms();
        if (IsOwnedByServer)
            placeWaystones();
        transportPlayer();

    }
    IEnumerator waitForChunksAndSpawnPlayer()
    {
        loadScreen.SetActive(true);

        while (!ChunkManager.Instance.allChunksLoaded)
        {
            yield return null;

        }
        transportPlayer();
        loadScreen.SetActive(false);

    }
    public bool syncTransforms = true;
    public float scatterSpawnsBy = 0.5f;
    void transportPlayer()
    {

        Vector3[] posTocheck =  {
             new Vector3(1,0,0) * scatterSpawnsBy,
             new Vector3(-1,0,0) * scatterSpawnsBy,
             new Vector3(0,0,1) * scatterSpawnsBy,
             new Vector3(0,0,-1) * scatterSpawnsBy
        };
        int playerno = 0;
        foreach (GameObject playerCapsule in players)
        {
            playerCapsule.transform.position = spawnPos.position + posTocheck[playerno];
            playerCapsule.GetComponent<CharacterController>().enabled = true;
            playerCapsule.GetComponent<NetworkTransform>().SyncPositionX = syncTransforms;
            playerCapsule.GetComponent<NetworkTransform>().SyncPositionY = syncTransforms;
            playerCapsule.GetComponent<NetworkTransform>().SyncPositionZ = syncTransforms;
            playerno++;
        }
    }
    bool started = false;
    void StartVoice()
    {
        Voice.Instance.ActivateVoice();
    }
    void Update()
    {
        if (ChunkManager.Instance.allChunksLoaded && !started)
        {
            StartVoice();
            setupWorld();
            ListenHooks();
            started = true;
            Invoke("transportPlayer", 0.1f);
        }
    }
    public bool placeWaystonesTriggerBool = false;
    List<GameObject> waystones;
    public GameObject upgradeWaystonePrefab;
    public GameObject endWaystonePrefab;
    public int upgradeStoneAmount;
    public int endStoneAmount;
    GameObject temp;
    public float stoneSpread = 60f;
    public float gameHeightRange = 42f;
    public float endStoneYmax = 0f;
    public float endStoneYmin = 0f;
    NetworkObject networkObject;
    void placeWaystones()
    {
        foreach (GameObject go in waystones)
        {

            if (!go)
            {
                waystones.Remove(go);

            }
            else
            {
                if (go.TryGetComponent(out networkObject))
                {
                    networkObject.Despawn();
                }

                Destroy(go);
            }

        }
        waystones.Clear();
        for (int i = 0; i < upgradeStoneAmount; i++)
        {
            temp = Instantiate(upgradeWaystonePrefab, transform);
            temp.transform.position = new Vector3(Random.Range(-stoneSpread, stoneSpread), Random.Range(gameHeightRange, -gameHeightRange), Random.Range(-stoneSpread, stoneSpread));
            waystones.Add(temp);
        }
        for (int i = 0; i < endStoneAmount; i++)
        {
            temp = Instantiate(endWaystonePrefab, transform);
            temp.transform.position = new Vector3(Random.Range(-stoneSpread, stoneSpread), Random.Range(endStoneYmax, endStoneYmin), Random.Range(-stoneSpread, stoneSpread));
            waystones.Add(temp);
        }
    }

    public CinemachineVirtualCamera GetAlivePlayerCamera()
    {
        foreach (var playerGO in players)
        {
            if (!playerGO.GetComponentInChildren<PlayerHealth>().GetPlayerIsDead())
            {
                return playerGO.GetComponentInChildren<Spectate>().mainCamera;
            }
        }
        return null;
    }

    void DisconnectedFromGame()
    {
        LobbyManager.Instance.LeaveLobby();
        enabled = false;
        Debug.Log("disconnect");
        started = false;
        players.Clear();
        mainMenu.SetActive(true);
        MenuCamera.Instance.gameObject.SetActive(true);
    }
    void OnConnectionEvent(NetworkManager networkManager, ConnectionEventData connectionEventData)
    {
        Debug.Log("event");

        if (connectionEventData.EventType == ConnectionEvent.ClientDisconnected || !networkManager.IsConnectedClient)
        {
            DisconnectedFromGame();
        }
    }
}
