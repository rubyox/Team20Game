using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using UnityEngine.Events;
using PlayFab.Json;
using PlayFab.DataModels;
using PlayFab.ProfilesModels;
public class GameManager : MonoBehaviourPun
{
    public PlayerController leftPlayer;
    public PlayerController rightPlayer;
    public int playerHighScore;
    public PlayerController curPlayer;      // the player who's currently having their turn
    public float postGameTime;              // time between the game ending and us going back to the menu
    
    // instance
    public static GameManager instance;

    void Awake ()
    {
        // set the instance to this script
        instance = this;
    }

    void Start ()
    {
        
          
        
        
        // the master client will set the players
        if (PhotonNetwork.IsMasterClient)
            SetPlayers();
         
    }

    // creates the player data and spawns in the units
    void SetPlayers ()
    {
        // set the owners of the two player's photon views
        leftPlayer.photonView.TransferOwnership(1);
        rightPlayer.photonView.TransferOwnership(2);

        // initialize the players
        leftPlayer.photonView.RPC("Initialize", RpcTarget.AllBuffered, PhotonNetwork.CurrentRoom.GetPlayer(1));
        rightPlayer.photonView.RPC("Initialize", RpcTarget.AllBuffered, PhotonNetwork.CurrentRoom.GetPlayer(2));

        // set the first player's turn
        photonView.RPC("SetNextTurn", RpcTarget.AllBuffered);
    }

    [PunRPC]
    void SetNextTurn ()
    {
        // is this the first turn?
        if(curPlayer == null)
            curPlayer = leftPlayer;
        else
            curPlayer = curPlayer == leftPlayer ? rightPlayer : leftPlayer;

        // if it's our turn - enable the end turn button
        if (curPlayer == PlayerController.me)
        {
            PlayerController.me.BeginTurn();
        }

        // toggle the end turn button
        GameUI.instance.ToggleEndTurnButton(curPlayer == PlayerController.me);
    }

    // returns the opposing player from the one sent
    public PlayerController GetOtherPlayer (PlayerController player)
    {
        return player == leftPlayer ? rightPlayer : leftPlayer;
    }

    // called by a player when their unit dies
    // we send over the enemy player
    public void CheckWinCondition ()
    {
        if(PlayerController.me.units.Count == 0)
            photonView.RPC("WinGame", RpcTarget.All, PlayerController.enemy == leftPlayer ? 0 : 1);
    }

    // called when a player has defeated all of the other player's units
    [PunRPC]
    void WinGame (int winner)
    {
        // get the winning player
        PlayerController player = winner == 0 ? leftPlayer : rightPlayer;
        if (PlayerController.me.units.Count == 0)
        {
            Invoke("GoBackToMenu", postGameTime);

        } else
        {
            LoginRegister.PFC.SetWins(1);
        }
        
        

    // set the win text
    GameUI.instance.SetWinText(player.photonPlayer.NickName);
       
        // go back to the menu after a few seconds
        Invoke("GoBackToMenu", postGameTime);
    }

    // leave the room and go back to the menu
    void GoBackToMenu ()
    {
        PhotonNetwork.LeaveRoom();
        NetworkManager.instance.ChangeScene("Menu");
    }
}