using UnityEngine;

public class GamePrefab : MonoBehaviour
{
    public enum PrefabType
    {
        Obstacle,
        Pickup,
        Quest
    }

    public PrefabType myType;
}
