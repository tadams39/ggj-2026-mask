using System;
using UnityEngine;
using UnityEngine.Events;

public class PlayerTrigger : MonoBehaviour
{
    public bool isEnabled = true;
     public UnityEvent OnTriggerEntered;

    internal void Disable()
    {
        isEnabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isEnabled)
        {
            return;
        }
        OnTriggerEntered?.Invoke();
        Disable();
    }
}
