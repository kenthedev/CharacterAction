using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEngine : MonoBehaviour
{
    public CoreData coreDataObject; 
    public static CoreData coreData;

    public static float hitStop; // hitPause

    // Start is called before the first frame update
    void Start()
    {
        coreData = coreDataObject;
        Application.targetFrameRate = 60;
    }

    // Freezing or slowing time right at the moment of an impact to create the impression that something hits harder, or for dramatic effect.
    public static void SetHitStop(float _pow)
    {
        if (_pow > hitStop) { hitStop = _pow;  }
    }

    // Update is called once per frame
    void Update()
    {
        if (hitStop > 0)
        {
            hitStop--; 
        }  
    }
}
