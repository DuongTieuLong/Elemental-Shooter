using UnityEngine;
using System.Collections.Generic;

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance;
    public Transform poolContainer; // Nơi chứa các Object đã được trả về Pool

    // Lưu trữ các Queue dựa trên Prefab gốc
    private Dictionary<GameObject, Queue<GameObject>> pools = new Dictionary<GameObject, Queue<GameObject>>();

    private void Awake() => Instance = this;

    // Lấy Object từ Pool
    public GameObject Get(GameObject prefab, Vector2 position)
    {
        if (!pools.ContainsKey(prefab))
        {
            pools.Add(prefab, new Queue<GameObject>());
        }

        GameObject obj;
        if (pools[prefab].Count > 0)
        {
            obj = pools[prefab].Dequeue();
        }
        else
        {
            // Nếu hết quái trong kho, tạo mới
            obj = Instantiate(prefab, poolContainer);
        }

        obj.transform.position = position;
        obj.SetActive(true);

        // Gọi logic Reset (nếu có)
        if (obj.TryGetComponent(out IPoolable poolable))
            poolable.OnSpawn();

        return obj;
    }

    // Trả Object về Pool
    public void ReturnToPool(GameObject prefab, GameObject obj)
    {
        if (obj.TryGetComponent(out IPoolable poolable))
            poolable.OnDespawn();

        obj.SetActive(false);
        pools[prefab].Enqueue(obj);
    }
}