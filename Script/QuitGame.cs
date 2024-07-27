using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class QuitGame : MonoBehaviour
{


    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            gameObject.transform.GetChild(0).gameObject.SetActive(!gameObject.transform.GetChild(0).gameObject.activeSelf);
        }
    }
    public void ShutdownManager()
    {
        GameStateController.Instance.quitLobby();
        NetworkManager.Singleton.Shutdown();
    }
}
