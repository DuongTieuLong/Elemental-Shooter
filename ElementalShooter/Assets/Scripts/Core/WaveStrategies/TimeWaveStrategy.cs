using System.Collections;
using UnityEngine;

public class TimeWaveStrategy : IWaveStrategy
{
    private SpawnManager manager;
    private WaveConfig config;
    private int waveIndex;
    
    private int totalEnemiesInCurrentWave;
    private int enemiesKilledInCurrentWave;

    public void Init(SpawnManager manager, WaveConfig config, int waveIndex)
    {
        this.manager = manager;
        this.config = config;
        this.waveIndex = waveIndex;
    }

    public IEnumerator ExecuteWave()
    {
        GameEvents.RaiseWaveStarted(WaveType.TimeBased);
        
        enemiesKilledInCurrentWave = 0;
        totalEnemiesInCurrentWave = 0;
        foreach (var configItem in config.enemiesInWave)
        {
            totalEnemiesInCurrentWave += configItem.count;
        }

        float spawnTime = totalEnemiesInCurrentWave * config.spawnInterval;
        float totalWaveTime = spawnTime + config.restTime;
        float currentWaveTimer = 0f;

        GameEvents.OnEnemyKilled += HandleEnemyKilled;

        // Phase 1: Spawning
        foreach (var configItem in config.enemiesInWave)
        {
            for(int i = 0; i < configItem.count; i++)
            {
                manager.SpawnEnemy(configItem.enemyPrefab);
                
                if (totalWaveTime > 0)
                {
                    currentWaveTimer += config.spawnInterval;
                    GameEvents.RaiseWaveProgressChanged(Mathf.Clamp01(currentWaveTimer / totalWaveTime), waveIndex + 1);
                }
                
                yield return new WaitForSeconds(config.spawnInterval);
            }
        }

        // Phase 2: Rest Time
        float restTimer = 0f;
        while (restTimer < config.restTime)
        {
            if (enemiesKilledInCurrentWave >= totalEnemiesInCurrentWave)
            {
                Debug.Log("Dọn sạch quái sớm! Skip restTime chuyển Wave luôn!");
                GameEvents.RaiseWaveProgressChanged(1f, waveIndex + 1);
                break; 
            }

            restTimer += Time.deltaTime;
            currentWaveTimer += Time.deltaTime;
            GameEvents.RaiseWaveProgressChanged(Mathf.Clamp01(currentWaveTimer / totalWaveTime), waveIndex + 1);
            yield return null;
        }

        GameEvents.OnEnemyKilled -= HandleEnemyKilled;
        GameEvents.RaiseWaveEnded(WaveType.TimeBased);
    }

    private void HandleEnemyKilled(Vector3 pos, int exp)
    {
        enemiesKilledInCurrentWave++;
    }
}
