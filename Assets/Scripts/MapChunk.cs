using UnityEngine;

public class MapChunk : MonoBehaviour
{
    public Transform enterAnchor;
    public Transform exitAnchor;

    public PrefabSpawnPoint[] spawnPoints;

    public void Awake()
    {
        spawnPoints = GetComponentsInChildren<PrefabSpawnPoint>();
    }

    public void OnEntered()
    {
        LevelGenerator.instance.ProgressChunks();
    }

    public void ConfigureTo(Transform otherAnchor)
    {
        // Store the local transform of enterAnchor relative to this MapChunk
        Vector3 localPos = enterAnchor.localPosition;
        Quaternion localRot = enterAnchor.localRotation;

        // First, set the MapChunk's rotation so that enterAnchor's rotation matches otherAnchor's rotation
        // If enterAnchor has local rotation localRot, and we want it to have world rotation otherAnchor.rotation,
        // then: transform.rotation * localRot = otherAnchor.rotation
        // So: transform.rotation = otherAnchor.rotation * Inverse(localRot)
        transform.rotation = otherAnchor.rotation * Quaternion.Inverse(localRot);

        // Then, set the MapChunk's position so that enterAnchor's position matches otherAnchor's position
        // The enterAnchor will be offset from the MapChunk by the rotated local position
        Vector3 worldOffset = transform.rotation * localPos;
        transform.position = otherAnchor.position - worldOffset;


        // Enable
        gameObject.SetActive(true);

        foreach (var i in spawnPoints)
        {
            i.Spawn();
        }
        
    }

    public void UnloadChunk()
    {
        // Disable self and move to 0,0,0
        gameObject.SetActive(false);
        transform.position = new Vector3(0,0,0);
        transform.eulerAngles = new Vector3(0,0,0);

        // Reset any internal state (chunk objects with resetters)
         foreach (var i in spawnPoints)
        {
            i.Reset();
        }
    }
}
