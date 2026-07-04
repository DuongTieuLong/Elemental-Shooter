using System.Collections;
using UnityEngine;

public class BossWaveStrategy : IWaveStrategy
{
    private SpawnManager manager;
    private WaveConfig config;
    private int waveIndex;
    private bool isBossDead = false;

    public void Init(SpawnManager manager, WaveConfig config, int waveIndex)
    {
        this.manager = manager;
        this.config = config;
        this.waveIndex = waveIndex;
    }

    public IEnumerator ExecuteWave()
    {
        GameEvents.RaiseWaveStarted(WaveType.Boss);
        
        if (AudioManager.Instance != null) AudioManager.Instance.PlayBossBGM();

        isBossDead = false;
        GameEvents.OnBossHealthChanged += HandleBossHealth;

        // Phase 1: Spawning Boss and Minions
        foreach (var configItem in config.enemiesInWave)
        {
            for(int i = 0; i < configItem.count; i++)
            {
                manager.SpawnEnemy(configItem.enemyPrefab);
                yield return new WaitForSeconds(config.spawnInterval);
            }
        }

        Debug.Log("Đang chờ tiêu diệt Boss để qua màn...");
        
        // Phase 2: Wait for Boss to die
        while(!isBossDead)
        {
            yield return null;
        }
        
        GameEvents.OnBossHealthChanged -= HandleBossHealth;
        if (AudioManager.Instance != null) AudioManager.Instance.ResumeNormalBGM();
        
        GameEvents.RaiseWaveEnded(WaveType.Boss);
    }

    private void HandleBossHealth(float current, float max)
    {
        if (current <= 0)
        {
            isBossDead = true;
        }
    }
}
