using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public static class GameEvents
{
    //Player
    public static Action<float,float> OnPlayerHealthChanged;
    public static Action OnPlayerDeath;
    public static Action<Transform> OnPlayerSpawned;
    public static Action OnPlayerHit;
    public static Action<Vector2> OnBulletFired;

    //Enemy Events
    public static Action<Vector3, int> OnEnemyKilled;
    public static event Action<float, float> OnBossHealthChanged;

    //EnergyDrop Events
    public static Action<int> OnEnergyCollected;

    //UI Events
    public static Action<float, int> OnWaveProgressChanged;
    public static Action<WaveType> OnWaveStarted;
    public static Action<WaveType> OnWaveEnded;
    public static Action<int> OnComboUpdated;
    public static Action<int> OnPlayTimeTicked;
    public static Action<int> OnKillCountChanged;

    //Level Events
    public static Action OnLevelUp;

    //Jackpot Events
    public static Action<Vector3, ScriptableObject> OnJackpotTriggered;
    public static Action<float> OnHitStopRequested;
    public static Action<float, float> OnCameraShakeRequested;

    //Upgrade Events
    public static Action<CardUpgrade> OnUpgradeSelected;
    public static Action<List<CardUpgrade>> OnRequestUpgradeUI;
    public static Action OnStatUpdated;
    public static Action OnVictory;
    public static Action<float, Vector3, ElementType> OnDamageDealt;
    public static Action<GameState> OnStateChanged;

    //Player Events
    public static void RaisePlayerHealthChanged(float currentHealth, float maxHealth)
    {
        OnPlayerHealthChanged?.Invoke(currentHealth,maxHealth);
    }
    public static void RaisePlayerDeath()
    {
        OnPlayerDeath?.Invoke();
    }
    public static void RaisePlayerHit()
    {
        OnPlayerHit?.Invoke();
    }

    //Enemy Events
    public static void RaiseEnemyKilled(Vector3 position, int expReward)
    {
        OnEnemyKilled?.Invoke(position, expReward);
    }
    public static void RaiseBossHealthChanged(float currentHealth, float maxHealth)
    {
        OnBossHealthChanged?.Invoke(currentHealth, maxHealth);
    }


    public static void RaisePlayerSpawned(Transform playerTranfrom)
    {
        OnPlayerSpawned?.Invoke(playerTranfrom);
        Debug.Log("Player spawned event raised with transform: " + playerTranfrom);
    }

    public static void RaiseEnergyCollected(int amount)
    {
        OnEnergyCollected?.Invoke(amount);
    }

    //UI Custom Events
    public static void RaiseWaveProgressChanged(float progress, int currentWave)
    {
        OnWaveProgressChanged?.Invoke(progress, currentWave);
    }
    public static void RaiseWaveStarted(WaveType type)
    {
        OnWaveStarted?.Invoke(type);
    }
    public static void RaiseWaveEnded(WaveType type)
    {
        OnWaveEnded?.Invoke(type);
    }
    public static void RaiseComboUpdated(int comboCount)
    {
        OnComboUpdated?.Invoke(comboCount);
    }
    public static void RaisePlayTimeTicked(int seconds)
    {
        OnPlayTimeTicked?.Invoke(seconds);
    }
    public static void RaiseKillCountChanged(int totalKills)
    {
        OnKillCountChanged?.Invoke(totalKills);
    }

    public static void RaiseRequestUpgradeUI(List<CardUpgrade> cardUpgrades)
    {
        OnRequestUpgradeUI?.Invoke(cardUpgrades);
    }

    public static void RaiseUpgradeSelected(CardUpgrade cardUpgrade) 
    {
        OnUpgradeSelected?.Invoke(cardUpgrade);
    }

    public static void RaiseStatUpdated()
    {
        OnStatUpdated?.Invoke();
    }

    public static void RaiseVictory()
    {
        OnVictory?.Invoke();
    }

    public static void RaiseDamageDealt(float damage, Vector3 position, ElementType element)
    {
        OnDamageDealt?.Invoke(damage, position, element);
    }

    public static void RaiseStateChanged(GameState state)
    {
        OnStateChanged?.Invoke(state);
    }
}
