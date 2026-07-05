using UnityEngine;

[CreateAssetMenu(fileName = "Explosion Reaction", menuName = "Reactions/ExplosionReaction")]
public class ExplosionReaction : ReactionEffect
{
    [Header("Settings")]
    public float reactionMultiplier = 2.0f; // Hệ số riêng của phản ứng nổ
    public float radius;
    public LayerMask enemyLayer;
    public ElementType elementType;

    [Header("VFX Settings")]
    public GameObject explosionVfxPrefab;

    public override void Execute(GameObject source, ElementData oldData, ElementData newData)
    {
    
        float baseDmg = Mathf.Max(oldData.SourceDamage, newData.SourceDamage); // số cao nhất lúc này
        float avgMultiplier = (oldData.Multiplier + newData.Multiplier) / 2;  // tính trung bình hệ số nhân của 2 nguyên tố
        //Debug.Log($"Base Damage: {baseDmg}, Old Multiplier: {oldData.Multiplier}, New Multiplier: {newData.Multiplier}, Avg Multiplier: {avgMultiplier}");

        // Tính toán damage cuối cùng
        float finalDamage = baseDmg * reactionMultiplier * (1 + avgMultiplier);

        // Logic nổ diện rộng như cũ nhưng dùng finalDamage
       // Debug.Log($"Phản ứng nổ gây: {finalDamage} damage ( baseDame {baseDmg} avgMultiplier {avgMultiplier})");

        // Spawn VFX vụ nổ
        if (explosionVfxPrefab != null)
        {
            GameObject vfx = PoolManager.Instance.Get(explosionVfxPrefab, source.transform.position);
            var poolReturn = vfx.GetComponent<ParticlePoolReturn>();
            if (poolReturn != null)
            {
                poolReturn.SetBasePrefabs(explosionVfxPrefab);
            }
        }

        // Phát âm thanh nổ phản ứng nguyên tố
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.explosionClip, source.transform.position, 1.0f, 0.08f);
        }

        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(source.transform.position, radius, enemyLayer);
        foreach (var hit in hitColliders)
        {
            IDamageable target = hit.GetComponent<IDamageable>();
            if (target != null)
            {
                target.TakeDamage(new DamageInfo
                {
                    Amount = finalDamage,
                    Element = elementType,
                });
            }
        }


    }
}