using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.UI;

public class EnemyHealth : NetworkBehaviour
{
    public float maxHealth = 100f;
    public float hitPoints;
    public UnityEvent OnDeathEnemy;
    public AudioSource[] audioSources;

    public void damage() { }
    void Start()
    {
        if (OnDeathEnemy == null)
            OnDeathEnemy = new UnityEvent();
        hitPoints = maxHealth;
        TryGetComponent(out navMeshAgent);
        speedBeforeFreeze = navMeshAgent.speed;
        angularSpeedBeforeFreeze = navMeshAgent.angularSpeed;
        AudioSource temp;
        if (TryGetComponent(out temp))
        {
            audioSources.Append(temp);
        }
    }
    [Rpc(SendTo.Server)]
    public void dealDamageRPC(float value)
    {
        hitPoints -= value;
        if (hitPoints < 0f)
        {
            OnDeathEnemy.Invoke();
            EnemySpawnController.Instance.destroyEnemy(gameObject);

        }
    }
    public void dealDamage(float value)
    {
        dealDamageRPC(value);
    }
    NavMeshAgent navMeshAgent;
    public float freezeSpeed = 10f;
    public float freezeBuildup = 0f;
    public float reverseFreezeSpeedFullFreeze;
    public float reverseFreezeSpeed;
    bool fullyFrozen = false;
    float speedBeforeFreeze;
    float angularSpeedBeforeFreeze;
    float lastTimeFreezeApplied;
    public Animator animator;

    void updateSpeed()
    {
        if (freezeBuildup >= 100f)
        {
            fullyFrozen = true;
            freezeBuildup = 100f;
        }
        if (freezeBuildup > 60f)
        {
            foreach (AudioSource audioSource in audioSources)
            {
                audioSource.volume = 0f;
            }
        }
        else
        {
            foreach (AudioSource audioSource in audioSources)
            {
                audioSource.volume = 1f;
            }
        }


        Debug.Log("freeze:" + freezeBuildup);
        if (0f >= freezeBuildup)
        {
            freezeBuildup = 0f;
            navMeshAgent.speed = speedBeforeFreeze;
            navMeshAgent.angularSpeed = angularSpeedBeforeFreeze;
        }
        else
        {
            navMeshAgent.speed = speedBeforeFreeze / freezeBuildup;
            navMeshAgent.angularSpeed = angularSpeedBeforeFreeze / freezeBuildup;
        }
        animator.speed = (100f - freezeBuildup) / 100f;
        Debug.Log("animator speed: " + animator.speed);
    }
    bool coroutineRunning = false;

    [Rpc(SendTo.Server)]
    public void dealFreezeRPC(float amount)
    {
        lastTimeFreezeApplied = Time.time;
        freezeBuildup += amount;
        updateSpeed();

        if (!coroutineRunning)
        {
            StartCoroutine(reverseFreezing());
        }
    }
    public void dealFreeze(float amount)
    {
        dealFreezeRPC(amount);

    }
    public float freezeUpdateSpeed = 0.1f;
    IEnumerator reverseFreezing()
    {
        while (true)
        {
            if (freezeBuildup >= 100f)
            {
                fullyFrozen = true;
                freezeBuildup = 100f;
            }
            if (freezeBuildup < 70)
            {
                fullyFrozen = false;
            }
            updateSpeed();
            if (freezeBuildup <= 0f)
            {
                coroutineRunning = false;
                StopAllCoroutines();
            }
            if (fullyFrozen)
            {
                freezeBuildup -= reverseFreezeSpeedFullFreeze;
                yield return new WaitForSeconds(freezeUpdateSpeed);
            }
            else
            {
                freezeBuildup -= reverseFreezeSpeed;
                yield return new WaitForSeconds(freezeUpdateSpeed);
            }

        }
    }
}
