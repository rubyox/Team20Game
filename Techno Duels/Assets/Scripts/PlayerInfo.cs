using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using Photon.Pun;
using Photon.Realtime;

public class PlayerInfo : MonoBehaviour
{
    [HideInInspector]
    public PlayerProfileModel profile;

    public static PlayerInfo instance;
    void Awake () { instance = this; }

    public void OnLoggedIn ()
    {
        GetPlayerProfileRequest getProfileRequest = new GetPlayerProfileRequest
        {
            PlayFabId = LoginRegister.instance.playFabId,
            ProfileConstraints = new PlayerProfileViewConstraints
            {
                ShowDisplayName = true
            }
        };

        PlayFabClientAPI.GetPlayerProfile(getProfileRequest,
            result =>
            {
                profile = result.PlayerProfile;
                Debug.Log("Loaded in player: " + profile.DisplayName);
                PhotonNetwork.NickName = profile.DisplayName;
            },
            error => Debug.Log(error.ErrorMessage)
        );
    }
}