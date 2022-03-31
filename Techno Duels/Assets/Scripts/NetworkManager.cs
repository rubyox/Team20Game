using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class NetworkManager : MonoBehaviourPun
{
    // instance
    public static NetworkManager instance;

    void Awake ()
    {
        // set the instance to this script
        instance = this;
        
    }

    void Start ()
    {
        // connect to the master server
       
    }

    public void ConnectToMaster()
    {
        PhotonNetwork.ConnectUsingSettings();
    }
    // joins a random room or creates a new room
    public void CreateOrJoinRoom ()
    {
        // if there are available rooms, join a random one
        if(PhotonNetwork.CountOfRooms > 0)
            PhotonNetwork.JoinRandomRoom();
        // otherwise, create a new room
        else
        {
            // set the max players to 2
            RoomOptions options = new RoomOptions();
            options.MaxPlayers = 2;

            PhotonNetwork.CreateRoom(null, options);
        }
    }

    // changes the scene using Photon's system
    [PunRPC]
    public void ChangeScene (string sceneName)
    {
        PhotonNetwork.LoadLevel(sceneName);
    }
}