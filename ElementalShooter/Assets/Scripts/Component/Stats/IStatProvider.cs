using UnityEngine;

public interface IStatProvider
{
    Stat GetStat(StatType type);
}