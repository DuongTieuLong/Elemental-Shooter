using UnityEngine;

[CreateAssetMenu(fileName = "Tornado Reaction", menuName = "Reactions/TornadoReaction")]
public class TornadoReaction : ReactionEffect
{
    [Header("Tornado References")]
    [SerializeField] private GameObject tornadoPrefab;

    [Header("Base Settings")]
    [SerializeField] private float baseDamage = 12f;
    [SerializeField] private float basePullSpeed = 4f;
    [SerializeField] private float pullRadius = 5f;
    [SerializeField] private float duration = 3.5f;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private ElementType reactionElement = ElementType.Fire;

    public override void Execute(GameObject target, ElementData oldData, ElementData newData)
    {
        if (tornadoPrefab == null)
        {
            Debug.LogWarning("TornadoPrefab is not assigned in TornadoReaction ScriptableObject!");
            return;
        }

        // Tính toán chỉ số nâng cao dựa trên sức mạnh nguyên tố của hai phát bắn
        float avgMultiplier = (oldData.Multiplier + newData.Multiplier) / 2f;
        float finalDamage = baseDamage * (1f + avgMultiplier);
        float finalPullSpeed = basePullSpeed * (1f + avgMultiplier * 0.2f);

        Debug.Log($"[TornadoReaction] Creating Tornado Vortex! Damage: {finalDamage:F1}, PullSpeed: {finalPullSpeed:F1}");

        // Sinh lốc xoáy tại vị trí va chạm của kẻ địch dính đạn
        GameObject tornadoObj = Instantiate(tornadoPrefab, target.transform.position, Quaternion.identity);
        
        if (tornadoObj.TryGetComponent<TornadoVortex>(out var vortex))
        {
            vortex.damagePerTick = finalDamage;
            vortex.pullSpeed = finalPullSpeed;
            vortex.pullRadius = pullRadius;
            vortex.duration = duration;
            vortex.enemyLayer = enemyLayer;
            vortex.elementType = reactionElement;
        }
    }
}