using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Hitbox))]
public class InspectorTools : Editor
{

    public CoreData coreData;
    public CharacterState state;

    public override void OnInspectorGUI()
    {
        Hitbox h = (Hitbox)target;
        DrawDefaultInspector();

        if (coreData == null)
        {
            foreach (string guid in AssetDatabase.FindAssets("t: CoreData"))
            {
                coreData = AssetDatabase.LoadAssetAtPath<CoreData>(AssetDatabase.GUIDToAssetPath(guid));
            }
        }

        if (GUILayout.Button("Apply Hitbox"))
        {

            state = coreData.characterStates[h.stateIndex];
            for (int i = 0; i < state.attacks.Count; i++)
            {
                Attack atk = state.attacks[i];
                atk.hitboxPos = h.transform.localPosition;
                atk.hitboxScale = h.transform.localScale;

                //atk.attackBox
            }
            // CRUCIAL for not losing data after its adjusted
            EditorUtility.SetDirty(coreData); 
            AssetDatabase.SaveAssets();
        }
    }
}
