using UnityEngine;

public abstract class ReactionEffect : ScriptableObject
{
    public abstract void Execute(GameObject target, ElementData oldData, ElementData newData);
}