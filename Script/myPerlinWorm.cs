using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
world up 72
world down -75
*/
public class myPerlinWorm : MonoBehaviour
{
    int wormRunForTimes = 0;
    public int wormindex = 0;
    public float speed;
    public float turnyness;
    public float heading;
    public GameObject prefab;
    AudioSource audioSource;
    public AudioClip audioClip;
    float placementTimer = 0f;
    // Start is called before the first frame update
    void ResetWormRunCount()
    {
        wormRunForTimes = 0;
    }
    void Start()
    {
        TryGetComponent<AudioSource>(out audioSource);
        GameStateController.Instance.GameRestartEvent.AddListener(ResetWormRunCount);
    }
    private void Awake()
    {
        getInitPos();
        if (speed == 0)
        {
            speed = Random.Range(2.0f, 5.0f);
            heading = Random.Range(0.0f, 360.0f);
            turnyness = Random.Range(1450.0f, 2600.0f);
        }
    }

    public float headingx;
    public float headingy;
    public float headingz;
    public bool runWormbool = false;
    public bool playsound = false;
    void Update()
    {
        position = transform.position;
        resetToInitPos();
        if (runWormbool)
        {
            resetToInitPos();
            runWorm2();
            if (playsound)
            {
                audioSource.PlayOneShot(audioClip);
            }
            runWormbool = false;
        }
    }
    Vector3 position;
    public int iterateCount = 200;
    public float range = 2f;
    float perlinOffsetX = 0f;
    float perlinOffsetY = 0f;
    float perlinOffsetZ = 0f;
    public float heightRange = 5f;

    void SetSeedRandom()
    {
        wormRunForTimes++;
        perlinOffsetX = NoiseManager.Instance.worldConfig.worldSeed + wormindex * 100 + wormRunForTimes * 50;
        perlinOffsetY = NoiseManager.Instance.worldConfig.worldSeed + wormindex * 100 + wormRunForTimes * 50;
        perlinOffsetZ = NoiseManager.Instance.worldConfig.worldSeed + wormindex * 100 + wormRunForTimes * 50;
        /*
        if (wormindex == 2)
        {
            Debug.Log("X: " + perlinOffsetX + " Y: " + perlinOffsetY + " Z: " + perlinOffsetZ);
        }
        if (wormindex == 24)
        {
            Debug.Log("X: " + perlinOffsetX + " Y: " + perlinOffsetY + " Z: " + perlinOffsetZ);
        }*/
    }
    void runWorm1()
    {
        for (int i = 0; i < iterateCount; i++)
        {
            placementTimer += 0.1f;
            float x = position.x;
            float z = position.y;
            float noise = Mathf.PerlinNoise(perlinOffsetX + x, perlinOffsetZ + z);
            noise -= 0.5f;
            float height = noise * heightRange;

            float turn = noise * turnyness;
            Vector3 direction = Quaternion.Euler(0, heading, 0) * Vector3.left;
            heading += turn * Time.deltaTime;
            position += direction * (speed * Time.deltaTime);
            position.y = height;
            transform.position = position;

            ChunkManagerNetwork.Instance.ModifyChunkData(position, range, -500, 999, -1, 2);
            // Instantiate(prefab, new Vector3(Mathf.Round(position.x), Mathf.Round(position.y), Mathf.Round(position.z)), Quaternion.identity);
            placementTimer = 0f;

        }
        /*

        */
    }
    public float steepness = 2f;
    bool seedSet = false;
    Vector3 pos = new Vector3();
    Vector3 scale = new Vector3();

    void getInitPos()
    {

        pos = transform.position;
        scale = transform.localScale;

    }
    void resetToInitPos()
    {

        transform.position = pos;
        transform.localScale = scale;
        placementTimer = 0f;/*
        if (wormindex == 2)
        {
            Debug.Log("pos: " + transform.position);
        }
        if (wormindex == 24)
        {
            Debug.Log("pos: " + transform.position);
        }*/
    }
    public float timesubstitute = 0.03f;
    public void runWorm2()
    {

        SetSeedRandom();
        seedSet = true;
        resetToInitPos();
        for (int i = 0; i < iterateCount; i++)
        {
            placementTimer += 1f;
            float x = position.x;
            float z = position.z;
            float noise = Mathf.PerlinNoise(perlinOffsetX + x, perlinOffsetZ + z);
            noise -= 0.5f;
            float height = noise * steepness;

            float turn = noise * turnyness;
            Vector3 direction = Quaternion.Euler(0, heading, 0) * Vector3.left;
            heading += turn * timesubstitute;
            position += direction * (speed * timesubstitute);
            position.y += height;
            //position.y = Mathf.Min(position.y, heightRange);
            //position.y = Mathf.Max(position.y, -heightRange);
            transform.position = position;
            ChunkManager.Instance.ModifyChunkData(position, range, -500, 999, -1, 2);
            // Instantiate(prefab, new Vector3(Mathf.Round(position.x), Mathf.Round(position.y), Mathf.Round(position.z)), Quaternion.identity);


        }

    }
}