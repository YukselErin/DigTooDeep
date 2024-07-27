using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Unity.Netcode;

public class PlayerXray : PlayerSpell
{
    PlayerCollectOre playerCollectOre;
    public GameObject prefab;

    public float useAmount = 1f;
    public float defaultUseAmount = 1f;
    AudioSource audioSource;
    public AudioClip scansound;
    public int usedOreType = 4;

    public override int getOreType()
    {
        return usedOreType;
    }
    void Start()
    {
        playerCollectOre = GetComponent<PlayerCollectOre>();
        audioSource = GetComponent<AudioSource>();
        gameObjectsQueue = new Queue<List<GameObject>>();
        Upgrades.Instance.newUpgradeEvent.AddListener(HandleUpgradeEvent);

    }
    void HandleUpgradeEvent(ulong ID, string upgradeName)
    {
        if (ID == NetworkManager.Singleton.LocalClientId)
        {

            if (upgradeName == "radarSavingOre")
            {
                useAmount = defaultUseAmount - 0.05f * Upgrades.Instance.getUpgradeLevel(upgradeName);
                useAmount = Mathf.Max(useAmount, 0.01f);
            }
        }
    }
    Vector3 tmp;
    RaycastHit raycastHit;
    // Update is called once per frame
    public override void StartCast()
    {
        StartCoroutine(Scan());
    }
    public int xrayLength = 10;
    public float xrayPeriod = 0.2f;
    List<RaycastHit> hits;
    public float StartOffset = 1f;
    EnemyXrayTarget smrtmp;
    public float monsterDetectDuration = 1f;
    IEnumerator detectSkinnedMesh(GameObject gameObject)
    {
        gameObject.layer = LayerMask.NameToLayer("RenderOnTop");
        yield return new WaitForSeconds(monsterDetectDuration);
        gameObject.layer = LayerMask.NameToLayer("Default");

    }
    bool detectLayerChange(RaycastHit raycastHit)
    {
        if (raycastHit.transform.gameObject.TryGetComponent<EnemyXrayTarget>(out smrtmp))
        {

            StartCoroutine(detectSkinnedMesh(smrtmp.skinnedMeshRenderer));
            return true;
        }
        else if (raycastHit.transform.tag == "DetectWithXray")
        {
            StartCoroutine(detectSkinnedMesh(raycastHit.transform.gameObject));
            return true;
        }
        return false;
    }
    public float raycastLength = 3f;
    bool characterHit = false;
    List<RaycastHit> LineArrayRaycast(Vector3 pos)
    {
        hits = new List<RaycastHit>();
        for (int j = 0; j < xrayLength; j++)
        {
            characterHit = false;
            Physics.Raycast(transform.position + pos + j * transform.right * xrayPeriod, -1f * transform.up, out raycastHit, raycastLength);
            if (raycastHit.transform != null)
            {
                characterHit = detectLayerChange(raycastHit);
            }

            if (raycastHit.point.y != 0f && raycastHit.transform.tag != "Player" && !characterHit)
            {
                hits.Add(raycastHit);
            }
            characterHit = false;
            Physics.Raycast(transform.position + pos + j * transform.right * xrayPeriod, transform.up, out raycastHit, raycastLength);
            if (raycastHit.transform != null)
            {
                characterHit = detectLayerChange(raycastHit);
            }
            if (raycastHit.point.y != 0f && raycastHit.transform.tag != "Player" && !characterHit)
            {
                hits.Add(raycastHit);
            }
            characterHit = false;
            Physics.Raycast(transform.position + pos + -1f * j * transform.right * xrayPeriod, -1f * transform.up, out raycastHit, raycastLength);
            if (raycastHit.transform != null)
            {
                characterHit = detectLayerChange(raycastHit);
            }
            if (raycastHit.point.y != 0f && raycastHit.transform.tag != "Player" && !characterHit)
            {

                hits.Add(raycastHit);
            }
            characterHit = false;
            Physics.Raycast(transform.position + pos + -1f * j * transform.right * xrayPeriod, transform.up, out raycastHit, raycastLength);
            if (raycastHit.transform != null)
            {
                characterHit = detectLayerChange(raycastHit);

            }
            if (raycastHit.point.y != 0f && raycastHit.transform.tag != "Player" && !characterHit)
            {
                hits.Add(raycastHit);
            }
        }
        return hits;

    }
    public int scanDistance = 10;
    public float scanStepDistance = 2f;
    public float scanSpeed = 0.05f;
    public float fadeoutRatio = 0.1f;
    public int soundPeriod = 5;
    public int requiredInt = 5;
    public float runtime = 0f;
    List<float> floats;
    Queue<List<GameObject>> gameObjectsQueue;
    bool stillScanning = false;
    float lastDestroy;
    public float markerLifeTime = 0.3f;
    IEnumerator destroyMarkers()
    {
        lastDestroy = Time.time;
        while (true)
        {
            if (lastDestroy + markerLifeTime < Time.time)
            {
                if (gameObjectsQueue.Count > 0)
                {
                    foreach (GameObject go in gameObjectsQueue.Dequeue())
                    {
                        go.GetComponent<XrayMarker>().Release();
                    }
                    lastDestroy = Time.time;

                }
                else
                {
                    if (stillScanning) { yield return null; } else { break; }
                }
            }
            yield return null;
        }


    }
    public int scanThickness = 5;
    IEnumerator Scan()
    {
        GameObject temp;
        //   floats = new List<float>();
        Vector3 offset = new Vector3(0f, StartOffset, 0f);
        //StartCoroutine(destroyMarkers());

        List<GameObject> tempGOs = new List<GameObject>();

        for (int i = 0; i < scanDistance; i++)
        {
            stillScanning = true;

            // floats.Add(Time.time - runtime);
            // runtime = Time.time;
            if (gameObjectsQueue.Count > scanThickness)
            {
                foreach (GameObject go in gameObjectsQueue.Dequeue())
                {
                    go.GetComponent<XrayMarker>().Release();
                }


            }
            LineArrayRaycast(StartOffset * transform.forward + transform.forward * i * scanStepDistance);

            foreach (var hit in hits)
            {
                temp = XrayMarkerSpawner.Instance._pool.Get();
                temp.transform.position = hit.point;
                tempGOs.Add(temp);
            }
            gameObjectsQueue.Enqueue(new List<GameObject>(tempGOs));
            tempGOs.Clear();

            if (i % soundPeriod == 0 && hits.Count > requiredInt)
            {
                audioSource.PlayOneShot(scansound, 1 / ((i * fadeoutRatio) + 1));
            }



            yield return new WaitForSeconds(scanSpeed);


        }
        while (gameObjectsQueue.Count > 0)
        {
            foreach (GameObject go in gameObjectsQueue.Dequeue())
            {
                go.GetComponent<XrayMarker>().Release();
            }


        }
        stillScanning = false;
        //   foreach (var hit in floats) { Debug.Log(hit); }
        //Debug.LogError("end");
    }
    public override void EndCast()
    {


    }
    public override bool ChannelingCost()
    {
        return true;

    }
    public override bool checkCost()
    {
        return playerCollectOre.tryUseOre(usedOreType, useAmount);
    }
}
