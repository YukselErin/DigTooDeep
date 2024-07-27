using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAnimationTriggerDamage : MonoBehaviour
{
    public EnemyAttack enemyAttack;
    public void triggerDealDamage()
    {
        enemyAttack.DealDamage();
    }
}
