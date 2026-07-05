using UnityEngine;

public static class FireStrategyFactory
{
    public static IFireStrategy GetStrategy(FireStrategyType type)
    {
        return type switch
        {
            FireStrategyType.SingleShot => new SingleLineStrategy(),
            FireStrategyType.SpreadShot => new SpreadFireStrategy(),
            FireStrategyType.BurstShot => new BurstFireStrategy(),
            FireStrategyType.MeleeSlash => new MeleeSlashStrategy(),
            _ => new SpreadFireStrategy(),
        };
    }
}