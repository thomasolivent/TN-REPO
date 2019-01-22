using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UMA;
using UMA.CharacterSystem;
using UMA.CharacterSystem.Examples;

public class CharacterCreator : MonoBehaviour
{

    [SerializeField]
    InputField characterName;
    [SerializeField]
    DynamicCharacterAvatar avatar;
    [SerializeField]
    DynamicRaceLibrary dynamicRaceLibrary;
    [SerializeField]
    GameObject cSaveButtonPrefab;
    [SerializeField]
    GameObject RaceButtonPrefab;
    [SerializeField]
    GameObject loadedCharacterPrefab;
    [SerializeField]
    public Transform imageTarget;
    [SerializeField]
    DungeonSaveLoad dSLscript;
    [SerializeField]
    InputField nameField;

    [SerializeField]
    GameObject SlotPrefab;
    [SerializeField]
    GameObject WardrobePrefab;
    [SerializeField]
    GameObject SlotPanel;
    [SerializeField]
    GameObject WardrobePanel;
    [SerializeField]
    GameObject ColorPrefab;
    [SerializeField]
    GameObject DnaPrefab;
    [SerializeField]
    GameObject LabelPrefab;
    [SerializeField]
    SharedColorTable HairColor;
    [SerializeField]
    SharedColorTable SkinColor;
    [SerializeField]
    SharedColorTable EyesColor;
    [SerializeField]
    SharedColorTable ClothingColor;

    GameObject loadedCharacter;

    List<GameObject> loadButtons = new List<GameObject>();
    List<GameObject> raceButtons = new List<GameObject>();

    [SerializeField]
    Button place, edit, delete;

    private string selectedCharacterName;
    public string Race = "Human";

    Dictionary<string, DnaSetter> dna;

    public UDTEventHandler UDTEH;
    public GameObject TargetToSetAsParent;

    public List<GameObject> characters = new List<GameObject>();
    public Dictionary<GameObject, string> charactersDictionary = new Dictionary<GameObject, string>();
    string myRecipe;

    public string SelectedCharacterName
    {
        get
        {
            return selectedCharacterName;
        }

        set
        {
            selectedCharacterName = value;

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

            if (imageTarget == null)
            {
                place.interactable = false;
            }
        }
    }

    void Start()
    {
        SelectedCharacterName = null;
    }

    private void Cleanup()
    {
        foreach (Transform t in SlotPanel.transform)
        {
            UMAUtils.DestroySceneObject(t.gameObject);
        }
        foreach (Transform t in WardrobePanel.transform)
        {
            UMAUtils.DestroySceneObject(t.gameObject);
        }
    }

    public void DnaClick()
    {
        Cleanup();
        Dictionary<string, DnaSetter> AllDNA = avatar.GetDNA();
        foreach (KeyValuePair<string, DnaSetter> ds in AllDNA)
        {
            // create a button. 
            // set set the dna setter on it.
            GameObject go = Instantiate(DnaPrefab);
            DNAHandler ch = go.GetComponent<DNAHandler>();
            ch.Setup(avatar, ds.Value, WardrobePanel);

            Text txt = go.GetComponentInChildren<Text>();
            txt.text = ds.Value.Name;
            go.transform.SetParent(SlotPanel.transform);
        }
    }

    public void ColorsClick()
    {
        Cleanup();

        foreach (OverlayColorData ocd in avatar.CurrentSharedColors)
        {
            GameObject go = Instantiate(ColorPrefab);
            AvailableColorsHandler ch = go.GetComponent<AvailableColorsHandler>();

            SharedColorTable currColors = ClothingColor;

            if (ocd.name.ToLower() == "skin")
                currColors = SkinColor;
            else if (ocd.name.ToLower() == "hair")
                currColors = HairColor;
            else if (ocd.name.ToLower() == "eyes")
                currColors = EyesColor;

            ch.Setup(avatar, ocd.name, WardrobePanel, currColors);

            Text txt = go.GetComponentInChildren<Text>();
            txt.text = ocd.name;
            go.transform.SetParent(SlotPanel.transform);
        }
    }

