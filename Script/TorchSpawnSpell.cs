using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Timeline;
using Unity.Netcode;


public class TorchSpawnSpell : PlayerSpell
{
    bool listeningToReset = false;
    List<GameObject> playerSpawnedObjects = new List<GameObject>();
    NetworkObject networkObject;
    void destroyPlayerSpawnedObjects()
    {
        foreach (GameObject gameObject in playerSpawnedObjects.ToArray())
        {
            if (!gameObject)
            {
                playerSpawnedObjects.Remove(gameObject);
                continue;
            }
            if (gameObject.TryGetComponent(out networkObject))
            {
                networkObject.Despawn();
            }
            Destroy(gameObject);
        }
        playerSpawnedObjects.Clear();
    }
    void listenToReset()
    {
        if (!listeningToReset)
        {
            listeningToReset = true;
            GameStateController.Instance.GameRestartEvent.AddListener(destroyPlayerSpawnedObjects);
        }
    }

    public int usedOreType = 4;

    public float useAmount = 1f;
    public float defaultUseAmount = 1f;

    public AudioClip placeSound;
    AudioSource audioSource;
    public GameObject torchprefab;
    GameObject torchInstance;
    bool Instantiated = false;
    PlayerControls inputActions;
    InputAction CancelInvoke;
    bool canceled = false;

    public override int getOreType()
    {
        return usedOreType;
    }
    void Awake()
    {
        inputActions = new PlayerControls();

    }
    void OnEnable()
    {
        CancelInvoke = inputActions.Player.CancelCast;
        CancelInvoke.Enable();
    }

    void OnDisable()
    {
        CancelInvoke.Disable();

    }
    PlayerCollectOre playerCollectOre;
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        playerCollectOre = GetComponent<PlayerCollectOre>();
        Upgrades.Instance.newUpgradeEvent.AddListener(HandleUpgradeEvent);
    }
    void HandleUpgradeEvent(ulong ID, string upgradeName)
    {
        if (ID == NetworkManager.Singleton.LocalClientId)
        {

            if (upgradeName == "torchSavingOre")
            {
                useAmount = defaultUseAmount - 0.05f * Upgrades.Instance.getUpgradeLevel(upgradeName);
                useAmount = Mathf.Max(useAmount, 0.01f);
            }
        }
    }

    RaycastHit raycastHit;
    public float range = 5f;
    // Update is called once per frame
    public override void StartCast()
    {
        listenToReset();
        canceled = false;
        Instantiated = false;
        torchInstance = Instantiate(torchprefab);

        if (torchInstance.TryGetComponent(out networkObject))
        {
            networkObject.Spawn();
        }
        if (Physics.Raycast(transform.position, transform.forward, out raycastHit, range))
        {
            Instantiated = true;
            torchInstance.transform.position = raycastHit.point;
        }
    }
    public override void EndCast()
    {
        if (canceled || !Instantiated)
        {
            Destroy(torchInstance);
            return;
        }
        if (torchInstance)
        {
            playerSpawnedObjects.Add(torchInstance);
        }
        audioSource.PlayOneShot(placeSound);

    }
    public override bool ChannelingCost()
    {
        if (canceled)
        {
            Destroy(torchInstance);
            return false;

        }
        if (Physics.Raycast(transform.position, transform.forward, out raycastHit, range))
        {
            Instantiated = true;
            torchInstance.transform.position = raycastHit.point;
        }
        if (CancelInvoke.IsPressed())
        {
            canceled = true;
        }
        return true;

    }
    public override bool checkCost()
    {
        /*
        if (playerCollectOre.ores.ContainsKey(usedOreType))
        { return playerCollectOre.ores[usedOreType] > useAmount; }
        else { return false; }*/
        return playerCollectOre.tryUseOre(usedOreType, useAmount);

    }
}
