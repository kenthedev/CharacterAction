using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Core Data", menuName = "Character Action/Core Data")]
[System.Serializable]
public class CoreData : ScriptableObject
{
    // Character States
    public List<CharacterState> characterStates;

    public List<CharacterScript> characterScripts;

    public List<InputCommand> commands;

    // Save Files

    public string[] GetScriptNames()
    {
        string[] _names = new string[characterScripts.Count];
        for (int i = 0; i < _names.Length; i++)
        {
            _names[i] = characterScripts[i].name;
        }
        return _names;
    }

    public string[] GetStateNames()
    {
        string[] _names = new string[characterStates.Count];
        for (int i = 0; i < _names.Length; i++)
        {
            _names[i] = characterStates[i].stateName;
        }
        return _names;
    }

    public string[] GetCommandNames()
    {
        string[] _names = new string[commands.Count];
        for (int i = 0; i < _names.Length; i++)
        {
            _names[i] = commands[i].inputString;
        }
        return _names;
    }

}
