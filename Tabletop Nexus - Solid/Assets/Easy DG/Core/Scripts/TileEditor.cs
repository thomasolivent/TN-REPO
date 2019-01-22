#if (UNITY_EDITOR)

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TileScript))]
public class TileEditor : Editor
{
    TileScript myScript;

    // Function to draw button and copy tile position to Manual Edit Mode when clicked.
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (myScript == null)
        {
            myScript = (TileScript)target;
        }

        if (GUILayout.Button("Copy this pos to Edit Mode"))
        {
            if (myScript.dungeonGeneratorInstance != null)
            {
                DungeonGenerator dunGen = myScript.dungeonGeneratorInstance;
                dunGen.defaultTemplatePosX = myScript.gameObject.transform.position.x;
                dunGen.defaultTemplatePosZ = myScript.gameObject.transform.position.z;
            }
        }
    }

}

#endif