using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerVoiceSource : NetworkBehaviour
{
    public void AttachToPlayerPrefab(ulong id)
    {
        NetworkObject temp;
        foreach (GameObject g in GameObject.FindGameObjectsWithTag("Player"))
        {
            if (g.transform.parent.TryGetComponent(out temp))
            {
                Debug.Log(temp.OwnerClientId + "given id: " + id);
                if (temp.OwnerClientId == id)
                {
                    transform.position = g.transform.position;
                    transform.SetParent(g.transform, false);
                }
            }

        }
    }

    // Update is called once per frame
}
