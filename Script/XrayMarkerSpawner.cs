using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Pool;

public class XrayMarkerSpawner : MonoBehaviour
{
    public ObjectPool<GameObject> _pool;
    public GameObject markerPrefab;
    public static XrayMarkerSpawner Instance { get; private set; }
    private void Awake()
    {
        // If there is an instance, and it's not me, delete myself.
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }


    void Start()
    {
        _pool = new ObjectPool<GameObject>(CreateMarker, OnTakeMarkerFromPool, OnReturnMarkerToPool, OnDestroyMarker, true, 10000, 30000);
    }

    GameObject CreateMarker()
    {
        GameObject bullet = Instantiate(markerPrefab, new Vector3(0f, 0f, 0f), quaternion.identity);
        bullet.GetComponent<XrayMarker>().SetPool(_pool);
        return bullet;
    }
    void OnTakeMarkerFromPool(GameObject marker)
    {
        marker.gameObject.SetActive(true);
    }

    void OnReturnMarkerToPool(GameObject marker)
    {
       marker.gameObject.SetActive(false);
//marker.transform.position = new Vector3(10f,10f,10f);
    }
    void OnDestroyMarker(GameObject marker)
    {
        Destroy(marker);

    }
}
