using System.Collections.Generic;
using UnityEngine;

public class StatusEffectController : MonoBehaviour, IEnemyComponent
{
    private Dictionary<System.Type, IActiveEffect> _activeEffects = new();
    private List<System.Type> _toRemove = new();
    private bool _isDespawning = false; // flag chặn Update khi đang despawn
    private EnemyController _controller;

    public void Initialize(EnemyController controller)
    {
        _controller = controller;
    }

    public void OnUpdateComponent(float deltaTime)
    {
        if (_isDespawning || _activeEffects.Count == 0) return;

        _toRemove.Clear();

        foreach (var kvp in _activeEffects)
        {
            kvp.Value.Tick(deltaTime);
            if (kvp.Value.IsExpired)
                _toRemove.Add(kvp.Key);

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

    public void OnSpawnComponent()
    {
        _isDespawning = false;
        _activeEffects.Clear();
        _toRemove.Clear();
    }

    public void OnDespawnComponent()
    {
        _isDespawning = true; // chặn Update trước
        ClearAllEffects();
    }

    private void ClearAllEffects()
    {
        // Vẫn phải lấy danh sách key vì OnExpire có thể sửa đổi dictionary (dù hiếm)
        // Dùng List tạm mượn Pool nếu cần, nhưng lúc chết gọi 1 lần thì tạm dùng List cũng được.
        // Để tối ưu triệt để, ta dùng _toRemove thay cho việc cấp phát mới:
        _toRemove.Clear();
        foreach (var kvp in _activeEffects)
        {
            _toRemove.Add(kvp.Key);
        }

        foreach (var key in _toRemove)
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