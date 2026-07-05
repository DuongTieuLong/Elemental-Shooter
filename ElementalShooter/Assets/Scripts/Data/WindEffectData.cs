using UnityEngine;

[CreateAssetMenu(fileName = "Wind Effect", menuName = "Effects/WindEffect", order = 3)]
public class WindEffectData : ScriptableObject, IStatusEffect
{
    [Header("Wind Settings")]
    public float duration = 0.5f; // Thời gian đẩy lùi ngắn
    public float knockbackForce = 8f; // Lực đẩy lùi ban đầu

    [Header("VFX Reference")]
    public GameObject vfxPrefab;

    public void Apply(GameObject target, StatusReceiver receiver, float multiplier)
    {
        var effect = new WindEffect(
            target, 
            duration, 
            knockbackForce * multiplier, 
            vfxPrefab,
            () => receiver.RemoveElementKey(ElementType.Wind)
        );
        receiver.Controller.ApplyEffect(effect);
    }
}
