using UnityEngine;

public class TriggerSound : MonoBehaviour
{
    public AudioSource trigSource;
    public AudioClip sound;

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Obstacle")
        {
            Debug.Log("Test:" + collision.gameObject.tag);
            trigSource.PlayOneShot(sound);
        }
    }
}
