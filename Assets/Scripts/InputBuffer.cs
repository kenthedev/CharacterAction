using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputBuffer
{
    //public static string[] rawInputList = new string[]
    //{
    //    "Jump",
    //    "Action",
    //    "Light Attack",
    //    "Heavy Attack"
    //};

    public List<InputBufferFrame> buffer; // = new List<InputBufferFrame>();
    public static int bufferWindow = 25;

    public List<int> buttonCommandCheck;
    //public List<int> motionCommandCheck;

    void InitializeBuffer()
    {
        buffer = new List<InputBufferFrame>();
        for (int i = 0; i < bufferWindow; i++)
        {
            InputBufferFrame newB = new InputBufferFrame();
            newB.InitializeFrame();
            buffer.Add(newB);
        }

        buttonCommandCheck = new List<int>();
        for (int i = 0; i < GameEngine.coreData.rawInputs.Count; i++)
        {
            buttonCommandCheck.Add(-1);
        }

        /*
        motionCommandCheck = new List<int>();
        for (int i = 0; i < GameEngine.coreData.motionCommands.Count; i++)
        {
            motionCommandCheck.Add(-1);
        }
        */
    }

    public void Update()
    {
        GameEngine.gameEngine.inputBuffer = this; // bad shouldn't do this

        if (buffer == null) { InitializeBuffer(); }
        if (buffer.Count < GameEngine.coreData.rawInputs.Count || buffer.Count == 0)
        {
            InitializeBuffer();
        }

        for (int i = 0; i < buffer.Count - 1; i++)
        {
            for (int r = 0; r < buffer[i].rawInputs.Count; r++)
            {
                buffer[i].rawInputs[r].value = buffer[i + 1].rawInputs[r].value;
                buffer[i].rawInputs[r].hold = buffer[i + 1].rawInputs[r].hold;
                buffer[i].rawInputs[r].used = buffer[i + 1].rawInputs[r].used;
            }
        }

        buffer[buffer.Count - 1].Update();

        for (int r = 0; r < buttonCommandCheck.Count; r++)
        {
            buttonCommandCheck[r] = -1;
            for (int b = 0; b < buffer.Count; b++)
            {
                if (buffer[b].rawInputs[r].CanExecute()) { buttonCommandCheck[r] = b; }
            }
            if (GameEngine.coreData.rawInputs[r].inputType == RawInput.InputType.IGNORE) { buttonCommandCheck[r] = 0; }
        }
    }

    public void UseInput(int _i)
    {
        buffer[buttonCommandCheck[_i]].rawInputs[_i].used = true;
        // Debug.Log("used up!!!> : " + buttonCommandCheck[_i].ToString());
        buttonCommandCheck[_i] = -1;
        // buffer[buttonCommandCheck[_i]].rawInputs[_i].hold = -2;
    }

}

public class InputBufferFrame
{
    public List<InputBufferFrameState> rawInputs;

    public void InitializeFrame()
    {
        rawInputs = new List<InputBufferFrameState>();
        for (int i = 0; i < GameEngine.coreData.rawInputs.Count; i++)
        {
            InputBufferFrameState newFS = new InputBufferFrameState();
            newFS.rawInput = i;
            rawInputs.Add(newFS);
        }
    }

    public void Update()
    {
        if (rawInputs == null) { InitializeFrame(); }
        if (rawInputs.Count == 0 || rawInputs.Count != GameEngine.coreData.rawInputs.Count) { InitializeFrame(); }
        foreach (InputBufferFrameState fs in rawInputs)
        {
            fs.ResolveCommand();
        }
    }

}

public class InputBufferFrameState
{
    public int rawInput;
    public float value;
    public int hold;
    public bool used;

    public void ResolveCommand()
    {
        used = false;
        switch (GameEngine.coreData.rawInputs[rawInput].inputType)
        {
            case RawInput.InputType.BUTTON:
                if (Input.GetButton(GameEngine.coreData.rawInputs[rawInput].name))
                {
                    HoldUp(1f);
                }
                else
                {
                    ReleaseHold();
                }
                break;
            case RawInput.InputType.AXIS:
                if (Mathf.Abs(Input.GetAxisRaw(GameEngine.coreData.rawInputs[rawInput].name)) > GameEngine.gameEngine.deadZone)
                {
                    HoldUp(Input.GetAxisRaw(GameEngine.coreData.rawInputs[rawInput].name));
                }
                else
                {
                    ReleaseHold();
                }
                break;
        }
    }

    public void HoldUp(float _val)
    {
        value = _val;

        if (hold < 0) { hold = 1; }
        else { hold += 1; }
    }

    public void ReleaseHold()
    {
        if (hold > 0) { hold = -1; used = false; }
        else { hold = 0; }
        value = 0;
        // GameEgnine.gameEngine.playerInputBuffer.buttonCommandCheck[rawInput]  = 0;
    }

    public bool CanExecute()
    {
        if (hold == 1 && !used) { return true; }
        return false;
    }

    public bool MotionNeutral()
    {
        if (Mathf.Abs(value) < GameEngine.gameEngine.deadZone) { return true; }
        return false;
    }


}

[System.Serializable]
public class RawInput
{
    public enum InputType { BUTTON, AXIS, DOUBLE_AXIS, DIRECTION, IGNORE }
    public InputType inputType;
    public string name;
}



/*
1. Going through each of our commands
2. Updating the buffer
    Look at the buffer for each command and for each subsequent buffer, set to the previous one

allows players to input button commands slightly before they are required to execute them. 
The game will then store these inputs in a buffer for a brief period, usually around half a second, 
and execute them as soon as possible, even if the player hasn't pressed the button 
at the exact moment the action is required.
*/
