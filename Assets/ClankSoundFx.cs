using UnityEngine;

public class ClankSoundFx : MonoBehaviour
{
    public float currentSpeed;
    public float lastSpeed;
    public float clankThreshhold = 0.3f;
    public float diff;
    public AudioClip[] audioClips;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        currentSpeed = GetComponent<Rigidbody>().linearVelocity.magnitude;

        diff = currentSpeed - lastSpeed;
        Debug.Log("diff: " + diff);

        lastSpeed = currentSpeed;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (diff > clankThreshhold)
        {
            Debug.Log("Play Sound Fx");
            //GetComponent<AudioSource>().Play();
            GetComponent<AudioSource>().clip = audioClips[Random.Range(0, audioClips.Length)];
            GetComponent<AudioSource>().Play();
        }
    }
}
