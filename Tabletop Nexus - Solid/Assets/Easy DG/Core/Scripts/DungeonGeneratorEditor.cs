#if (UNITY_EDITOR)

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DungeonGenerator))]
public class DungeonGeneratorEditor : Editor {

    DungeonGenerator myScript;

    // Controls for Manual Edit Mode.
    // I've not exposed these to keep the editor interface cleaner.

    // To redefine the keys, just change these KeyCodes to the desired key.
    // If you can't find or aren't sure which KeyCode to use, see below in the OnSceneGUI function. 
    // I've included some debugging that should help find the correct key.

    private KeyCode leftKey = KeyCode.LeftArrow;
    private KeyCode rightKey = KeyCode.RightArrow;
    private KeyCode upKey = KeyCode.UpArrow;
    private KeyCode downKey = KeyCode.DownArrow;

    private KeyCode confirmKey = KeyCode.Space;
    private KeyCode deleteKey = KeyCode.Delete;

    private KeyCode increaseLengthKey = KeyCode.Alpha2;
    private KeyCode decreaseLengthKey = KeyCode.Alpha1;

    private KeyCode increaseWidthKey = KeyCode.Equals;
    private KeyCode decreaseWidthKey = KeyCode.Minus;

    // Manual edit mode variables, you don't need to change these.

    [HideInInspector]
    public int currentL;
    [HideInInspector]
    public int currentW;

    private int maxL = 10;
    private int maxW = 10;


    // Function to draw editor / inspector buttons.
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        // Set target (the DungeonGenerator script instance in the scene).
        if (myScript == null)
        {
            myScript = (DungeonGenerator)target;
        }

        // Draw and handle buttons.
        if (GUILayout.Button("Generate Dungeon"))
        {
            myScript.PluginCheck();
        }

        if (GUILayout.Button("Generate Blank Dungeon"))
        {
            myScript.GenerateBlankDungeon();
        }
        
        if (GUILayout.Button("Reset Dungeon Count"))
        {
            myScript.ResetDungeonCount();
        }

        if (GUILayout.Button("Delete Dungeon"))
        {
            myScript.DeleteDungeon();
        }

        if (GUILayout.Button("Toggle Manual Edit Mode"))
        {
            if (myScript.manualModeOn == true)
            {
                myScript.ToggleManualMode(false);
            }
            else
            {
                myScript.ToggleManualMode(true);
                currentL = 1;
                currentW = 1;
            }
        }

        if (GUILayout.Button("Finalise Dungeon Layout"))
        {
            if (myScript.dungeonParent != null)
            {
                myScript.FinaliseDungeon();
            }
            else
            {
                Debug.Log("Easy DG - Log: " +"No dungeon in scene.");
            }
        }

        if (GUILayout.Button("Clear Dungeon Prefabs"))
        {
            if (myScript.dungeonParent != null)
            {
                myScript.ClearDungeon();
            }
            else
            {
                Debug.Log("Easy DG - Log: " +"No dungeon in scene.");
            }
        }

        if (GUILayout.Button("Import Dungeon"))
        {
            if (myScript.dungeonParent == null)
            {
                myScript.ImportDungeon();
            }
            else
            {
                Debug.Log("Easy DG - Log: " + "There is a generated dungeon in scene, please delete.");
            }
        }

