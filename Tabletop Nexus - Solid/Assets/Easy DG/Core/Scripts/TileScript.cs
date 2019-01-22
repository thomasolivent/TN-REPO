using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class TileScript : MonoBehaviour {

    [Tooltip("Set to TRUE if you want this tile prefab ignored as a possible location for a door.")]
    public bool ignoreForDoorChecks;

    // Variables to store tile info for DungeonGenerator script.
    [HideInInspector]
    public bool isDoor;
    [HideInInspector]
    public int neighboursTotal;
    [HideInInspector]
    public bool horizontalDoor;

    // Finds and stores a reference to the DungeonGenerator script instance in the scene.
    // Required for the TileEditor script to function.
    [HideInInspector]
    public DungeonGenerator dungeonGeneratorInstance;
    
    void Start()
    {
        dungeonGeneratorInstance = GameObject.FindObjectOfType<DungeonGenerator>();
    }
}
