using UnityEngine;

[CreateAssetMenu(fileName = "Shockwave Reaction", menuName = "Reactions/ShockwaveReaction")]
public class ShockwaveReaction : ReactionEffect
{
    [Header("Shockwave Settings")]
    public float radius = 5.0f;
    public float maxSpreadAngle = 90f; // Góc quét cánh cung 90 độ
    public float knockbackForce = 1.5f; // Lực đẩy lùi
    public float stunDuration = 1.5f; // Thời gian choáng
    public LayerMask enemyLayer;
    public ElementType reactionElement = ElementType.Electric;
    
    public GameObject shockwaveVfxPrefab;

    public override void Execute(GameObject source, ElementData oldData, ElementData newData)
    {
        // 1. Xác định hướng đẩy lùi (từ Player đến quái vật dính đòn gốc)
        Transform playerTransform = PlayerRegistry.PlayerTransform;

        Vector2 direction = Vector2.right;
        if (playerTransform != null)
        {
            direction = (source.transform.position - playerTransform.position).normalized;
        }

        // 2. Gây sát thương và choáng cho chính mục tiêu dính đòn gốc
        float baseDmg = Mathf.Max(oldData.SourceDamage, newData.SourceDamage);
        float avgMultiplier = (oldData.Multiplier + newData.Multiplier) / 2f;
        float finalDamage = baseDmg * 1.5f * (1f + avgMultiplier);

        if (source.TryGetComponent<IDamageable>(out var mainTarget))
        {
            mainTarget.TakeDamage(new DamageInfo
            {
                Amount = finalDamage,
                Element = reactionElement
            });
        }

        if (source.TryGetComponent<StatusReceiver>(out var mainReceiver))
        {
            var speedStat = source.GetComponent<IStatProvider>()?.GetStat(StatType.MoveSpeed);
            if (speedStat != null)
            {
                var stun = new StunEffect(source, speedStat, stunDuration, 0f, null, () => mainReceiver.RemoveElementKey(ElementType.Electric));
                mainReceiver.Controller.ApplyEffect(stun);
            }
        }

        // 3. Spawn VFX sóng cánh cung xoay theo hướng đẩy lùi
        if (shockwaveVfxPrefab != null)
        {
            Quaternion rotation = Quaternion.LookRotation(Vector3.forward, direction);
            GameObject vfx = Instantiate(shockwaveVfxPrefab, source.transform.position, rotation);

            // Tự động hủy VFX sau khi phát xong
            var ps = vfx.GetComponentInChildren<ParticleSystem>();
            if (ps != null)
            {
                var main = ps.main;
                main.stopAction = ParticleSystemStopAction.Destroy;
            }
            else
            {
                Destroy(vfx, 1.5f);
            }
        }

        // 4. Phát âm thanh phản ứng
        if (AudioManager.Instance != null && AudioManager.Instance.shootClip != null)
        {
            // Tăng âm lượng phát nổ
            AudioManager.Instance.PlaySFX(AudioManager.Instance.shootClip, source.transform.position, 1.0f, -0.3f);
        }

        // 5. Quét diện rộng hình nón (cánh cung) ra sau lưng quái vật dính đòn
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(source.transform.position, radius, enemyLayer);
        foreach (var col in hitColliders)
        {
            if (col.gameObject == source) continue; // Bỏ qua quái dính đòn gốc

            Vector3 toEnemy = (col.transform.position - source.transform.position).normalized;
            toEnemy.z = 0;

            // Tính góc lệch giữa hướng đẩy lùi và hướng đến kẻ địch xung quanh
            float angleDiff = Vector3.Angle(direction, toEnemy);
            if (angleDiff <= maxSpreadAngle / 2f)
            {
                // Nằm trong sóng cánh cung! Gây sát thương lan
                if (col.TryGetComponent<IDamageable>(out var target))
                {
                    target.TakeDamage(new DamageInfo
                    {
                        Amount = finalDamage * 0.7f, // Damage lan bằng 70% damage chính
                        Element = reactionElement
                    });
                }

                // Áp dụng Stun
                if (col.TryGetComponent<StatusReceiver>(out var receiver))
                {
                    var speedStat = col.GetComponent<IStatProvider>()?.GetStat(StatType.MoveSpeed);
                    if (speedStat != null)
                    {
                        var stun = new StunEffect(col.gameObject, speedStat, stunDuration, 0f, null, () => receiver.RemoveElementKey(ElementType.Electric));
                        receiver.Controller.ApplyEffect(stun);
                    }
                }
         
                // Áp dụng lực đẩy lùi
                col.transform.position += (Vector3)(direction * knockbackForce);
            }
        }
    }
}