    public void WardrobeClick()
    {
        Cleanup();

        Dictionary<string, List<UMATextRecipe>> recipes = avatar.AvailableRecipes;

        foreach (string s in recipes.Keys)
        {
            GameObject go = Instantiate(SlotPrefab);
            SlotHandler sh = go.GetComponent<SlotHandler>();
            sh.Setup(avatar, s, WardrobePanel);
            Text txt = go.GetComponentInChildren<Text>();
            txt.text = s;
            go.transform.SetParent(SlotPanel.transform);
        }
    }

    // for populating list of races like populating other button lists
    public void GetAllRaces()
    {
        RaceData[] raceData = dynamicRaceLibrary.GetAllRacesBase();

        foreach (GameObject go in raceButtons)
        {
            Destroy(go.gameObject);
        }

        raceButtons.Clear();

        //had to do this temp for the dumb default races that have dcs added
        foreach (RaceData file in raceData)
        {
            if (file.raceName.Contains("Male") && file.raceName.Contains("DCS"))//had to do this temp for the dumb default races that have dcs added
            {
                GameObject newButton = Instantiate(RaceButtonPrefab);
                newButton.transform.SetParent(RaceButtonPrefab.transform.parent, false);
                newButton.name = "RACE-BUTTON-" + file.raceName;

                newButton.GetComponent<RaceLoadButton>().SetupButton(file.raceName.Substring(file.raceName.Length - 12, 5));

                raceButtons.Add(newButton);
                newButton.SetActive(true);
            }
            else if (file.raceName.Contains("Male") && !file.raceName.Contains("DCS"))
            {
                GameObject newButton = Instantiate(RaceButtonPrefab);
                newButton.transform.SetParent(RaceButtonPrefab.transform.parent, false);
                newButton.name = "RACE-BUTTON-" + file.raceName;

                newButton.GetComponent<RaceLoadButton>().SetupButton(file.raceName.Substring(file.raceName.Length - 4, 4));

                raceButtons.Add(newButton);
                newButton.SetActive(true);
            }
        }
    }

    // takes a string and adds male to it to load a new actual race eg elves, dwarfs, etc
    public void SwitchRace(string race)
    {
        Race = race;
        if (Race == "Human") //had to do this temp for the dumb default races that have dcs added
        {
            avatar.ChangeRace(Race + "MaleDCS");
        }
        else if (avatar.activeRace.name != Race + "Male" || avatar.activeRace.name != Race + "Female")
        {
            avatar.ChangeRace(Race + "Male");
        }
    }

    // takes the race name and adds male or female to it and loads that uma race eg male or female of whatever race is selected (uma races are base models)
    public void SwitchGender(bool male)
    {
        if (male && avatar.activeRace.name != Race + "Male")
        {
            avatar.ChangeRace(Race + "MaleDCS");//had to do this temp for the dumb default races that have dcs added
        }

        if (!male && avatar.activeRace.name != Race + "Female")
        {
            avatar.ChangeRace(Race + "FemaleDCS");//had to do this temp for the dumb default races that have dcs added
        }
    }

    public void SaveRecipe()
    {
        myRecipe = avatar.GetCurrentRecipe();
        if (characterName.text != "")
        {
            File.WriteAllText(Application.persistentDataPath + "/characterSaves/" + characterName.text + ".txt", myRecipe);
        }
        else
        {
            //Throw Error Message
        }

        avatar.ClearSlots();
    }

    public void GetAllSavedCharacters()
    {
        foreach (GameObject go in loadButtons)
        {
            Destroy(go.gameObject);
        }

        loadButtons.Clear();

        if (!ES2.Exists("characterSaves/"))
        {
            Debug.Log("characterSaves Not Found");
            ES2.Save<int>(0, "characterSaves/init/fin.dat");
        }

        string[] filesInFolder = ES2.GetFiles("characterSaves/");

        if (filesInFolder == null)
        {
            Debug.Log("No saved characters.");
            return;
        }

        foreach (string file in filesInFolder)
        {
            string fileExt = file.Substring(file.Length - 4, 4);
            if (fileExt == ".txt")
            {
                GameObject newButton = Instantiate(cSaveButtonPrefab);
                newButton.transform.SetParent(cSaveButtonPrefab.transform.parent, false);
                newButton.name = "LOAD-BUTTON-" + file;

                newButton.GetComponent<CharacterLoadButton>().SetupButton(Path.GetFileNameWithoutExtension(Application.persistentDataPath + "/characterSaves/" + file));

                loadButtons.Add(newButton);
                newButton.SetActive(true);
            }
        }
    }

