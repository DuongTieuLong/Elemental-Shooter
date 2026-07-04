using System.Collections.Generic;
using UnityEngine;

public class StatusEffectController : MonoBehaviour, IPoolable
{
    private Dictionary<System.Type, IActiveEffect> _activeEffects = new();
    private List<System.Type> _toRemove = new();
    private bool _isDespawning = false; // flag chặn Update khi đang despawn

    private void Update()
    {
        if (_isDespawning || _activeEffects.Count == 0) return;

        float dt = Time.deltaTime;
        _toRemove.Clear();

        var keys = new List<System.Type>(_activeEffects.Keys);

        foreach (var key in keys)
        {
            // Phải check lại vì có thể effect đã bị xóa bởi logic chết ở phía trước
            if (_activeEffects.TryGetValue(key, out var effect))
            {
                effect.Tick(dt);
                if (effect.IsExpired)
                    _toRemove.Add(key);
            }

            // Nếu quái chết trong lúc Tick, dừng vòng lặp ngay
            if (_isDespawning) return;
        }

        // Xử lý xóa các effect đã hết hạn tự nhiên
        foreach (var key in _toRemove)
        {
            if (_activeEffects.TryGetValue(key, out var effect))
            {
                _activeEffects.Remove(key);
                effect.OnExpire(gameObject, false);
            }
        }
    }

    public void ApplyEffect(IActiveEffect effect)
    {
        if (_isDespawning) return; // quái đang chết, không nhận effect mới

        var type = effect.GetType();
        if (_activeEffects.TryGetValue(type, out var existing))
            existing.Refresh(effect);
        else
        {
            _activeEffects[type] = effect;
            effect.OnApply();
        }
    }

    public void OnSpawn()
    {
        _isDespawning = false;
        _activeEffects.Clear();
        _toRemove.Clear();
    }

    public void OnDespawn()
    {
        _isDespawning = true; // chn Update trướcặ
        ClearAllEffects();
    }

    private void ClearAllEffects()
    {
        
        var keys = new List<System.Type>(_activeEffects.Keys);
        foreach (var key in keys)
        {
            if (_activeEffects.TryGetValue(key, out var effect))
            {
                _activeEffects.Remove(key);
                effect.OnExpire(gameObject, true);
            }
        }
        _activeEffects.Clear();
        _toRemove.Clear();
    }

    public bool TryGetEffectDuration(System.Type effectType, out float durationRemaining, out float maxDuration)
    {
        if (_activeEffects.TryGetValue(effectType, out var effect))
        {
            durationRemaining = effect.DurationRemaining;
            maxDuration = effect.MaxDuration;
            return true;
        }
        durationRemaining = 0f;
        maxDuration = 0f;
        return false;
    }
}