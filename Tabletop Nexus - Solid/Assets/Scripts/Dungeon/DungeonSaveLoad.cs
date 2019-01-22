using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Vuforia;

public class DungeonSaveLoad : MonoBehaviour
{
    [HideInInspector]
    public DataSet theDataSet;
    [SerializeField]
    public UDTEventHandler udtEventHandler;

    [SerializeField]
    DungeonGenerator dGen;
    [SerializeField]
    PropPlacement pPlace;
    [SerializeField]
    GameObject dSaveButtonPrefab;
    [SerializeField]
    Transform imageTarget;
    [SerializeField]
    InputField nameField;

    //dungeon buttons
    [SerializeField]
    public Button place, edit, delete;

    public List<Vector3> tileSpawnPositions;
    public Dictionary<Transform, int> propDict;
    public List<GameObject> propsToSync = new List<GameObject>();

    [SerializeField]
    private NetworkManager networkManager;

    [HideInInspector]
    public GameObject dun;

    List<GameObject> loadButtons = new List<GameObject>();

    public List<GameObject> dungeons = new List<GameObject>();

    private string selectedLoadFile;

    GameObject TargetToSetAsParent;

    string saveFileExtension = ".edg";

    public string SelectedLoadFile
    {
        get
        {
            return selectedLoadFile;
        }

        set
        {
            selectedLoadFile = value;

            if (value == null)
            {
                place.interactable = false;
                edit.interactable = false;
                delete.interactable = false;
            }
            else
            {
                place.interactable = true;
                edit.interactable = true;
                delete.interactable = true;
            }
        }
    }

    void Start()
    {
        SelectedLoadFile = null;
    }

    public void SaveDungeon(string saveName)
    {
        Debug.Log(saveName);
        Prop[] props = FindObjectsOfType<Prop>();
        Dictionary<Transform, int> dungeonProps = new Dictionary<Transform, int>();

        foreach(Prop p in props)
        {
            if (p.placed == true)
            {
                dungeonProps.Add(p.transform, p.identifier);
            }
        }

        TileScript[] allTiles = FindObjectsOfType<TileScript>();
        List<Vector3> tilePositions = new List<Vector3>();

        foreach (TileScript t in allTiles)
        {
            tilePositions.Add(t.gameObject.transform.position);
        }

        using (ES2Writer writer = ES2Writer.Create("dungeonSaves/" + saveName + saveFileExtension))
        {
            writer.Write(dungeonProps, "dungeonProps");
            writer.Write(tilePositions, "tilePositions");
            writer.Save();
        }
    }

    public void SelectDungeon(string dName)
    {
        SelectedLoadFile = dName;
    }

    public void PlaceDungeonAR()
    {
        if ((!PhotonNetwork.connected) || (PhotonNetwork.connected && PhotonNetwork.isMasterClient))
        {
            tileSpawnPositions = GetDungeonData();
            propDict = GetDungeonProps();
            BuildDungeon(tileSpawnPositions);
        }

        if(PhotonNetwork.connected && !PhotonNetwork.isMasterClient)
        {
            //BuildDungeon(tileSpawnPositions);  //commented 1/5/2019
            PlaceDungeonARSpecific();
        }
    }

    public void PlaceDungeonARSpecific()
    {
        if(PhotonNetwork.connectedAndReady && !PhotonNetwork.isMasterClient)
        {
            //tilesspawnpositions is set in udteventhandler
            tileSpawnPositions = networkManager.targetsAndChildren.ElementAt(0).Value;
        }
        else
        {
            tileSpawnPositions = GetDungeonData();
        }

        //possibly remove if statement before testing sync state
        if(PhotonNetwork.connectedAndReady && PhotonNetwork.isMasterClient)
        {

        }
        else
        {
            if (dGen.dungeonParent != null)
            {
                dGen.DeleteDungeon();
                dGen.ResetDungeonCount();
                dGen.spawnedTiles.Clear();
            }

            dGen.GenerateBlankDungeon();
            string loadName = SelectedLoadFile + ".edg";
            dGen.dungeonParent.name = loadName.Substring(0, loadName.Length - 4);

            Debug.Log(networkManager.targetsAndChildren.Count + " entries in the dictionary.");
            Debug.Log(tileSpawnPositions.Count + " entries in the tilespawnpositions list.");
            if (tileSpawnPositions.Count > 0)
            {
                foreach (Vector3 v in tileSpawnPositions)
                {
                    dGen.SpawnTile(new Vector3(v.x - 1000, v.y, v.z));
                }
            }
            else
            {
                Debug.Log("File loaded was empty.");
            }

            dGen.FinaliseDungeon();

            dun = FindObjectOfType<TileScript>().transform.parent.gameObject;

            dun.transform.position = new Vector3(0, 0, 0);

            foreach(GameObject t in udtEventHandler.targets)
            {
                if(t.name == udtEventHandler.SelectedTarget)
                {
                    TargetToSetAsParent = t;
                }
            }

            if(TargetToSetAsParent == null)
            {
                TargetToSetAsParent = udtEventHandler.ImageTargetTemplate.gameObject;
            }

            dun.transform.parent = TargetToSetAsParent.transform;
            dun.tag = "Dungeon";

        }
    }

