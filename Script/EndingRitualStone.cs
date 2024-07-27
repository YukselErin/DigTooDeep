using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class EndingRitualStone : MonoBehaviour
{
    public void promptEndingRitual()
    {
        EndingRitualUI.Instance.activateUI();
        Debug.Log("prompt");

    }
    IEnumerator delayUI()
    {
        /*
        while (enterTime < promptAfterSeconds)
        {
            yield return null;
        }*/
        yield return new WaitForSeconds(promptAfterSeconds);
        promptEndingRitual();
    }
    bool prompted = false;
    //float enterTime;
    public float promptAfterSeconds = 1f;
    void OnTriggerEnter(Collider collider)
    {
        if (collider.tag != "Player")
            return;
        Debug.Log("enter trig id:" + collider.transform.parent.GetComponent<NetworkObject>().OwnerClientId);



        if (collider.transform.parent.GetComponent<NetworkObject>().OwnerClientId != NetworkManager.Singleton.LocalClientId)
            return;
        EndingRitualUI.Instance.playerEntered();
        StopAllCoroutines();
        //enterTime = Time.time;
        Debug.Log("courtoutine called");
        StartCoroutine(delayUI());
    }
    void OnTriggerExit(Collider collider)
    {
        if (collider.transform.parent.GetComponent<NetworkObject>().OwnerClientId != NetworkManager.Singleton.LocalClientId)
            return;
        StopAllCoroutines();
        Debug.Log("exit trig");

        EndingRitualUI.Instance.playerLeft();
    }

}
