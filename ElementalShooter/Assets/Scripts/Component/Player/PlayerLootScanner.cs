using UnityEngine;

public class PlayerLootScanner : MonoBehaviour
{
    [Header("Scanner Settings")]
    public float scanRadius = 3.5f;
    public float scanInterval = 0.15f;
    public LayerMask lootLayer; // Set this to "Loot" layer

    private Collider2D[] results = new Collider2D[50]; // Pre-allocated mảng để chứa kết quả (0 GC Alloc)
    private ContactFilter2D filter;
    private float nextScanTime;

    private void Start()
    {
        // Cấu hình filter để chỉ quét Layer Loot và có sử dụng Trigger
        filter = new ContactFilter2D();
        filter.useTriggers = true;
        filter.useLayerMask = true;
        filter.SetLayerMask(lootLayer);
    }

    private void Update()
    {
        if (Time.time >= nextScanTime)
        {
            ScanForLoot();
            nextScanTime = Time.time + scanInterval;
        }
    }

    private void ScanForLoot()
    {
        // Sử dụng OverlapCircle NonAlloc của Unity
        int count = Physics2D.OverlapCircle(transform.position, scanRadius, filter, results);

        for (int i = 0; i < count; i++)
        {
            if (results[i].TryGetComponent(out DropItem dropItem))
            {
                if (!dropItem.isAttracting && LootManager.Instance != null)
                {
                    LootManager.Instance.StartAttractingDrop(dropItem, transform);
                }
            }
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, scanRadius);
    }
}
