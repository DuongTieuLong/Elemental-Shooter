using System.Collections.Generic;
using UnityEngine;

public enum WaveType
{
    TimeBased,
    KillBased,
    Boss
}

[System.Serializable]
public class EnemySpawnConfig
{
    public GameObject enemyPrefab;
    public int count;
}

[CreateAssetMenu(fileName = "WaveConfig", menuName = "ScriptableObjects/WaveConfig")]
public class WaveConfig : ScriptableObject
{
    public List<EnemySpawnConfig> enemiesInWave;
    public float spawnInterval = 1f; // 1 giây/con như cậu muốn
    public float restTime = 15f;     // Thời gian nghỉ giữa các wave
    public WaveType waveType = WaveType.TimeBased; // Loại Wave
}