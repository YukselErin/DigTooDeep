using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class DisableLobbyMenu : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        NetworkManager.Singleton.OnClientStarted += () => { gameObject.SetActive(false); };

    }

    // Update is called once per frame
    void Update()
    {

    }
}
