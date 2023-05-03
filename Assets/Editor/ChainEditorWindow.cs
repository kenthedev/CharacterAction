using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ChainEditorWindow : EditorWindow
{
    [MenuItem("Window/Movelist Editor")]
    static void Init()
    {
        EditorWindow.GetWindow(typeof(ChainEditorWindow), false, "MoveList Editor");
    }

    public int currentCommandStateIndex;
    public int currentChainStep;
    
    CoreData coreData;
    Vector2 scrollView;
    int sizer = 0;
    int sizerStep = 30;
    Vector2 xButton = new Vector2(20, 20);

    public enum LineDrawType { CENTER, END_TO_END, BEZIER_END_TO_END }
    public LineDrawType currentLineType;
    
    int drawBase = -1; // -1: Draw Base and Followups, 0: DON'T Draw Base and Followups
    bool drawBaseToggle;

    private void OnGUI()
    {
        // GUI.DrawTexture(new Rect(0, 0, maxSize.x, maxSize.y), Texture2D.blackTexture, ScaleMode.StretchToFill);
        if (coreData == null)
        {
            foreach (string guid in AssetDatabase.FindAssets("t: CoreData"))
            {
                coreData = AssetDatabase.LoadAssetAtPath<CoreData>(AssetDatabase.GUIDToAssetPath(guid));
            }
        }

        MoveList currentMoveList = coreData.moveLists[coreData.currentMovelistIndex];

        currentMoveList.name = GUILayout.TextField(currentMoveList.name);

        GUILayout.BeginHorizontal();
        GUILayout.Label("");
        coreData.currentMovelistIndex = GUILayout.Toolbar(coreData.currentMovelistIndex, coreData.GetMoveListNames());


        coreData.currentMovelistIndex = Mathf.Clamp(coreData.currentMovelistIndex, 0, coreData.moveLists.Count - 1);
        if (GUILayout.Button("New Move List", GUILayout.Width(175))) { coreData.moveLists.Add(new MoveList()); }
        GUILayout.EndHorizontal();

        
        
        CommandState currentCommandStateObject = currentMoveList.commandStates[currentCommandStateIndex];

        currentCommandStateIndex = Mathf.Clamp(currentCommandStateIndex, 0, currentMoveList.commandStates.Count - 1);
        GUILayout.BeginHorizontal();
        currentCommandStateIndex = GUILayout.Toolbar(currentCommandStateIndex, coreData.GetCommandStateNames());
        
        if (GUILayout.Button("New Command State", GUILayout.Width(175))) { currentMoveList.commandStates.Add(new CommandState()); }
        GUILayout.EndHorizontal();
        currentCommandStateObject.stateName = GUILayout.TextField(currentCommandStateObject.stateName, GUILayout.Width(200));
        //coreData.commandStates[currentCommandState].stateName = GUILayout.TextField(coreData.commandStates[currentCommandState].stateName, GUILayout.Width(200));

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Add Command", GUILayout.Width(120)))
        {
            if (currentCommandStateObject.commandSteps == null) { currentCommandStateObject.commandSteps = new List<CommandStep>(); }
            currentCommandStateObject.AddCommandStep();
            currentCommandStateObject.CleanUpBaseState();

            //coreData.commandStates[currentCommandState].chainSteps.Add(new ChainStep(coreData.commandStates[currentCommandState].chainSteps.Count));
        }

        GUILayout.Label("Draw Style:", GUILayout.Width(75));
        currentLineType = (LineDrawType)EditorGUILayout.EnumPopup(currentLineType, GUILayout.Width(150));
        drawBaseToggle = GUILayout.Toggle(drawBaseToggle, "Draw Base Node", EditorStyles.miniButton, GUILayout.Width(150));
        if (drawBaseToggle) { drawBase = -1; } else { drawBase = 0; }
        GUILayout.EndHorizontal();
        scrollView = EditorGUILayout.BeginScrollView(scrollView);

        GUILayout.Label("", GUILayout.Height(2000), GUILayout.Width(2000));

        // draw your nodes here

        Handles.BeginGUI();

        int sCounter = 0;

        //foreach (CommandStep s in coreData.commandStates[currentCommandState].commandSteps)
        foreach (CommandStep s in currentCommandStateObject.commandSteps)
        {
            if (sCounter > drawBase)
            {
                int deleteMe = -1;
                int fCounter = 0;
                foreach (int f in s.followUps)
                {
                    CommandStep t = currentCommandStateObject.commandSteps[f];

                    if (t.activated)
                    {
                        switch (currentLineType)
                        {
                            case LineDrawType.CENTER:
                                Handles.DrawBezier(
                                    s.myRect.center,
                                    t.myRect.center,
                                    s.myRect.center,
                                    t.myRect.center,
                                    Color.white, null, 3f);
                                break;
                            case LineDrawType.END_TO_END:
                                Handles.DrawBezier(
                                    new Vector2(s.myRect.xMax - 2f, s.myRect.center.y),
                                    new Vector2(t.myRect.xMin + 2, t.myRect.center.y),
                                    new Vector2(s.myRect.xMax, s.myRect.center.y),
                                    new Vector2(t.myRect.xMin, t.myRect.center.y),
                                    Color.white, null, 3f);
                                break;
                            case LineDrawType.BEZIER_END_TO_END:
                                Handles.DrawBezier(
                                    new Vector2(s.myRect.xMax - 2f, s.myRect.center.y),
                                    new Vector2(t.myRect.xMin + 2f, t.myRect.center.y),
                                    new Vector2(s.myRect.xMax + 30f, s.myRect.center.y),
                                    new Vector2(t.myRect.xMin - 30f, t.myRect.center.y),
                                    Color.white, null, 3f);
                                break;
                        }

                        if (GUI.Button(new Rect((t.myRect.center + s.myRect.center) * 0.5f + (xButton * -0.5f), xButton), "X"))
                        {
                            deleteMe = fCounter;
                        }
                    }
                    fCounter++;
                }
                if (deleteMe > -1) { s.followUps.RemoveAt(deleteMe); currentCommandStateObject.CleanUpBaseState(); }
            }
            sCounter++;
        }

        Handles.EndGUI();

        BeginWindows();
        sizerStep = 30;
        // GUI.backgroundColor = Color.black;
        int cCounter = 0;
        foreach (CommandStep c in currentCommandStateObject.commandSteps)
        {
            if (c.activated && cCounter > drawBase)
            {
                c.myRect = GUI.Window(c.idIndex, c.myRect, WindowFunction, "", EditorStyles.miniButton);
            }
            cCounter++;
        }

        EndWindows();
        EditorGUILayout.EndScrollView();
        EditorUtility.SetDirty(coreData);
    }

    void WindowFunction(int windowID)
    {
        MoveList currentMoveList = coreData.moveLists[coreData.currentMovelistIndex];
        CommandState currentCommandStateObject = currentMoveList.commandStates[currentCommandStateIndex];
        
        if (currentCommandStateIndex >= currentMoveList.commandStates.Count) { currentCommandStateIndex = 0; }
        if (windowID >= currentCommandStateObject.commandSteps.Count) { return; }
        currentCommandStateObject.commandSteps[windowID].myRect.width = 175;
        currentCommandStateObject.commandSteps[windowID].myRect.height = 50;

        EditorGUI.LabelField(new Rect(6, 7, 35, 20), windowID.ToString());

        currentCommandStateObject.commandSteps[windowID].command.motionCommand =
            EditorGUI.IntPopup(new Rect(25, 5, 50, 20), currentCommandStateObject.commandSteps[windowID].command.input, coreData.GetMotionCommandNames(), null, EditorStyles.miniButtonLeft);

        currentCommandStateObject.commandSteps[windowID].command.input =
            EditorGUI.IntPopup(new Rect(75, 5, 65, 20), currentCommandStateObject.commandSteps[windowID].command.input, coreData.GetRawInputNames(), null, EditorStyles.miniButtonMid);
        currentCommandStateObject.commandSteps[windowID].command.state =
            EditorGUI.IntPopup(new Rect(40, 26, 70, 20), currentCommandStateObject.commandSteps[windowID].command.state, coreData.GetStateNames(), null, EditorStyles.miniButton);

        currentCommandStateObject.commandSteps[windowID].priority =
            EditorGUI.IntField(new Rect(6, 26, 20, 20), currentCommandStateObject.commandSteps[windowID].priority);

        int nextFollowup = -1;
        nextFollowup = EditorGUI.IntPopup(new Rect(150, 15, 21, 20), nextFollowup, coreData.GetFollowUpNames(currentCommandStateIndex, true), null, EditorStyles.miniButton);
        
        if (nextFollowup != -1)
        {
            if (currentCommandStateObject.commandSteps.Count > 0)
            {
                if (nextFollowup >= currentCommandStateObject.commandSteps.Count + 1)
                {
                    currentCommandStateObject.RemoveChainCommands(windowID);
                }
                else if (nextFollowup >= currentCommandStateObject.commandSteps.Count)
                {
                    CommandStep nextCommand = currentCommandStateObject.AddCommandStep();
                    nextCommand.myRect.x = currentCommandStateObject.commandSteps[windowID].myRect.xMax + 40f;
                    nextCommand.myRect.y = currentCommandStateObject.commandSteps[windowID].myRect.center.y - 15f;
                    nextCommand.command.input = currentCommandStateObject.commandSteps[windowID].command.input;
                    nextCommand.command.state = currentCommandStateObject.commandSteps[windowID].command.state;

                    currentCommandStateObject.commandSteps[windowID].AddFollowUp(nextCommand.idIndex);
                }
                else { currentCommandStateObject.commandSteps[windowID].AddFollowUp(nextFollowup); }
            }
            else
            {
                currentCommandStateObject.commandSteps[windowID].AddFollowUp(nextFollowup);
            }
            currentCommandStateObject.CleanUpBaseState();
        }

        if ((Event.current.button == 0) && (Event.current.type == EventType.MouseDown))
        {
            currentChainStep = windowID;
        }

        GUI.DragWindow();
    }
}
