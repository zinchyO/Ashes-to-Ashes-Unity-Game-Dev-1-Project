using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    public float duration = 1.0f;

    void Start()
    {
        // Automatically destroy the explosion effect after the specified duration
        Destroy(gameObject, duration);
    }
}
