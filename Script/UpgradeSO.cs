using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "UpgradeSo", menuName = "ScriptableObjects/UpgradeSo", order = 2)]

public class UpgradeSO : ScriptableObject
{
    public string upgradeName;
    public string descriptionText;
    public int level = 0;
    public string[] prerequisite;
    public int[] requiredOreStarType;
    public int[] requiredOreStarAmount;
}
