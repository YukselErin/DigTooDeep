using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class Upgrades : NetworkBehaviour
{
    public string[] UpgradeKeywords;
    public UpgradeSO[] possibleUpgradeSOs;
    Dictionary<ulong, Dictionary<string, int>> choosenUpgrades;
    [System.Serializable]
    public class NewUpgrade : UnityEvent<ulong, string>
    {
    }
    public UnityEvent noUpgrade;
    public NewUpgrade newUpgradeEvent;
    public static Upgrades Instance { get; private set; }
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
        if (newUpgradeEvent == null)
            newUpgradeEvent = new NewUpgrade();

        if (noUpgrade == null)
            noUpgrade = new UnityEvent();

    }

    void Start()
    {
        choosenUpgrades = new Dictionary<ulong, Dictionary<string, int>>();

    }
    bool listening = false;
    public void getPlayers()
    {
        Debug.Log("getplayers count: " + GameStateController.Instance.players.Count);
        foreach (GameObject playerGO in GameStateController.Instance.players)
        {
            playerGO.transform.parent.GetComponentInChildren<PlayerHealth>().PlayerDeathEvent.RemoveListener(HandlePlayerDeath);
            playerGO.transform.parent.GetComponentInChildren<PlayerHealth>().PlayerDeathEvent.AddListener(HandlePlayerDeath);
        }
    }
    List<string> keys;
    void HandlePlayerDeath()    //REVERSE ALL UPGRADES BY GIVING UPGRADE LEVEL 0
    {
        keys = new List<string>();
       // Debug.Log("handle: " + GameStateController.Instance.players.Count);

        foreach (GameObject playerGO in GameStateController.Instance.players)
        {
           // Debug.Log("trying: ");

            if (playerGO.transform.parent.GetComponentInChildren<PlayerHealth>().GetPlayerIsDead())
            {
                //Debug.Log("getplayerdead: " + GameStateController.Instance.players.Count);

                ulong deadClientID = playerGO.transform.parent.GetComponentInChildren<PlayerHealth>().OwnerClientId;
                foreach (var keyvalue in choosenUpgrades[deadClientID])
                {
                    keys.Add(keyvalue.Key);
                }
                foreach (var key in keys)
                {
                    choosenUpgrades[deadClientID][key] = 0;
                    newUpgradeEvent.Invoke(deadClientID, key);
                }
                choosenUpgrades[deadClientID].Clear();
                choosenUpgrades[deadClientID] = new Dictionary<string, int>();
            }
        }

    }
    void OnNetworkConnect()
    {
        addPlayerRPC(NetworkManager.Singleton.LocalClientId);

    }
    void syncNamesFromSOs()
    {
        foreach (UpgradeSO so in possibleUpgradeSOs)
        {
            if (!UpgradeKeywords.Contains(so.upgradeName))
            {
                UpgradeKeywords.Append(so.upgradeName);
            }
        }
    }
    public int getRandomUpgradeChoice()
    {
        bool upgradeavailable = true;
        int returnIndex = -1;
        int trialTimes = 20;
        do
        {
            returnIndex = Random.Range(0, possibleUpgradeSOs.Length);
            UpgradeSO so = possibleUpgradeSOs[returnIndex];
            foreach (string preq in so.prerequisite)
            {
                if (!UpgradeKeywords.Contains(preq))
                {
                    upgradeavailable = false;
                }
            }
            if (UpgradeKeywords.Contains(so.upgradeName))
            {
                upgradeavailable = false;
            }
            trialTimes--;
        }
        while (!upgradeavailable && trialTimes > 0);

        return upgradeavailable ? returnIndex : -1;
    }


    // [Rpc(SendTo.Everyone)]
    public void addPlayerRPC(ulong playerID)
    {
        if (!choosenUpgrades.ContainsKey(playerID))
        {
            choosenUpgrades.Add(playerID, new Dictionary<string, int>());
            //Debug.Log("ID added: " + playerID);

        }
        else
        {
            //Debug.Log("Already has the id!");
        }
    }
    public int getUpgradeLevel(string upgradeName)
    {
        return choosenUpgrades[NetworkManager.LocalClientId][upgradeName];
    }
    public void addUpgradeToPlayer(ulong playerID, string upgradeName)
    {
        if (!listening)
        {
            GameStateController.Instance.PlayerDeathEvent.AddListener(HandlePlayerDeath);
            listening = true;
        }
        Debug.Log("ID upgraded: " + playerID);

        if (!choosenUpgrades.ContainsKey(playerID))
        {
            addPlayerRPC(playerID);
            Debug.Log("Can't find player!");
        }
        if (!choosenUpgrades[playerID].ContainsKey(upgradeName))
        {
            choosenUpgrades[playerID].Add(upgradeName, 1);
            UpgradeKeywords.Append(upgradeName);
            Debug.Log(playerID + " Upgrade added " + upgradeName);
            newUpgradeEvent.Invoke(playerID, upgradeName);

        }
        else
        {
            Debug.Log(playerID + " Upgrade level up " + upgradeName);
            choosenUpgrades[playerID][upgradeName]++;
            newUpgradeEvent.Invoke(playerID, upgradeName);

        }


    }
}
