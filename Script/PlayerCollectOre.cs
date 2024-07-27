using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR;

public class PlayerCollectOre : NetworkBehaviour
{
    [System.Serializable]
    public class OreAmountChangedEvent : UnityEvent<int, float>
    {
    }
    public GameObject playerCapsule;
    public OreAmountChangedEvent oreAmountChanged;
    // Start is called before the first frame update
    void Start()
    {
        if (oreAmountChanged == null)
            oreAmountChanged = new OreAmountChangedEvent();
        CollectOre.Instance.register(this);
        ores = new Dictionary<int, float>();
        oreStars = new Dictionary<int, int>();
        Upgrades.Instance.newUpgradeEvent.AddListener(HandleUpgradeEvent);

        playerCapsule.GetComponentInChildren<PlayerHealth>().PlayerDeathEvent.AddListener(HandlePlayerDeath);
        GameStateController.Instance.GameRestartEvent.AddListener(HandlePlayerRespawn);
    }
    void HandlePlayerRespawn()
    {
        enabled = true;
    }
    void HandlePlayerDeath()
    {
        foreach (var ore in ores.ToArray())
        {
            ores[ore.Key] = 0f;
        }
        foreach (var ore in oreStars.ToArray())
        {
            oreStars[ore.Key] = 0;
        }
        enabled = false;
    }
    void HandleUpgradeEvent(ulong ID, string upgradeName)
    {
        if (ID == NetworkManager.Singleton.LocalClientId)
        {
            //    Debug.Log("Applying:" + upgradeName + "!");
            if (upgradeName == "oreStars")
            {
                AmountToTurnIntoStar = defaultAmountToTurnIntoStar - 100f * Upgrades.Instance.getUpgradeLevel(upgradeName);
                AmountToTurnIntoStar = Mathf.Max(AmountToTurnIntoStar, 3000f);
                AmountTurnsToStar = AmountToTurnIntoStar - 1000f;
                AmountTurnsBack = AmountTurnsToStar - 2000f;
            }
            oreRegenClassify(upgradeName);

        }
    }
    Dictionary<int, float> amountToRegen;
    bool regenActive = false;
    IEnumerator oreRegen()
    {
        //Debug.Log("started coroutine");

        while (regenActive)
        {
            // Debug.Log("coroutine");

            foreach (var ore in amountToRegen)
            {
                // Debug.Log("foreach");

                oreCollected(ore.Key, ore.Value, OwnerClientId);
            }
            yield return new WaitForSeconds(1f);
        }


    }
    public TMP_Text Text;
    bool modified = false;
    public Dictionary<int, float> ores;
    public Dictionary<int, int> oreStars;
    string textString;
    void updateUI()
    {
        textString = "";
        if (ores.ContainsKey(9)) textString = "10 = " + ores[9] + "\n";
        Text.text = textString;
        /*
                foreach (var ore in oreStars)
                {
                    Debug.Log("Type: " + ore.Key + " Amount:" + ore.Value);
                }
        */

    }
    void Update()
    {
        if (modified)
        {
            modified = false;
            updateUI();
        }
    }

    public bool debugAllFree;
    public bool tryUseOre(int typeNo, float amount)
    {
        if (debugAllFree)
        {
            return true;
        }
        if (ores.ContainsKey(typeNo))
        {
            if (ores[typeNo] > amount)
            {
                ores[typeNo] -= amount;
                oreAmountChanged.Invoke(typeNo, ores[typeNo]);
                return true;

            }
            if (ores[typeNo] < AmountToTurnIntoStar)
            {
                if (oreStars.ContainsKey(typeNo) && oreStars[typeNo] > 0)
                {
                    oreStars[typeNo]--;
                    ores[typeNo] += AmountTurnsBack;
                }
            }
        }
        return false;
    }
    public float defaultAmountToTurnIntoStar = 10000f;
    public float AmountToTurnIntoStar = 10000f;
    public float defaultAmountTurnsToStar = 9000f;
    public float AmountTurnsToStar = 9000f;
    public float defaultAmountTurnsBack = 7000f;
    public float AmountTurnsBack = 7000f;
    public void oreCollected(int type, float value, ulong givenClientID)
    {
        //  Debug.Log("Owner id:" + OwnerClientId + " given ID:" + givenClientID);

        if (OwnerClientId != givenClientID) { return; }

        if (ores.ContainsKey(type))
        {
            ores[type] = ores[type] + value;
            oreAmountChanged.Invoke(type, ores[type]);

        }
        else
        {
            ores.Add(type, value);
            oreAmountChanged.Invoke(type, value);

        }
        if (ores[type] >= AmountToTurnIntoStar)
        {
            if (oreStars.ContainsKey(type))
            {
                oreStars[type]++;
                ores[type] -= AmountTurnsToStar;
            }
            else
            {
                oreStars.Add(type, 1);
                ores[type] -= AmountTurnsToStar;
            }

        }
        modified = true;
    }
    void oreRegenClassify(string upgradeName)
    {
        if (upgradeName.CompareTo("oreRegen1") == 0)
        {
            oreWithInt(1);
        }
        if (upgradeName.CompareTo("oreRegen2") == 0)
        {
            oreWithInt(2);
        }
        if (upgradeName.CompareTo("oreRegen3") == 0)
        {
            oreWithInt(3);
        }
        if (upgradeName.CompareTo("oreRegen4") == 0)
        {
            oreWithInt(4);
        }
        if (upgradeName.CompareTo("oreRegen5") == 0)
        {
            oreWithInt(5);
        }
    }
    void oreWithInt(int i)
    {
        int oreNo = i;

        if (!regenActive)
        {

            amountToRegen = new Dictionary<int, float>();
            amountToRegen.Add(oreNo, 20);
            regenActive = true;
            StartCoroutine(oreRegen());
        }
        else
        {
            if (amountToRegen.ContainsKey(oreNo))
            {
                amountToRegen[oreNo] += 20;
            }
            else
            {
                amountToRegen.Add(oreNo, 20);
            }
        }
    }
}
