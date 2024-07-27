using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinotaurAxe : MonoBehaviour
{
    PlayerHealth playerHealth;
    public float minotaurAxeDamage = 20f;
    void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.TryGetComponent(out playerHealth))
        {
            playerHealth.dealDamage(minotaurAxeDamage);
        }
    }
}