    public List<Vector3> GetDungeonData()
    {
        string loadName = SelectedLoadFile + ".edg";
        List<Vector3> tileSpawnPositions = new List<Vector3>();
        using (ES2Reader reader = ES2Reader.Create("dungeonSaves/" + loadName))
        {
            tileSpawnPositions = reader.ReadList<Vector3>("tilePositions");
        }
        return tileSpawnPositions;
    }

    public Dictionary<Transform, int> GetDungeonProps()
    {
        string loadName = selectedLoadFile + ".edg";
        Dictionary<Transform, int> dungeonProps = new Dictionary<Transform, int>();
        using (ES2Reader reader = ES2Reader.Create("dungeonSaves/" + loadName))
        {
            dungeonProps = reader.ReadDictionary<Transform, int>("dungeonProps");
        }
        return dungeonProps;
    }

    public void BuildDungeon(List<Vector3> tileSpawnPositions)
    {
        if (dGen.dungeonParent != null)
        {
            dGen.DeleteDungeon();
            dGen.ResetDungeonCount();
            dGen.spawnedTiles.Clear();
        }

        dGen.GenerateBlankDungeon();
        string loadName = SelectedLoadFile + ".edg";
        dGen.dungeonParent.name = loadName.Substring(0, loadName.Length - 4);

        if (tileSpawnPositions.Count > 0)
        {
            foreach (Vector3 v in tileSpawnPositions)
            {
                dGen.SpawnTile(new Vector3(v.x - 1000, v.y, v.z));
            }
        }
        else
        {
            Debug.Log("File loaded was empty.");
        }

        dGen.FinaliseDungeon();

        dun = FindObjectOfType<TileScript>().transform.parent.gameObject;

        dun.transform.position = new Vector3(0, 0, 0);

        dun.transform.parent = imageTarget.transform;
        dun.tag = "Dungeon";

        dGen.spawnedTiles.Clear();

        foreach(KeyValuePair<Transform, int> entry in propDict)
        {
            foreach (GameObject go in pPlace.propList)
            {
                if (entry.Value == go.GetComponent<Prop>().identifier)
                {
                    var propPos = entry.Key.position;
                    var propRot = entry.Key.rotation;
                    if (PhotonNetwork.connectedAndReady)
                    {
                        GameObject newProp = PhotonNetwork.Instantiate(pPlace.propList[go.GetComponent<Prop>().identifier].name, propPos, propRot, 0);
                        newProp.GetComponent<Prop>().placed = true;
                    }
                    else
                    {
                        GameObject newProp = Instantiate(go, new Vector3(propPos.x - 1000, propPos.y, propPos.z), new Quaternion(propRot.x, propRot.y, propRot.z, propRot.w), dun.transform);
                        newProp.GetComponent<Prop>().placed = true;
                        propsToSync.Add(newProp);
                        Debug.Log(newProp.name);
                    }
                }
            }
        }

    }

    public void AddDungeonToList()
    {
        dun = FindObjectOfType<TileScript>().transform.parent.gameObject;
        dungeons.Add(dun);
    }

