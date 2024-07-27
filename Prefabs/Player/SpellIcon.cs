using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using UnityEngine.UI;

public class SpellIcon : MonoBehaviour
{
    Image selectionImage;
    Image oreChargeImage;
    bool selectedSpell = false;
    public int chargeOreType = -1;
    PlayerCollectOre playerCollectOre;
    Material oreChargeMaterial;
    public float materialOffset = -425f;
    public float materialRateMax = 0.5f;
    public float materialRateMin = -425f;
    public float maxOreAmount = 100f;
    public void setChargeOreType(int type)
    {
        if (type != -1)
        {
            chargeOreType = type;
            playerCollectOre = transform.parent.parent.GetComponentInChildren<PlayerCollectOre>();
            playerCollectOre.oreAmountChanged.AddListener(UpdateOreDisplay);
        }
    }
    public GameObject prefab;
    List<GameObject> glints;
    List<GameObject> glintPool;
    public float xStackOffset = 5f;
    public float xSpawnOffset = -20f;
    public float ySpawnOffset = -20f;
    public float yStackOffset = -5f;
    public int imageCount = 0;
    public int glintCount = 0;

    void UpdateOreDisplay(int type, float amount)
    {
        if (type != -1 && type == chargeOreType)
        {
            maxOreAmount = playerCollectOre.defaultAmountToTurnIntoStar;
            imageCount = glintPool.Count + glints.Count;
            float matRateRange = 75 / maxOreAmount;
            float matFill = materialRateMin + amount * matRateRange;
            oreChargeMaterial.SetFloat("_FillRate", matFill);
            if (playerCollectOre.oreStars.ContainsKey(chargeOreType))
            {
                if (glintCount != playerCollectOre.oreStars[chargeOreType])
                {
                    glintCount = playerCollectOre.oreStars[chargeOreType];
                    foreach (GameObject go in glints.ToArray())
                    {
                        glintPool.Add(go);
                        glints.Remove(go);
                    }
                    for (int i = 0; i < playerCollectOre.oreStars[chargeOreType]; i++)
                    {
                        GameObject temp;
                        if (glintPool.Count == 0)
                        {
                            temp = Instantiate(prefab, transform);
                            temp.name = "glint" + i;
                            glintPool.Add(temp);
                        }
                        temp = glintPool[0];
                        temp.GetComponent<Image>().color = UpgradeUI.Instance.typeColors[chargeOreType - 1];

                        temp.SetActive(true);
                        glints.Add(temp);
                        glintPool.Remove(temp);
                        //Debug.Log("placing " + temp.name);
                        float xOffset = (i % 2 == 0 ? xStackOffset : -1f * xStackOffset) + xSpawnOffset;
                        temp.GetComponent<RectTransform>().localPosition = new Vector3(0f, 0f, 0f);
                        temp.GetComponent<RectTransform>().Translate(new Vector3(xOffset, ySpawnOffset + (i * yStackOffset), 0f));
                    }
                }

            }
            else
            {
                foreach (GameObject go in glints.ToArray())
                {
                    glintPool.Add(go);
                    glints.Remove(go);
                }

            }
            foreach (GameObject go in glintPool.ToArray())
            {
                go.SetActive(false);
            }
        }

    }
    void ResetUIElement()
    {
        float startamount = 0;
        if (playerCollectOre.ores.ContainsKey(chargeOreType))
        {
            startamount = playerCollectOre.ores[chargeOreType];
        }
        UpdateOreDisplay(chargeOreType, startamount);
    }
    void Start()
    {
        glints = new List<GameObject>();
        glintPool = new List<GameObject>();
        selectionImage = GetComponent<Image>();
        selectionImage.enabled = false;
        oreChargeImage = transform.GetChild(0).GetComponent<Image>();
        oreChargeMaterial = oreChargeImage.material;
        float startamount = 0;
        if (playerCollectOre.ores.ContainsKey(chargeOreType))
        {
            startamount = playerCollectOre.ores[chargeOreType];
        }
        UpdateOreDisplay(chargeOreType, startamount);
        GameStateController.Instance.GameRestartEvent.AddListener(ResetUIElement);
    }
    public void select()
    {
        selectedSpell = true;
        HighlightImage(true);
    }
    public void deselect()
    {
        selectedSpell = false;
        HighlightImage(false);
    }
    void HighlightImage(bool state)
    {
        if (selectionImage)
        {
            selectionImage.enabled = state;
        }
        else
        {
            if (TryGetComponent(out selectionImage))
            {
                selectionImage.enabled = state;
            }
        }
    }
}
