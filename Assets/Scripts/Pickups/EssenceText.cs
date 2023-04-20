using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EssenceText : MonoBehaviour
{
    public TextMeshProUGUI essenceText;
    [SerializeField] private int essenceCount;

    private void OnEnable()
    {
        Essence.OnEssenceCollected += IncrementEssenceCount;
    }

    private void OnDisable()
    {
        Essence.OnEssenceCollected -= IncrementEssenceCount;
    }

    private void IncrementEssenceCount()
    {
        essenceCount++;
        essenceText.text = $"Essence: {essenceCount}";
    }
}
