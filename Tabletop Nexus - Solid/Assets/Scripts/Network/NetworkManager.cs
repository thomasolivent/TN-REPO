using System.Text;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManager : Photon.PunBehaviour {

    [SerializeField]
    private DungeonSaveLoad dungeonLoader;
    [SerializeField]
    DungeonSaveLoad dsl;
    [SerializeField]
    CharacterCreator cc;
    [SerializeField]
    PropPlacement pPlace;
    //public List<Vector3> dungeonData = new List<Vector3>(); //commented out 1/4/2019
    public Dictionary<string, List<Vector3>> targetsAndChildren = new Dictionary<string, List<Vector3>>();
    RaiseEventOptions options = new RaiseEventOptions()
    {
        CachingOption = EventCaching.DoNotCache,
        Receivers = ReceiverGroup.Others            //just changed from ALL to OTHERS
    };

    private enum NetworkMessage 
    {
        SyncData,
        SetTargets,
        SetNewTarget
    }

    void Awake()
    {
        PhotonNetwork.autoJoinLobby = false;
        // send rate is how many updates per second a PhotonView should send
        // default is 10
        PhotonNetwork.sendRate = 10;
        PhotonNetwork.sendRateOnSerialize = 10;
        PhotonNetwork.automaticallySyncScene = true;
        PhotonNetwork.OnEventCall += OnNetworkMessage;
    }

    public void Connect()
    {
        PhotonNetwork.ConnectUsingSettings("0.1"); //Version Compatibility
        //print("Connecting...");
    }

    public override void OnConnectedToMaster()
    {
        //print("Connected!");
    }

    public override void OnCreatedRoom()
    {
        //InitialCharacterSync();
    }

    public override void OnPhotonPlayerConnected(PhotonPlayer player)
    {
        Debug.Log(player.NickName + " has joined the room.");
        Debug.Log(targetsAndChildren.Count());
        // dungeons are synced with new players when they connect
        if (PhotonNetwork.isMasterClient && targetsAndChildren.Count > 0)
        {
            RaiseEventOptions receivers = new RaiseEventOptions();
            receivers.TargetActors = new int[] {player.ID};

            Debug.Log("Photon Player Connected");
            SyncDictionary(targetsAndChildren);
        }
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined Room");
        if (!PhotonNetwork.isMasterClient)
        {
            targetsAndChildren.Clear();
            dsl.RemoveDungeonFromPlay();
            cc.RemoveCharacterFromPlay();
            cc.characters.Clear();
            dsl.place.interactable = false;     //FIX: turn this back on when disconnected
        }
        else                    //added this block 1/19/2019 for adding characters to the room network if placed before creating the room
        {
            //InitialCharacterSync();
            //InitialPropSync();    Commented out until fixed //1.19.2019
        }
    }
    
    public void InitialCharacterSync() //added this block 1/19/2019 for adding characters to the room network if placed before creating the room
    {
        if (cc.charactersDictionary.Count > 0)
        {
            GameObject refChar = cc.charactersDictionary.ElementAt(0).Key;
            GameObject newCharGo = PhotonNetwork.Instantiate("New Character Prefab", refChar.transform.position, refChar.transform.rotation, 0, new object[] { cc.charactersDictionary.ElementAt(0).Value});
            cc.charactersDictionary.Remove(cc.charactersDictionary.ElementAt(0).Key);
            Destroy(refChar);
            InitialCharacterSync();
        }

    }

    void InitialPropSync()      //added this block 1/19/2019 for adding props to the room network if placed before creating the room
    {
        if (dsl.propsToSync.Count > 0)
        {
            foreach (GameObject go in dsl.propsToSync)
            {
                GameObject newSyncedProp = PhotonNetwork.Instantiate(pPlace.propList[go.GetComponent<Prop>().identifier].name, go.transform.position, go.transform.rotation, 0);
                //Destroy(go);
            }

            //dsl.propsToSync.Clear();
        }
    }

    public override void OnPhotonPlayerDisconnected(PhotonPlayer player)
    {
        // called when other player disconnects
    }

    public override void OnMasterClientSwitched(PhotonPlayer newMasterClient)
    {
        // called when master client switches after original master client disconnects
    }

    public override void OnPhotonJoinRoomFailed(object[] codeAndMsg)
    {
        // called when player fails to join a room
        Debug.Log("Failed to join room!");
    }

    public override void OnConnectionFail(DisconnectCause cause)
    {
        // called when player fails to connect or disconnects from Photon servers
        Debug.Log("Disconnected!");
    }

    void OnNetworkMessage(byte messageId, object msg, int senderId)
    {
        
        if(messageId == (int)NetworkMessage.SyncData)
        {
            Debug.Log("Network Message: SyncData");
            targetsAndChildren = ObjectToDictionary((object[])msg);
            dsl.udtEventHandler.SettingRoomTargets();
        }

        if(messageId == (int)NetworkMessage.SetTargets && !PhotonNetwork.isMasterClient)
        {
            Debug.Log("Network Message: SetTargets");
            dsl.udtEventHandler.SettingRoomTargets();
        }

        if(messageId == (int)NetworkMessage.SetNewTarget && !PhotonNetwork.isMasterClient)              
        {
            Debug.Log("Network Message: SetNewTarget");
            Dictionary<string, List<Vector3>> tempDict = ObjectToDictionary((object[])msg);
            targetsAndChildren.Add(tempDict.ElementAt(0).Key, tempDict.ElementAt(0).Value);
            dsl.udtEventHandler.SettingRoomTargets();
        }
    }

    void KickPlayer(string name)
    {
        // basic kick function - kicks player with given name
        // only master client can do this
        foreach (PhotonPlayer player in PhotonNetwork.playerList)
        {
            if (player.NickName == name)
            {
                PhotonNetwork.CloseConnection(player);
                return;
            }
        }
    }

    static object[] DictionaryToObject(Dictionary<string, List<Vector3>> dictionary)
    {
        Debug.Log("Turning the dictionary into an object");
        object[] o = new object[dictionary.Count];
        for (int i = 0; i < dictionary.Count; ++i)
        {
            var item = dictionary.ElementAt(i);
            o[i] = new string[] { item.Key, SerializeVector3List(item.Value) };
        }
        return o;
    }

    static Dictionary<string, List<Vector3>> ObjectToDictionary(object[] o)
    {
        Debug.Log("Turning the object into a dictionary");
        Dictionary<string, List<Vector3>> d = new Dictionary<string, List<Vector3>>();
        for (int i = 0; i < o.Length; ++i)
        {
            string[] entry = (string[])o[i];
            d.Add(entry[0], DeserializeVector3List(entry[1]));
        }
        return d;
    }

    // for turning the dict<string, list> to dict<string, string>
    public static string SerializeVector3List(List<Vector3> list)
    {
        Debug.Log("Turning the list into a string");
        StringBuilder sb = new StringBuilder();
        foreach(Vector3 v in list)
        {
            sb.Append(v.x).Append(" ").Append(v.y).Append(" ").Append(v.z).Append("|");
        }

        if(sb.Length > 0)
        {
            sb.Remove(sb.Length - 1, 1);
        }

        return sb.ToString();
    }

    // for turning the vector3
    public static List<Vector3> DeserializeVector3List(string aData)
    {
        Debug.Log("Turning the string into a list");
        string[] vectors = aData.Split('|');
        List<Vector3> result = new List<Vector3>();
        for (int i = 0; i < vectors.Length; i++)
        {
            string[] values = vectors[i].Split(' ');
            if (values.Length != 3)
                throw new System.FormatException("component count mismatch. Expected 3 components but got " + values.Length);
            result.Add(new Vector3(float.Parse(values[0]), float.Parse(values[1]), float.Parse(values[2])));
        }
        return result;
    }

    void SyncDictionary(Dictionary<string, List<Vector3>> dictionary)
    {
        Debug.Log("Syncing Dictionary");
        object newDictionary = DictionaryToObject(dictionary);
        //object[] data = new object[] { newDictionary };

        PhotonNetwork.RaiseEvent((int)NetworkMessage.SyncData, newDictionary, true, options);
    }

    public void UpdateDictionary(string targetName, List<Vector3> tileSpawnPositions)           //added 1/2/2019
    {
        targetsAndChildren.Add(targetName, tileSpawnPositions);

        Dictionary<string, List<Vector3>> tempDict = new Dictionary<string, List<Vector3>>();
        tempDict.Add(targetName, tileSpawnPositions);
        object o = DictionaryToObject(tempDict);

        PhotonNetwork.RaiseEvent((int)NetworkMessage.SetNewTarget, o, true, options);
    }

    public void DisconnectFromPhoton()
    {
        PhotonNetwork.Disconnect();
        dsl.udtEventHandler.targets.Clear();
    }
}