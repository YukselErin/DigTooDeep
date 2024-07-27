using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Unity.Netcode;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PlayerHealth : NetworkBehaviour
{
    public Volume currentvolume;
    Vignette vignette;
    public float blackoutrate = 1f;
    float currentIntensity;
    float vignetteDefaultIntensity;
    public void RestoreBlackout()
    {
        StopAllCoroutines();
        vignette.intensity.Override(vignetteDefaultIntensity);
    }
    public float vignetteintensityoffset = 2f;
    public void Blackout()
    {
        currentIntensity = vignette.intensity.GetValue<float>();
        ClampedFloatParameter blackout = new ClampedFloatParameter(((100f - hitPoints)/ 100f)-vignetteintensityoffset, 0f, 1f, false);
        vignette.intensity.Override(((100f - hitPoints)/ 100f)-vignetteintensityoffset);
    }



    public UnityEvent PlayerDeathEvent;
    public NetworkVariable<bool> PlayerDead = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public bool GetPlayerIsDead()
    {
        return PlayerDead.Value;
    }
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            PlayerDead.Value = false;
        }
        GameStateController.Instance.listenPlayerDeaths();
    }
    void Start()
    {
        if (PlayerDeathEvent == null)
            PlayerDeathEvent = new UnityEvent();
        Upgrades.Instance.newUpgradeEvent.AddListener(HandleUpgradeEvent);
        currentvolume = GlobalVolumeController.Instance.volume;
        currentvolume.profile.TryGet<Vignette>(out vignette);

        GameStateController.Instance.GameRestartEvent.AddListener(HandlePlayerRespawn);
    }
    void HandlePlayerRespawn()
    {
        PlayerDead.Value = false;
        hitPoints = MaxHitPoints;
        RestoreBlackout();
    }

    public bool debugDealDamage = false;
    void Update()
    {


        if (debugDealDamage)
        {
            dealDamage(10f);
            debugDealDamage = false;
        }
    }
    void HandleUpgradeEvent(ulong ID, string upgradeName)
    {
        if (ID == NetworkManager.Singleton.LocalClientId)
        {

            if (upgradeName == "increaseHp")
            {
                MaxHitPoints = defaultMaxHitPoints + (defaultMaxHitPoints * 0.1f) * Upgrades.Instance.getUpgradeLevel(upgradeName);
            }
            if (upgradeName == "increaseRegen")
            {
                regenCD = defaultRegenCD + (defaultRegenCD * 0.1f) * Upgrades.Instance.getUpgradeLevel(upgradeName);

            }
        }
    }
    public float defaultMaxHitPoints = 100f;
    public float MaxHitPoints = 100f;
    public float hitPoints = 100f;
    float lastTimeTookDamage;
    bool coroutineRunning = false;
    public float defaultRegenCD = 10f;
    public float regenCD = 10f;
    public float defaultRegenAmount = 1f;
    public float regenAmount = 1f;
    void PlayerDeath()
    {

        PlayerDead.Value = true;
        PlayerDeathEvent.Invoke();
    }
    public void dealDamage(float value)
    {
        if (!IsOwner)
            return;
        hitPoints -= value;
        Blackout();
        if (hitPoints <= 0f)
        {
            PlayerDeath();
        }
        else
        {
            lastTimeTookDamage = Time.time;
            if (!coroutineRunning)
            {
                StartCoroutine(regen());
            }
            coroutineRunning = true;
        }
    }
    IEnumerator regen()
    {
        if (coroutineRunning)
        {
            yield return null;
        }
        else
        {
            while (MaxHitPoints > hitPoints)
            {
                Blackout();
                if (lastTimeTookDamage + regenCD < Time.time)
                {
                    hitPoints += regenAmount * Time.deltaTime;
                    yield return null;
                }
                if (hitPoints <= 0f)
                {

                    break;
                }
                yield return null;

            }
            coroutineRunning = false;
        }
    }
}
