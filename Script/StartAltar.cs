using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class StartAltar : MonoBehaviour
{
    public GameObject canvas;
    public void promptStartUI()
    {
        canvas.SetActive(true);
    }
    IEnumerator delayUI()
    {
        /*
        while (enterTime < promptAfterSeconds)
        {
            yield return null;
        }*/
        yield return new WaitForSeconds(promptAfterSeconds);
        promptStartUI();
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
        StopAllCoroutines();
        //enterTime = Time.time;
        Debug.Log("courtoutine called");
        StartCoroutine(delayUI());
    }
    NetworkObject networkObject;
    void OnTriggerExit(Collider collider)
    {

        if (collider.transform.parent.TryGetComponent<NetworkObject>(out networkObject))
            if (networkObject.OwnerClientId == NetworkManager.Singleton.LocalClientId)
            {
                StopAllCoroutines();
                Debug.Log("exit trig");
            }

    }


}
