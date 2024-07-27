using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class StartHost : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    public void startHost()
    {
        NetworkManager.Singleton.StartHost();        // Starts the NetworkManager as both a server and a client (that is, has local client)

    }
    public void startClient()
    {
        NetworkManager.Singleton.StartClient();        // Starts the NetworkManager as both a server and a client (that is, has local client)

    }
}
