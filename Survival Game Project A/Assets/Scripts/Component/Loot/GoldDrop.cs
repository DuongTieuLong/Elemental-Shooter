using UnityEngine;

public class GoldDrop : DropItem
{
    public override void OnCollected()
    {
        // Bạn có thể thêm GameEvents.RaiseGoldCollected(value); nếu có hệ thống tiền tệ
        Debug.Log($"Collected {value} Gold!");
    }
}
