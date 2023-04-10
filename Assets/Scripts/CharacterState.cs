using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CharacterState
{
    public string stateName;
    [HideInInspector]
    public int index;

    public float length;
    public bool loop;
    public float blendRate = 0.1f;

    public int frame;

    public float currentTime;
    public int currentState;

    public List<StateEvent> events;
    public List<Attack> attacks;
}

[System.Serializable]
public class StateEvent
{
    public float start;
    public float end;
    public float variable;

    [IndexedItem(IndexedItemAttribute.IndexedItemType.SCRIPTS)]
    public int script;
}

[System.Serializable]
public class CharacterScript
{
    [HideInInspector]
    public int index;

    public string name;
    //public float variable; // probs a list of variables?
}

[System.Serializable]
public class InputCommand
{
    public string inputString;
    [IndexedItem(IndexedItemAttribute.IndexedItemType.STATES)]
    public int state;
}
