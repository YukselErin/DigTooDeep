using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class EndingRitualUI : NetworkBehaviour
{
    public GameObject endCanvas;
    public GameObject warningCanvas;
    bool ritualStarted = false;
    public static EndingRitualUI Instance { get; private set; }
    List<PlayerEnemyTarget> players;
    List<bool> playerActive;

    void Awake()
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
        playerActive = new List<bool>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        endCanvas.SetActive(false);
        warningCanvas.SetActive(false);
    }
    void getPlayers()
    {
        if (playerActive.Count == 0)
        {
            players = EnemySpawnController.Instance.players;
            foreach (var p in players)
            {
                playerActive.Add(false);
            }
        }

    }
    public bool debugPlayersNotIn = true;
    bool checkIfAllPlayersIn()
    {
        if (debugPlayersNotIn) return false;
        bool temp = true;
        foreach (bool state in playerActive)
        {
            temp = temp & state;
        }
        return temp;
    }
    public void activateUI()
    {
        if (ritualStarted) return;
        if (checkIfAllPlayersIn())
        {
            endCanvas.SetActive(true);
        }

        else
        {
            warningCanvas.SetActive(true);
        }
    }
    public void playerEntered()
    {
        getPlayers();
        changePlayerStateRPC(NetworkManager.LocalClientId, true);
    }
    public void playerLeft()
    {
        getPlayers();

        changePlayerStateRPC(NetworkManager.LocalClientId, false);

    }
    public float endTime = 60f;
    [Rpc(SendTo.Server)]
    public void startEndingRitualRPC() //called with button press
    {
        if (ritualStarted) return;

        if (checkIfAllPlayersIn())
        {
            ritualStarted = true;
            EnemySpawnController.Instance.StartEndingRitualAssault();
            if (IsServer || IsHost)
            {
                Invoke("waitBeforeInvoke", endTime);
            }
        }

    }
    void waitBeforeInvoke()
    {
        GameStateController.Instance.CycleWonReload();
        ritualStarted = false;
    }
    [Rpc(SendTo.Server)]
    void changePlayerStateRPC(ulong id, bool state)
    {
        getPlayers();
        NetworkObject playerNO;
        for (int i = 0; i < players.Count; i++)
        {
            if (!players[i])
            {
                players.Remove(players[i]);
            }
            if (players[i].transform.parent.gameObject.TryGetComponent<NetworkObject>(out playerNO))
            {
                if (playerNO.OwnerClientId == id)
                {
                    playerActive[i] = state;
                }
            }


        }
    }
}
