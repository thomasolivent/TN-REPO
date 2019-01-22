using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerModePlugin : MonoBehaviour
{
    [SerializeField]
    private GameObject fillerPrefab;

    [Tooltip("If TRUE, this will add an additional layer on top of the dungeon, to 'cap' it off.\n\nIf FALSE, top layer will be left exposed.")]
    [SerializeField]
    private bool capTopFloor;

    public void FillAsTower()
    {
        DungeonGenerator dGen = GetComponent<DungeonGenerator>();
        List<Vector3> allTilePos = new List<Vector3>();

        float minX = 0;
        float maxX = 0;
        float minZ = 0;
        float maxZ = 0;
        float minY = 0;
        float maxY = 0;

        float tileHeight = dGen.GetDungeonHeight();

        foreach (DungeonGenerator.TileItem tile in dGen.spawnedTiles)
        {
            Vector3 newPos = tile.myTile.gameObject.transform.position;
            allTilePos.Add(newPos);

            if (newPos.x < minX) minX = newPos.x;
            if (newPos.x > maxX) maxX = newPos.x;

            if (newPos.z < minZ) minZ = newPos.z;
            if (newPos.z > maxZ) maxZ = newPos.z;

            if (newPos.y < minY) minY = newPos.y;
            if (newPos.y > maxY) maxY = newPos.y;
        }

        minX -= dGen.tileSize;
        minZ -= dGen.tileSize;
        Vector3 spawnPos = new Vector3(minX, minY, minZ);
        
        if (capTopFloor) maxY += tileHeight;

        while (spawnPos.y <= (maxY))
        {
            while (spawnPos.z <= (maxZ + (dGen.tileSize * 2)))
            {
                while (spawnPos.x <= (maxX + (dGen.tileSize * 2)))
                {
                    bool blocked = false;
                    foreach (Vector3 p in allTilePos)
                    {
                        if (Vector3.Distance(p, spawnPos) < 0.01f)
                        {
                            blocked = true;
                        }
                    }

                    if (blocked == false)
                    {
                        GameObject newFill = Instantiate(fillerPrefab, spawnPos + (Vector3.up * (tileHeight / 2)), Quaternion.identity);

                        GameObject newFloor = Instantiate(dGen.tilePrefab, spawnPos, Quaternion.identity);
                        newFloor.transform.SetParent(dGen.dungeonParent.transform);
                        newFloor.GetComponent<BoxCollider>().enabled = false;

                        newFill.transform.SetParent(dGen.dungeonParent.transform, false);
                        newFill.transform.localScale = new Vector3(dGen.tileSize, tileHeight, dGen.tileSize);
                    }

                    spawnPos.x += dGen.tileSize;
                }

                spawnPos.x = minX;
                spawnPos.z += dGen.tileSize;
            }

            spawnPos.x = minX;
            spawnPos.z = minZ;
            spawnPos.y += tileHeight;
        }
    }

    private bool Compare(float v1, float v2)
    {
        bool isClose = false;

        if (v1 < (v2 + 0.01f) && v1 > (v2 - 0.01f))
        {
            isClose = true;
        }

        return isClose;
    }
}
