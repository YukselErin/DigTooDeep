using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Windows;
public class EnemyAI : MonoBehaviour
{
    NavMeshAgent navMeshAgent;
    Transform transformplayer;
    CharacterController characterController;
    public float speed;
    public bool active = true;
    public bool useNavmesh = true;
    Animator animator;
    Rigidbody rb;
    void Start()
    {

        transformplayer = GameObject.FindWithTag("Player").transform;
        characterController = GetComponent<CharacterController>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        rb = GetComponentInChildren<Rigidbody>();
        jmpVector = new Vector3(0f, 0f, 0f);
    }
    void Attack()
    {
        animator.SetTrigger("Attack");
        navMeshAgent.speed = 2f;
        huntAgentSpeed = 2f;
    }
    // Update is called once per frame
    void Update()
    {
        animator.SetFloat("Run Speed Multiplier", huntAnimatorSpeed);
        navMeshAgent.speed = huntAgentSpeed;
        if (UnityEngine.Input.GetKeyDown(KeyCode.T))
        {
            active = !active;
        }
        if (UnityEngine.Input.GetKeyDown(KeyCode.F))
        {
            Attack();
        }
        if (!characterController)
        {
            characterController = GetComponent<CharacterController>();
        }
        if (!navMeshAgent)
        {
            navMeshAgent = GetComponent<NavMeshAgent>();
        }
        if (!transformplayer)
        {
            transformplayer = GameObject.FindWithTag("Player").transform;

        }
        RaycastHit hit;
        if (!sniffPerformed && Physics.Raycast(transform.position, (transformplayer.position - transform.position).normalized, out hit))
        {
            if (active && hit.collider.CompareTag("Player"))
            {
                navMeshAgent.isStopped = true;
                StartCoroutine(SmellCoroutine());

            }
        }
        if (active && !useNavmesh)
        {

            transformplayer = GameObject.FindWithTag("Player").transform;
            if (!characterController)
            {
                characterController = GetComponent<CharacterController>();
            }
            navMeshAgent.enabled = false;
            characterController.enabled = true;
            transform.LookAt(transformplayer);
            characterController.SimpleMove(transform.forward * speed * Time.deltaTime);
        }
        if (active && useNavmesh)
        {

            navMeshAgent.enabled = true;
            characterController.enabled = false;


            navMeshAgent.SetDestination(transformplayer.position);
        }
        if(!active){  animator.SetBool("Sniff", true);}else{  animator.SetBool("Sniff", false);}
        //if (!active && sniffPerformed) jumpCCUpdate();
    }
    public float jmpheight = 2f;
    Vector3 jmpVector;
    public float gravity;
    void jumpCCUpdate()
    {
        characterController.SimpleMove(jmpVector * speed * Time.deltaTime);
        jmpVector.y -= gravity * Time.deltaTime;
    }
    public void jumpStart()
    {
        if (!sniffPerformed) return;
        navMeshAgent.enabled = false;
        active = false;
        characterController.enabled = true;
        jmpVector = (transformplayer.position - transform.position).normalized;
        jmpVector.y = jmpheight;

    }
    public void jumpEnd()
    {
        if (!sniffPerformed) return;
        jmpVector = new Vector3(0f, 0f, 0f);
    }
    public float huntAgentSpeed = 4f;
    public float huntAnimatorSpeed = 3f;
    public float sniffSeconds = 1f;
    bool sniffPerformed = false;
    IEnumerator SmellCoroutine()
    {
        sniffPerformed = true;
        if (!animator)
        {
            animator = GetComponentInChildren<Animator>();
        }
        active = false;
        animator.SetBool("Sniff", true);
        yield return new WaitForSeconds(sniffSeconds);
        navMeshAgent.speed = huntAgentSpeed;
        animator.SetFloat("Run Speed Multiplier", huntAnimatorSpeed); animator.SetBool("Sniff", false);
        navMeshAgent.isStopped = false;
        active = true;

    }/*
    void OnTriggerEnter(Collider collider)
    {
        if (collider.tag == "Player")
        {
            active = true;

        }
    }
    void OnTriggerExit(Collider collider)
    {
        if (collider.tag == "Player")
        {
            active = false;

        }
    }*/
}
