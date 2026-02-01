using UnityEngine;

// Legacy script - plays sound when hitting tagged obstacles
// Main collision handling is now in GamePrefab.cs
public class TriggerSound : MonoBehaviour
{
    public AudioSource trigSource;
    public AudioClip sound;

    void OnCollisionEnter(Collision collision)
    {
        // Only handle legacy tagged obstacles (without GamePrefab component)
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            if (trigSource != null && sound != null)
            {
                trigSource.PlayOneShot(sound);
            }
        }
    }
}
