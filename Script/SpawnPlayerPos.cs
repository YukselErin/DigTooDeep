using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public class SpawnPlayerPos : MonoBehaviour
{
    public GameObject spiderEnemyPrefab;
    public GameObject minotaurPrefab;
    void Start()
    {
        prefab = minotaurPrefab;
    }

    // Update is called once per frame
    Transform temp;
    Vector3 spawnpos;
    GameObject prefab;
    IEnumerator layingInWait()
    {
        temp = GameObject.FindWithTag("Player").transform;
        spawnpos = temp.position;
        yield return null;
        while (Vector3.Distance(temp.position, spawnpos) < 5f)
        {
            yield return null;
        }

        Instantiate(prefab, spawnpos, quaternion.identity, transform);

    }
    void spawnGobbo()
    {
        StartCoroutine(layingInWait());
    }
    bool toggle = true;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            spawnGobbo();
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            prefab = toggle ? spiderEnemyPrefab : minotaurPrefab;
            toggle = !toggle;
        }
    }
}
