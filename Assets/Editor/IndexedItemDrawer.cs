using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(IndexedItemAttribute))]
public class IndexedItemDrawer : PropertyDrawer
{
    CoreData coreData;

    // Draw the property within the given rect
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // First get the attribute since it contains the range for hte slider
        IndexedItemAttribute indexedItem = attribute as IndexedItemAttribute;

        if (coreData == null)
        {
            foreach (string guid in AssetDatabase.FindAssets("t: CoreData")) // searches project for an asset called CoreData
            {
                coreData = AssetDatabase.LoadAssetAtPath<CoreData>(AssetDatabase.GUIDToAssetPath(guid));
            }
        }

        switch (indexedItem.type)
        {
            case IndexedItemAttribute.IndexedItemType.SCRIPTS:
                property.intValue = EditorGUI.IntPopup(position, property.intValue, coreData.GetScriptNames(), null); 
                break;
            case IndexedItemAttribute.IndexedItemType.STATES:
                property.intValue = EditorGUI.IntPopup(position, property.intValue, coreData.GetStateNames(), null);
                break;
        }

    }
}
