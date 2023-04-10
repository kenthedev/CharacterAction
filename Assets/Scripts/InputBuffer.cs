using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputBuffer
{

}

public class InputBufferItem
{

}

public class InputBufferItemState
{

}

/*
public class InputBuffer
{
    public List<InputBufferItem> buttonList = new List<InputBufferItem>();
    //public inputBufferItem direction;
    
    public static string[] inputList = new string[]
    {
        "Jump", 
        "Attack",
        "Dash"
    };

    public void Update()
    {
        //UpdateLeftStick();
        
        InitializeBuffer();

        foreach(InputBufferItem c in buttonList)
        {
            c.ResolveCommand();
            for (int b = 0; b < c.buffer.Count - 1; b++)
            {
                c.buffer[b].hold = c.buffer[b + 1].hold;
                c.buffer[b].used = c.buffer[b + 1].used;

*/
