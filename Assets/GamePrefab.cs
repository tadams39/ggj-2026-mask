using UnityEngine;

public class GamePrefab : MonoBehaviour
{
    public enum PrefabType
    {
        Obstacle,
        Pickup,
        Score
    }

    public PrefabType myType;
    public enum PickupType
    {
        None,
        Speed,
        Invincibility,
        Jump
    }
    public PickupType pickupType; // Should be None if obstacle or score
}
