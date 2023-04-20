using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Essence : MonoBehaviour, ICollectible
{
    public static event Action OnEssenceCollected;
    public void Collect()
    {
        Debug.Log("You collected essence.");
        Destroy(gameObject);
        OnEssenceCollected?.Invoke();
    }
}
