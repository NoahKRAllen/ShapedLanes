using System;
using UnityEngine;
using UnityEngine.Events;

public class DestroyInvoke : MonoBehaviour
{
    public UnityEvent destroyEvent;
    private bool isQuiting;
    void OnDestroy()
    {
        if (!isQuiting && gameObject.scene.isLoaded)
        {
            destroyEvent.Invoke();
        }
    }

    private void OnApplicationQuit()
    {
        isQuiting = true;
    }
}
