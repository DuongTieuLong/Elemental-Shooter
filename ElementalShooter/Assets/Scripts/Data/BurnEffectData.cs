using UnityEngine;

// File: BurnEffectData.cs
[CreateAssetMenu(fileName = "Burn Effect", menuName = "Effects/BurnEffect",order =1)]
// Mỗi effect có 1 file data riêng, chỉnh trong Inspector
public class BurnEffectData : ScriptableObject, IStatusEffect
{
    [Header("Burn Settings")]
    public float duration = 3f;
    public float damagePerTick = 5f;
    public float tickInterval = 0.5f;
    public ElementType elementType = ElementType.Fire;

    [Header("VFX Reference")]
    public GameObject vfxPrefab;

    // Factory method — Data tự tạo ra Effect instance
    public IActiveEffect CreateEffect(GameObject targetGo, IDamageable target, float multiplier, StatusReceiver receiver)
    {
        return new BurnEffect(targetGo, target, duration, damagePerTick * multiplier, tickInterval, elementType, vfxPrefab, () => receiver.RemoveElementKey(elementType));
    }

    public void Apply(GameObject target, StatusReceiver receiver, float multiplier)
    {
        var damageable = target.GetComponent<IDamageable>();
        var effect = CreateEffect(target, damageable, multiplier, receiver);
        receiver.Controller.ApplyEffect(effect);


    }
}