    public void BuildDungeonForEdit()
    {
        Dictionary<Transform, int> propsForEditDerpDerp = GetDungeonProps();
        List<Vector3> tileSpawnPositions = GetDungeonData();

        if (dGen.dungeonParent != null)
        {
            dGen.DeleteDungeon();
            dGen.ResetDungeonCount();
            dGen.spawnedTiles.Clear();
        }

        dGen.GenerateBlankDungeon();
        string loadName = SelectedLoadFile + ".edg";
        dGen.dungeonParent.name = loadName.Substring(0, loadName.Length - 4);

        if (tileSpawnPositions.Count > 0)
        {
            foreach (Vector3 v in tileSpawnPositions)
            {
                dGen.SpawnTile(new Vector3(v.x, v.y, v.z));
            }
        }
        else
        {
            Debug.Log("File loaded was empty.");
        }

        dGen.FinaliseDungeon();

        dun = FindObjectOfType<TileScript>().transform.parent.gameObject;
        dun.transform.position = new Vector3(0, 0, 0);
        nameField.text = SelectedLoadFile;

        foreach (KeyValuePair<Transform, int> entry in propsForEditDerpDerp)
        {
            foreach (GameObject go in pPlace.propList)
            {
                if (entry.Value == go.GetComponent<Prop>().identifier)
                {
                    var propPos = entry.Key.position;
                    var propRot = entry.Key.rotation;
                    GameObject newProp = Instantiate(go, new Vector3(propPos.x, propPos.y, propPos.z), new Quaternion(propRot.x, propRot.y, propRot.z, propRot.w), dun.transform);
                    newProp.GetComponent<Prop>().placed = true;
                }
            }
        }

    }

    public void GetAllSavedDungeons()
    {
        foreach (GameObject go in loadButtons)
        {
            Destroy(go.gameObject);
        }

        loadButtons.Clear();

        if (!ES2.Exists("dungeonSaves/"))
        {
            Debug.Log("dungeonSaves Not Found");
            ES2.Save<int>(0, "dungeonSaves/init/fin.dat");
        }

        string[] filesInFolder = ES2.GetFiles("dungeonSaves/");

        if (filesInFolder == null)
        {
            Debug.Log("No saved dungeons.");
            return;
        }

        foreach (string file in filesInFolder)
        {
            string fileExt = file.Substring(file.Length - 4, 4);
            if (fileExt == saveFileExtension)
            {
                GameObject newButton = Instantiate(dSaveButtonPrefab);
                newButton.transform.SetParent(dSaveButtonPrefab.transform.parent, false);
                newButton.name = "LOAD-BUTTON-" + file;

                newButton.GetComponent<DungeonLoadButton>().SetupButton(Path.GetFileNameWithoutExtension(Application.persistentDataPath + "/dungeonSaves/" + file));
                loadButtons.Add(newButton);
                newButton.SetActive(true);
            }
        }
    }

    public void DestroyDungeonFile()
    {
        File.Delete(Application.persistentDataPath + "/dungeonSaves/" + SelectedLoadFile + ".edg");
        SelectedLoadFile = null;
        GetAllSavedDungeons();
    }

    public void RemoveDungeonFromPlay()
    {
        List<GameObject> temp = new List<GameObject>();

        if (PhotonNetwork.connected && PhotonNetwork.isMasterClient) //if connected and master client, deletes specific selected dungeon
        {
            foreach (GameObject d in dungeons)
            {
                if (d.name == selectedLoadFile)
                {
                    temp.Add(d);
                }
            }

            foreach (GameObject d in temp)
            {
                dungeons.Remove(d);
                Destroy(d);
            }
        }
        else if (!PhotonNetwork.connected || (PhotonNetwork.connected && !PhotonNetwork.isMasterClient))
        {
            if (PhotonNetwork.connected && !PhotonNetwork.isMasterClient) //if connected, deletes all dungeons
            {
                foreach (GameObject d in dungeons)
                {
                    temp.Add(d);
                }

                foreach (GameObject d in temp)
                {
                    dungeons.Remove(d);
                    Destroy(d);
                }
            }
            else
            { //remove selected dungeon in offline mode
                foreach (GameObject d in dungeons)
                {
                    if (d.name == selectedLoadFile)
                    {
                        temp.Add(d);
                    }
                }

                foreach (GameObject d in temp)
                {
                    dungeons.Remove(d);
                    Destroy(d);
                }
            }
        }
    }
}

//CONSIDER: maybe add new dungeons to non masterclient clients list