using UnityEngine;

[CreateAssetMenu(fileName = "NewWeapon", menuName = "Weapon/WeaponData")]
public class WeaponData : ScriptableObject
{
    [Header("Universal Info")]
    public string weaponName;
    public Sprite weaponIcon;

    [Header("Core Stats")]
    public float baseDamage = 10f;
    public float baseAttackSpeed = 1f;

    [Header("Balance Limits")]
    public float maxDamage = 200f;
    public float maxAttackSpeed = 12f;
    public int maxProjectileCount = 10;

    [Header("Game Feel")]
    public float cameraShakeIntensity = 0.5f; // Rung màn hình thống nhất

    [Header("Behavior")]
    public FireStrategyType strategyType; // Enum để Manager biết cần tạo Strategy nào

    [SerializeReference]
    public IAttackStrategyData attackData;

    [Header("Grip Settings")]
    public WeaponGripStyle gripStyle = WeaponGripStyle.OneHanded;
    public Vector2 leftHandGripOffset;    // Vị trí bám của tay trái trên súng (tương đối)

    private void OnValidate()
    {
        bool isRanged = (strategyType == FireStrategyType.SingleShot || 
                         strategyType == FireStrategyType.SpreadShot || 
                         strategyType == FireStrategyType.BurstShot);
        
        bool isMelee = (strategyType == FireStrategyType.MeleeSlash);

        if (isRanged && !(attackData is RangedStrategyData))
        {
            attackData = new RangedStrategyData();
        }
        else if (isMelee && !(attackData is MeleeStrategyData))
        {
            attackData = new MeleeStrategyData();
        }
    }
}

public enum WeaponGripStyle { OneHanded, TwoHanded, Melee }

public enum FireStrategyType { SingleShot, SpreadShot, BurstShot, MeleeSlash }
