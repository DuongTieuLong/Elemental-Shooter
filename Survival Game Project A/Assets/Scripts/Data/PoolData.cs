using UnityEngine;

[CreateAssetMenu(fileName = "NewPool", menuName = "Pool/PoolData")]
public class PoolData : ScriptableObject
{
    public GameObject prefab;
    public int defaultCapacity = 20; // Số lượng khởi tạo ban đầu
}