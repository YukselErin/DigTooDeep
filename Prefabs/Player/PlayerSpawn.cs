using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawn : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Invoke("tp", 5f);
    }
    void tp()
    {
        transform.position = new Vector3(0f, 100f, 0f);
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R)) { transform.position = new Vector3(0f, 100f, 0f); }
    }
}
