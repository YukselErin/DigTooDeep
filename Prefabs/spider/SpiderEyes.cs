using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class SpiderEyes : MonoBehaviour
{
    Material material;
    Renderer renderer;
    Color emmissiveColor;
    int id;
    Transform player;
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;

        if (TryGetComponent<Renderer>(out renderer))
        {
            material = renderer.material;
            emmissiveColor = material.GetColor("_EmissionColor");
            id = material.shader.GetPropertyNameId(material.shader.FindPropertyIndex("_EmissionColor"));

        }

    }
    float intensity = 0f;
    public float maxFlashIntensity = 2f;
    public float flashSpeed = 0.2f;
    public float topIntensityCD = 0.2f;
    public float distanceDelay = 10f;
    IEnumerator flashEyeEmmissive()
    {
        yield return new WaitForSeconds(Vector3.Distance(player.position, transform.position) * distanceDelay);
        material.EnableKeyword("_EMISSION");
        while (intensity < maxFlashIntensity)
        {
            intensity += flashSpeed * Time.deltaTime;
            material.SetColor(id, emmissiveColor * Mathf.Pow(2, intensity));
            yield return null;
        }
        yield return new WaitForSeconds(topIntensityCD);
        StartCoroutine(reduceIntensityEyeEmmissive());
    }
    public float reduceSpeed = 0.2f;

    IEnumerator reduceIntensityEyeEmmissive()
    {
        while (intensity > -10f)
        {
            intensity -= reduceSpeed * Time.deltaTime;
            material.SetColor(id, emmissiveColor * Mathf.Pow(2, intensity));
            yield return null;
        }

    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            StartCoroutine(flashEyeEmmissive());
        }
    }
}
