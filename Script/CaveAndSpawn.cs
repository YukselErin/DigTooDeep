using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;


public class CaveAndSpawn : MonoBehaviour
{
    [SerializeField]
    public GameObject gobboEnemyPrefab;


    // Update is called once per frameF
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            Vector3 pos = new Vector3(-10f, -10f, -10f);
            ChunkManager.Instance.ModifyChunkData(pos, 5f, -255f, 0, 0);
            Instantiate(gobboEnemyPrefab, pos, quaternion.identity).transform.localScale = new Vector3(2f, 2f, 2f);

        }
    }
}
