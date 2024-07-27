using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Rendering;
using Unity.Netcode;
public class CollectOre : MonoBehaviour
{
    public static CollectOre Instance { get; private set; }
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
    }
    public List<PlayerCollectOre> registered;
    public void register(PlayerCollectOre registerScript)
    {
        registered.Add(registerScript);
    }


    public void collected(int type, float value, ulong givenClientID)
    {
        foreach (var script in registered)
        {
            script.oreCollected(type, value, givenClientID);
        }

    }
    void Start()
    {
        registered = new List<PlayerCollectOre>();
    }

}
