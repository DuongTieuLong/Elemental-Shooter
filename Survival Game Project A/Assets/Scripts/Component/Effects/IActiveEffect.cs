

using UnityEngine;

public interface IActiveEffect
{
    bool IsExpired { get; }
    float DurationRemaining { get; }
    float MaxDuration { get; }
    void OnApply();  // ← thêm
    void Tick(float deltaTime);   // Logic mỗi frame
    void Refresh(IActiveEffect newEffect); // Khi bị áp lại lần 2
    void OnExpire(GameObject target, bool isForced = false);
}