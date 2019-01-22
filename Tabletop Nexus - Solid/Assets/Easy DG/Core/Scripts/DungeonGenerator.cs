using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[HelpURL("https://youtu.be/UxjyCzs0zcE")]
[ExecuteInEditMode]
public class DungeonGenerator : MonoBehaviour
{
    // Control Variables:
    // ------------------------------------------- //
    public string dungeonName;
    [Space(1)]

    [Header("Object Prefab Arrays:")]
    public GameObject tilePrefab;
    public GameObject[] wallPrefabs;
    public GameObject[] doorPrefabs;
    public GameObject[] cornerPrefabs;
    [Space(1)]

    [Header("Tile Spawn Control:")]
    [Tooltip("Controls how many 'lines' or 'hallways' are generated.")]
    public int totalLines;
    [Tooltip("How many tiles per line.\n\nDepending on other settings, having more lines but less tiles will generally mean a more compact dungeon.")]
    public int maxTilesPerLine;
    [Space(1)]
    [Header("Dungeon Boundaries:")]
    [Tooltip("Bound settings will only be applied if this is TRUE")]
    [SerializeField]
    private bool boundariesOn;
    [Tooltip("Boundary X - Will spawn this many tiles on -X and +X from centre. Use this to limit the overall size of the area used by the Dungeon.\n\nDungeon will be this value * 2 tiles on the X axis.")]
    public int boundsX;
    [Tooltip("Boundary Z - Will spawn this many tiles on -Z and +Z from centre. Use this to limit the overall size of the area used by the Dungeon.\n\nSo dungeon will be this value * 2 tiles on the Z axis.")]
    public int boundsZ;
    [Tooltip("This bool controls what lines do if they hit the bound limits.\n\nTRUE will terminate the lines and begin the next one, essentially 'snipping' the dungeon at edges.\n\nFALSE will cause the lines to 'reset' back to centre and continue, leading to very compact dungeons. When FALSE, it is recommended to reduce other settings (e.g. total lines, tiles per line, room sizes, etc.) to prevent excessively cluttered, or 'one room' Dungeons.")]
    [SerializeField]
    private bool terminateAtBounds;

    [Space(1)]

    [Header("Chance Variables:")]
    [Tooltip("Controls how often a line will change direction.")]
    [Range(0, 100)]
    public int changeDirChance;
    [Tooltip("How often doors will spawn.")]
    [Range(0, 100)]
    public int doorSpawnChance;
    private int doorSpawnStart;
    [Tooltip("How often rooms will be created.")]
    [Range(0, 100)]
    public int roomSpawnChance;
    [Space(1)]

    [Header("Generation Control Booleans:")]
    [Tooltip("Delete dungeon when a new one is generated.")]
    public bool deleteDungeonOnNew;
    [Tooltip("Although not perfect, turning this off will mean lines that run parallel to each other will try and avoid touching.\n\nTurning it on will generally lead to large rooms and less hallways.")]
    public bool allowParallelLinesToTouch;
    [Tooltip("When turned on, lines will start from the end of the previous line.\n\nWhen turned off, lines will always start generating from the start point (dungeon centre).")]
    public bool generateContinuous;
    [Tooltip("Turn off to prevent rooms being generated.")]
    public bool generateRooms;
    [Tooltip("Turn off to generate a dungeon with no walls.")]
    public bool generateWalls;
    private bool genWallsStart;
    [Tooltip("Turn off to generate a dungeon with no corners (columns).")]
    public bool generateCorners;
    private bool genCornersStart;
    [Tooltip("Turn off to allow doors to spawn in hallways, rather than just at the ends.")]
    public bool onlySpawnDoorsAtHallEnds;
    [Space(1)]

    [Header("Room Sizes:")]
    public int minRoomLength;
    public int maxRoomLength;
    public int minRoomWidth;
    public int maxRoomWidth;
    [Space(1)]

    [Header("Room Distance Controls:")]
    [Tooltip("Rooms will only spawn if the line has at least this many tiles left before it ends.")]
    public int roomMinDistanceEndOfLine;
    [Tooltip("Rooms will only spawn if the line is now at least this many tiles away from the last room.")]
    public int roomMinDistanceFromLast;
    [Space(1)]

    [Header("Manual Edit Controls:")]
    public GameObject manualModeTemplatePrefab;
    [Tooltip("Spawn position X of manual edit mode template.\nUse this to adjust where manual edit mode starts.")]
    public float defaultTemplatePosX;
    [Tooltip("Spawn position Z of manual edit mode template.\nUse this to adjust where manual edit mode starts.")]
    public float defaultTemplatePosZ;

    // ------------------------------------------- //

    // Private / internal variables used by the script. User should not need to change these.

    // Boundary variables, store results of caluclations using prefab sizes.
    float maxXBound;
    float maxZBound;


    // Main bool to handle if Edit Mode is on or off.
    [HideInInspector]
    public bool manualModeOn;

    // Bool to handle blank dungeon spawning, turns on when 'Generate Blank Dungeon' button is pressed and off again after generation completes.
    // Used to ensure values set before clicking button are retained, as it just updates the generation variables to achieve the blank dungeon.
    private bool blankDungeonGen;

    // Variables to handle line direction changes.
    private Vector3[] directions;
    private Vector3 currentDirection;
    private Vector3 lastDirection;

    private bool canChangeDirNow;

    // Bool determines if current line is running horizontally or vertically.
    private bool dirHorizontal;

    // Prefab size variables. Based on Collider.bounds.size.x/y/z, used to control prefab positioning on the tile.
    // These are changed when an object is instantiated to match the current object.
    // Exception is tileSize / tileHeight, these are set at the start of the generation run and don't change.
    [HideInInspector]
    public float tileSize;
    [HideInInspector]
    public float tileHeight;
    private float wallWidth;
    private float wallHeight;
    private float doorHeight;
    private float cornerWidth;
    private float cornerHeight;

    // Arrays used to store the bounds of the above prefabs, the element IDs will match up with the prefabs (e.g. doorHeights[1] will be the stored height of doorPrefabs[1]).
    private float[] wallWidths;
    private float[] wallHeights;
    private float[] doorHeights;
    private float[] cornerWidths;
    private float[] cornerHeights;

    // Room generation variables.

    // Bool to determine if room currently being spawned.
    private bool spawningRoom;

    // Current room sizes.
    private int currentRoomLength;
    private int currentRoomWidth;

    // Counter to track length of room.
    private int currentRoomCounter;

    // Variable to store the end tile of the last room.
    private int lastRoomEndTile;

    // Main position variable used to control generation.
    private Vector3 currentPos;

    // Lists to track tiles and prefabs generated.
    [HideInInspector]
    public List<TileItem> spawnedTiles = new List<TileItem>();
    [HideInInspector]
    public List<TileItem> doorTiles;
    [HideInInspector]
    public List<GameObject> allWalls;
    [HideInInspector]
    public List<GameObject> allDoors;
    [HideInInspector]
    public List<GameObject> allCorners;

    // Bool to control if we clear the tiles list at generation.
    [HideInInspector]
    public bool clearTileList = true;

    // Variable to store a reference to the most recently generated Dungeon.
    [HideInInspector]
    public GameObject dungeonParent;