        // Required to ensure values set are retained when game is run.
        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }
    }

    // Function to receive and handle key presses when in Manual Edit Mode.
    void OnSceneGUI()
    {
        // Prevent following from running if myScript not set.
        if (myScript == null)
        {
            return;
        }

        if (myScript.manualModeOn == true)
        {
            // Get control ID.
            int controlID = GUIUtility.GetControlID(FocusType.Passive);
            // Set control ID for control.
            GUIUtility.hotControl = controlID;

            // Get current event, store to 'e'.
            Event e = Event.current;

            // If event type is key down.
            if (e.type == EventType.KeyDown)
            {
                // If key press is nothing - prevents functions running twice if keypress also registers as a char.
                if (e.keyCode == KeyCode.None)
                {
                    Event.current.Use();
                    return;
                }

                // FIND KEY CODES:
                // If you're unsure what the keycode for character presses are, uncomment the block between 
                // these lines and press a few keys in the scene view to log the results to console
                // The manual editor will stop working while this block is uncommented, be sure to comment it again when done!
                // -----------------------------------------------------------------
                /* 

                if (e.keyCode != KeyCode.None)
                {
                    Debug.Log("Easy DG - Log: " +"Key Code - " + e.keyCode);
                }
                else
                {
                    Debug.Log("Easy DG - Log: " +"No Key Code for key. Try another.");
                }
                return;

                */
                // ----------------------------------------------------------------              

                // This part handles the key presses, updates myScript to generate the manual edit templates / handle spawning.
                KeyCode kCode = e.keyCode;

                if (myScript.dungeonParent != null)
                {
                    // Left / Right / Up / Down movement.
                    if (kCode == leftKey)
                    {
                        try
                        {
                            myScript.ChangeManualTemplate(currentL, currentW, Vector3.left);
                        }
                        catch
                        {
                            myScript.ToggleManualMode(false);
                            Debug.Log("Easy DG - Log: " + "Manual Edit Mode Failure - reimport Dungeon.");
                            Debug.Log("Easy DG - Log: " + "Most often caused due to running play mode during editing.");
                        }
                    }

                    if (kCode == rightKey)
                    {
                        try
                        {
                            myScript.ChangeManualTemplate(currentL, currentW, Vector3.right);
                        }
                        catch
                        {
                            myScript.ToggleManualMode(false);
                            Debug.Log("Easy DG - Log: " + "Manual Edit Mode Failure - reimport Dungeon.");
                            Debug.Log("Easy DG - Log: " + "Most often caused due to running play mode during editing.");
                        }
                    }

                    if (kCode == upKey)
                    {

                        try
                        {
                            myScript.ChangeManualTemplate(currentL, currentW, Vector3.forward);
                        }
                        catch
                        {
                            myScript.ToggleManualMode(false);
                            Debug.Log("Easy DG - Log: " + "Manual Edit Mode Failure - reimport Dungeon.");
                            Debug.Log("Easy DG - Log: " + "Most often caused due to running play mode during editing.");
                        }
                    }

                    if (kCode == downKey)
                    {
                        try
                        {
                            myScript.ChangeManualTemplate(currentL, currentW, Vector3.back);
                        }
                        catch
                        {
                            myScript.ToggleManualMode(false);
                            Debug.Log("Easy DG - Log: " + "Manual Edit Mode Failure - reimport Dungeon.");
                            Debug.Log("Easy DG - Log: " + "Most often caused due to running play mode during editing.");
                        }
                    }

                    // Confirm and delete.
                    if (kCode == confirmKey)
                    {
                        try
                        {
                            myScript.SpawnAtTemplates();
                        }
                        catch
                        {
                            myScript.ToggleManualMode(false);
                            Debug.Log("Easy DG - Log: " + "Manual Edit Mode Failure - reimport Dungeon.");
                            Debug.Log("Easy DG - Log: " + "Most often caused due to running play mode during editing.");
                        }
                    }

                    if (kCode == deleteKey)
                    {
                        try
                        {
                            myScript.DeleteAtTemplates();
                        }
                        catch
                        {
                            myScript.ToggleManualMode(false);
                            Debug.Log("Easy DG - Log: " + "Manual Edit Mode Failure - reimport Dungeon.");
                            Debug.Log("Easy DG - Log: " + "Most often caused due to running play mode during editing.");
                        }
                    }

                    // Length / Width controls.
                    if (kCode == increaseLengthKey)
                    {
                        if (currentL < maxL)
                        {
                            currentL++;
                            myScript.ChangeManualTemplate(currentL, currentW, Vector3.zero);
                        }
                        else
                        {
                            Debug.Log("Easy DG - Log: " +"Already at max - cannot increase.");
                        }
                    }

                    if (kCode == decreaseLengthKey)
                    {
                        if (currentL > 1)
                        {
                            currentL--;
                            myScript.ChangeManualTemplate(currentL, currentW, Vector3.zero);
                        }
                        else
                        {
                            Debug.Log("Easy DG - Log: " +"Already at min - cannot decrease.");
                        }
                    }

                    if (kCode == increaseWidthKey)
                    {
                        if (currentW < maxW)
                        {
                            currentW++;
                            myScript.ChangeManualTemplate(currentL, currentW, Vector3.zero);
                        }
                        else
                        {
                            Debug.Log("Easy DG - Log: " +"Already at max - cannot increase.");
                        }
                    }

                    if (kCode == decreaseWidthKey)
                    {
                        if (currentW > 1)
                        {
                            currentW--;
                            myScript.ChangeManualTemplate(currentL, currentW, Vector3.zero);
                        }
                        else
                        {
                            Debug.Log("Easy DG - Log: " +"Already at min - cannot decrease.");
                        }
                    }
                }
                else
                {
                    Debug.Log("Easy DG - Log: " +"Must have a dungeon in scene to use manual edit mode!");
                }

                // Stops keypresses caught above from continuing to function when scene view active.
                e.Use();
            }

        }
    }
}

#endif
