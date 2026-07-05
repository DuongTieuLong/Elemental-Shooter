using UnityEngine;

[RequireComponent(typeof(EnemyHealth))] // Bắt buộc phải đi kèm với EnemyHealth
public class BossHealthBroadcaster : MonoBehaviour
{
    private EnemyHealth bossHealth;

    private void Awake()
    {
        bossHealth = GetComponent<EnemyHealth>();
    }

    private void OnEnable()
    {
        // Khi Boss xuất hiện, báo cho GameEvents biết máu khởi điểm
        bossHealth.OnHealthChanged += HandleBossHealthChanged;
    }

    private void OnDisable()
    {
        bossHealth.OnHealthChanged -= HandleBossHealthChanged;

        // Boss chết hoặc biến mất thì báo kết thúc
    }

    private void HandleBossHealthChanged(float current, float max)
    {
        Debug.Log("Handle Boss health changede     1");
        // Dịch sự kiện cục bộ thành sự kiện toàn cục để UI cập nhật
        GameEvents.RaiseBossHealthChanged(current, max);
    }
}