    // Automatic counter to track how many dungeons have been spawned.
    [HideInInspector]
    [SerializeField]
    private int dungeonsSpawned = 0;

    // Counters used during generation to track spawn progress, if you need a list of all prefabs, totals, etc, use the spawnedTiles and allWalls/Doors/Corners List objects above.
    // Don't use these counters as they may not be completely accurate.
    private int totalWalls;
    private int totalCorners;

    // Plugin bools.
    private bool multiStoryPlugin;
    private bool towerModePlugin;

    // Function to delete current Dungeon.
    public void DeleteDungeon()
    {
        if (dungeonParent != null)
        {
            if (dungeonParent.transform.parent == null)
            {
                DestroyImmediate(dungeonParent.gameObject, false);
            }
            else
            {
                //DestroyImmediate(dungeonParent.transform.parent.gameObject, false);
            }

            if (manualModeOn == true)
            {
                ToggleManualMode(false);
            }
        }
    }

    // Function to generate a blank Dungeon.
    public void GenerateBlankDungeon()
    {
        blankDungeonGen = true;

        if (generateWalls == true)
        {
            genWallsStart = true;
            generateWalls = false;
        }

        if (doorSpawnChance > 0)
        {
            doorSpawnStart = doorSpawnChance;
            doorSpawnChance = 0;
        }

        if (generateCorners == true)
        {
            genCornersStart = true;
            generateCorners = false;
        }

        GenDungeon();
    }

    // Function to reset Dungeon counter.
    public void ResetDungeonCount()
    {
        dungeonsSpawned = 0;
    }

    // Function to update line direction during generation.
    Vector3 GetNewDir()
    {
        Vector3 newDir = lastDirection;

        if (allowParallelLinesToTouch == false)
        {
            if (canChangeDirNow == true)
            {
                while (newDir == lastDirection)
                {
                    newDir = directions[Random.Range(0, directions.Length)];
                }

                canChangeDirNow = false;
            }
            else
            {
                newDir = currentDirection;
                canChangeDirNow = true;
            }
        }
        else
        {
            while (newDir == lastDirection)
            {
                newDir = directions[Random.Range(0, directions.Length)];
            }
        }

        if (newDir == Vector3.left || newDir == Vector3.right)
        {
            dirHorizontal = true;
        }
        else
        {
            dirHorizontal = false;
        }

        return newDir;
    }

