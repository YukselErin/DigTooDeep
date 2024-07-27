using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayersOwnModelSetScript : NetworkBehaviour
{
    public GameObject model;
    public GameObject axeModel;
    void Start()
    {
        if (IsOwner)
        {
            int maskint = LayerMask.NameToLayer("PlayersOwnModel");
            model.layer = maskint;
            axeModel.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
