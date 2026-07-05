using System;
using UnityEngine;

[Serializable]
public class MeleeStrategyData : IAttackStrategyData
{
    [Header("Melee Settings")]
    public float meleeAttackRange = 1.5f;
    public float swingArcAngle = 160f;
    public float dashForwardDistance = 1.2f;
    
    [Header("Visuals & Effects")]
    public MeleeConfigData meleeConfig;
}