    // Random chance check to determine if direction will change. Returns true if so.
    bool ChangeDirCheck()
    {
        int rnd = Random.Range(1, 101);

        if (rnd <= changeDirChance)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    // Function to spawn a tile into the game world.
    // This function is important, as it adds the tile to the appropriate lists and updates the TileItem attached to it.
    // If you're generating tiles manually via another script, you should use this function and only pass it the position you want a tile to spawn.
    // Requires a current Dungeon in the scene set as dungeonParent to work.
    public void SpawnTile(Vector3 spawnPos)
    {
        GameObject newTile;

        newTile = Instantiate(tilePrefab, spawnPos, Quaternion.identity);
        newTile.transform.SetParent(dungeonParent.transform, false);
        TileItem ti = new TileItem();
        ti.myTile = newTile;
        ti.myScript = ti.myTile.GetComponent<TileScript>();
        ti.myScript.isDoor = false;
        ti.myScript.horizontalDoor = true;
        ti.myScript.neighboursTotal = 0;
        spawnedTiles.Add(ti);
    }

    // Function to check, via raycast, if the provided position is empty / clear.
    // Returns true if nothing is hit.
    public bool CheckPosClear(Vector3 checkPos)
    {
        if (Physics.Raycast(checkPos + (Vector3.up * tileHeight), Vector3.down, tileHeight * 1))
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    // Random number check to determine if room will spawn during generation.
    bool WillSpawnRoom()
    {
        int rnd = Random.Range(1, 101);

        if (rnd <= roomSpawnChance)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    //Function to generate random values for room size based on min / max variables.
    void SetRoomSize()
    {
        currentRoomLength = Random.Range(minRoomLength, maxRoomLength + 1);
        currentRoomWidth = Random.Range(minRoomWidth, maxRoomWidth + 1);
    }

    // Function to check prefab arrays and determine their sizes using MeshRenderers.
    void SetPrefabSizes()
    {
        MeshRenderer tileRend = tilePrefab.GetComponent<MeshRenderer>();

        // Tile is only 1 prefab, so we don't use arrays for it.
        tileSize = tileRend.bounds.size.x;
        tileHeight = tileRend.bounds.size.y;

        /* For walls, doors and corners, we use arrays to store the sizes.
        For every prefab object in each array, we generate another 2 arrays to store the width and height.
        These arrays match up. E.g. wallWidths[1] and wallHeights[1] are the width and height values for wallPrefabs[1].
        
        If a prefab's parent object has no MeshRenderer attached, the function will look through the child objects and use
        the one with the largest bounds value. This is why prefabs must have a MeshRenderer on either themselves or a child. */
        wallWidths = new float[wallPrefabs.Length];
        for (int i = 0; i < wallPrefabs.Length; i++)
        {
            if (wallPrefabs[i].GetComponent<MeshRenderer>())
            {
                wallWidths[i] = wallPrefabs[i].GetComponent<MeshRenderer>().bounds.size.z;
            }
            else
            {
                MeshRenderer[] attachedRends = wallPrefabs[i].GetComponentsInChildren<MeshRenderer>();

                float biggestBounds = 0;
                foreach (MeshRenderer rend in attachedRends)
                {
                    if (rend.bounds.size.z > biggestBounds)
                    {
                        biggestBounds = rend.bounds.size.z;
                        wallWidths[i] = biggestBounds;
                    }
                }
            }
        }

        wallHeights = new float[wallPrefabs.Length];
        for (int i = 0; i < wallPrefabs.Length; i++)
        {
            if (wallPrefabs[i].GetComponent<MeshRenderer>())
            {
                wallHeights[i] = wallPrefabs[i].GetComponent<MeshRenderer>().bounds.size.y;
            }
            else
            {
                MeshRenderer[] attachedRends = wallPrefabs[i].GetComponentsInChildren<MeshRenderer>();

                float biggestBounds = 0;
                foreach (MeshRenderer rend in attachedRends)
                {
                    if (rend.bounds.size.y > biggestBounds)
                    {
                        biggestBounds = rend.bounds.size.y;
                        wallHeights[i] = biggestBounds;
                    }
                }
            }
        }

        doorHeights = new float[doorPrefabs.Length];
        for (int i = 0; i < doorPrefabs.Length; i++)
        {
            if (doorPrefabs[i].GetComponent<MeshRenderer>())
            {
                doorHeights[i] = doorPrefabs[i].GetComponent<MeshRenderer>().bounds.size.y;
            }
            else
            {
                MeshRenderer[] attachedRends = doorPrefabs[i].GetComponentsInChildren<MeshRenderer>();

                float biggestBounds = 0;
                foreach (MeshRenderer rend in attachedRends)
                {
                    if (rend.bounds.size.y > biggestBounds)
                    {
                        biggestBounds = rend.bounds.size.y;
                        doorHeights[i] = biggestBounds;
                    }
                }
            }
        }

        cornerWidths = new float[cornerPrefabs.Length];
        for (int i = 0; i < cornerPrefabs.Length; i++)
        {
            if (cornerPrefabs[i].GetComponent<MeshRenderer>())
            {
                cornerWidths[i] = cornerPrefabs[i].GetComponent<MeshRenderer>().bounds.size.z;
            }
            else
            {
                MeshRenderer[] attachedRends = cornerPrefabs[i].GetComponentsInChildren<MeshRenderer>();

                float biggestBounds = 0;
                foreach (MeshRenderer rend in attachedRends)
                {
                    if (rend.bounds.size.z > biggestBounds)
                    {
                        biggestBounds = rend.bounds.size.z;
                        cornerWidths[i] = biggestBounds;
                    }
                }
            }
        }

        cornerHeights = new float[cornerPrefabs.Length];
        for (int i = 0; i < cornerPrefabs.Length; i++)
        {
            if (cornerPrefabs[i].GetComponent<MeshRenderer>())
            {
                cornerHeights[i] = cornerPrefabs[i].GetComponent<MeshRenderer>().bounds.size.y;
            }
            else
            {
                MeshRenderer[] attachedRends = cornerPrefabs[i].GetComponentsInChildren<MeshRenderer>();

                float biggestBounds = 0;
                foreach (MeshRenderer rend in attachedRends)
                {
                    if (rend.bounds.size.y > biggestBounds)
                    {
                        biggestBounds = rend.bounds.size.y;
                        cornerHeights[i] = biggestBounds;
                    }
                }
            }
        }

    }

    public float GetDungeonHeight()
    {
        float newHeight = 0;

        SetPrefabSizes();

        float tallestWall = 0;
        foreach (float h in wallHeights)
        {
            if (h > tallestWall)
            {
                tallestWall = h;
            }
        }

        newHeight = tileHeight + tallestWall;

        return newHeight;
    }

    // Function called by Generate Dungeon button.
    // This checks for any plugins and runs them accordingly.

    public void PluginCheck()
    {
        // No, changing these will not make the plugins available.
        multiStoryPlugin = false;
        towerModePlugin = false;

        if (GetComponent<MultiStoryPlugin>())
        {
            multiStoryPlugin = true;
            if (spawnedTiles != null) spawnedTiles.Clear();
            else spawnedTiles = new List<TileItem>();
            MultiStoryPlugin mspi = GetComponent<MultiStoryPlugin>();
            mspi.MultiStoryGen();
        }
        else
        {
            if (spawnedTiles != null) spawnedTiles.Clear();
            else spawnedTiles = new List<TileItem>();
            GenDungeon();
        }

        if (GetComponent<TowerModePlugin>())
        {
            towerModePlugin = true;
            GetComponent<TowerModePlugin>().FillAsTower();
        }
    }

    // Main function called to start Dungeon generation.
    public void GenDungeon()
    {
        // The max value used to prevent the system entering an infinite loop.
        // If required, this can be adjusted. See provided documentation for more info.
        int preventInfLoopMax = 5000;

        // Don't touch this.
        int preventInfLoopCounter = 0;

        // Call to set the prefab sizes.
        SetPrefabSizes();

        // Initialise lists to store all objects generated in the Dungeon.
        allWalls = new List<GameObject>();
        allDoors = new List<GameObject>();
        allCorners = new List<GameObject>();
        doorTiles = new List<TileItem>();
                
        // Initialise array of directions for up, down, left, right.
        directions = new Vector3[] { Vector3.forward, Vector3.back, Vector3.left, Vector3.right };

        // Reset room bool and start point.
        spawningRoom = false;
        currentPos = Vector3.zero;

        // Increment Dungeon counter.
        dungeonsSpawned++;

        // Set start direction.
        currentDirection = GetNewDir();
        lastDirection = currentDirection;

        // Set initial values for boundary control.
        maxXBound = boundsX * tileSize;
        maxZBound = boundsZ * tileSize;

        // If dungeon exists in scene, destroy it to make way for new.
        // Requires deleteDungeonOnNew to be true.
        if (dungeonParent != null && deleteDungeonOnNew == true)
        {
            DestroyImmediate(dungeonParent.gameObject, false);
        }

        // Check if start pos blocked by collider.
        if (CheckPosClear(Vector3.zero) == false && blankDungeonGen == false)
        {
            Debug.Log("Easy DG - ERROR: " + "Something is blocking the generation.");
            Debug.Log("Easy DG - " + "Ensure there are no objects / colliders at 0,0,0.");
            return;
        }

        // Create new Dungeon parent object.
        dungeonParent = new GameObject();
        dungeonParent.layer = 8;
        dungeonParent.transform.position = new Vector3(1000, 0, 0);
        dungeonParent.name = dungeonName + dungeonsSpawned;
        //dungeonParent.transform.position = Vector3.zero;

        // Start and spawn lines from the centre.
        for (int line = 1; line <= totalLines;)
        {
            lastRoomEndTile = 0;
            canChangeDirNow = true;

            if (generateContinuous == false)
            {
                // Reset current pos so each line starts from 0,0,0.
                currentPos = Vector3.zero;
            }

            // Spawn tiles in line.
            for (int tile = 1; tile <= maxTilesPerLine;)
            {
                // Counter check to prevent infinite loop.
                if (preventInfLoopCounter == preventInfLoopMax)
                {
                    Debug.Log("Easy DG - ERROR: " + "Generation force quit to prevent infinite loop.");
                    Debug.Log("Easy DG - " + "No free space can be found, unable to spawn new tiles.");
                    Debug.Log("Easy DG - " + "Try lowering lines, max tiles or direction change variables.");
                    FinaliseDungeon();
                    return;
                }

                if (boundariesOn && (currentPos.x < -maxXBound || currentPos.x > maxXBound || currentPos.z < -maxZBound || currentPos.z > maxZBound))
                {
                    if (terminateAtBounds)
                    {
                        tile++;
                    }
                    else
                    {
                        Debug.Log("DEV-LOG: Line wrapped.");
                        currentPos = Vector3.zero;
                    }
                }
                else
                {
                    // Check if spawn area clear.
                    bool areaClear = CheckPosClear(currentPos);

                    if (areaClear == true)
                    {
                        if (blankDungeonGen == false)
                        {
                            // Spawn tile and inc tile counter.
                            SpawnTile(currentPos);
                            tile++;
                            preventInfLoopCounter = 0;

                            // If not currently spawning a room and generate rooms is true.
                            if (spawningRoom == false && generateRooms == true)
                            {
                                // If the current tile meets the requirements for room generation distances.
                                if (tile < (maxTilesPerLine - roomMinDistanceEndOfLine) && tile >= roomMinDistanceEndOfLine && tile > (lastRoomEndTile + roomMinDistanceFromLast))
                                {
                                    // Start spawning new room.
                                    if (WillSpawnRoom() == true)
                                    {
                                        currentRoomCounter = 0;
                                        spawningRoom = true;
                                        SetRoomSize();
                                    }
                                }
                            }
                        }
                        else
                        {
                            // Only runs if a blank Dungeon.
                            tile++;
                            preventInfLoopCounter = 0;
                        }

                        // Room spawn control.
                        if (spawningRoom == true)
                        {
                            currentRoomCounter++;

                            for (int w = 1; w <= currentRoomWidth; w++)
                            {
                                float moveAmt = tileSize * w;
                                float parCheckAmt = tileSize * (w + 1);

                                if (allowParallelLinesToTouch == false)
                                {
                                    if (dirHorizontal == false)
                                    {
                                        Vector3 spawnPosL = currentPos + (Vector3.left * moveAmt);
                                        Vector3 checkPosL = currentPos + (Vector3.left * parCheckAmt);
                                        if (CheckPosClear(spawnPosL) == true && CheckPosClear(checkPosL) == true)
                                        {
                                            SpawnTile(spawnPosL);
                                        }
                                        Vector3 spawnPosR = currentPos + (Vector3.right * moveAmt);
                                        Vector3 checkPosR = currentPos + (Vector3.right * parCheckAmt);
                                        if (CheckPosClear(spawnPosR) == true && CheckPosClear(checkPosR) == true)
                                        {
                                            SpawnTile(spawnPosR);
                                        }
                                    }
                                    else
                                    {
                                        Vector3 spawnPosU = currentPos + (Vector3.forward * moveAmt);
                                        Vector3 checkPosU = currentPos + (Vector3.forward * parCheckAmt);
                                        if (CheckPosClear(spawnPosU) == true && CheckPosClear(checkPosU) == true)
                                        {
                                            SpawnTile(spawnPosU);
                                        }
                                        Vector3 spawnPosD = currentPos + (Vector3.back * moveAmt);
                                        Vector3 checkPosD = currentPos + (Vector3.back * parCheckAmt);
                                        if (CheckPosClear(spawnPosD) == true && CheckPosClear(checkPosD) == true)
                                        {
                                            SpawnTile(spawnPosD);
                                        }
                                    }
                                }
                                else
                                {
                                    if (dirHorizontal == false)
                                    {
                                        Vector3 spawnPosL = currentPos + (Vector3.left * moveAmt);
                                        if (CheckPosClear(spawnPosL) == true)
                                        {
                                            SpawnTile(spawnPosL);
                                        }
                                        Vector3 spawnPosR = currentPos + (Vector3.right * moveAmt);
                                        if (CheckPosClear(spawnPosR) == true)
                                        {
                                            SpawnTile(spawnPosR);
                                        }
                                    }
                                    else
                                    {
                                        Vector3 spawnPosU = currentPos + (Vector3.forward * moveAmt);
                                        if (CheckPosClear(spawnPosU) == true)
                                        {
                                            SpawnTile(spawnPosU);
                                        }
                                        Vector3 spawnPosD = currentPos + (Vector3.back * moveAmt);
                                        if (CheckPosClear(spawnPosD) == true)
                                        {
                                            SpawnTile(spawnPosD);
                                        }
                                    }
                                }
                            }

                            if (currentRoomCounter == currentRoomLength)
                            {
                                lastRoomEndTile = tile;
                                spawningRoom = false;
                                currentRoomLength = 0;
                                currentRoomWidth = 0;
                            }
                        }
                    }

                    // Prevents changing direction when room is being spawned.
                    if (spawningRoom == false)
                    {
                        // Check if dir change.
                        bool changeDir = ChangeDirCheck();

                        if (changeDir == true)
                        {
                            // Update vars if so.
                            currentDirection = GetNewDir();
                            lastDirection = currentDirection;
                        }
                    }

                    // Update currentPos based on currentDirection.
                    currentPos += (currentDirection * tileSize);
                }

                preventInfLoopCounter++;
            }

            // Updates room control variables if we're still spawning a room.
            if (spawningRoom == true)
            {
                lastRoomEndTile = 0;
                spawningRoom = false;
                currentRoomLength = 0;
                currentRoomWidth = 0;
            }

            // Inc line.
            line++;
        }

        // Start spawning walls.
        SpawnWalls();
    }

    // Function to perform neighbour checks for wall spawning.
    // Stores results of these checks on the tile's TileItem for later use.
    void UpdateTileInfo(TileItem tile)
    {
        int totalNeighbours = 0;
        tile.uNeighbour = false;
        tile.dNeighbour = false;
        tile.lNeighbour = false;
        tile.rNeighbour = false;

        if (Physics.Raycast(tile.myTile.transform.position + (Vector3.up * tileHeight) + (Vector3.forward * tileSize), Vector3.down, tileHeight * 1))
        {
            tile.uNeighbour = true;
            totalNeighbours++;
        }

        if (Physics.Raycast(tile.myTile.transform.position + (Vector3.up * tileHeight) + (Vector3.back * tileSize), Vector3.down, tileHeight * 1))
        {
            tile.dNeighbour = true;
            totalNeighbours++;
        }

        if (Physics.Raycast(tile.myTile.transform.position + (Vector3.up * tileHeight) + (Vector3.left * tileSize), Vector3.down, tileHeight * 1))
        {
            tile.lNeighbour = true;
            totalNeighbours++;
        }

        if (Physics.Raycast(tile.myTile.transform.position + (Vector3.up * tileHeight) + (Vector3.right * tileSize), Vector3.down, tileHeight * 1))
        {
            tile.rNeighbour = true;
            totalNeighbours++;
        }

        tile.myScript.neighboursTotal = totalNeighbours;

        // Check to see if this tile will be a door.
        if (totalNeighbours == 2 && tile.myScript.ignoreForDoorChecks == false)
        {
            // Check to see if positioned horizontally or vertically.
            // Uses neighbours to determine.
            if (tile.uNeighbour == true && tile.dNeighbour == true)
            {
                tile.myScript.isDoor = true;
                doorTiles.Add(tile);
                tile.myScript.horizontalDoor = true;
            }
            else if (tile.lNeighbour == true && tile.rNeighbour == true)
            {
                tile.myScript.isDoor = true;
                doorTiles.Add(tile);
                tile.myScript.horizontalDoor = false;
            }
        }
    }

    // Function to spawn the walls after performing checks.
    void SpawnTileWalls(TileItem tile)
    {
        Quaternion hRot = Quaternion.Euler(new Vector3(0, 0, 0));
        Quaternion vRot = Quaternion.Euler(new Vector3(0, 90, 0));

        // Uses neighbours to determine which sides will spawn walls.
        if (tile.uNeighbour == false)
        {
            int newWallID = Random.Range(0, wallPrefabs.Length);

            wallHeight = wallHeights[newWallID];
            wallWidth = wallWidths[newWallID];

            float moveAmt = ((float)tileSize / 2) - (wallWidth / 2);

            Vector3 spawnPos = new Vector3(tile.myTile.transform.position.x, tile.myTile.transform.position.y + (wallHeight / 2) + (tileHeight / 2), tile.myTile.transform.position.z + moveAmt);
            GameObject newWall = Instantiate(wallPrefabs[newWallID], spawnPos, hRot);
            allWalls.Add(newWall);
            totalWalls++;
            newWall.transform.SetParent(tile.myTile.transform);
        }

        if (tile.dNeighbour == false)
        {
            int newWallID = Random.Range(0, wallPrefabs.Length);

            wallHeight = wallHeights[newWallID];
            wallWidth = wallWidths[newWallID];

            float moveAmt = ((float)tileSize / 2) - (wallWidth / 2);

            Vector3 spawnPos = new Vector3(tile.myTile.transform.position.x, tile.myTile.transform.position.y + (wallHeight / 2) + (tileHeight / 2), tile.myTile.transform.position.z - moveAmt);
            GameObject newWall = Instantiate(wallPrefabs[newWallID], spawnPos, hRot);
            allWalls.Add(newWall);
            totalWalls++;
            newWall.transform.SetParent(tile.myTile.transform);
        }

        if (tile.lNeighbour == false)
        {
            int newWallID = Random.Range(0, wallPrefabs.Length);

            wallHeight = wallHeights[newWallID];
            wallWidth = wallWidths[newWallID];

            float moveAmt = ((float)tileSize / 2) - (wallWidth / 2);

            Vector3 spawnPos = new Vector3(tile.myTile.transform.position.x - moveAmt, tile.myTile.transform.position.y + (wallHeight / 2) + (tileHeight / 2), tile.myTile.transform.position.z);
            GameObject newWall = Instantiate(wallPrefabs[newWallID], spawnPos, vRot);
            allWalls.Add(newWall);
            totalWalls++;
            newWall.transform.SetParent(tile.myTile.transform);
        }

        if (tile.rNeighbour == false)
        {
            int newWallID = Random.Range(0, wallPrefabs.Length);

            wallHeight = wallHeights[newWallID];
            wallWidth = wallWidths[newWallID];

            float moveAmt = ((float)tileSize / 2) - (wallWidth / 2);

            Vector3 spawnPos = new Vector3(tile.myTile.transform.position.x + moveAmt, tile.myTile.transform.position.y + (wallHeight / 2) + (tileHeight / 2), tile.myTile.transform.position.z);
            GameObject newWall = Instantiate(wallPrefabs[newWallID], spawnPos, vRot);
            allWalls.Add(newWall);
            totalWalls++;
            newWall.transform.SetParent(tile.myTile.transform);
        }
    }

    // Main function to generate walls.
    void SpawnWalls()
    {
        if (spawnedTiles == null)
        {
            Debug.Log("Easy DG - Log: " + "No reference to tiles, reimport Dungeon.");
            return;
        }

        // Perform following for every tile in current Dungeon.
        foreach (TileItem tile in spawnedTiles)
        {
            UpdateTileInfo(tile);

            if (tile.myScript.neighboursTotal < 4 && generateWalls == true)
            {
                SpawnTileWalls(tile);
            }
        }

        // Start generating doors.
        SpawnDoors();
    }

    // Function to double check door tile and update it.
    // This is used to control the onlySpawnDoorsAtHallEnds behaviour.
    int IsReallyADoor(TileItem tile)
    {
        int doorsHit = 0;

        // If door is horizontal / vertical, check to see if neighboured by any other door tiles.
        if (tile.myScript.horizontalDoor == false)
        {
            RaycastHit rayHitL;
            RaycastHit rayHitR;

            Physics.Raycast(tile.myTile.transform.position + (Vector3.left * tileSize) + (Vector3.up * tileHeight), Vector3.down, out rayHitL, tileHeight * 1);
            Physics.Raycast(tile.myTile.transform.position + (Vector3.right * tileSize) + (Vector3.up * tileHeight), Vector3.down, out rayHitR, tileHeight * 1);

            if (rayHitL.collider.GetComponent<TileScript>() != null)
            {
                if (rayHitL.collider.gameObject.GetComponent<TileScript>().isDoor == true)
                {
                    doorsHit++;
                }
            }
            else
            {
                Debug.Log("Misfire detected");
            }

            if (rayHitR.collider.GetComponent<TileScript>() != null)
            {
                if (rayHitR.collider.gameObject.GetComponent<TileScript>().isDoor == true)
                {
                    doorsHit++;
                }
            }
            else
            {
                Debug.Log("Misfire detected");
            }
        }
        else
        {
            RaycastHit rayHitU;
            RaycastHit rayHitD;

            Physics.Raycast(tile.myTile.transform.position + (Vector3.forward * tileSize) + (Vector3.up * tileHeight), Vector3.down, out rayHitU, tileHeight * 1);
            Physics.Raycast(tile.myTile.transform.position + (Vector3.back * tileSize) + (Vector3.up * tileHeight), Vector3.down, out rayHitD, tileHeight * 1);

            Vector3 newpos = tile.myTile.transform.position;

            if (rayHitU.collider.GetComponent<TileScript>() != null)
            {
                if (rayHitU.collider.GetComponent<TileScript>().isDoor == true)
                {
                    doorsHit++;
                }
            }
            else
            {
                Debug.Log("Misfire detected");
            }

            if (rayHitD.collider.GetComponent<TileScript>() != null)
            {
                if (rayHitD.collider.GetComponent<TileScript>().isDoor == true)
                {
                    doorsHit++;
                }
            }
            else
            {
                Debug.Log("Misfire detected");
            }
        }

        // Returns a total number of doors hit.
        return doorsHit;
    }

    // Function to spawn the door.
    void SpawnDoor(TileItem tile)
    {
        Quaternion rot;

        if (tile.myScript.horizontalDoor == true)
        {
            rot = Quaternion.Euler(Vector3.zero);
        }
        else
        {
            rot = Quaternion.Euler(new Vector3(0, 90, 0));
        }

        int doorPrefabID = Random.Range(0, doorPrefabs.Length);
        doorHeight = doorHeights[doorPrefabID];

        Vector3 spawnPos = new Vector3(tile.myTile.transform.position.x, tile.myTile.transform.position.y + (doorHeight / 2) + (tileHeight / 2), tile.myTile.transform.position.z);

        GameObject newDoor = Instantiate(doorPrefabs[doorPrefabID], spawnPos, rot);
        allDoors.Add(newDoor);
        newDoor.transform.SetParent(tile.myTile.transform);
    }

    // Main function to generate doors.
    void SpawnDoors()
    {
        int groupCounter = 0;
        float startTime = Time.time;

        // List used to remove doors found unsuitable.
        List<TileItem> remDoors = new List<TileItem>();

        // Check doors once, if spawn at hall ends is on and tile is neighboured 
        // by anything other than 1 door tile, then tile is no longer a door.
        foreach (TileItem tile in doorTiles)
        {
            int reallyDoor = IsReallyADoor(tile);

            if (reallyDoor != 1 && onlySpawnDoorsAtHallEnds == true)
            {
                remDoors.Add(tile);
            }
        }

        // Remove non-door tiles from the doorTiles list.
        foreach (TileItem tile in remDoors)
        {
            // We set isDoor false here to ensure check is consistent till it finishes.
            // Else tiles would update during generation and cause results to be off.
            tile.myScript.isDoor = false;
            doorTiles.Remove(tile);
        }

        // Clear list.
        remDoors.Clear();

        foreach (TileItem tile in doorTiles)
        {
            // Repeat check.
            int reallyDoor = IsReallyADoor(tile);

            // Second check ensures you never get two doors next to each other.
            if (reallyDoor > 0 && onlySpawnDoorsAtHallEnds == true)
            {
                tile.myScript.isDoor = false;
                remDoors.Add(tile);
            }
            else
            {
                // Random chance check to see if door spawns.
                int rnd = Random.Range(1, 101);

                if (rnd <= doorSpawnChance)
                {
                    SpawnDoor(tile);
                    groupCounter++;
                }
                else
                {
                    tile.myScript.isDoor = false;
                    remDoors.Add(tile);
                }
            }
        }

        // Clean up lists.
        foreach (TileItem tile in remDoors)
        {
            doorTiles.Remove(tile);
        }

        // Adjust walls to match door placement.
        CheckDoorWalls();
    }

    // Function to perform checks to determine if tile has any diagonal neighbours.
    // I.e. tiles positioned up left, up right, down left, down right of the tile being checked.
    int CheckCorner(TileItem tile)
    {
        int cornersHit = 0;
        tile.neighbourUL = false;
        tile.neighbourUR = false;
        tile.neighbourDL = false;
        tile.neighbourDR = false;

        if (Physics.Raycast(tile.myTile.transform.position + (Vector3.up * tileHeight) + (Vector3.forward * tileSize) + (Vector3.left * tileSize), Vector3.down, tileHeight * 1))
        {
            tile.neighbourUL = true;
            cornersHit++;
        }

        if (Physics.Raycast(tile.myTile.transform.position + (Vector3.up * tileHeight) + (Vector3.forward * tileSize) + (Vector3.right * tileSize), Vector3.down, tileHeight * 1))
        {
            tile.neighbourUR = true;
            cornersHit++;
        }

        if (Physics.Raycast(tile.myTile.transform.position + (Vector3.up * tileHeight) + (Vector3.back * tileSize) + (Vector3.left * tileSize), Vector3.down, tileHeight * 1))
        {
            tile.neighbourDL = true;
            cornersHit++;
        }

        if (Physics.Raycast(tile.myTile.transform.position + (Vector3.up * tileHeight) + (Vector3.back * tileSize) + (Vector3.right * tileSize), Vector3.down, tileHeight * 1))
        {
            tile.neighbourDR = true;
            cornersHit++;
        }

        return cornersHit;
    }

    // Function to spawn the corner prefab.
    void SpawnCorner(TileItem tile)
    {
        // If no diagonal neighbour, also check to ensure tile is really a corner piece.
        // E.g. if up-left is clear of neighbours, check up AND left and only spawn corner if neighbours present.
        // Prevents corner pieces spawning constantly in hallways.
        if (tile.neighbourUL == false)
        {
            if (tile.lNeighbour == true && tile.uNeighbour == true)
            {
                int cornerPrefabID = Random.Range(0, cornerPrefabs.Length);

                cornerWidth = cornerWidths[cornerPrefabID];
                cornerHeight = cornerHeights[cornerPrefabID];

                float moveAmt = (tileSize / 2) - (cornerWidth / 2);

                GameObject newCorner = Instantiate(cornerPrefabs[cornerPrefabID], new Vector3(tile.myTile.transform.position.x - moveAmt, tile.myTile.transform.position.y + (cornerHeight / 2) + (tileHeight / 2), tile.myTile.transform.position.z + moveAmt), Quaternion.identity);
                newCorner.transform.SetParent(tile.myTile.transform);
                allCorners.Add(newCorner);
                totalCorners++;
            }
        }

        if (tile.neighbourUR == false)
        {
            if (tile.rNeighbour == true && tile.uNeighbour == true)
            {
                int cornerPrefabID = Random.Range(0, cornerPrefabs.Length);

                cornerWidth = cornerWidths[cornerPrefabID];
                cornerHeight = cornerHeights[cornerPrefabID];

                float moveAmt = (tileSize / 2) - (cornerWidth / 2);

                GameObject newCorner = Instantiate(cornerPrefabs[cornerPrefabID], new Vector3(tile.myTile.transform.position.x + moveAmt, tile.myTile.transform.position.y + (cornerHeight / 2) + (tileHeight / 2), tile.myTile.transform.position.z + moveAmt), Quaternion.identity);
                newCorner.transform.SetParent(tile.myTile.transform);
                allCorners.Add(newCorner);
                totalCorners++;
            }
        }

        if (tile.neighbourDL == false)
        {
            if (tile.lNeighbour == true && tile.dNeighbour == true)
            {
                int cornerPrefabID = Random.Range(0, cornerPrefabs.Length);

                cornerWidth = cornerWidths[cornerPrefabID];
                cornerHeight = cornerHeights[cornerPrefabID];

                float moveAmt = (tileSize / 2) - (cornerWidth / 2);

                GameObject newCorner = Instantiate(cornerPrefabs[cornerPrefabID], new Vector3(tile.myTile.transform.position.x - moveAmt, tile.myTile.transform.position.y + (cornerHeight / 2) + (tileHeight / 2), tile.myTile.transform.position.z - moveAmt), Quaternion.identity);
                newCorner.transform.SetParent(tile.myTile.transform);
                allCorners.Add(newCorner);
                totalCorners++;
            }
        }

        if (tile.neighbourDR == false)
        {
            if (tile.rNeighbour == true && tile.dNeighbour == true)
            {
                int cornerPrefabID = Random.Range(0, cornerPrefabs.Length);

                cornerWidth = cornerWidths[cornerPrefabID];
                cornerHeight = cornerHeights[cornerPrefabID];

                float moveAmt = (tileSize / 2) - (cornerWidth / 2);

                GameObject newCorner = Instantiate(cornerPrefabs[cornerPrefabID], new Vector3(tile.myTile.transform.position.x + moveAmt, tile.myTile.transform.position.y + (cornerHeight / 2) + (tileHeight / 2), tile.myTile.transform.position.z - moveAmt), Quaternion.identity);
                newCorner.transform.SetParent(tile.myTile.transform);
                allCorners.Add(newCorner);
                totalCorners++;
            }
        }
    }

    // Main function to generate corners.
    void SpawnCorners()
    {
        totalCorners = 0;

        foreach (TileItem tile in spawnedTiles)
        {
            int cornersHit = CheckCorner(tile);

            if (cornersHit != 4 && generateCorners == true)
            {
                SpawnCorner(tile);
            }
        }

        // Finish Dungeon generation.
        FinishGen();
    }

    // Function to double check door tiles and update surrounding walls.
    void CheckDoorWalls()
    {
        // For each remaining door tile.
        foreach (TileItem tile in doorTiles)
        {
            /* 
            Depending on door's rotation (horizontal vs. vertical), we perform a couple of raycasts to try and find the door tile's wall objects.
            Once found, these are updated to have the same Mesh and Material as wallPrefabs[0] to ensure control over what type of wall will spawn near a door.    
            */

            if (tile.myScript.horizontalDoor == true)
            {
                Vector3 doorPos = new Vector3(tile.myTile.transform.position.x, tile.myTile.transform.position.y + tileHeight, tile.myTile.transform.position.z + (tileSize / 4));

                RaycastHit lRay;
                RaycastHit rRay;

                Physics.Raycast(doorPos, Vector3.left, out lRay);
                Physics.Raycast(doorPos, Vector3.right, out rRay);

                if (lRay.collider != null)
                {
                    lRay.collider.GetComponent<MeshFilter>().mesh = wallPrefabs[0].GetComponent<MeshFilter>().sharedMesh;
                    lRay.collider.GetComponent<MeshRenderer>().material = wallPrefabs[0].GetComponent<MeshRenderer>().sharedMaterial;
                }

                if (rRay.collider != null)
                {
                    rRay.collider.GetComponent<MeshFilter>().mesh = wallPrefabs[0].GetComponent<MeshFilter>().sharedMesh;
                    rRay.collider.GetComponent<MeshRenderer>().material = wallPrefabs[0].GetComponent<MeshRenderer>().sharedMaterial;
                }
            }
            else
            {
                Vector3 doorPos = new Vector3(tile.myTile.transform.position.x + (tileSize / 4), tile.myTile.transform.position.y + tileHeight, tile.myTile.transform.position.z);

                RaycastHit uRay;
                RaycastHit dRay;

                Physics.Raycast(doorPos, Vector3.forward, out uRay);
                Physics.Raycast(doorPos, Vector3.back, out dRay);

                if (uRay.collider != null)
                {
                    uRay.collider.GetComponent<MeshFilter>().mesh = wallPrefabs[0].GetComponent<MeshFilter>().sharedMesh;
                    uRay.collider.GetComponent<MeshRenderer>().material = wallPrefabs[0].GetComponent<MeshRenderer>().sharedMaterial;
                }

                if (dRay.collider != null)
                {
                    dRay.collider.GetComponent<MeshFilter>().mesh = wallPrefabs[0].GetComponent<MeshFilter>().sharedMesh;
                    dRay.collider.GetComponent<MeshRenderer>().material = wallPrefabs[0].GetComponent<MeshRenderer>().sharedMaterial;
                }
            }
        }

        // Start spawning corners.
        SpawnCorners();
    }

    void FinishGen()
    {
        // If this was a blank Dungeon, reset variables back to the user's once done.
        // (These are set to 0 / false at start of blank Dungeon generation).
        if (blankDungeonGen == true)
        {
            blankDungeonGen = false;
            generateWalls = genWallsStart;
            doorSpawnChance = doorSpawnStart;
            generateCorners = genCornersStart;
        }

        // Additional debugging information to collect totals of generation:
        /*
        Debug.Log("Easy DG - Log: " +"Dungeon #" + dungeonsSpawned + " generation complete.");
        Debug.Log("Easy DG - Log: " +"Spawned a total of " + spawnedTiles.Count + " tiles.");
        Debug.Log("Easy DG - Log: " +"Spawned a total of " + totalWalls + " walls.");
        Debug.Log("Easy DG - Log: " +"Spawned a total of " + doorTiles.Count + " doors.");
        Debug.Log("Easy DG - Log: " +"Spawned a total of " + totalC32wwwrners + " corners.");
        */

        // Ensure Manual Edit Mode is turned off.
        ToggleManualMode(false);
    }

    // These functions and variables are for the manual edit mode. 
    // I don't recommend calling or changing these yourself unless you're very, very sure you have a good reason to do so.

    private Vector3 manCurrentPos;
    private int manCurrentL;
    private int manCurrentW;
    private bool templatesOut;
    [HideInInspector]
    public List<GameObject> currentTemplates = new List<GameObject>();

    // Function to toggle mode.
    public void ToggleManualMode(bool toggle)
    {
        if (toggle == true)
        {
            if (tileSize == 0 || manualModeTemplatePrefab == null || dungeonParent == null)
            {
                Debug.Log("Easy DG - ERROR: " + "Cannot use manual edit mode - need to have generated a dungeon and set prefabs first!");
                return;
            }

            manCurrentPos = new Vector3(defaultTemplatePosX, 0, defaultTemplatePosZ + tileSize);
            ChangeManualTemplate(1, 1, Vector3.zero);
            manualModeOn = true;
        }
        else
        {
            if (templatesOut == true)
            {
                ClearTemplates();
                manualModeOn = false;
                transform.position = new Vector3(0, 0, 0);
            }
        }
    }

    // Function to delete / clear any templates in scene.
    void ClearTemplates()
    {
        foreach (GameObject template in currentTemplates)
        {
            DestroyImmediate(template.gameObject, false);
        }

        currentTemplates.Clear();
        templatesOut = false;
    }

    // Function to update the Manual Edit Mode template grid.
    public void ChangeManualTemplate(int length, int width, Vector3 direction)
    {
        if (currentTemplates == null)
        {
            Debug.Log("Easy DG - ERROR: " + "Manual Edit Mode interuppted, reimport Dungeon.");
            return;
        }

        manCurrentL = length;
        manCurrentW = width;

        if (currentTemplates.Count > 0)
        {
            ClearTemplates();
        }

        if (direction == Vector3.forward || direction == Vector3.back)
        {
            manCurrentPos += (direction * (manCurrentW * tileSize));
        }
        else if (direction == Vector3.left || direction == Vector3.right)
        {
            manCurrentPos += (direction * (manCurrentL * tileSize));
        }

        Vector3 newPos = manCurrentPos;

        for (int l = 0; l < manCurrentL; l++)
        {
            for (int w = 0; w < manCurrentW; w++)
            {
                newPos = new Vector3(newPos.x, 0, newPos.z - (tileSize));

                GameObject newTempCube = Instantiate(manualModeTemplatePrefab, newPos, Quaternion.identity);

                currentTemplates.Add(newTempCube);
            }

            newPos = new Vector3(newPos.x + (tileSize), 0, manCurrentPos.z);
        }

        templatesOut = true;
        transform.position = currentTemplates[0].transform.position;
    }

    // Function to spawn tiles at template grid positions.
    public void SpawnAtTemplates()
    {
        if (templatesOut == true && manualModeOn == true)
        {
            foreach (GameObject template in currentTemplates)
            {
                if (CheckPosClear(template.transform.position) == true)
                {
                    SpawnTile(template.transform.position);
                }
            }
        }
        else
        {
            Debug.Log("Easy DG - ERROR: " + "Must be in Manual Edit Mode.");
        }
    }

    // Function to delete tiles at template grid positions.
    public void DeleteAtTemplates()
    {
        if (templatesOut == true && manualModeOn == true)
        {
            foreach (GameObject template in currentTemplates)
            {
                if (CheckPosClear(template.transform.position) == false)
                {
                    RaycastHit newHit;

                    Physics.Raycast(template.transform.position + (Vector3.up * tileHeight), Vector3.down, out newHit, tileHeight * 1);

                    List<TileItem> remTiles = new List<TileItem>();

                    if (newHit.collider != null)
                    {
                        foreach (TileItem tile in spawnedTiles)
                        {
                            if (tile.myTile == newHit.collider.gameObject)
                            {
                                remTiles.Add(tile);
                            }
                        }

                        foreach (TileItem remTile in remTiles)
                        {
                            spawnedTiles.Remove(remTile);
                        }

                        DestroyImmediate(newHit.collider.gameObject, false);
                    }
                }
            }
        }
        else
        {
            Debug.Log("Easy DG - ERROR: " + "Must be in Manual Edit Mode.");
        }
    }

    public void DeleteTileAtPos(Vector3 pos)
    {
        if (CheckPosClear(pos) == false)
        {
            RaycastHit newHit;

            Physics.Raycast(pos + (Vector3.up * tileHeight), Vector3.down, out newHit, tileHeight * 1);

            List<TileItem> remTiles = new List<TileItem>();

            if (newHit.collider != null)
            {
                foreach (TileItem tile in spawnedTiles)
                {
                    if (tile.myTile == newHit.collider.gameObject)
                    {
                        remTiles.Add(tile);
                    }
                }

                foreach (TileItem remTile in remTiles)
                {
                    spawnedTiles.Remove(remTile);
                }

                DestroyImmediate(newHit.collider.gameObject, false);
            }
        }
        else
        {
            Debug.Log("Easy DG - ERROR: " + "No tile found to delete.");
        }
    }

    // Function to finalise Dungeon.
    // Simply deletes all prefabs and starts generation process from SpawnWalls.
    public void FinaliseDungeon()
    {
        ClearDungeon();

        // Clear all relevent lists before finalising, to prevent duplication and deleted tiles from being checked.

        if (allWalls == null)
        {
            allWalls = new List<GameObject>();
        }
        else
        {
            allWalls.Clear();
        }

        if (allDoors == null)
        {
            allDoors = new List<GameObject>();
        }
        else
        {
            allDoors.Clear();
        }

        if (allCorners == null)
        {
            allCorners = new List<GameObject>();
        }
        else
        {
            allCorners.Clear();
        }

        if (doorTiles == null)
        {
            doorTiles = new List<TileItem>();
        }
        else
        {
            doorTiles.Clear();
        }

        SpawnWalls();
    }

    // Uses the allPrefabs lists attached to the current Dungeon to delete all wall, door and corner prefabs.
    public void ClearDungeon()
    {
        foreach (GameObject wall in allWalls)
        {
            DestroyImmediate(wall.gameObject, false);
        }

        allWalls.Clear();

        foreach (GameObject door in allDoors)
        {
            DestroyImmediate(door.gameObject, false);
        }

        allDoors.Clear();

        foreach (GameObject corner in allCorners)
        {
            DestroyImmediate(corner.gameObject, false);
        }

        allCorners.Clear();
    }

    // Function to import Dungeon.
    public void ImportDungeon()
    {
        if (dungeonParent == null)
        {
            // Uses name to find import.
            GameObject importDungeon = GameObject.Find("Import");

            if (importDungeon != null)
            {
                GenerateBlankDungeon();

                TileScript[] importTiles = importDungeon.GetComponentsInChildren<TileScript>();

                if (importTiles.Length > 0)
                {
                    foreach (TileScript tile in importTiles)
                    {
                        SpawnTile(tile.gameObject.transform.position);
                    }

                    DestroyImmediate(importDungeon.gameObject, false);
                    dungeonParent.name = "(Imported) " + dungeonParent.name;

                    Debug.Log("Easy DG - Log: " + "Dungeon imported successfully.");
                    Debug.Log("Easy DG - " + "Imported Dungeon saved as: " + dungeonParent.name);
                }
                else
                {
                    Debug.Log("Easy DG - ERROR: " + "No tiles found to import, are you trying to import the correct Dungeon?");
                }

            }
            else
            {
                Debug.Log("Easy DG - ERROR: " + "Dungeon: 'Import' not found in scene.");
            }
        }
        else
        {
            Debug.Log("Easy DG - ERROR: " + "There is a generated dungeon in scene, please delete.");
        }
    }

    // Function to call when you want to generate a dungeon via script.
    public void EasyGen(int lines, int tiles, int dirChance, int doorChance, int roomChance, bool allowPar, bool genCon, bool genRooms,
        bool genWalls, bool genCorners, bool hallEnds, int minRoomL, int maxRoomL, int minRoomW, int maxRoomW, int distEOL, int distFromLast)
    {
        deleteDungeonOnNew = true;
        totalLines = lines;
        maxTilesPerLine = tiles;
        changeDirChance = dirChance;
        doorSpawnChance = doorChance;
        roomSpawnChance = roomChance;
        allowParallelLinesToTouch = allowPar;
        generateContinuous = genCon;
        generateRooms = genRooms;
        generateWalls = genWalls;
        generateCorners = genCorners;
        onlySpawnDoorsAtHallEnds = hallEnds;
        minRoomLength = minRoomL;
        maxRoomLength = maxRoomL;
        minRoomWidth = minRoomW;
        maxRoomWidth = maxRoomW;
        roomMinDistanceEndOfLine = distEOL;
        roomMinDistanceFromLast = distFromLast;

        PluginCheck();
    }

    // TileItem class, used mostly to control wall, door and corner placement.
    // A new instance of this class is created each time a tile is generated.
    // Stores a reference to the the tile object itself, the TileScript attached to it and the tile's neighbour info.
    public class TileItem
    {
        public GameObject myTile;
        public TileScript myScript;

        public bool uNeighbour;
        public bool dNeighbour;
        public bool lNeighbour;
        public bool rNeighbour;

        public bool neighbourUL;
        public bool neighbourUR;
        public bool neighbourDL;
        public bool neighbourDR;
    }

    string[] templateOptionsFSM = new string[] { "Few", "Some", "Many" };       //objects
    string[] templateOptionsLMH = new string[] { "Low", "Medium", "High" };     //room density
    string[] templateOptionsSML = new string[] { "Small", "Medium", "Large" };  //overall size

    public void SetLMH(float LMH)
    {
        switch ((int)LMH)
        {
            case 0:
                totalLines = 6;
                maxTilesPerLine = 40;
                changeDirChance = 20;
                allowParallelLinesToTouch = false;
                generateContinuous = true;
                break;
            case 1:
                totalLines = 8;
                maxTilesPerLine = 50;
                changeDirChance = 35;
                allowParallelLinesToTouch = false;
                generateContinuous = false;
                break;
            case 2:
                totalLines = 10;
                maxTilesPerLine = 70;
                changeDirChance = 50;
                allowParallelLinesToTouch = true;
                generateContinuous = false;
                break;
        }

        /*
        switch ((int)LMH)
        {
            case 0:
                totalLines += 2;
                maxTilesPerLine += 15;
                break;
            case 1:
                totalLines += 4;
                maxTilesPerLine += 30;
                break;
            case 2:
                totalLines += 6;
                maxTilesPerLine += 60;
                break;
        }
        */
    }

    public void SetSML(float SML)
    {
        switch ((int)SML)
        {
            case 0:
                roomSpawnChance = 75;
                roomMinDistanceEndOfLine = 15;
                roomMinDistanceFromLast = 25;
                break;
            case 1:
                roomSpawnChance = 90;
                roomMinDistanceEndOfLine = 10;
                roomMinDistanceFromLast = 20;
                break;

            case 2:
                roomSpawnChance = 130;
                roomMinDistanceEndOfLine += 6;
                roomMinDistanceFromLast += 11;
                break;
        }

        switch ((int)SML)
        {
            case 0:
                minRoomLength = 4;
                maxRoomLength = 8;
                minRoomWidth = 3;
                maxRoomWidth = 5;
                break;
            case 1:
                minRoomLength = 4;
                maxRoomLength = 10;
                minRoomWidth = 4;
                maxRoomWidth = 6;
                break;

            case 2:
                minRoomLength = 6;
                maxRoomLength = 12;
                minRoomWidth = 5;
                maxRoomWidth = 9;
                break;
        }
    }

}