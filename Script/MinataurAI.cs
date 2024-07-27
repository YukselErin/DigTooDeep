using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;
public class MinataurAI : MonoBehaviour
{
    public Animator animator;
    NavMeshAgent navMeshAgent;
    Transform transformplayer;
    int attackHash;
    AudioSource audioSource;
    void Start()
    {

        animator = GetComponent<Animator>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        attackHash = Animator.StringToHash("Attack");
        chunkManager = ChunkManagerNetwork.Instance;
        audioSource = GetComponent<AudioSource>();
    }

    bool active = false;

    private ChunkManagerNetwork chunkManager;
    bool activatePatrol = false;
    int stuckForAsses = 0;

    void Update()
    {
        playAudio();

        if (activatePatrol)
        {
            minotaurSwingAtPlayer();
            if (lastAsses + assesCD < Time.time)
            {
                lastAsses = Time.time;
                if (checkIfStuck())
                {
                    MinataurAttack();
                }
                if (noTargets())
                {
                    navMeshAgent.SetDestination(transform.position + transform.forward * 1f);

                }
                if (checkPlayerLOS())
                {
                    if (LOSPlayerTransform != null)
                    {
                        navMeshAgent.SetDestination(LOSPlayerTransform.position);
                    }

                }
            }
        }
        else
        {
            if (checkPlayerLOS())
            {
                if (LOSPlayerTransform != null)
                {
                    navMeshAgent.SetDestination(LOSPlayerTransform.position);
                    activatePatrol = true;
                }

            }
        }
        previousAssesPos = transform.position;
    }
    Transform targetTransform;
    public float attackRange = 3f;
    public float attackCD = 1f;
    public float lastAttack;
    bool SearchTarget()
    {
        foreach (var player in EnemySpawnController.Instance.players)
        {
            var heading = player.transform.position - transform.position;
            float dot = Vector3.Dot(heading, transform.forward);
            if (Vector3.Distance(player.transform.position, transform.position) < attackRange && (dot > 0))
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
    void minotaurSwingAtPlayer()
    {
        if (lastAttack + attackCD < Time.time)
        {
            if (SearchTarget())
            {
                lastAttack = Time.time;
                MinataurAttack();
            }
        }

    }
    bool noTargets()
    {
        return (castPlayerTransform) || (LOSPlayerTransform);
    }
    public float stuckMetric = .5f;
    bool checkIfStuck()
    {
        if (Vector3.Distance(previousAssesPos, transform.position) < stuckMetric * Time.deltaTime)
        {
            //Debug.Log("Stuck");
            return true;
        }

        return false;
    }
    RaycastHit raycastHit;
    public float modiferStrengh = -50f;
    [Tooltip("Size of the brush, number of vertex modified")]
    public float sizeHit = 6;
    public float digRange = 4f;
    public AudioClip hit;
    public AudioClip[] rockFalls;
    void playRockfall()
    {

        audioSource.PlayOneShot(rockFalls[UnityEngine.Random.Range(0, rockFalls.Length)]);

    }
    public float swingCD = 1f;
    float lastSwing;
    void MinataurAttack()
    {
        if (swingCD + lastSwing > Time.time)
            return;
        lastSwing = Time.time;
        animator.SetTrigger(attackHash);
        audioSource.PlayOneShot(hit);
        Vector3 swing;
        if (targetTransform)
        {
            swing = targetTransform.position - transform.position;
        }
        else
        {
            swing = transform.forward;
        }

        chunkManager.ModifyChunkData(transform.position + new Vector3(0f, 2f, 0f) + (swing * digRange), sizeHit, modiferStrengh, (ulong)99, 0, 2);
    }

    void minotaurChaseSound()
    {
        if (castPlayerTransform == null)
            return;
        //Debug.Log("event running");
        checkPlayerLOS();
        activatePatrol = true;

        if (LOSPlayerTransform != null)
        {
            if (Vector3.Distance(transform.position, LOSPlayerTransform.position) < Vector3.Distance(transform.position, castPlayerTransform.position))
            {
                navMeshAgent.SetDestination(LOSPlayerTransform.position);
                return;
            }

        }
        navMeshAgent.SetDestination(castPlayerTransform.position);
    }
    RaycastHit raycastout;
    Transform LOSPlayerTransform;
    bool checkPlayerLOS()
    {
        foreach (PlayerEnemyTarget playerEnemyTarget in EnemySpawnController.Instance.players)
        {

            if (Physics.Raycast(transform.position, playerEnemyTarget.transform.position - transform.position, out raycastout))
            {
                if (raycastout.collider.tag == "Player")
                {
                    LOSPlayerTransform = (raycastout.transform);
                    activatePatrol = true;
                    return true;
                }
            }
            if (Physics.Raycast(transform.position + new Vector3(0f, 2f, 0f), playerEnemyTarget.transform.position - transform.position, out raycastout))
            {
                if (raycastout.collider.tag == "Player")
                {
                    LOSPlayerTransform = (raycastout.transform);
                    activatePatrol = true;
                    return true;
                }
            }
        }

        return false;
    }
    Transform castPlayerTransform;
    private Vector3 previousAssesPos;
    private float lastAsses;
    public float assesCD = 0.5f;

    public void triggerEnter(Collider collider)
    {
        if (collider.tag == "Player")
        {
            castPlayerTransform = collider.transform;
            collider.transform.parent.GetComponentInChildren<CastSpell>().OnChannellingSpell.AddListener(minotaurChaseSound);
            GetComponent<AudioSource>().PlayOneShot(AmbientSounds.Instance.minotaurWarningSound[UnityEngine.Random.Range(0, AmbientSounds.Instance.minotaurWarningSound.Length)]);
        }
    }
    public void triggerExit(Collider collider)
    {
        if (collider.tag == "Player")
        {
            collider.transform.parent.GetComponentInChildren<CastSpell>().OnChannellingSpell.RemoveListener(minotaurChaseSound);
            castPlayerTransform = null;
        }
    }
    public AudioSource growlSource;
    public AudioClip idle;
    public AudioClip chasingClip;
    float lastTimeSoundPlayed;
    public float soundPeriod;
    bool previouslyChasing = false;
    void playAudio()
    {
        if (!previouslyChasing && LOSPlayerTransform)
        {
            previouslyChasing = true;
            growlSource.clip = chasingClip;
            growlSource.Play();
            lastTimeSoundPlayed = Time.time;
        }
        if (!growlSource.isPlaying && lastTimeSoundPlayed + soundPeriod < Time.time)
        {
            if (LOSPlayerTransform)
            {
                growlSource.clip = chasingClip;
                growlSource.Play();
            }
            else
            {
                growlSource.clip = idle;
                growlSource.Play();
            }
            lastTimeSoundPlayed = Time.time;

        }
    }
}
