using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CreateRoom : MonoBehaviour {

    [SerializeField]
    Button ConnectButton;
    [SerializeField]
    Text roomName;
    Text RoomName
    {
        get { return roomName; }
    }

    public void JoinOrCreateRoom()
    {
        if (RoomName.text != "")
        {
            RoomOptions roomOptions = new RoomOptions() {
                IsVisible = false,
                IsOpen = true,
                MaxPlayers = 20,
                CleanupCacheOnLeave = true,
                PublishUserId = true};
            
            PhotonNetwork.JoinOrCreateRoom(RoomName.text, roomOptions, TypedLobby.Default);
        }
        else
        {
            Debug.Log("Session Name Required");
        }
    }

    void OnPhotonCreateRoomFailed(object[] codeAndMessage)
    {
        print("create room failed: " + codeAndMessage[1]);
    }

    void OnCreatedRoom()
    {
        print("Room Created Successfully");
        ConnectButton.gameObject.SetActive(false);
    }

    void OnJoinedRoom()
    {
        ConnectButton.gameObject.SetActive(false);
    }

}
