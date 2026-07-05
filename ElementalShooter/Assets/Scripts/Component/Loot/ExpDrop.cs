using UnityEngine;

public class ExpDrop : DropItem
{
    public override void OnCollected()
    {
        // Thông báo cho hệ thống Level
        GameEvents.RaiseEnergyCollected(value);
    }
}
