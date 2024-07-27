using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Multiplayer.Samples.Utilities.ClientAuthority;
using Unity.Netcode;
using UnityEngine;

public class BarrierSpell : PlayerSpell
{
    bool listeningToReset = false;
    List<GameObject> playerSpawnedObjects = new List<GameObject>();
    NetworkObject networkObject;
    void destroyPlayerSpawnedObjects()
    {
        Debug.Log("resetting barriers");
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
            playerSpawnedObjects.Remove(gameObject);
            Destroy(gameObject); playerSpawnedObjects.Remove(gameObject);

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
    public int usedOreType = 3;
    public AudioClip placeSound;
    AudioSource audioSource;
    PlayerCollectOre playerCollectOre;
    public ParticleSystem particleSystem;
    public float useAmount = 1f;
    public float defaultUseAmount = 1f;
    public GameObject barrier;
    public override int getOreType()
    {
        return usedOreType;
    }
    // Start is called before the first frame update  
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

            if (upgradeName == "barrierSavingOre")
            {
                useAmount = defaultUseAmount - 0.05f * Upgrades.Instance.getUpgradeLevel(upgradeName);
                useAmount = Mathf.Max(useAmount, 0.01f);
            }
        }
    }
    public bool Instantiated { get; private set; }
    public float spawnDistance = 2f;
    public override void StartCast()
    {
        listenToReset();
        if (barrierInstance != null)
        {
            EndCast();
        }
        particleSystem.Play();
        barrierInstance = Instantiate(barrier);
        barrierInstance.GetComponent<NetworkObject>().Spawn();
        barrierInstance.GetComponent<ClientNetworkTransform>().enabled = false;
        barrierInstance.GetComponent<BarrierScript>().spawnBarrier(transform);
        barrierInstance.transform.position = barrierInstance.transform.position + barrierInstance.transform.forward * spawnDistance;
        barrierInstance.GetComponent<ClientNetworkTransform>().enabled = true;

    }
    GameObject barrierInstance;
    public override void EndCast()
    {

        if (barrierInstance)
        {
            playerSpawnedObjects.Add(barrierInstance);
        }
        audioSource.PlayOneShot(placeSound);
        particleSystem.Stop();
        barrierInstance.GetComponent<BarrierScript>().endCast();
        barrierInstance.transform.SetParent(null, true);
        barrierInstance = null;

    }
    public override bool ChannelingCost()
    {
        Physics.Raycast(transform.position, transform.right, out raycastHit);
        dist1 = raycastHit.distance;
        Physics.Raycast(transform.position, -1f * transform.right, out raycastHit);
        dist2 = raycastHit.distance;
        scale.x = (dist1 + dist2);
        Physics.Raycast(transform.position, transform.up, out raycastHit);
        dist1 = raycastHit.distance;
        Physics.Raycast(transform.position, -1f * transform.up, out raycastHit);
        dist2 = raycastHit.distance;
        scale.y = (dist1 + dist2);

        barrierInstance.transform.localScale = scale;
        return true;
    }
    RaycastHit raycastHit;
    float dist1;
    float dist2;
    Vector3 scale;
    public override bool checkCost()
    {

        return playerCollectOre.tryUseOre(usedOreType, useAmount);
    }
}
