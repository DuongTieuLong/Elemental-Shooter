using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "ScriptableObjects/EnemyData", order = 2)]
public class EnemyData : ScriptableObject
{
    public GameObject prefab;
    public float speed = 2f;
    public float health = 100f;
    public float stopDistance = 0f;
    public int expReward = 10;
}
