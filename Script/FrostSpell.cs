using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class FrostSpell : PlayerSpell
{
    public int usedOreType = 2;

    PlayerCollectOre playerCollectOre;
    public ParticleSystem particleSystem;
    public ParticleSystem particleSystemRunes;
    public float useAmount = 1f;
    public float defaultUseAmount = 1f;
    public float casttimef = 0.5f;
    public override int getOreType()
    {
        return usedOreType;
    }
    IEnumerator casttime()
    {

        particleSystemRunes.Play();
        yield return new WaitForSeconds(casttimef);
        particleSystemRunes.Stop();

        particleSystem.Play();

    }

    // Start is called before the first frame update
    void Start()
    {
        playerCollectOre = GetComponent<PlayerCollectOre>();

        Upgrades.Instance.newUpgradeEvent.AddListener(HandleUpgradeEvent);

    }
    void HandleUpgradeEvent(ulong ID, string upgradeName)
    {
        if (ID == NetworkManager.Singleton.LocalClientId)
        {
            if (upgradeName == "freezeSpeed")
            {
                freezeAmount = defaultFreezeAmount + Upgrades.Instance.getUpgradeLevel(upgradeName);
            }
            if (upgradeName == "freezeRange")
            {
                raycastRange = defaultRaycastRange + 0.1f * Upgrades.Instance.getUpgradeLevel(upgradeName);
            }
            if (upgradeName == "freezeSavingOre")
            {
                useAmount = defaultUseAmount - 0.05f * Upgrades.Instance.getUpgradeLevel(upgradeName);
                useAmount = Mathf.Max(useAmount, 0.01f);
            }
        }
    }
    // Update is called once per frame
    public override void StartCast()
    {
        StartCoroutine(casttime());

    }
    public override void EndCast()
    {
        particleSystem.Stop();
        StopAllCoroutines();
        stopAudioEffect();
    }
    RaycastHit raycastHit;
    EnemyHealth enemyHealth;
    public float raycastCD = 0.1f;
    float lastTimeRaycast;
    public float raycastRange = 2f;
    public float defaultRaycastRange = 2f;
    public float defaultFreezeAmount = 10f;
    public float freezeAmount = 10f;
    public override bool ChannelingCost()
    {
        bool successful = playerCollectOre.tryUseOre(usedOreType, useAmount * Time.deltaTime);
        if (successful && lastTimeRaycast + raycastCD < Time.time)
        {
            if (Physics.Raycast(transform.position, transform.forward, out raycastHit, raycastRange))
            {
                if (raycastHit.collider.gameObject.TryGetComponent<EnemyHealth>(out enemyHealth))
                {
                    lastTimeRaycast = Time.time;
                    enemyHealth.dealFreeze(freezeAmount);
                }
            }
            playAudioEffect();
        }
        return successful;
    }

    public AudioSource audioSource;
    public AudioClip audioClip;
    void playAudioEffect()
    {
        if (!audioSource.isPlaying)
        {
            audioSource.clip = audioClip;
            audioSource.Play();
        }
    }
    void stopAudioEffect()
    {
        audioSource.Stop();
    }
    public override bool checkCost()
    {
        /*
        if (playerCollectOre.ores.ContainsKey(usedOreType))
        { return playerCollectOre.ores[usedOreType] > useAmount; }
        else { return false; }
*/
        return playerCollectOre.tryUseOre(usedOreType, useAmount);

    }
}
