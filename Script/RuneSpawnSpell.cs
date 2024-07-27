using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Unity.Netcode;

public class RuneSpawnSpell : PlayerSpell
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
    public int usedOreType = 1;
    public float useAmount = 1f;
    public float defaultUseAmount = 1f;

    public AudioClip placeSound;
    AudioSource audioSource;
    public GameObject runePrefab;
    GameObject runeInstance;

    public override int getOreType()
    {
        return usedOreType;
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

            if (upgradeName == "runeSavingOre")
            {
                useAmount = defaultUseAmount - 0.05f * Upgrades.Instance.getUpgradeLevel(upgradeName);
                useAmount = Mathf.Max(useAmount, 0.01f);
            }
        }
    }
    RaycastHit raycastHit;
    public float range = 5f;
    // Update is called once per frame
    public float decalDistanceToHit = 0.1f;
    DecalProjector decalProjector;

    public override void StartCast()
    {
        canceled = false;
        Instantiated = false;
        listenToReset();
        runeInstance = Instantiate(runePrefab);
        decalProjector = runeInstance.GetComponent<DecalProjector>();
        decalProjector.fadeFactor = 0;
        decalProjector.GetComponentInChildren<RuneEnemyDetector>().faceTransform = transform;
        if (Physics.Raycast(transform.position, transform.forward, out raycastHit, range))
        {
            Instantiated = true;
            runeInstance.transform.position = raycastHit.point + (raycastHit.point - transform.position) * decalDistanceToHit;
            runeInstance.transform.LookAt(raycastHit.point);
        }
    }
    public float fadeintime = 0.02f;
    private bool canceled;

    public bool Instantiated { get; private set; }

    IEnumerator fadein()
    {
        float time = 0f;
        while (time < 0.1f)
        {
            time += fadeintime;

            decalProjector.fadeFactor = time;
            yield return null;
        }
    }
    public override void EndCast()
    {
        if (canceled || !Instantiated)
        {
            Destroy(runeInstance);
            return;
        }
        if (runeInstance)
        {
            playerSpawnedObjects.Add(runeInstance);
        }
        audioSource.PlayOneShot(placeSound);
        StartCoroutine(fadein());

    }
    public override bool ChannelingCost()
    {
        if (Physics.Raycast(transform.position, transform.forward, out raycastHit, range))
        {
            Instantiated = true;
            runeInstance.transform.position = raycastHit.point + (raycastHit.point - transform.position) * decalDistanceToHit;
            runeInstance.transform.LookAt(raycastHit.point);
        }
        return true;

    }
    public override bool checkCost()
    {/*
        if (playerCollectOre.ores.ContainsKey(usedOreType))
        { return playerCollectOre.ores[usedOreType] > useAmount; }
        else { return false; }*/
        return playerCollectOre.tryUseOre(usedOreType, useAmount);

    }
}
