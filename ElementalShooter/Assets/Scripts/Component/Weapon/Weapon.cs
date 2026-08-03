using System;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public Transform muzzle;

    public WeaponAim weaponAim;

    public StatHandler playerStat; // Tham chiếu đến StatHandler để lấy thông tin về các chỉ số của người chơi

    public LayerMask enemyLayer; // Lớp đối tượng địch để kiểm tra va chạm khi bắn
    public bool IsPullingTrigger { get; set; }

    public WeaponFire weaponFire;
    public WeaponVisual weaponVisual;
    public Weaponstats weaponStats;


    public SpreadCalculator spreadCalculator;
    public RecoilController recoilController;


    [SerializeField] private WeaponData currentData;
    public event Action<WeaponData> OnWeaponChanged;

    public WeaponData CurrentData
    {
        get
        {
            return currentData;
        }
        private set { }
    }

    private IFireStrategy _fireStrategy;

    // Lưu trữ nâng cấp tích lũy riêng của từng loại vũ khí tại runtime

    private void Update()
    {

        if (IsPullingTrigger)
        {
            if (weaponFire.TryFire())
            {
                PullTrigger();
            }
        }
    }

    private void Awake()
    {
      
        weaponAim = GetComponent<WeaponAim>();
        weaponFire = GetComponent<WeaponFire>();
        weaponVisual = GetComponent<WeaponVisual>();
        weaponStats = GetComponent<Weaponstats>();

        spreadCalculator = GetComponent<SpreadCalculator>();
        recoilController = GetComponent<RecoilController>();
    }

    private void Start()
    {
        EquipWeapon(currentData);
    }

    public void ApplyWeaponUpgrade(WeaponData targetWeapon, System.Collections.Generic.List<StatChange> changes)
    {
        weaponStats.ApplyWeaponUpgrade(targetWeapon, changes);
    }

    public void EquipWeapon(WeaponData data)
    {
        _fireStrategy = FireStrategyFactory.GetStrategy(data.strategyType);
        _fireStrategy.Initialize(data.attackData);
        currentData = data;

        OnWeaponChanged?.Invoke(data);
    }

    public void PullTrigger()
    {
        if (currentData == null || _fireStrategy == null) return;
        
        _fireStrategy?.ExecuteFire(this, muzzle, playerStat, weaponAim, enemyLayer);
    }
}
