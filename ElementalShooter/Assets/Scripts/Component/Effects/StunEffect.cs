using System;
using UnityEngine;

public class StunEffect : IActiveEffect
{
    private float _duration;
    private float _maxDuration; // Thêm biến lưu thời gian tối đa
    private Stat _speedStat;
    private int _modifierId = -1; // -1 = chưa áp dụng
    private float _modValue;

    private GameObject _targetObj;
    private GameObject _vfxPrefab;
    private GameObject _vfxInstance;

    public bool IsExpired => _duration <= 0;
    public float DurationRemaining => _duration;
    public float MaxDuration => _maxDuration;

    private Action onExpireCallback;

    public StunEffect(GameObject targetObj, Stat speedStat, float duration, float speedMultiplier, GameObject vfxPrefab, Action onExpireCallback = null)
    {
        _targetObj = targetObj;
        _speedStat = speedStat;
        _duration = duration;
        _maxDuration = duration;
        _modValue = speedMultiplier - 1f;
        _vfxPrefab = vfxPrefab;

        this.onExpireCallback = onExpireCallback;
    }

    // Gọi 1 lần duy nhất khi effect được ADD vào Controller
    public void OnApply()
    {
        _modifierId = _speedStat.AddModifier(_modValue, ModifierType.Percent);

        if (_vfxPrefab != null && _targetObj != null && _targetObj.activeInHierarchy)
        {
            _vfxInstance = GameObject.Instantiate(_vfxPrefab, _targetObj.transform);
        }
    }

    public void Tick(float dt) => _duration -= dt;

    public void Refresh(IActiveEffect incoming)
    {
        if (incoming is StunEffect s)
        {
            _duration = Mathf.Max(_duration, s._duration);
            _maxDuration = Mathf.Max(_maxDuration, s._maxDuration);
        }
    }

    public void OnExpire(GameObject target, bool isForced = false)
    {
        if (_modifierId >= 0)
            _speedStat.RemoveModifier(_modifierId);

        if (_vfxInstance != null)
        {
            GameObject.Destroy(_vfxInstance);
            _vfxInstance = null;
        }

        if (!isForced)
        {
            onExpireCallback?.Invoke();
        }
    }
}