using UnityEngine;

public interface IPoolable
{
    void OnSpawn();      // Gọi khi lấy từ Pool ra
    void OnDespawn();   // Gọi khi trả lại vào Pool
}