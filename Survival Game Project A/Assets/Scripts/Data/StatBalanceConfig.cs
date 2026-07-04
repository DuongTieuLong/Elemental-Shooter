using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StatBalanceConfig", menuName = "ScriptableObjects/StatBalanceConfig", order = 5)]
public class StatBalanceConfig : ScriptableObject
{
    [System.Serializable]
    public struct StatLimit
    {
        public StatType statType;
        public float minValue;
        public float maxValue;
    }

    public List<StatLimit> limits;

    public float ClampValue(StatType type, float rawValue)
    {
        if (limits == null) return rawValue;
        foreach (var limit in limits)
        {
            if (limit.statType == type)
            {
                return Mathf.Clamp(rawValue, limit.minValue, limit.maxValue);
            }
        }
        return rawValue;
    }
}
