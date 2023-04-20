using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEngine : MonoBehaviour
{
    public CoreData coreDataObject; 
    public static CoreData coreData;

    public static float hitStop; // hitPause

    public static GameEngine gameEngine;

    // Start is called before the first frame update
    void Start()
    {
        coreData = coreDataObject;
        gameEngine = this;
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

    public static GameObject GetMainCharacterObject()
    {
        return GameObject.Find("Player");
    }

    public static void GlobalPrefab(int _index, GameObject _obj)
    {
        GameObject nextPrefab = Instantiate(coreData.globalPrefabs[_index], _obj.transform);
        
        /*
        // If VFX
        foreach (Animator myAnimator in nextPrefab.transform.GetComponentsInChildren<Animator>())
        {
            VFXControl[] behaves = myAnimator.GetBehaviours<VFXControl>();

            for (int i = 0; i < behaves.Length; i++)
            {
                behaves[i].vfxRoot = nextPrefab.transform;
            }
            //myAnimator.Update(Time.deltaTime);
        }
        */
    }

    public InputBuffer playerInputBuffer;
    private void OnGUI()
    {
        int xSpace = 20;
        int ySpace = 25;
        //GUI.Label(new Rect(10, 10, 100, 20), "Hello World!");

        for (int i = 0; i < playerInputBuffer.inputList.Count; i++)
        {
            GUI.Label(new Rect(xSpace, i * ySpace, 100, 20), playerInputBuffer.inputList[i].button + ":");
            for (int j = 0; j < playerInputBuffer.inputList[i].buffer.Count; j++)
            {
                if (playerInputBuffer.inputList[i].buffer[i].used) 
                { GUI.Label(new Rect(j * xSpace + 100, i * ySpace, 100, 20), playerInputBuffer.inputList[i].buffer[j].hold.ToString() + "*"); }
                else { GUI.Label(new Rect(j * xSpace + 50, i * ySpace, 100, 20), playerInputBuffer.inputList[i].buffer[j].hold.ToString()); }
            }
        }
    }
}
