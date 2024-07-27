using UnityEngine;
using UnityEngine.AI;

public class scoutCreepAI : MonoBehaviour
{
    NavMeshAgent navMeshAgent;
    Vector3 initialPos;
    Vector3 spawnPos;
    public bool isScout = false;
    float initiateTime = 0f;
    void Start()
    {
        initialPos = new Vector3(0f, 0f, 0f);
        spawnPos = transform.position;
        GetComponentInChildren<EnemyHealth>().OnDeathEnemy.AddListener(scoutDeath);
        navMeshAgent = GetComponent<NavMeshAgent>();
    }
    void scoutDeath()
    {
        if (isScout && initiateTime < 60f * 2)
        {
            EnemySpawnController.Instance.spawnWaveCreepOnPlayer(spawnPos, transform);
        }
    }
    public void initiateFirstPath(Transform playertransform)
    {
        if (playertransform == null)
            return;
        initialPos = playertransform.position;
        if (TryGetComponent(out navMeshAgent))
        {
            navMeshAgent.SetDestination(playertransform.position);
        }
        initiateTime = Time.time;
    }
    RaycastHit raycastout;

    bool checkPlayerLOS()
    {
        float playerDistance = 999f;
        Vector3 target = new Vector3(); ;
        foreach (PlayerEnemyTarget playerEnemyTarget in EnemySpawnController.Instance.players)
        {

            if (Physics.Raycast(transform.position, playerEnemyTarget.transform.position - transform.position, out raycastout))
            {
                if (raycastout.collider.tag == "Player")
                {
                    if (Vector3.Distance(raycastout.collider.transform.position, transform.position) < playerDistance)
                    {
                        target = raycastout.transform.position;
                        playerDistance = Vector3.Distance(raycastout.collider.transform.position, transform.position);

                    }
                }

            }
            if (Physics.Raycast(transform.position + new Vector3(0f, 2f, 0f), playerEnemyTarget.transform.position - transform.position, out raycastout))
            {
                if (raycastout.collider.tag == "Player")
                {
                    if (Vector3.Distance(raycastout.collider.transform.position, transform.position) < playerDistance)
                    {
                        target = raycastout.transform.position;
                        playerDistance = Vector3.Distance(raycastout.collider.transform.position, transform.position);

                    }
                }
            }
        }
        if (playerDistance < 999f)
        {
            navMeshAgent.SetDestination(target);
            return true;
        }

        return false;
    }
    Vector3 previousAssesPos;

    public float stuckThreshold = 0.5f;
    public bool accountFrameTimeForStuck = false;

    bool checkIfStuck()
    {
        float stuckTh = stuckThreshold * (accountFrameTimeForStuck ? Time.deltaTime : 1f);

        if (Vector3.Distance(previousAssesPos, transform.position) < stuckTh)
        {
            // Debug.Log("Stuck!");
            return true;
        }

        return false;
    }
    float lastAsses = 0f;
    public float assesCD = 0.1f;
    bool chasing = true;
    int stuckForAsses = 0;
    public AudioSource growlSource;
    public AudioClip idle;
    public AudioClip chasingClip;
    float lastTimeSoundPlayed;
    public float soundPeriod;
    bool previouslyChasing = false;
    void playAudio()
    {
        if (!previouslyChasing && chasing)
        {
            previouslyChasing = true;
            growlSource.clip = chasingClip;
            growlSource.Play();
            lastTimeSoundPlayed = Time.time;
        }
        if (!growlSource.isPlaying && lastTimeSoundPlayed + soundPeriod < Time.time)
        {
            if (chasing)
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
    void Update()
    {
        playAudio();

        if (lastAsses + assesCD < Time.time)
        {
            lastAsses = Time.time;

            if (checkPlayerLOS())
            {
                chasing = true;
            }
            if (chasing)
            {
                chase();
                if (checkIfStuck())
                    stuckForAsses++;
                if (stuckForAsses > 3)
                {
                    stuckForAsses = 0;
                    chasing = false;
                }

            }
            else
            {
                patrol();

            }
            previousAssesPos = transform.position;

        }
    }
    void chase()
    {

    }
    void patrol()
    {
        int rotateTimes = 0;
        if (checkIfStuck())
        {
            navMeshAgent.SetDestination(transform.position + transform.forward * 1f);
            return;
            while (rotateTimes < 10)
            {
                transform.Rotate(new Vector3(0f, 10f, 0f));
                rotateTimes++;
                if (Physics.Raycast(transform.position, transform.forward, out raycastout))
                {

                    if (Vector3.Distance(raycastout.point, transform.position) > 0.1f)
                        navMeshAgent.SetDestination(raycastout.point);
                }
            }
        }
    }
}
