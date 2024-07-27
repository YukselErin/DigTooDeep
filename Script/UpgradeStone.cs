using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System.Net.Http.Headers;

public class UpgradeStone : MonoBehaviour
{
    public GameObject upgradesCanvas;
    public GameObject upgradesGO;
    bool upgradeStoneUsed = false;
    bool UIactivated = false;
    void Start()
    {
        upgradesCanvas = UpgradeUI.Instance.canvas;
        upgradesGO = UpgradeUI.Instance.gameObject;
    }
    public void noUpgrade()
    {
        if (!upgradeStoneUsed)
        {
            UIactivated = false;
        }
    }
    void upgraded(ulong id, string upname)
    {
        if (UIactivated && id == NetworkManager.Singleton.LocalClientId)
        {
            upgradeStoneUsed = true;
        }

    }
    int stoneID = 0;
    public void promptUpradeUI()
    {
        if (stoneID == 0)
        {
            stoneID = Random.Range(1, 100);

        }
        UIactivated = true;
        Upgrades.Instance.noUpgrade.AddListener(noUpgrade);
        Upgrades.Instance.newUpgradeEvent.AddListener(upgraded);
        upgradesGO.GetComponent<UpgradeUI>().presentOptions(stoneID);
        upgradesCanvas.SetActive(true);
    }
    IEnumerator delayUI()
    {
        /*
        while (enterTime < promptAfterSeconds)
        {
            yield return null;
        }*/

        yield return new WaitForSeconds(promptAfterSeconds);
        promptUpradeUI();
    }
    bool prompted = false;
    //float enterTime;
    public float promptAfterSeconds = 1f;
    NetworkObject networkObject;
    void OnTriggerEnter(Collider collider)
    {
        if (upgradeStoneUsed)
            return;
        if (collider.tag != "Player")
            return;
        Debug.Log("enter trig id:" + collider.transform.parent.GetComponent<NetworkObject>().OwnerClientId);
        if (collider.transform.parent.GetComponent<NetworkObject>().OwnerClientId != NetworkManager.Singleton.LocalClientId)
            return;
        StopAllCoroutines();
        //enterTime = Time.time;
        Debug.Log("courtoutine called");
        StartCoroutine(delayUI());
    }
    void OnTriggerExit(Collider collider)
    {
        if (upgradeStoneUsed)
            return;
        bool player = false;
        if (collider.transform.parent)
            if (collider.transform.parent.TryGetComponent<NetworkObject>(out networkObject))
                if (networkObject.OwnerClientId != NetworkManager.Singleton.LocalClientId)
                    player = true;

        if (!player)
            return;

        StopAllCoroutines();
        Debug.Log("exit trig");
    }
}
