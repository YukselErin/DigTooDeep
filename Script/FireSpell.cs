using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;
using Unity.Netcode;

public class FireSpell : PlayerSpell
{
    public int usedOreType = 2;

    PlayerCollectOre playerCollectOre;
    public ParticleSystem particleSystem;
    public ParticleSystem particleSystemRunes;
    public float defaultUseAmount = 1f;
    public float useAmount = 1f;
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
        Upgrades.Instance.newUpgradeEvent.AddListener(HandleUpgradeEvent);

    }
    void HandleUpgradeEvent(ulong ID, string upgradeName)
    {
        if (ID == NetworkManager.Singleton.LocalClientId)
        {
            if (upgradeName == "fireDamage")
            {
                damage = defaultDamage + 0.1f * Upgrades.Instance.getUpgradeLevel(upgradeName);
            }
            if (upgradeName == "fireRange")
            {
                raycastRange = defaultRaycastRange + 0.1f * Upgrades.Instance.getUpgradeLevel(upgradeName);
            }

            if (upgradeName == "fireSavingOre")
            {
                useAmount = defaultUseAmount - 0.05f * Upgrades.Instance.getUpgradeLevel(upgradeName);
                useAmount = Mathf.Max(useAmount, 0.01f);
            }
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        playerCollectOre = GetComponent<PlayerCollectOre>();
        damage = defaultDamage;
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
    public float defaultRaycastRange = 2f;
    public float raycastRange = 2f;
    public AudioSource audioSource;
    public AudioClip audioClip;
    public float defaultDamage = 1f;
    public float damage = 1f;
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
    public override bool ChannelingCost()
    {
        bool successful = playerCollectOre.tryUseOre(usedOreType, useAmount*Time.deltaTime);
        if (successful && lastTimeRaycast + raycastCD < Time.time)
        {
            if (Physics.Raycast(transform.position, transform.forward, out raycastHit, raycastRange))
            {
                if (raycastHit.collider.gameObject.TryGetComponent<EnemyHealth>(out enemyHealth))
                {
                    lastTimeRaycast = Time.time;
                    Debug.Log("Raycast hit");
                    enemyHealth.dealDamage(damage);
                }
            }
            playAudioEffect();
        }
        return successful;
    }
    public override bool checkCost()
    {
        /*if (playerCollectOre.ores.ContainsKey(usedOreType))
        { return playerCollectOre.ores[usedOreType] > useAmount; }
        else { return false; }*/
        return playerCollectOre.tryUseOre(usedOreType, useAmount);

    }
}
