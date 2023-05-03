using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CharacterStateEditorWindow : EditorWindow
{
    [MenuItem("Window/Character State Editor")]
    static void Init()
    {
        EditorWindow.GetWindow(typeof(CharacterStateEditorWindow), false, "Character State Editor");
    }

   
}
