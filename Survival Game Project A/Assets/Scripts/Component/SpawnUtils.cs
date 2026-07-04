using UnityEngine;

public static class SpawnUtils
{
    public static Vector2 GetRandomCirclePos(Vector3 center, float minRadius, float maxRadius)
    {
        Vector2 randomDir = UnityEngine.Random.insideUnitCircle.normalized;
        return (Vector2)center + randomDir * UnityEngine.Random.Range(minRadius, maxRadius);
    }
}