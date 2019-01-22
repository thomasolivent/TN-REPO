using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class MultiStoryPlugin : MonoBehaviour {

    private DungeonGenerator dGen;

    [Header("Multi-Story controls:")]
    [Tooltip("How many floors will the dungeon have. Top floor is 1, bottom is 2, then goes up.\n\n E.g. if you generate 4 floors, the top floor will be 1, then downwards it goes 4, 3, 2")]
    [SerializeField]
    private int totalFloors;

    [Tooltip("How many holes will the dungeon have per floor.\n\nClamped to prevent abuse, you can increase these values if you're happy you know what you're doing. Higher values can cause the process to abort if the generation is taking longer than it should, if you get errors, try reducing this value.")]
    [Range(0, 25)]
    [SerializeField]
    private int maxHolesPerFloor;

    [Tooltip("Prefab for the hole tile, replaces the floor tile when a hole is found.")]
    [SerializeField]
    private GameObject holeTilePrefab;

    private GameObject parent;
    private GameObject[] children;

    public void MultiStoryGen()
    {
        if (parent != null)
        {
            DestroyImmediate(parent.gameObject, false);
        }

        if (dGen == null)
        {
            dGen = GetComponent<DungeonGenerator>();

            if (dGen == null)
            {
                Debug.Log("Easy DG - Plugin (Multi-Story): " + "DungeonGenerator component not found.");
                return;
            }
        }

        float storyHeight = dGen.GetDungeonHeight();
        dGen.clearTileList = false;
        bool deleteOriginal = dGen.deleteDungeonOnNew;
        dGen.deleteDungeonOnNew = false;

        int reverseCounter = totalFloors - 1;
        children = new GameObject[reverseCounter];

        for (int i = 0; i < totalFloors; i++)
        {
            dGen.GenDungeon();

            if (i == 0)
            {
                parent = dGen.dungeonParent;
            }
            else
            {
                children[reverseCounter] = dGen.dungeonParent;
            }

            dGen.dungeonParent.gameObject.transform.position += ((Vector3.down * storyHeight) * (reverseCounter));
            reverseCounter--;
        }

        foreach (GameObject c in children)
        {
            c.transform.SetParent(parent.transform, true);
        }

        FindHoles();

        dGen.clearTileList = true;
        dGen.deleteDungeonOnNew = deleteOriginal;
    }

    private void FindHoles()
    {
        List<DungeonGenerator.TileItem> delTiles = new List<DungeonGenerator.TileItem>();

        // Break counter to prevent infinite loop.
        int breakCounter = 0;
        int breakMax = 10000;

        float dHeight = dGen.GetDungeonHeight();

        DungeonGenerator.TileItem newTile;

        for (int ii = 1; ii <= maxHolesPerFloor; ii++)
        {
            for (int i = 0; i < totalFloors-1; i++)
            {
                bool holeFound = false;

                while (holeFound == false)
                {
                    breakCounter++;
                    if (breakCounter > breakMax)
                    {
                        Debug.Log("Easy DG - Plugin (Multi-Story): " + "Hole check break point hit - try reducing the number of holes.");
                        break;
                    }

                    newTile = dGen.spawnedTiles[Random.Range(0, dGen.spawnedTiles.Count)];

                    float tileY = newTile.myTile.transform.position.y;

                    if (tileY < (-(dHeight * i) + 0.01f) && tileY > (-(dHeight * i) - 0.01f))
                    {
                        if (Physics.Raycast(newTile.myTile.transform.position + (Vector3.down * (dHeight / 2)), Vector3.down, dHeight))
                        {
                            GameObject newTileObject = Instantiate(holeTilePrefab, newTile.myTile.transform.position, Quaternion.identity);

                            DungeonGenerator.TileItem ti = new DungeonGenerator.TileItem();
                            ti.myTile = newTileObject;
                            ti.myScript = ti.myTile.GetComponent<TileScript>();
                            ti.myScript.isDoor = false;
                            ti.myScript.horizontalDoor = true;
                            ti.myScript.neighboursTotal = 0;
                            dGen.spawnedTiles.Add(ti);
                            ti.myTile.transform.SetParent(newTile.myTile.transform.parent);

                            delTiles.Add(newTile);
                            holeFound = true;
                            breakCounter = 0;
                        }
                    }
                }
            }
        }

        dGen.FinaliseDungeon();

        foreach(DungeonGenerator.TileItem dt in delTiles)
        {
            dGen.spawnedTiles.Remove(dt);
            DestroyImmediate(dt.myTile.gameObject, false);
        }
    }
}