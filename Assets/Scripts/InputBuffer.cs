using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputBuffer
{
    public static string[] rawInputList = new string[]
    {
        "Jump",
        "Action",
        "Melee",
        "Gun"
    };

    public List<InputBufferItem> inputList = new List<InputBufferItem>();

    public void Update()
    {
        GameEngine.gameEngine.playerInputBuffer = this; // bad shouldn't do this

        if (inputList.Count < rawInputList.Length || inputList.Count == 0)
        {
            InitializeBuffer();
        }

        foreach (InputBufferItem c in inputList)
        {
            c.ResolveCommand();
            for (int b = 0; b < c.buffer.Count - 1; b++)
            {
                c.buffer[b].hold = c.buffer[b + 1].hold;
                c.buffer[b].used = c.buffer[b + 1].used;
            }
        }
    }

    void InitializeBuffer()
    {
        inputList = new List<InputBufferItem>();
        foreach (string s in rawInputList)
        {
            InputBufferItem newB = new InputBufferItem();
            newB.button = s;
            inputList.Add(newB);
        }
    }
}

public class InputBufferItem
{
    public string button;
    public List<InputStateItem> buffer;

    public static int bufferWindow = 12;
    public InputBufferItem()
    {
        buffer = new List<InputStateItem>();
        for (int i = 0; i < bufferWindow; i++)
        {
            buffer.Add(new InputStateItem());
        }
    }

    public void ResolveCommand()
    {
        if (Input.GetButton(button))
        {
            buffer[buffer.Count - 1].HoldUp();
        }
        else
        {
            buffer[buffer.Count - 1].ReleaseHold();
        }
    }
}

public class InputStateItem
{
    public int hold;
    public bool used;

    public bool CanExecute()
    {
        if (hold == 1 && !used) { return true; }
        return false;
    }
    public void HoldUp()
    {
        if (hold < 0) { hold = 1; }
        else { hold += 1; }
    }

    public void ReleaseHold()
    {
        if (hold > 0) { hold = -1; used = false; }
        else { hold = 0; }
    }
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
