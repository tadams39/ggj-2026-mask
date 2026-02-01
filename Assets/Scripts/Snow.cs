using UnityEngine;

public class Snow : MonoBehaviour
{
    GameObject player;

    void Start()
    {
        player = GameObject.Find("PlayerPositionMarker");
    }

    // Update is called once per frame
    void LateUpdate()
    {
        transform.position = player.GetComponent<Rigidbody>().position + new Vector3(0,25,0);
    }
}
