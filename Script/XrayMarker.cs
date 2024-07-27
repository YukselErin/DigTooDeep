using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class XrayMarker : MonoBehaviour
{
    ObjectPool<GameObject> _pool;
   // public float lifetimeSeconds = 1f;
    public void Release()
    {
        _pool.Release(this.gameObject);
    }
    /*void Update()
    {
        //float sizeRatio = 1 / lifetimeFrame;
        if (remainingLife > 0)
        {
            // transform.localScale = transform.localScale - new Vector3(sizeRatio, sizeRatio, sizeRatio);
            remainingLife -= Time.deltaTime;

        }
        else
        {
            _pool.Release(this.gameObject);

        }
    }*/
    float remainingLife;
    void OnEnable()
    {
        transform.localScale = new Vector3(1f, 1f, 1f);
        //  remainingLife = lifetimeSeconds;

    }
    // Update is called once per frame
    public void SetPool(ObjectPool<GameObject> pool)
    {
        _pool = pool;
    }
}
