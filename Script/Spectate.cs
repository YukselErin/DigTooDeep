using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Unity.Netcode;

public class Spectate : NetworkBehaviour
{
    public CinemachineVirtualCamera mainCamera;
    public CinemachineVirtualCamera temporarySpectateCamera;
    public bool spectating = false;

    GameObject spectatedPlayersModel;
    void Start()
    {
        if (IsOwner)
        {
            GetComponentInChildren<PlayerHealth>().PlayerDeathEvent.AddListener(HandlePlayerDeath);
            GameStateController.Instance.GameRestartEvent.AddListener(HandlePlayerRespawn);
        }
    }

    void HandlePlayerDeath()
    {
        if (IsOwner)
        {
            temporarySpectateCamera = GameStateController.Instance.GetAlivePlayerCamera();
            if (temporarySpectateCamera)
            {
                spectatedPlayersModel = temporarySpectateCamera.transform.parent.GetComponentInChildren<PlayersOwnModelSetScript>().model;
                spectatedPlayersModel.SetActive(false);
                spectating = true;
                temporarySpectateCamera.enabled = true;
                mainCamera.enabled = false;
            }
        }
    }

    public void HandlePlayerRespawn()
    {
        if (IsOwner)
        {
            spectating = false;
            if (temporarySpectateCamera)
            {
                temporarySpectateCamera.enabled = false;
                temporarySpectateCamera = null;


            }
            if (spectatedPlayersModel)
            {
                spectatedPlayersModel.SetActive(true);
                spectatedPlayersModel = null;
            }
            mainCamera.enabled = true;
        }
    }
}
