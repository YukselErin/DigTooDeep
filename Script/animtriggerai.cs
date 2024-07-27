using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class animtriggerai : MonoBehaviour
{
    public EnemyAI enemyAI;
    public void str(){
        enemyAI.jumpStart();
    }
     public void end(){
        enemyAI.jumpEnd();
    }
}
