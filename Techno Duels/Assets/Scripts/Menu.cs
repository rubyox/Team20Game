using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class Menu : MonoBehaviourPunCallbacks
{
    [Header("Screens")]
    public GameObject mainScreen;
    public GameObject lobbyScreen;

    [Header("Main Screen")]
    public Button playButton;
    public Button quitButton;

    [Header("Lobby Screen")]
    public TextMeshProUGUI player1NameText;
    public TextMeshProUGUI player2NameText;
    public TextMeshProUGUI gameStartingText;

    void Start ()
    {
        // disable the play button before we connect to the master server
        playButton.interactable = false;
        gameStartingText.gameObject.SetActive(false);
    }

    // called when we connect to the master server
    public override void OnConnectedToMaster ()
    {
        playButton.interactable = true;
    }

    // toggles the currently visible screen
    public void SetScreen (GameObject screen)
    {
        // disable all screens
        mainScreen.SetActive(false);
        lobbyScreen.SetActive(false);

        // enable the requested screen
        screen.SetActive(true);
    }


    // called when the "Play" button is pressed
    public void OnPlayButton ()
    {
        NetworkManager.instance.CreateOrJoinRoom();
    }

    // called when we join a room
    public override void OnJoinedRoom ()
    {
        SetScreen(lobbyScreen);
        photonView.RPC("UpdateLobbyUI", RpcTarget.All);
    }

    // called when a player leaves the room
    public override void OnPlayerLeftRoom (Player otherPlayer)
    {
        UpdateLobbyUI();
    }

    // updates the lobby screen UI
    [PunRPC]
    void UpdateLobbyUI ()
    {
        // set the player name texts
        player1NameText.text = PhotonNetwork.CurrentRoom.GetPlayer(1).NickName;
        player2NameText.text = PhotonNetwork.PlayerList.Length == 2 ? PhotonNetwork.CurrentRoom.GetPlayer(2).NickName : "...";

        // set the game starting text
        if(PhotonNetwork.PlayerList.Length == 2)
        {
            gameStartingText.gameObject.SetActive(true);

            if(PhotonNetwork.IsMasterClient)
                Invoke("TryStartGame", 3.0f);
        }
    }

    // checks if 2 players are in the lobby and if so - start the game
    void TryStartGame ()
    {
        // if we have 2 players in the lobby, load the Game scene
        if(PhotonNetwork.PlayerList.Length == 2)
            NetworkManager.instance.photonView.RPC("ChangeScene", RpcTarget.All, "Game");
        else gameStartingText.gameObject.SetActive(false);
    }

    // called when the "Leave" button is pressed
    public void OnLeaveButton ()
    {
        PhotonNetwork.LeaveRoom();
        SetScreen(mainScreen);
    }

    // called when the "quit" button is pressed
    public void OnQuitButton()
    {
        Debug.Log("QUIT!");
        Application.Quit();
        
    }


}