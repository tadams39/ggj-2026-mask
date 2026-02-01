using UnityEngine;

public class Snow : MonoBehaviour
{
    public GameObject player;

    void Start()
    {
    }

    // Update is called once per frame
    void LateUpdate()
    {
        transform.position = player.GetComponent<Rigidbody>().position + new Vector3(0,25,0);
    }
}
