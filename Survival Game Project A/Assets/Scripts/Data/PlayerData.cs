using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "PlayerData", menuName = "ScriptableObjects/PlayerData", order = 1)]
public class PlayerData : ScriptableObject
{
    public string CharacterName;
    public Sprite icon;
    public float moveSpeed = 5f;
    public float maxHealth = 100f;
    public float attackDamage = 10f;
    public float attackSpeed = 1f;
    public float attackRange = 10f;
}