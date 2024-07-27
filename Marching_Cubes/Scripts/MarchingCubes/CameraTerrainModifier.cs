using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class CameraTerrainModifier : NetworkBehaviour
{
    public GameObject playerCapsule;

    public AudioClip[] audioClips;
    public Text textSize;
    public Text textMaterial;
    [Tooltip("Range where the player can interact with the terrain")]
    public float rangeHit = 100;
    [Tooltip("Force of modifications applied to the terrain")]
    public float modiferStrengh = 10;
    [Tooltip("Size of the brush, number of vertex modified")]
    public float sizeHit = 6;
    [Tooltip("Color of the new voxels generated")]
    [Range(0, Constants.NUMBER_MATERIALS - 1)]
    public int buildingMaterial = 0;

    private RaycastHit hit;
    private ChunkManagerNetwork chunkManager;

    void Awake()
    {
        chunkManager = ChunkManagerNetwork.Instance;
        UpdateUI();
    }
    public float cooldown = 0.5f;
    public float deafultCooldown = 0.5f;
    float lasttime;
    public Animator animator;
    int swingHash;
    // Update is called once per frame
    AudioSource audioSource;
    public void Start()
    {
        swingHash = Animator.StringToHash("Swing");
        audioSource = GetComponent<AudioSource>();
        Upgrades.Instance.newUpgradeEvent.AddListener(HandleUpgradeEvent);
        deafultCooldown = cooldown; //at start they are the same
        playerCapsule.GetComponentInChildren<PlayerHealth>().PlayerDeathEvent.AddListener(HandlePlayerDeath);
        GameStateController.Instance.GameRestartEvent.AddListener(HandlePlayerRespawn);
    }
    void HandlePlayerRespawn()
    {
        enabled = true;
    }
    void HandlePlayerDeath()
    {
        enabled = false;
    }
    void HandleUpgradeEvent(ulong ID, string upgradeName)
    {
        if (ID == NetworkManager.Singleton.LocalClientId)
        {
            if (upgradeName == "digSpeed")
            {
                cooldown = deafultCooldown - 0.02f * Upgrades.Instance.getUpgradeLevel(upgradeName);
            }
        }
    }
    public int power = 1;
    void modifyChunk()
    {
        chunkManager.ModifyChunkData(hit.point, sizeHit, modification, OwnerClientId, buildingMaterial, power);
        audioSource.PlayOneShot(audioClips[Random.Range(0, audioClips.Length)]);
    }
    float modification;
    public bool allowPlacement = false;
    void Update()
    {
        if (!IsOwner) { return; }
        bool placement = allowPlacement && Input.GetMouseButton(0);
        if ((Input.GetMouseButton(1) || placement) && Time.time > cooldown + lasttime)
        {
            lasttime = Time.time;
            modification = (Input.GetMouseButton(0)) ? modiferStrengh : -modiferStrengh;
            if (!allowPlacement)
                modification = -modiferStrengh;
            if (Physics.Raycast(transform.position, transform.forward, out hit, rangeHit))
            {
                animator.SetTrigger(swingHash);
                //modifyChunk();
                Invoke("modifyChunk", 0.5f);

            }
        }

        //Inputs
        if (Input.GetAxis("Mouse ScrollWheel") > 0 && buildingMaterial != Constants.NUMBER_MATERIALS - 1)
        {
            buildingMaterial++;
            UpdateUI();
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0 && buildingMaterial != 0)
        {
            buildingMaterial--;
            UpdateUI();
        }

        if (Input.GetKeyDown(KeyCode.Plus) || Input.GetKeyDown(KeyCode.KeypadPlus))
        {
            sizeHit++;
            UpdateUI();
        }
        else if ((Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus)) && sizeHit > 1)
        {
            sizeHit--;
            UpdateUI();
        }

    }

    public void UpdateUI()
    {
        //textSize.text = "(+ -) Brush size: " + sizeHit;
        //textMaterial.text = "(Mouse wheel) Actual material: " + buildingMaterial;
    }
}
