using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class DungeonEditor : MonoBehaviour {

    [SerializeField]
    PropPlacement pPlace;

    // Var to hold DungeonGenerator in scene.
    [SerializeField]
    private DungeonGenerator dGen;

    [SerializeField]
    private GameObject dungeonCamera;

    // Var to hold DungeonSaveLoad in scene.
    [SerializeField]
    private DungeonSaveLoad dSave;

    // Var to hold dungeon name text input field.
    [SerializeField]
    private InputField nameInputField;
    private string nameInputString;

    // Var to track template object in scene.
    [HideInInspector]
    public GameObject currentTemplate;
    // Var for tracking position of template in scene.
    private Vector3 currentTemplatePos;

    // Array of move directions, just makes life easier 
    // to use this when moving template via UI buttons.
    // 0 = Forward. 1 = Back. 2 = Left. 3 = Right.
    private Vector3[] moveDirs = new Vector3[] { Vector3.forward, Vector3.back, Vector3.left, Vector3.right };

    private bool isEditModeOn = false;

    private void Update()
    {
        if (!isEditModeOn) return;

        if (Input.GetKeyDown("w"))
        {
            MoveTemplate(0);
        }
        if (Input.GetKeyDown("a"))
        {
            MoveTemplate(2);
        }
        if (Input.GetKeyDown("s"))
        {
            MoveTemplate(1);
        }
        if (Input.GetKeyDown("d"))
        {
            MoveTemplate(3);
        }

        if (Input.GetKeyDown("space"))
        {
            PlaceTile();
        }

        if (Input.GetKeyDown("enter"))
        {
            SaveDungeon();
        }

        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            RaycastHit hit;
            Ray ray = dungeonCamera.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 1000f) && hit.collider.gameObject.tag == "Prop" && hit.collider.gameObject.GetComponent<Prop>().placed == true)
            {
                //Destroy(hit.collider.gameObject);
                pPlace.selectedProp = hit.collider.gameObject;
                hit.collider.gameObject.GetComponent<Prop>().selected = true;
            }
        }
        //do a ray from camera, if hit and prop, check if placed, if placed, set to selectedProp, then call deleteprop
    }

    // All functions can be called from UI.
    // Avoiding using parameters that aren't basic types.

    // Called when starting a new dungeon in edit mode.
    public void StartNewDungeon()
    {
        
        // Check to ensure we start with fresh dungeon.
        if (dGen.dungeonParent != null)
        {
            Destroy(dGen.dungeonParent.gameObject);
            dGen.spawnedTiles.Clear();
        }

        // Create blank dungeon for editing.
        dGen.GenerateBlankDungeon();

    }

    // Secondary function, called by script.
    public void EditModeOn()
    {
        isEditModeOn = true;
        
        // Check to ensure template ready for use.
        if (currentTemplate != null) Destroy(currentTemplate.gameObject);

        // Reset position.
        currentTemplatePos = new Vector3(1000, 0, 0);

        // Create template object in scene.
        currentTemplate = Instantiate(dGen.manualModeTemplatePrefab, currentTemplatePos, Quaternion.identity);

    }

    // Secondary function, called by script.
    public void EditModeOff()
    {
        isEditModeOn = false;
        // Destroy the template, reset the position.
        Destroy(currentTemplate.gameObject);
        //currentTemplatePos = Vector3.zero;
    }

    // Move the template, pass it an INT to match up with moveDirs array.
    public void MoveTemplate(int dirID)
    {
        if (currentTemplate == null)
        {
            Debug.Log("LOGIC ERROR - MoveTemplate called without currentTemplate defined.");
            return;
        }

        currentTemplate.transform.position += (moveDirs[dirID] * dGen.tileSize);

        
        dungeonCamera.transform.position = new Vector3(currentTemplate.transform.position.x, 
                                                        currentTemplate.transform.position.y + 20f, 
                                                        currentTemplate.transform.position.z);
                                                        
    }

    // Repos the dungeon, pass it an INT to match up with moveDirs array.
    public void MoveDungeon(int dirID)
    {
        if (dGen.dungeonParent == null)
        {
            Debug.Log("LOGIC ERROR - MoveDungeon called without dungeonParent defined.");
            return;
        }

        dGen.dungeonParent.gameObject.transform.position += (moveDirs[dirID] * dGen.tileSize);
    }

    // Place a tile at template position.
    public void PlaceTile()
    {
        // Logic error catching:

        if (currentTemplate == null)
        {
            Debug.Log("PlaceTile called without currentTemplate defined.");
            return;
        }

        if (dGen.dungeonParent == null)
        {
            Debug.Log("PlaceTile called without dungeonParent defined in DG.");
            return;
        }

        // Turn off template's collider to ensure pos clear.
        if (currentTemplate.GetComponent<Collider>()) currentTemplate.GetComponent<Collider>().enabled = false;

        // Check position to ensure no obstructions.
        if (dGen.CheckPosClear(currentTemplate.transform.position))
        {
            // If clear, create tile via DG.
            dGen.SpawnTile(new Vector3(currentTemplate.transform.position.x - 1000, currentTemplate.transform.position.y, currentTemplate.transform.position.z));   //Thomas: I don't know what's going on, but this seems to work.
        }

        // Turn collider back on.
        if (currentTemplate.GetComponent<Collider>()) currentTemplate.GetComponent<Collider>().enabled = true;
    }

    public void DeleteTile()
    {
        // Logic error catching:

        if (currentTemplate == null)
        {
            Debug.Log("DeleteTile called without currentTemplate defined.");
            return;
        }

        if (dGen.dungeonParent == null)
        {
            Debug.Log("DeleteTile called without dungeonParent defined in DG.");
            return;
        }

        // Turn off template's collider to ensure pos clear.
        if (currentTemplate.GetComponent<Collider>()) currentTemplate.GetComponent<Collider>().enabled = false;

        dGen.DeleteTileAtPos(currentTemplate.transform.position);

        // Turn collider back on.
        if (currentTemplate.GetComponent<Collider>()) currentTemplate.GetComponent<Collider>().enabled = true;
    }

    // End editing function, save the dungeon, reset edit mode, etc.
    public void SaveDungeon()
    {
        // Get the user's dungeon name.
        string newName = nameInputField.text;
        if (newName == "")
        {
            Debug.Log("Enter a name!");
            return;
        }

        if (newName == null || newName == "") newName = dGen.dungeonParent.gameObject.name;
        dGen.dungeonName = newName;
        dGen.dungeonParent.gameObject.name = newName;

        dSave.SaveDungeon(dGen.dungeonParent.name);
        EditModeOff();
    }
}