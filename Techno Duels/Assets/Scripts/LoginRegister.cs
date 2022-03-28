﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using UnityEngine.Events;
using PlayFab.Json;
using PlayFab.DataModels;
using PlayFab.ProfilesModels;

public class LoginRegister : MonoBehaviour
{
    public static LoginRegister PFC;
    [HideInInspector]
    public string playFabId;

    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;

    public TextMeshProUGUI displayText;

    public UnityEvent onLoggedIn;

    public static LoginRegister instance;
    void Awake () { instance = this; }

    private void OnEnable()
    {
        if (LoginRegister.PFC == null)
        {
            LoginRegister.PFC = this;
        }
        else
        {
            if (LoginRegister.PFC != this)
            {
                Destroy(this.gameObject);
            }
        }
        DontDestroyOnLoad(this.gameObject);
    }
    // encryption
    string Encrypt(string pass)
    {
        System.Security.Cryptography.MD5CryptoServiceProvider x = new System.Security.Cryptography.MD5CryptoServiceProvider();
        byte[] bs = System.Text.Encoding.UTF8.GetBytes(pass);
        bs = x.ComputeHash(bs);
        System.Text.StringBuilder s = new System.Text.StringBuilder();
        foreach (byte b in bs)
        {
            s.Append(b.ToString("x2").ToLower());
        }
        return s.ToString();


    }

    // called when the 'Login' button is pressed
    public void OnLoginButton ()
    {
        // request to login a user
        LoginWithPlayFabRequest loginRequest = new LoginWithPlayFabRequest
        {
            Username = usernameInput.text,
            Password = Encrypt (passwordInput.text)
        };

        // send the request to the API
        PlayFabClientAPI.LoginWithPlayFab(loginRequest,
            // callback function for if register SUCCEEDED
            result =>
            {
                SetDisplayText("Logged in as: " + result.PlayFabId, Color.green);
                playFabId = result.PlayFabId;
                GetStats();
                if(onLoggedIn != null)
                    onLoggedIn.Invoke();
            },
            // callback function for if register FAILED
            error => SetDisplayText(error.ErrorMessage, Color.red)
        );
    }

    

    // called when the 'Register' button is pressed
    public void OnRegisterButton ()
    {
        // request to register a new user
        RegisterPlayFabUserRequest registerRequest = new RegisterPlayFabUserRequest
        {
            Username = usernameInput.text,
            DisplayName = usernameInput.text,
            Password = Encrypt (passwordInput.text),
            RequireBothUsernameAndEmail = false
        };

        // send the request to the API
        PlayFabClientAPI.RegisterPlayFabUser(registerRequest,
            // callback function for if register SUCCEEDED
            result => SetDisplayText("Registered a new account as: " + result.PlayFabId, Color.green),
            // callback function for if register FAILED
            error => SetDisplayText(error.ErrorMessage, Color.red)
        );
    }

    // sets the display text and color
    void SetDisplayText (string text, Color color)
    {
        displayText.text = text;
        displayText.color = color;
    }

    #region PlayerStats
    public int playerHighScore;
    
    //LoginRegister.PFC.SetStats()
    public void SetStats()
    {
        PlayFabClientAPI.UpdatePlayerStatistics(new UpdatePlayerStatisticsRequest
        {
            // request.Statistics is a list, so multiple StatisticUpdate objects can be defined if required.
            Statistics = new List<StatisticUpdate> {
        new StatisticUpdate { StatisticName = "playerHighScore", Value =  playerHighScore },
            }
        },
        result => { Debug.Log("User statistics updated"); },
        error => { Debug.LogError(error.GenerateErrorReport()); });
    }

    void GetStats()
    {
        PlayFabClientAPI.GetPlayerStatistics(
            new GetPlayerStatisticsRequest(),
            OnGetStats,
            error => Debug.LogError(error.GenerateErrorReport())
        );
    }

    void OnGetStats(GetPlayerStatisticsResult result)
    {
        Debug.Log("Received the following Statistics:");
        foreach (var eachStat in result.Statistics)
        {
            Debug.Log("Statistic (" + eachStat.StatisticName + "): " + eachStat.Value);
            switch (eachStat.StatisticName)
            {
                case "playerHighScore":
                    playerHighScore = eachStat.Value;
                    break;
            }
        }
            
    }

    // Build the request object and access the API
    public void StartCloudUpdatePlayerStats()
    {
        PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest()
        {
            FunctionName = "UpdatePlayerStats", // Arbitrary function name (must exist in your uploaded cloud.js file)
            FunctionParameter = new {highScore = playerHighScore}, // The parameter provided to your function
            GeneratePlayStreamEvent = true, // Optional - Shows this event in PlayStream
        }, OnCloudUpdateStats, OnErrorShared);
    }
    // OnCloudHelloWorld defined in the next code block
    private static void OnCloudUpdateStats(ExecuteCloudScriptResult result)
    {
        // Cloud Script returns arbitrary results, so you have to evaluate them one step and one parameter at a time
        Debug.Log((result.FunctionResult));
        JsonObject jsonResult = (JsonObject)result.FunctionResult;
        object messageValue;
        jsonResult.TryGetValue("messageValue", out messageValue); // note how "messageValue" directly corresponds to the JSON values set in Cloud Script
        Debug.Log((string)messageValue);
    }
    private static void OnErrorShared(PlayFabError error)
    {
        Debug.Log(error.GenerateErrorReport());
    }

    #endregion PlayerStats


    #region Leaderboard
    public void GetLeaderboarder()
    {
        var requestLeaderboard = new GetLeaderboardRequest { StartPosition = 0, StatisticName = "playerHighScore", MaxResultsCount = 20 };
        PlayFabClientAPI.GetLeaderboard(requestLeaderboard, OnGetLeadboard, OnErrorLeaderboard);
    }
    void OnGetLeadboard(GetLeaderboardResult result)
    {
        //Debug.Log(result.Leaderboard[0].StatValue);
        foreach(PlayerLeaderboardEntry player in result.Leaderboard)
        {
            Debug.Log(player.DisplayName + ": " + player.StatValue);
        }
    }
    
    void OnErrorLeaderboard(PlayFabError error)
    {
        Debug.LogError(error.GenerateErrorReport());
    }
    
    #endregion Leaderboard

}