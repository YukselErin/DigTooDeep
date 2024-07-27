using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class EnemyAttack : NetworkBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        attackHash = Animator.StringToHash("Attack");
    }
    public AudioSource audioSource;
    public AudioClip[] attackSounds;
    int attackHash;
    public Animator animator;
    public float assesCD = 30f;
    float lastAsses;
    public float attackRange = 1f;
    Transform targetTransform;
    void Update()
    {
        if (lastAsses + assesCD < Time.time)
        {
            lastAsses = Time.time;
            if (!SearchTarget()) return;
            AttackAnimationRPC();
            //Invoke("DealDamage", 0.2f);
        }
    }
    bool SearchTarget()
    {
        foreach (var player in EnemySpawnController.Instance.players)
        {
            if (Vector3.Distance(player.transform.position, transform.position) < attackRange)
            {
                targetTransform = player.transform;
                return true;
            }
            else
            {
                targetTransform = null;
            }
        }
        return false;

    }
    [Rpc(SendTo.Everyone)]
    void AttackAnimationRPC()
    {
        animator.SetTrigger(attackHash);
        audioSource.PlayOneShot(attackSounds[Random.Range(0, attackSounds.Length)]);

    }
    public void DealDamage()//Can be called if animation connects
    {
        if (!SearchTarget()) return;
        var heading = targetTransform.position - transform.position;
        float dot = Vector3.Dot(heading, transform.forward);
        if (dot > 0)
            targetTransform.GetComponent<PlayerHealth>().dealDamage(10f);
    }
}
