using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct LootPrefabMapping
{
    public LootType type;
    public GameObject prefab;
}

public class LootManager : MonoBehaviour
{
    public static LootManager Instance { get; private set; }

    public List<LootPrefabMapping> lootPrefabs;
    
    private List<DropItem> activeDrops = new List<DropItem>(1000);
    private List<DropItem> attractingDrops = new List<DropItem>(100);

    public float mergeRadiusSqr = 1.5f * 1.5f;

    private Dictionary<LootType, GameObject> prefabDict;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        prefabDict = new Dictionary<LootType, GameObject>();
        foreach (var mapping in lootPrefabs)
        {
            prefabDict[mapping.type] = mapping.prefab;
        }
    }

    public void RequestDrop(Vector3 position, int value, LootType type)
    {
        // 1. Thử gộp với hạt đã có (Merging)
        // Chỉ gộp đối với tài nguyên thông thường (không gộp Jackpot nếu không cần thiết)
        if (type == LootType.ExpEnergy || type == LootType.Gold || type == LootType.Material)
        {
            for (int i = 0; i < activeDrops.Count; i++)
            {
                DropItem item = activeDrops[i];
                if (item.lootType == type && !item.isAttracting)
                {
                    if ((item.transform.position - position).sqrMagnitude < mergeRadiusSqr)
                    {
                        item.Merge(value);
                        return; // Gộp thành công, không sinh hạt mới
                    }
                }
            }
        }

        // 2. Không có hạt để gộp, tạo mới
        if (prefabDict.TryGetValue(type, out GameObject prefab))
        {
            GameObject obj = PoolManager.Instance.Get(prefab, position);
            if (obj.TryGetComponent(out DropItem dropItem))
            {
                dropItem.Initialize(value, type);
                activeDrops.Add(dropItem);
            }
        }
        else
        {
            Debug.LogWarning($"LootManager: Không tìm thấy prefab cho loại {type}");
        }
    }

    public void StartAttractingDrop(DropItem item, Transform target)
    {
        if (!item.isAttracting)
        {
            item.StartAttracting(target);
            activeDrops.Remove(item);
            attractingDrops.Add(item);
        }
    }

    private void Update()
    {
        // Xử lý di chuyển cho các vật phẩm đang bị hút về Player
        // Duyệt ngược để an toàn khi xóa phần tử khỏi list
        for (int i = attractingDrops.Count - 1; i >= 0; i--)
        {
            DropItem item = attractingDrops[i];
            
            if (item.target == null) 
            {
                // Nếu player chết hoặc mất mục tiêu, trả về active drops
                item.OnDespawn();
                attractingDrops.RemoveAt(i);
                activeDrops.Add(item);
                continue;
            }

            // Cập nhật tốc độ (bay từ từ rồi nhanh dần)
            item.currentSpeed += item.acceleration * Time.deltaTime;
            item.currentSpeed = Mathf.Min(item.currentSpeed, item.maxSpeed);

            // Di chuyển
            item.transform.position = Vector3.MoveTowards(item.transform.position, item.target.position, item.currentSpeed * Time.deltaTime);

            // Kiểm tra đã nhặt được chưa
            if ((item.transform.position - item.target.position).sqrMagnitude < 0.25f) // Gần hơn 0.5 đơn vị
            {
                item.OnCollected();
                attractingDrops.RemoveAt(i);
                
                // Cần trả về PoolManager. Cần prefab nguyên bản để trả, ta có thể dùng dictionary
                if (prefabDict.TryGetValue(item.lootType, out GameObject prefab))
                {
                    PoolManager.Instance.ReturnToPool(prefab, item.gameObject);
                }
                else
                {
                    item.gameObject.SetActive(false); // Fallback
                }
            }
        }
    }
}
