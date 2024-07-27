using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OreMarker : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }
    public float dissapearScale = 0.1f;
    // Update is called once per frame
    void Update()
    {
        transform.localScale = transform.localScale - new Vector3(dissapearScale, dissapearScale, dissapearScale) * Time.deltaTime;
        if (transform.localScale.x < 0.1f)
        {
            Destroy(this.gameObject);
        }
    }
}
