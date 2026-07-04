using System.Collections;
using UnityEngine;

public class KillWaveStrategy : IWaveStrategy
{
    private SpawnManager manager;
    private WaveConfig config;
    private int waveIndex;
    
    private int totalEnemiesInCurrentWave;
    private int enemiesKilledInCurrentWave;
    private Coroutine spawnCoroutine;

    public void Init(SpawnManager manager, WaveConfig config, int waveIndex)
    {
        this.manager = manager;
        this.config = config;
        this.waveIndex = waveIndex;
    }

    public IEnumerator ExecuteWave()
    {
        GameEvents.RaiseWaveStarted(WaveType.KillBased);
        
        enemiesKilledInCurrentWave = 0;
        totalEnemiesInCurrentWave = 0;
        foreach (var configItem in config.enemiesInWave)
        {
            totalEnemiesInCurrentWave += configItem.count;
        }

        GameEvents.OnEnemyKilled += HandleEnemyKilled;
        
        GameEvents.RaiseWaveProgressChanged(0f, waveIndex + 1);

        // Bắt đầu spawn quái
        spawnCoroutine = manager.StartCoroutine(SpawnRoutine());

        // Đợi cho đến khi giết hết
        while (enemiesKilledInCurrentWave < totalEnemiesInCurrentWave)
        {
            yield return null;
        }

        if (spawnCoroutine != null)
        {
            manager.StopCoroutine(spawnCoroutine);
        }

        GameEvents.RaiseWaveProgressChanged(1f, waveIndex + 1);
        GameEvents.OnEnemyKilled -= HandleEnemyKilled;
        GameEvents.RaiseWaveEnded(WaveType.KillBased);
        
        // Đã giết đủ số lượng quái yêu cầu nên không cần chờ restTime
        // Sẽ lập tức chuyển sang Wave mới (hoặc Endgame nếu là Wave cuối)
    }
    
    private IEnumerator SpawnRoutine()
    {
        foreach (var configItem in config.enemiesInWave)
        {
            for(int i = 0; i < configItem.count; i++)
            {
                manager.SpawnEnemy(configItem.enemyPrefab);
                yield return new WaitForSeconds(config.spawnInterval);
            }
        }
    }

    private void HandleEnemyKilled(Vector3 pos, int exp)
    {
        enemiesKilledInCurrentWave++;
        float progress = (float)enemiesKilledInCurrentWave / totalEnemiesInCurrentWave;
        // Đảm bảo progress không vượt quá 1
        GameEvents.RaiseWaveProgressChanged(Mathf.Clamp01(progress), waveIndex + 1);
    }
}
