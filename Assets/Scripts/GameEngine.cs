using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEngine : MonoBehaviour
{
    public CoreData coreDataObject; 
    public static CoreData coreData;

    public static float hitStop; // hitPause
    public float deadZone = 0.2f;
    public CharacterObject mainCharacter;

    public static GameEngine gameEngine;

    public int globalMovelistIndex;
     

    // Start is called before the first frame update
    void Start()
    {
        coreData = coreDataObject;
        gameEngine = this;
        Application.targetFrameRate = 60;
    }

    // Freezing or slowing time right at the moment of an impact
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

    public MoveList CurrentMoveList()
    {
        return coreData.moveLists[globalMovelistIndex];
    }

    public void ToggleMoveList() // could do this for Spectre's Form Change
    {
        globalMovelistIndex++;
        if (globalMovelistIndex > coreData.moveLists.Count - 1) { globalMovelistIndex = 0; }
    }

    public static void GlobalPrefab(int _index, GameObject _obj)
    {
        GameObject nextPrefab = Instantiate(coreData.globalPrefabs[_index], _obj.transform.root);
        
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

    public Vector3 VectorOffset(Vector3 _start, Vector3 _end)
    {
        return _end - _start;
    }

    public static float AngleSignedVector3(Vector3 v1, Vector3 v2, Vector3 n)
    {
        return Mathf.Atan2(
            Vector3.Dot(n, Vector3.Cross(v1, v1)),
            Vector3.Dot(v1, v2)) * Mathf.Rad2Deg;
    }

    #region Input Buffer

    public InputBuffer inputBuffer;
    private void DisplayBuffer()
    {
        int xSpace = 25;
        int ySpace = 15;

        for (int i = 0; i < inputBuffer.buttonCommandCheck.Count; i++)
        {
            GUI.Label(new Rect(10f + (i * xSpace), 15f, 100, 20), inputBuffer.buttonCommandCheck[i].ToString());
        }

        for (int b = 0; b < inputBuffer.buffer.Count; b++)
        {
            for (int i = 0; i < inputBuffer.buffer[b].rawInputs.Count; i++)
            {
                if (inputBuffer.buffer[b].rawInputs[i].used)
                {
                    GUI.Label(new Rect(10f + (i * xSpace), 35f + (b * ySpace), 100, 20), inputBuffer.buffer[b].rawInputs[i].hold.ToString("0") + ">");
                }
                else
                {
                    GUI.Label(new Rect(10f + (i * xSpace), 35f + (b * ySpace), 100, 20), inputBuffer.buffer[b].rawInputs[i].hold.ToString("0"));
                }
            }
        }

        for (int m = 0; m < inputBuffer.motionCommandCheck.Count; m++)
        {
            GUI.Label(new Rect(500f - 25f, m * ySpace, 100, 20), inputBuffer.motionCommandCheck[m].ToString());
            GUI.Label(new Rect(500f, m * ySpace, 100, 20), coreData.motionCommands[m].name);
        }
        
        GUI.Label(new Rect(200f, 10f, 100, 20), CurrentMoveList().ToString());
    }

    #endregion

    private void OnGUI()
    {
        DisplayBuffer();
    }

}
