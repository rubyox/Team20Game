using System.Collections;
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
    public NetworkManager np;
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
                SetDisplayText("Logged in with playfab ID: " + result.PlayFabId, Color.green);
                playFabId = result.PlayFabId;
                Debug.LogError("<color=green> encrypted password was : " + Encrypt(passwordInput.text));
                np.ConnectToMaster();
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
        new StatisticUpdate { StatisticName = "Wins", Value =  playerHighScore },
            }
        },
        result => { Debug.Log("User statistics updated"); },
        error => { Debug.LogError(error.GenerateErrorReport()); });
    }

    public List<PlayerLeaderboardEntry> GetWins()
    {
        GetLeaderboardRequest request = new GetLeaderboardRequest();
        request.MaxResultsCount = 20;
        request.StatisticName = "Wins";
        List<PlayerLeaderboardEntry> temp = new List<PlayerLeaderboardEntry>();
        PlayFabClientAPI.GetLeaderboard(request, result => {
            temp = result.Leaderboard;
        }, error =>{
           
        });
        if(temp.Count < 0)
        {
            return null;
        }
        return temp;
    }


    public void SetWins(int value)
    {
        PlayFabClientAPI.UpdatePlayerStatistics(new UpdatePlayerStatisticsRequest
        {
            // request.Statistics is a list, so multiple StatisticUpdate objects can be defined if required.
            Statistics = new List<StatisticUpdate> {
        new StatisticUpdate { StatisticName = "Wins", Value =  playerHighScore },
            }
        },
        result => { Debug.Log("User statistics updated"); },
        error => { Debug.LogError(error.GenerateErrorReport()); });

        GetPlayerStatisticsRequest findRequest = new GetPlayerStatisticsRequest();
        List<string> names = new List<string>();
        names.Add("Wins");
        findRequest.StatisticNames = names;
        int temp = 0;
        PlayFabClientAPI.GetPlayerStatistics(findRequest, result =>
        {

            temp = result.Statistics[0].Value;
            UpdatePlayerStatisticsRequest request = new UpdatePlayerStatisticsRequest();
            List<StatisticUpdate> listUpdate = new List<StatisticUpdate>();
            value += temp;
            StatisticUpdate su = new StatisticUpdate();
            su.StatisticName = "Wins";
            su.Value += value;
            listUpdate.Add(su);
            request.Statistics = listUpdate;
            playerHighScore = su.Value;
            PlayFabClientAPI.UpdatePlayerStatistics(request, result2 => {

                Debug.Log("Wins have been set!");

            }, error => {

            });

        

    }, error => { Debug.LogError(error.ErrorMessage); });

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
    public void StartCloudUpdatePlayerStats(int value)
    {
        GetPlayerStatisticsRequest findRequest = new GetPlayerStatisticsRequest();
        List<string> names = new List<string>();
        names.Add("Wins");
        findRequest.StatisticNames = names;
        int temp = 0;
        PlayFabClientAPI.GetPlayerStatistics(findRequest, result =>
        {

            temp = result.Statistics[0].Value;
            UpdatePlayerStatisticsRequest request = new UpdatePlayerStatisticsRequest();
            List<StatisticUpdate> listUpdate = new List<StatisticUpdate>();
            value += temp;
            StatisticUpdate su = new StatisticUpdate();
            su.StatisticName = "Wins";
            su.Value += value;
            listUpdate.Add(su);
            request.Statistics = listUpdate;
            playerHighScore = su.Value;

            PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest()
            {
                FunctionName = "UpdatePlayerStats", // Arbitrary function name (must exist in your uploaded cloud.js file)
                FunctionParameter = new { wins = su.Value }, // The parameter provided to your function
                GeneratePlayStreamEvent = true, // Optional - Shows this event in PlayStream
            }, OnCloudUpdateStats, OnErrorShared);

        }, error => { Debug.LogError(error.ErrorMessage); });

        PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest()
        {
            FunctionName = "UpdatePlayerStats", // Arbitrary function name (must exist in your uploaded cloud.js file)
            FunctionParameter = new {  }, // The parameter provided to your function
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

    public GameObject leaderboardPanel;
    public GameObject listingPrefab;
    public Transform listingContainer;

    #region Leaderboard
    public void GetLeaderboarder()
    {
        var requestLeaderboard = new GetLeaderboardRequest { StartPosition = 0, StatisticName = "Wins", MaxResultsCount = 100 };
        PlayFabClientAPI.GetLeaderboard(requestLeaderboard, OnGetLeadboard, OnErrorLeaderboard);
    }
    void OnGetLeadboard(GetLeaderboardResult result)
    {
        leaderboardPanel.SetActive(true);
        //Debug.Log(result.Leaderboard[0].StatValue);
        foreach(PlayerLeaderboardEntry player in result.Leaderboard)
        {
            GameObject tempListing = Instantiate(listingPrefab, listingContainer);
            LeaderboardListing LL = tempListing.GetComponent<LeaderboardListing>();
            LL.playerNameText.text = player.DisplayName;
            LL.playerScoreText.text = player.StatValue.ToString();
            Debug.Log(player.DisplayName + ": " + player.StatValue);
        }
    }
    public void CloseLeaderboardPanel()
    {
        leaderboardPanel.SetActive(false);
        for(int i = listingContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(listingContainer.GetChild(i).gameObject);
        }
    }
    void OnErrorLeaderboard(PlayFabError error)
    {
        Debug.LogError(error.GenerateErrorReport());
    }
    
    #endregion Leaderboard

}