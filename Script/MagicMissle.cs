using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicMissle : PlayerSpell
{

    PlayerCollectOre playerCollectOre;
    public ParticleSystem particleSystemWindUp;
    public ParticleSystem particleSystemFire;
    public float useAmount = 1f;
    public int usedOreType = -1;
    public override int getOreType()
    {
        return usedOreType;
    }
    void Start()
    {
        playerCollectOre = GetComponent<PlayerCollectOre>();

    }

    // Update is called once per frame
    public override void StartCast()
    {
        particleSystemWindUp.Play();

    }
    public override void EndCast()
    {
        particleSystemWindUp.Stop();
        particleSystemFire.Play();


    }
    public override bool ChannelingCost()
    {
        return playerCollectOre.tryUseOre(9, useAmount);

    }
    public override bool checkCost()
    {
        return playerCollectOre.tryUseOre(9, useAmount);
    }
}
