using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeUI : MonoBehaviour
{
    public static UpgradeUI Instance { get; private set; }
    void Awake()
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
    public GameObject canvas;
    List<int> currentOptions;
    List<GameObject> cachedOreGlints;
    List<GameObject> usedOreGlints;
    public Transform choiceGlintsPos1;
    public Transform choiceGlintsPos2;
    public Transform choiceGlintsPos3;
    Transform[] glintsPositions;

    public TextMeshProUGUI choiceText1;
    public TextMeshProUGUI choiceText2;
    public TextMeshProUGUI choiceText3;
    TextMeshProUGUI[] choiceTexts;
    public GameObject glintPrefab;
    void Start()
    {
        stones = new Dictionary<int, (int, int, int)>();
        cachedOreGlints = new List<GameObject>();
        usedOreGlints = new List<GameObject>();
        currentOptions = new List<int>();
        choiceTexts = new TextMeshProUGUI[3];
        choiceTexts[0] = choiceText1;
        choiceTexts[1] = choiceText2;
        choiceTexts[2] = choiceText3;
        glintsPositions = new Transform[3];

        glintsPositions[0] = choiceGlintsPos1;
        glintsPositions[1] = choiceGlintsPos2;
        glintsPositions[2] = choiceGlintsPos3;
        gameObject.SetActive(false);
    }
    Dictionary<int, (int, int, int)> stones;
    public void presentOptions(int stoneID)
    {
        bool getoption = true;
        int loopTrial = 10;
        currentOptions.Clear();
        if (stones.ContainsKey(stoneID) && stoneID != 0)
        {
            currentOptions.Add(stones[stoneID].Item1);
            currentOptions.Add(stones[stoneID].Item2);
            currentOptions.Add(stones[stoneID].Item3);
            populateAll();
            return;
        }

        int choice1 = Upgrades.Instance.getRandomUpgradeChoice();
        currentOptions.Add(choice1);
        int choice2 = Upgrades.Instance.getRandomUpgradeChoice();
        while (getoption && loopTrial > 0)
        {
            loopTrial--;

            if (currentOptions.Contains(choice2))
            {
                choice2 = Upgrades.Instance.getRandomUpgradeChoice();
            }
            else
            {
                currentOptions.Add(choice2); getoption = false;

            }

        }
        getoption = true;
        loopTrial = 20;
        int choice3 = Upgrades.Instance.getRandomUpgradeChoice();
        while (getoption && loopTrial > 0)
        {
            loopTrial--;
            if (currentOptions.Contains(choice3))
            {
                choice3 = Upgrades.Instance.getRandomUpgradeChoice();
            }
            else
            {
                currentOptions.Add(choice3); getoption = false;

            }

        }

        if (!stones.ContainsKey(stoneID) && stoneID != 0)
        { stones.Add(stoneID, (choice1, choice2, choice3)); }
        populateAll();
    }
    void populateAll()
    {
        if (usedOreGlints.Count > 0)
        {
            foreach (GameObject glint in usedOreGlints.ToArray())
            {
                cachedOreGlints.Add(glint);
                usedOreGlints.Remove(glint);
                glint.SetActive(false);
            }
        }
        for (int i = 0; i < currentOptions.Count; i++)
        {
            populateOption(i);
        }

        gameObject.SetActive(true);
    }
    public bool test = false;
    void Update()
    {
        if (test)
        {
            test = false;
            presentOptions(0);

        }
    }
    public Color[] typeColors;
    public float textOffset = 330f;
    public float xItOffset = 25f;
    void populateOption(int optionIndex)
    {
        Debug.Log("populatiıng" + optionIndex);
        int[] requiredTypes;
        int[] requiredAmount;
        int iterateAmout;
        int SOindex = currentOptions[optionIndex];
        int glintAdded = 0;
        UpgradeSO upgrade = Upgrades.Instance.possibleUpgradeSOs[SOindex];
        requiredTypes = upgrade.requiredOreStarType;
        requiredAmount = upgrade.requiredOreStarAmount;
        iterateAmout = Mathf.Min(requiredAmount.Length, requiredTypes.Length);
        if (requiredAmount.Length != requiredTypes.Length)
        {
            Debug.LogError("Upgrade SO ore lists do not match!");
        }

        for (int i = 0; i < iterateAmout; i++)
        {
            Color glintColor;
            if (requiredTypes[i] < typeColors.Length)
            {
                glintColor = typeColors[requiredTypes[i]];
            }
            else
            {
                glintColor = Color.magenta;
            }
            for (int j = 0; j < requiredAmount[i]; j++)
            {
                GameObject temp;
                if (cachedOreGlints.Count != 0)
                {
                    temp = cachedOreGlints[0];
                    cachedOreGlints.Remove(temp);
                    temp.SetActive(true);
                    temp.transform.SetParent(glintsPositions[optionIndex], false);
                    // = glintsPositions[optionIndex].position + new Vector3(i * 500f, 0f, 0f);
                }
                else
                {
                    temp = Instantiate(glintPrefab, glintsPositions[optionIndex]);
                    temp.SetActive(true);
                }
                temp.GetComponent<RectTransform>().localPosition = new Vector3(0f, 0f, 0f);
                float yOffset = 0f;
                if (requiredAmount[i] != 1)
                    yOffset = j % 2 == 0 ? 15f : -15f;
                temp.GetComponent<RectTransform>().Translate(new Vector3((glintAdded) * xItOffset, yOffset, 0f));
                temp.GetComponent<Image>().color = glintColor;

                if (upgrade.descriptionText != null)
                {
                    choiceTexts[optionIndex].text = upgrade.descriptionText;
                    choiceTexts[optionIndex].gameObject.GetComponent<RectTransform>().localPosition = new Vector3(0f, 0f, 0f);
                    choiceTexts[optionIndex].gameObject.GetComponent<RectTransform>().Translate(new Vector3(textOffset + (glintAdded) * xItOffset, 0f, 0f));
                }
                glintAdded++;
                usedOreGlints.Add(temp);
            }

        }

    }
    PlayerCollectOre playerOreScript;
    void getPlayerOreScript()
    {
        foreach (PlayerCollectOre oreScript in CollectOre.Instance.registered)
        {
            if (oreScript.OwnerClientId == NetworkManager.Singleton.LocalClientId)
            {
                playerOreScript = oreScript;
            }
        }
        if (!playerOreScript)
        {
            Debug.LogError("Can't find ores!");
        }
    }
    public void optionSelected(int index)
    {
        UpgradeSO SO = Upgrades.Instance.possibleUpgradeSOs[currentOptions[index]];
        bool hasRequiredOres = true;
        for (int i = 0; i < SO.requiredOreStarType.Length; i++)
        {
            if (!playerOreScript)
            {
                getPlayerOreScript();
            }
            int starType = SO.requiredOreStarType[i];
            if (playerOreScript.oreStars.ContainsKey(starType))
            {
                if (playerOreScript.oreStars[starType] < SO.requiredOreStarAmount[i])
                {
                    Debug.Log("Not enough contain key:" + starType + " Needed: " + SO.requiredOreStarAmount[i] + " Has: " + playerOreScript.oreStars[starType]);

                    hasRequiredOres = false;
                }
            }
            else
            {
                Debug.Log("Does not contain key:" + starType);
                hasRequiredOres = false;
            }
        }
        if (hasRequiredOres)
        {
            Debug.Log("Has ores!");

            for (int i = 0; i < SO.requiredOreStarType.Length; i++)
            {
                int starType = SO.requiredOreStarType[i];
                playerOreScript.oreStars[starType] -= SO.requiredOreStarAmount[i];
            }
            Upgrades.Instance.addUpgradeToPlayer(NetworkManager.Singleton.LocalClientId, Upgrades.Instance.possibleUpgradeSOs[currentOptions[index]].upgradeName);
            closeBackdrop();
            playSuccsessSound();
        }

    }
    void closeBackdrop() { canvas.SetActive(false); }
    void playSuccsessSound() { GetComponent<AudioSource>().Play(); }
}