    public void LoadRecipeForEdit()
    {
        myRecipe = File.ReadAllText(Application.persistentDataPath + "/characterSaves/" + SelectedCharacterName + ".txt");
        nameField.text = SelectedCharacterName;
        avatar.LoadFromRecipeString(myRecipe);
        avatar.BuildCharacter();
    }

    public void SelectCharacter(string cName)
    {
        SelectedCharacterName = cName;
    }

    public void LoadCharacterForPlay()
    {

        if (SelectedCharacterName != "")
        {
            myRecipe = File.ReadAllText(Application.persistentDataPath + "/characterSaves/" + SelectedCharacterName + ".txt");

            if (PhotonNetwork.connected)
            {
                loadedCharacter = PhotonNetwork.Instantiate("New Character Prefab", loadedCharacterPrefab.transform.position, loadedCharacterPrefab.transform.rotation, 0, new object[] { myRecipe });
                Debug.Log(loadedCharacter);
            }
            else
            {
                loadedCharacter = Instantiate(loadedCharacterPrefab);
                loadedCharacter.name = SelectedCharacterName;
                DynamicCharacterAvatar loadedAvatar = loadedCharacter.GetComponent<DynamicCharacterAvatar>();
                loadedAvatar.LoadFromRecipeString(myRecipe);
                loadedAvatar.BuildCharacter();

                foreach(GameObject t in UDTEH.targets)
                {
                    if (t.name == UDTEH.SelectedTarget)
                    {
                        TargetToSetAsParent = t;
                    }
                }

                loadedCharacter.transform.parent = TargetToSetAsParent.transform;

                if (loadedCharacter.transform.parent == null)
                {
                    Debug.Log(imageTarget.name);
                    //open up the placer menu and let them set the character to load onto a image target
                    //change the accessor for place button to work after adding fx here
                }

                float xyz = (loadedCharacter.transform.localScale.x * imageTarget.localScale.x) / 10;
                loadedCharacter.transform.localScale = new Vector3(xyz, xyz, xyz);
            }
            charactersDictionary.Add(loadedCharacter, myRecipe);
            characters.Add(loadedCharacter);
        }
    }

    public void DeleteCharacter()
    {
        File.Delete(Application.persistentDataPath + "/characterSaves/" + SelectedCharacterName + ".txt");
        SelectedCharacterName = null;
        GetAllSavedCharacters();
    }


    public void RemoveCharacterFromPlay()
    {
        List<GameObject> temp = new List<GameObject>();

        if (PhotonNetwork.connected && PhotonNetwork.isMasterClient) //if connected and master client, deletes specific selected character
        {
            foreach (GameObject c in characters)
            {
                if (c.name == selectedCharacterName)
                {
                    temp.Add(c);
                }
            }

            foreach(GameObject c in temp)
            {
                characters.Remove(c);
                Destroy(c);
            }
        }else if (!PhotonNetwork.connected || (PhotonNetwork.connected && !PhotonNetwork.isMasterClient))
        {
            if (PhotonNetwork.connected && !PhotonNetwork.isMasterClient) //if connected, deletes all characters
            {
                foreach (GameObject c in characters)
                {
                    temp.Add(c);
                }

                foreach (GameObject c in temp)
                {
                    characters.Remove(c);
                    Destroy(c);
                }
            }
            else
            { //remove selected dungeon in offline mode
                foreach (GameObject c in characters)
                {
                    if (c.name == selectedCharacterName)
                    {
                        temp.Add(c);
                    }
                }

                foreach (GameObject c in temp)
                {
                    characters.Remove(c);
                    Destroy(c);
                }
            }
        }
    }
}

//TO DO: Add check if targettosetasparent == null, then buttons are inactive on target select screen