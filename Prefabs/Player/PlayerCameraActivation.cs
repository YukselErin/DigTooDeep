using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class PlayerCameraActivation : NetworkBehaviour
{
    Camera camera;
    CinemachineVirtualCamera cinemachineVirtualCamera;
    void Start()
    {
        if (!IsOwner)
        {
            if (gameObject.TryGetComponent<Camera>(out camera))
            {
                camera.enabled = false;
                gameObject.GetComponent<AudioListener>().enabled = false;
            }
            else
            {

            }


        }
        else
        {
            string[] layers = { "Default", "XRAY", "RenderOnTop" };
            if (TryGetComponent<Camera>(out camera))
            {
                camera.cullingMask = LayerMask.GetMask(layers);
            }
            if (TryGetComponent(out cinemachineVirtualCamera))
                cinemachineVirtualCamera.enabled = true;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
