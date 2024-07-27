using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerUICanvas : NetworkBehaviour
{
    void Start()
    {
        if (!IsOwner)
        {
            gameObject.SetActive(false);
        }
    }
}
