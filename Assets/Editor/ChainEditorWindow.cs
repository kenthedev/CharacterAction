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

    public int currentCommandState;
    public int currentChainStep;

    // shouldn't need these
    //Rect windowRect = new Rect(100 + 100, 100, 100, 100);
    //Rect windowRect2 = new Rect(100, 100, 100, 100);

    CoreData coreData;
    Vector2 scrollView;
    int sizer = 0;
    int sizerStep = 30;
    Vector2 xButton = new Vector2(20, 20);

    public enum LineDrawType { CENTER, END_TO_END, BEZIER_END_TO_END }
    public LineDrawType currentLineType;
    //public string[] linetypes = new string[] { "Center", "End to End", "End to End Bezier" };
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

        currentCommandState = Mathf.Clamp(currentCommandState, 0, coreData.commandStates.Count - 1);
        GUILayout.BeginHorizontal();
        currentCommandState = GUILayout.Toolbar(currentCommandState, coreData.GetCommandStateNames());
        
        if(GUILayout.Button("New Command State", GUILayout.Width(175))) { coreData.commandStates.Add(new CommandState()); }
        GUILayout.EndHorizontal();
        coreData.commandStates[currentCommandState].stateName = GUILayout.TextField(coreData.commandStates[currentCommandState].stateName, GUILayout.Width(200));

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Add Command", GUILayout.Width(120)))
        {
            if (coreData.commandStates[currentCommandState].commandSteps == null) { coreData.commandStates[currentCommandState].commandSteps = new List<CommandStep>(); }
            coreData.commandStates[currentCommandState].AddCommandStep();
            coreData.commandStates[currentCommandState].CleanUpBaseState();

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

        foreach (CommandStep s in coreData.commandStates[currentCommandState].commandSteps)
        {
            if (sCounter > drawBase)
            {
                int deleteMe = -1;
                int fCounter = 0;
                foreach (int f in s.followUps)
                {
                    CommandStep t = coreData.commandStates[currentCommandState].commandSteps[f];

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
                if (deleteMe > -1) { s.followUps.RemoveAt(deleteMe); coreData.commandStates[currentCommandState].CleanUpBaseState(); }
            }
            sCounter++;
        }

        Handles.EndGUI();

        BeginWindows();
        sizerStep = 30;
        // GUI.backgroundColor = Color.black;
        int cCounter = 0;
        foreach (CommandStep c in coreData.commandStates[currentCommandState].commandSteps)
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
        if (currentCommandState >= coreData.commandStates.Count) { currentCommandState = 0; }
        if (windowID >= coreData.commandStates[currentCommandState].commandSteps.Count) { return; }
        coreData.commandStates[currentCommandState].commandSteps[windowID].myRect.width = 240;
        coreData.commandStates[currentCommandState].commandSteps[windowID].myRect.height = 30;

        EditorGUI.LabelField(new Rect(6, 7, 35, 20), windowID.ToString());
        coreData.commandStates[currentCommandState].commandSteps[windowID].command.input =
            EditorGUI.IntPopup(new Rect(25, 5, 65, 20), coreData.commandStates[currentCommandState].commandSteps[windowID].command.input, coreData.GetRawInputNames(), null, EditorStyles.miniButtonLeft);
        coreData.commandStates[currentCommandState].commandSteps[windowID].command.state =
            EditorGUI.IntPopup(new Rect(90, 5, 125, 20), coreData.commandStates[currentCommandState].commandSteps[windowID].command.state, coreData.GetStateNames(), null, EditorStyles.miniButtonRight);


        int nextFollowup = -1;
        nextFollowup = EditorGUI.IntPopup(new Rect(215, 5, 21, 20), nextFollowup, coreData.GetFollowUpNames(currentCommandState, true), null, EditorStyles.miniButton);
        
        if (nextFollowup != -1)
        {
            if (coreData.commandStates[currentCommandState].commandSteps.Count > 0)
            {
                if (nextFollowup >= coreData.commandStates[currentCommandState].commandSteps.Count + 1)
                {
                    coreData.commandStates[currentCommandState].RemoveChainCommands(windowID);
                }
                else if (nextFollowup >= coreData.commandStates[currentCommandState].commandSteps.Count)
                {
                    CommandStep nextCommand = coreData.commandStates[currentCommandState].AddCommandStep();
                    nextCommand.myRect.x = coreData.commandStates[currentCommandState].commandSteps[windowID].myRect.xMax + 40f;
                    nextCommand.myRect.y = coreData.commandStates[currentCommandState].commandSteps[windowID].myRect.center.y - 15f;
                    nextCommand.command.input = coreData.commandStates[currentCommandState].commandSteps[windowID].command.input;
                    nextCommand.command.state = coreData.commandStates[currentCommandState].commandSteps[windowID].command.state;

                    coreData.commandStates[currentCommandState].commandSteps[windowID].AddFollowUp(nextCommand.idIndex);
                }
                else { coreData.commandStates[currentCommandState].commandSteps[windowID].AddFollowUp(nextFollowup); }
            }
            else
            {
                coreData.commandStates[currentCommandState].commandSteps[windowID].AddFollowUp(nextFollowup);
            }
            coreData.commandStates[currentCommandState].CleanUpBaseState();
        }

        if ((Event.current.button == 0) && (Event.current.type == EventType.MouseDown))
        {
            currentChainStep = windowID;
        }

        GUI.DragWindow();
    }
}
