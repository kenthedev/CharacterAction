using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CharacterScriptEditorWindow : EditorWindow
{
    [MenuItem("Window/Character Script Editor")]
    static void Init()
    {
        EditorWindow.GetWindow(typeof(CharacterScriptEditorWindow), false, "Character Script Editor");
    }

    CoreData coreData;
    CharacterState currentCharacterState;
    int currentScriptIndex;

    Vector2 scrollView;
    private void OnGUI()
    {
        if (coreData == null)
        {
            foreach (string guid in AssetDatabase.FindAssets("t: CoreData"))
            {
                coreData = AssetDatabase.LoadAssetAtPath<CoreData>(AssetDatabase.GUIDToAssetPath(guid));
            }
        }

        scrollView = GUILayout.BeginScrollView(scrollView);
        currentScriptIndex = EditorGUILayout.Popup(currentScriptIndex, coreData.GetScriptNames());
        CharacterScript currentScript = coreData.characterScripts[currentScriptIndex];

        currentScript.name = EditorGUILayout.TextField("Name : ", currentScript.name);

        int deleteParam = -1;

        for (int p = 0; p < currentScript.parameters.Count; p++)
        {
            ScriptParameter currentParam = currentScript.parameters[p];
            GUILayout.BeginHorizontal();
            currentParam.name = EditorGUILayout.TextField("Parameter Name : ", currentParam.name);
            if (GUILayout.Button("X", GUILayout.Width(25))) { deleteParam = p; }
            GUILayout.EndHorizontal();
            currentParam.val = EditorGUILayout.FloatField("Default : ", currentParam.val);

            
        }

        if (deleteParam > -1) { currentScript.parameters.RemoveAt(deleteParam); }

        if (GUILayout.Button("+", GUILayout.Width(25)))
        {
            currentScript.parameters.Add(new ScriptParameter());
        }


        GUILayout.EndScrollView();
        EditorUtility.SetDirty(coreData); // set dirty whenever we do stuff on the UI
    }


}
