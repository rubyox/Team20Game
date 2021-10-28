using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Realtime;
using Photon.Pun;

public class mainMenu : MonoBehaviourPunCallbacks
{
   
    
    void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Server");
        PhotonNetwork.AutomaticallySyncScene=true;
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("Could not find game, creating room");
        MakeRoom();
    }

    void MakeRoom()
    {
        int randomRoomName = Random.Range(0, 5000);
        RoomOptions roomOptions = new RoomOptions() {
            IsVisible = true,
            IsOpen = true,
            MaxPlayers = 2
        };
        PhotonNetwork.CreateRoom("RoomName_" + randomRoomName, roomOptions);
        Debug.Log("Room Created, Waiting for Another Player");
    }

    public void playGame()
    {
        PhotonNetwork.JoinRandomRoom();
        Debug.Log("Searching for a Game");
        
    }

    public void stopSearch()
    {
        PhotonNetwork.LeaveRoom();
        Debug.Log("Canceled Search");
    }

    public void quitGame()
    {
        Debug.Log("QUIT!");
        Application.Quit();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount == 2 && PhotonNetwork.IsMasterClient)
        {
            Debug.Log(PhotonNetwork.CurrentRoom.PlayerCount + "/2 Starting Game");
            PhotonNetwork.LoadLevel(1);
        }

    }
    
}
