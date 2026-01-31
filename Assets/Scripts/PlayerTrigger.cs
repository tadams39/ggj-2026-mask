using UnityEngine;
using UnityEngine.Events;

public class PlayerTrigger : MonoBehaviour
{
     public UnityEvent OnTriggerEntered;

    private void OnTriggerEnter(Collider other)
    {
        OnTriggerEntered?.Invoke();
    }
}
