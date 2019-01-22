using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SetupWizard : EditorWindow {
    
    // Control bools:
    private bool genInScene = false;
    private bool samplePrefabFound = false;

    // GameObject variables:
    private GameObject curGen;
    private GameObject dgPrefab;

    // Component variables:
    private DungeonGenerator dGen;

    // Control counters:
    private int createWarningCounter = 0;

    // Popup controls
    private string[] prefabOptions = new string[] { "Use sample prefabs", "Use custom prefabs" };
    private int prefabOptionsIndex = 0;

    private string[] templateOptionsFSM = new string[] { "Few", "Some", "Many" };       //objects
    private string[] templateOptionsLMH = new string[] { "Low", "Medium", "High" };     //room density
    private string[] templateOptionsSML = new string[] { "Small", "Medium", "Large" };  //overall size

    private int dDensity = 1;
    private int dSize = 1;
    private int dObjectDensity = 1;
    private int dRoomSize = 1;
    private int dRoomFrequency = 1;

    // Generator options:
    private string dName = "Dungeon-";
    private bool dLayoutOnly = false;
    private bool dGenRooms = true;
    private bool dMessyProps = false;

    [MenuItem("Easy DG / Generator Setup Wizard")]

    public static void ShowWindow()
    {
        EditorWindow thisWindow = EditorWindow.GetWindow(typeof(SetupWizard));
        thisWindow.minSize = new Vector2(610, 550);
    }

    private void OnGUI()
    {
        // Initial calcs / functions.
        if (curGen == null)
        {
            genInScene = false;
        }

        // Begin layout:
        GUILayout.Label("Easy DG - Generator Setup Wizard:", EditorStyles.boldLabel);
        EditorGUILayout.Space(); // --------------------------------------------------------------------------------------------- NEWLINE.

        // Custom prefabs option:
        GUIContent pcontent = new GUIContent("Use custom prefabs:", "Use custom prefabs if you have your own pre-made walls, floors, etc. These will not be pre-filled.\n\nElse, you can select to pre-fill the generator with the provided sample assets. This option requires you to have also imported the 'Sample Scene \\ Resources' folder and assets");
        GUILayout.Label(pcontent);
        prefabOptionsIndex = EditorGUILayout.Popup(prefabOptionsIndex, prefabOptions, GUILayout.MaxWidth(150));

        if (prefabOptionsIndex == 0 && dgPrefab == null)
        {
            dgPrefab = (GameObject)Resources.Load("DungeonGenerator");

            if (dgPrefab == null)
            {
                samplePrefabFound = false;
            }
            else
            {
                samplePrefabFound = true;
            }
        }

        if (genInScene == false)
        {
            EditorGUILayout.Space(); // --------------------------------------------------------------------------------------------- NEWLINE.
            if (prefabOptionsIndex == 0 && samplePrefabFound == true)
            {
                // Create button:
                if (GUILayout.Button("CREATE GENERATOR"))
                {
                    GameObject existingDG = GameObject.Find("DungeonGenerator");

                    if (existingDG != null && createWarningCounter == 0) createWarningCounter = 1;
                    else if (createWarningCounter == 1) createWarningCounter = 2;
                    else createWarningCounter = 2;

                    if (createWarningCounter == 2)
                    {
                        createWarningCounter = 0;
                        CreateGen();
                    }
                }
            }
            else if (prefabOptionsIndex == 0 && samplePrefabFound == false)
            {
                // Show error if sample prefabs selected but not found.
                GUILayout.Label("ERROR: DungeonGenerator prefab not found.\nPlease ensure Sample Scene + assets have been imported.", EditorStyles.boldLabel);
            }
            else if (genInScene == false)
            {
                // Create button:
                if (GUILayout.Button("CREATE GENERATOR"))
                {
                    GameObject existingDG = GameObject.Find("DungeonGenerator");
                    if (existingDG != null && createWarningCounter == 0) createWarningCounter = 1;
                    else if (createWarningCounter == 1) createWarningCounter = 2;
                    else createWarningCounter = 2;

                    if (createWarningCounter == 2)
                    {
                        createWarningCounter = 0;
                        CreateGen();
                    }
                }
            }
        }

        if (createWarningCounter == 1)
        {
            GUILayout.Label("WARNING: The scene has a pre-existing DungeonGenerator object which will be destroyed.\nClick create button again to confirm.", EditorStyles.boldLabel);
            EditorGUILayout.Space(); // --------------------------------------------------------------------------------------------- NEWLINE.
        }

        if (GUILayout.Button("RESET WIZARD - DEFAULT SETTINGS"))
        {
            ResetAll();
        }

        // Pre-fill options:
        if (genInScene == false)
        {
            EditorGUILayout.Space(); // --------------------------------------------------------------------------------------------- NEWLINE.
            GUILayout.Label("No DungeonGenerator object active. Please create to continue.", EditorStyles.boldLabel);
        }
        else
        {
            EditorGUILayout.Space(); // --------------------------------------------------------------------------------------------- NEWLINE.
            GUIContent ocontent = new GUIContent("DungeonGenerator pre-fill / template options:", "Use these as a 'template' - they just help to pre-fill the dungeon generator variables based on your choices.\n\nThese are totally optional, you can configure all these settings manually and more precisely on the DungeonGenerator object itself in the Inspector.");
            GUILayout.Label(ocontent, EditorStyles.boldLabel);

            EditorGUILayout.Space(); // --------------------------------------------------------------------------------------------- NEWLINE.
            ocontent = new GUIContent("Dungeon naming convention:", "Name given to all Dungeon objects generated in the scene.\n\nThe system will automatically add an incrementing number to the dungeon, you do not need to specify a number here.");
            GUILayout.Label(ocontent);
            dName = GUILayout.TextField(dName);

            EditorGUILayout.Space(); // --------------------------------------------------------------------------------------------- NEWLINE.
            ocontent = new GUIContent("Dungeon density:", "Controls how compact the dungeon is. \n\nLow means dungeon will spread far out from centre, high means dungeon will stay close to centre.");
            GUILayout.Label(ocontent);
            dDensity = EditorGUILayout.Popup(dDensity, templateOptionsLMH, GUILayout.MaxWidth(75));

            ocontent = new GUIContent("Dungeon size:", "Controls overall size of dungeon. \n\nLow will mean less lines and less tiles per line, leading to smaller dungeon sizes overall. High is the opposite.");
            GUILayout.Label(ocontent);
            dSize = EditorGUILayout.Popup(dSize, templateOptionsSML, GUILayout.MaxWidth(75));

            EditorGUILayout.Space(); // --------------------------------------------------------------------------------------------- NEWLINE.
            ocontent = new GUIContent("Generate rooms:", "Set to FALSE if you only want hallways and lines. No rooms will be generated.");
            GUILayout.Label(ocontent, EditorStyles.boldLabel);
            dGenRooms = EditorGUILayout.Toggle(dGenRooms.ToString(), dGenRooms);
            EditorGUILayout.Space(); // --------------------------------------------------------------------------------------------- NEWLINE.

            if (dGenRooms)
            {
                ocontent = new GUIContent("Room generation density:", "Controls frequency and spacing of room generation.\n\nLow will mean less rooms, more spaced apart. High means more rooms, placed closer together.");
                GUILayout.Label(ocontent);
                dRoomFrequency = EditorGUILayout.Popup(dRoomFrequency, templateOptionsLMH, GUILayout.MaxWidth(75));

                ocontent = new GUIContent("Room size:", "Controls overall size of rooms.");
                GUILayout.Label(ocontent);
                dRoomSize = EditorGUILayout.Popup(dRoomSize, templateOptionsSML, GUILayout.MaxWidth(75));
            }

            EditorGUILayout.Space(); // --------------------------------------------------------------------------------------------- NEWLINE.
            ocontent = new GUIContent("Generate layout only:", "Set to TRUE if you wish to only generate a dungeon layout. No walls, doors, etc, will be spawned, only floor tiles.");
            GUILayout.Label(ocontent, EditorStyles.boldLabel);
            dLayoutOnly = EditorGUILayout.Toggle(dLayoutOnly.ToString(), dLayoutOnly);
            EditorGUILayout.Space(); // --------------------------------------------------------------------------------------------- NEWLINE.

            if (dLayoutOnly == false)
            {
                ocontent = new GUIContent("Messy prop spawning:", "Set to TRUE if you want doors, props, etc, to spawn anywhere, randomly. \n\nIn the case of doors, they will still only ever spawn in hallways, not in the middle of rooms.");
                GUILayout.Label(ocontent);
                dMessyProps = EditorGUILayout.Toggle(dMessyProps.ToString(), dMessyProps);

                ocontent = new GUIContent("Object / Prop density:", "Controls how many props are spawned. \n\nCurrently really only controls door spawning frequency. Hopefully will be used later to implement dungeon prop spawning.");
                GUILayout.Label(ocontent);
                dObjectDensity = EditorGUILayout.Popup(dObjectDensity, templateOptionsFSM, GUILayout.MaxWidth(75));
            }

            if (GUILayout.Button("GENERATE TEMPLATE DUNGEON"))
            {
                SetupGen();
                GenerateFirstDungeon();
            }
        }
    }

    private void GenerateFirstDungeon()
    {
        dGen.PluginCheck();
    }

    private void SetupGen()
    {
        dGen.deleteDungeonOnNew = true;
        dGen.totalLines = 0;
        dGen.maxTilesPerLine = 0;
        dGen.changeDirChance = 0;
        dGen.doorSpawnChance = 0;
        dGen.minRoomLength = 0;
        dGen.maxRoomLength = 0;
        dGen.minRoomWidth = 0;
        dGen.maxRoomWidth = 0;
        dGen.roomMinDistanceEndOfLine = 0;
        dGen.roomMinDistanceFromLast = 0;
        dGen.ResetDungeonCount();

        dGen.dungeonName = dName;

        switch (dDensity)
        {
            case 0:
                dGen.changeDirChance += 5;
                dGen.allowParallelLinesToTouch = false;
                dGen.generateContinuous = true;
                break;
            case 1:
                dGen.totalLines += 2;
                dGen.maxTilesPerLine += 10;
                dGen.changeDirChance += 15;
                dGen.allowParallelLinesToTouch = false;
                dGen.generateContinuous = false;
                break;
            case 2:
                dGen.totalLines += 4;
                dGen.maxTilesPerLine += 30;
                dGen.changeDirChance += 30;
                dGen.allowParallelLinesToTouch = true;
                dGen.generateContinuous = false;
                break;
        }

        switch (dSize)
        {
            case 0:
                dGen.totalLines += 2;
                dGen.maxTilesPerLine += 15;
                break;
            case 1:
                dGen.totalLines += 4;
                dGen.maxTilesPerLine += 30;
                break;
            case 2:
                dGen.totalLines += 6;
                dGen.maxTilesPerLine += 60;
                break;
        }

        if (dGenRooms)
        {
            dGen.generateRooms = true;

            switch (dRoomFrequency)
            {
                case 0:
                    dGen.roomSpawnChance += 5;
                    dGen.roomMinDistanceEndOfLine += 10;
                    dGen.roomMinDistanceFromLast += 15;
                    break;
                case 1:
                    dGen.roomSpawnChance += 20;
                    dGen.roomMinDistanceEndOfLine += 5;
                    dGen.roomMinDistanceFromLast += 10;
                    break;

                case 2:
                    dGen.roomSpawnChance += 60;
                    dGen.roomMinDistanceEndOfLine += 1;
                    dGen.roomMinDistanceFromLast += 1;
                    break;
            }

            switch (dRoomSize)
            {
                case 0:
                    dGen.minRoomLength += 2;
                    dGen.maxRoomLength += 3;
                    dGen.minRoomWidth += 1;
                    dGen.maxRoomWidth += 2;
                    break;
                case 1:
                    dGen.minRoomLength += 2;
                    dGen.maxRoomLength += 5;
                    dGen.minRoomWidth += 2;
                    dGen.maxRoomWidth += 3;
                    break;

                case 2:
                    dGen.minRoomLength += 4;
                    dGen.maxRoomLength += 7;
                    dGen.minRoomWidth += 3;
                    dGen.maxRoomWidth += 6;
                    break;
            }
        }
        else
        {
            dGen.generateRooms = false;
        }

        if (dLayoutOnly == false)
        {
            if (dMessyProps == false)
            {
                dGen.onlySpawnDoorsAtHallEnds = true;
            }
            else
            {
                dGen.onlySpawnDoorsAtHallEnds = false;
            }

            switch (dObjectDensity)
            {
                case 0:
                    dGen.doorSpawnChance += 5;
                    break;
                case 1:
                    dGen.doorSpawnChance += 15;
                    break;
                case 2:
                    dGen.doorSpawnChance += 50;
                    break;
            }

            dGen.generateWalls = true;
            dGen.generateCorners = true;
        }
        else
        {
            dGen.generateWalls = false;
            dGen.generateCorners = false;
        }
    }

    private void CreateGen()
    {
        GameObject existingDG = GameObject.Find("DungeonGenerator");
        if (existingDG != null) DestroyImmediate(existingDG.gameObject);

        if (prefabOptionsIndex == 0)
        {
            curGen = Instantiate(dgPrefab, Vector3.zero, Quaternion.identity);
            curGen.name = "DungeonGenerator";
            dGen = curGen.GetComponent<DungeonGenerator>();
        }
        else
        {
            curGen = new GameObject("DungeonGenerator");
            curGen.transform.position = Vector3.zero;
            dGen = curGen.AddComponent<DungeonGenerator>();
        }

        genInScene = true;
    }

    private void ResetAll()
    {
        if (curGen != null) DestroyImmediate(curGen.gameObject);
        if (dgPrefab != null) dgPrefab = null;
        prefabOptionsIndex = 0;
        createWarningCounter = 0;
    }
}
