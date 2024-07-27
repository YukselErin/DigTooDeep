using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinotaurTrigger : MonoBehaviour
{
    public MinataurAI minataurAI;
    // Start is called before the first frame update
    void OnTriggerEnter(Collider collider)
    {
        minataurAI.triggerEnter(collider);
    }
    void OnTriggerExit(Collider collider)
    {
        minataurAI.triggerExit(collider);

    }
}
