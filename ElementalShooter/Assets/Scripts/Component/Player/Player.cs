using NUnit.Framework;
using UnityEngine;

public class Player : MonoBehaviour
{
    private void Start()
    {
        GameEvents.RaisePlayerSpawned(this.transform);
    }

}
