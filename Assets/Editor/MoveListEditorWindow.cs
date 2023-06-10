using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Codice.Client.Common.TreeGrouper;

public class MoveListEditorWindow : EditorWindow
{
    [MenuItem("Window/Movelist Editor")]
    static void Init()
    {
        EditorWindow editorWindow = EditorWindow.GetWindow(typeof(MoveListEditorWindow), false, "MoveList Editor");
        editorWindow.Show();
    }

    public int currentCommandStateIndex;
    public int currentChainStep;
    
    CoreData coreData;
    Vector2 scrollView;
    Vector2 xButton = new Vector2(20, 20);

    public GUIStyle nodeStyle;

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

        #region Movelist Toolbar
        MoveList currentMoveList = coreData.moveLists[coreData.currentMovelistIndex];

        currentMoveList.name = GUILayout.TextField(currentMoveList.name);

        GUILayout.BeginHorizontal();
        //GUILayout.Label(""); 
        coreData.currentMovelistIndex = GUILayout.Toolbar(coreData.currentMovelistIndex, coreData.GetMoveListNames());

        coreData.currentMovelistIndex = Mathf.Clamp(coreData.currentMovelistIndex, 0, coreData.moveLists.Count - 1);
        if (GUILayout.Button("New Move List", GUILayout.Width(175))) { coreData.moveLists.Add(new MoveList()); }
        GUILayout.EndHorizontal();
        #endregion

        #region Command State Tools
        CommandState currentCommandStateObject = currentMoveList.commandStates[currentCommandStateIndex];

        currentCommandStateIndex = Mathf.Clamp(currentCommandStateIndex, 0, currentMoveList.commandStates.Count - 1);
        GUILayout.BeginHorizontal();
        currentCommandStateIndex = GUILayout.Toolbar(currentCommandStateIndex, coreData.GetCommandStateNames());
        
        if (GUILayout.Button("New Command State", GUILayout.Width(175))) { currentMoveList.commandStates.Add(new CommandState()); }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Enter Command State Name: ", GUILayout.MaxWidth(175));
        currentCommandStateObject.stateName = GUILayout.TextField(currentCommandStateObject.stateName, GUILayout.MaxWidth(150));
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Add Command", GUILayout.Width(120)))
        {
            if (currentCommandStateObject.commandSteps == null) { currentCommandStateObject.commandSteps = new List<CommandStep>(); }
            currentCommandStateObject.AddCommandStep();
            currentCommandStateObject.CleanUpBaseState();
        }
        GUILayout.EndHorizontal();
        #endregion

        #region Node Drawing Settings
        GUILayout.BeginHorizontal();
        GUILayout.Label("Draw Style:", GUILayout.Width(75));
        currentLineType = (LineDrawType)EditorGUILayout.EnumPopup(currentLineType, GUILayout.Width(150));
        drawBaseToggle = GUILayout.Toggle(drawBaseToggle, "Draw Base Node", EditorStyles.miniButton, GUILayout.Width(150));
        if (drawBaseToggle) { drawBase = -1; } else { drawBase = 0; }
        GUILayout.EndHorizontal();
        #endregion

        #region Line Drawing Between Nodes
        scrollView = EditorGUILayout.BeginScrollView(scrollView);
        GUILayout.Label(currentMoveList.name + " Move List:", GUILayout.Height(2000), GUILayout.Width(2000));
        // EditorGUILayout.HelpBox("Add nodes below to construct move list", MessageType.Info);
        // draw your nodes here
 
        Handles.BeginGUI();
        int sCounter = 0;
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
        #endregion

        #region Drawing Command Step Nodes
        BeginWindows();

        int cCounter = 0;
        foreach (CommandStep c in currentCommandStateObject.commandSteps)
        {
            if (c.activated && cCounter > drawBase)
            {
                c.myRect = GUI.Window(c.idIndex, c.myRect, WindowFunction, "");
            }
            cCounter++;
        }

        EndWindows();
        #endregion

        EditorGUILayout.EndScrollView();
        EditorUtility.SetDirty(coreData);
    }

    void WindowFunction(int windowID)
    {
        MoveList currentMoveList = coreData.moveLists[coreData.currentMovelistIndex];
        CommandState currentCommandStateObject = currentMoveList.commandStates[currentCommandStateIndex];
        
        if (currentCommandStateIndex >= currentMoveList.commandStates.Count) { currentCommandStateIndex = 0; }
        if (windowID >= currentCommandStateObject.commandSteps.Count) { return; }
        currentCommandStateObject.commandSteps[windowID].myRect.width = 160; // 175
        currentCommandStateObject.commandSteps[windowID].myRect.height = 50; // 50

        // NOTE: Rect: x, y, width, height

        // Move Index (window ID, top left number)
        EditorGUI.LabelField(new Rect(6, 2, 35, 20), windowID.ToString());

        currentCommandStateObject.commandSteps[windowID].priority =
            EditorGUI.IntField(new Rect(6, 25, 20, 20), currentCommandStateObject.commandSteps[windowID].priority);

        // Motion Command Button
        currentCommandStateObject.commandSteps[windowID].command.motionCommand =
            EditorGUI.IntPopup(
                new Rect(30, 5, 40, 20), // 25 5 50 20
                currentCommandStateObject.commandSteps[windowID].command.motionCommand, 
                coreData.GetMotionCommandNames(), 
                null, 
                EditorStyles.miniButtonLeft);

        // Raw Input Button
        currentCommandStateObject.commandSteps[windowID].command.input =
            EditorGUI.IntPopup(
                new Rect(80, 5, 40, 20), // 75, 5, 85, 40
                currentCommandStateObject.commandSteps[windowID].command.input,
                coreData.GetRawInputNames(),
                null,
                EditorStyles.miniButtonRight);
        
        // Command State Button
        currentCommandStateObject.commandSteps[windowID].command.state =
            EditorGUI.IntPopup(
                new Rect(38, 25, 85, 20), 
                currentCommandStateObject.commandSteps[windowID].command.state, 
                coreData.GetStateNames(), 
                null, 
                EditorStyles.miniButtonMid);

        int nextFollowup = -1; // 215 15 21 20
        nextFollowup = EditorGUI.IntPopup(new Rect(125, 15, 21, 20), nextFollowup, coreData.GetFollowUpNames(currentCommandStateIndex, true), null, EditorStyles.miniButton);
        
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
