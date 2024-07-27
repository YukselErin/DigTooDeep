using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MenuCamera : MonoBehaviour
{
    public static MenuCamera Instance { get; private set; }
    void Start()
    {
        NetworkManager.Singleton.OnClientStarted += () => { DisableMenuCamera(); };
        GameStateController.Instance.GameRestartEvent.AddListener(DisableMenuCamera);
        GameStateController.Instance.PlayerDeathEvent.AddListener(DisableMenuCamera);

    }
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
    // Update is called once per frame
    void DisableMenuCamera()
    {
        gameObject.SetActive(false);
    }
}
