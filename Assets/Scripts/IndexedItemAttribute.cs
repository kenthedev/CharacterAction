using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class IndexedItemAttribute : PropertyAttribute
{
    public enum IndexedItemType { SCRIPTS, STATES, RAW_INPUTS }
    public IndexedItemType type;

    public IndexedItemAttribute(IndexedItemType type)
    {
        this.type = type;
    }
}
