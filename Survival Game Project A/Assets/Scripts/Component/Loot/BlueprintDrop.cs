using UnityEngine;

public class BlueprintDrop : DropItem
{
    [Header("Blueprint Reward Data")]
    public ScriptableObject blueprintData; // Có thể kéo thả ScriptableObject chứa công thức/súng mới vào đây

    public override void Initialize(int startValue, LootType type)
    {
        base.Initialize(startValue, type);
        // Blueprint không merge
    }

    public override void OnCollected()
    {
        // Bắn sự kiện Jackpot với dữ liệu Blueprint
        GameEvents.OnJackpotTriggered?.Invoke(transform.position, blueprintData);
        Debug.Log("Collected a Blueprint Jackpot!");
    }
}
