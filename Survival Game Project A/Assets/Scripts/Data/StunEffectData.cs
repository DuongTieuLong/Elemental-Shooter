using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Stun Effect", menuName = "Effects/StunEffect", order = 2)]
public class StunEffectData : ScriptableObject, IStatusEffect
{
    [Header("Stun Settings")]
    public float duration = 1.5f;
    public float speedMultiplier = 0f; // 0 = dừng hẳn, 0.5 = đi nửa tốc độ

    [Header("VFX Reference")]
    public GameObject vfxPrefab;

    public void Apply(GameObject target, StatusReceiver receiver, float multiplier)
    {
        var speedStat = target.GetComponent<IStatProvider>().GetStat(StatType.MoveSpeed);
        var effect = new StunEffect(target, speedStat, duration * multiplier, speedMultiplier, vfxPrefab, () => receiver.RemoveElementKey(ElementType.Electric));
        receiver.Controller.ApplyEffect(effect);
    }
}