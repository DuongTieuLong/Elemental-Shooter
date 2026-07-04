using UnityEngine;

public class LootDropper : MonoBehaviour
{
    public GameObject lootPrefab; // The prefab for the loot to drop
    public int experiencePoint;
    private void Start()
    {
        GameEvents.OnEnemyKilled += DropLoot; // Subscribe to the enemy killed event
    }

    private void DropLoot(Vector3 enemy, int expReward)
    {
        // Cố định lượng Exp là 10 (tương tự EnergyLoot cũ) -> Thay đổi: Lấy theo expReward của quái
        if (LootManager.Instance != null)
        {
            LootManager.Instance.RequestDrop(enemy, expReward, LootType.ExpEnergy);
            LootManager.Instance.RequestDrop(enemy,10, LootType.Gold);
        }
        else
        {
            Debug.LogWarning("LootManager is missing! Cannot drop item.");
        }
    }

    private void OnDestroy()
    {
        GameEvents.OnEnemyKilled -= DropLoot; // Hủy đăng ký
    }
}
