using Facebook.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun.Demo.PunBasics;

public class FacebookManager : MonoBehaviour
{
    public static FacebookManager Instance;
    public bool isLoggedIn;
    public string userName;
    public Sprite userProfilePic;
    private PlayerNameInputField playerNameInput;

    [HideInInspector]
    public FacebookUI facebookUI;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Instance = null;
            Destroy(this.gameObject);
        }

        playerNameInput = FindObjectOfType<PlayerNameInputField>();
        InitFacebook();
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            GameRequest();
        }
    }

    /// <summary>
    /// Initializes Facebook SDK
    /// </summary>
    private void InitFacebook()
    {
        if (!FB.IsInitialized)
        {
            // Initialize the Facebook SDK
            FB.Init(InitCallback, OnHideUnity);
        }
        else
        {
            // Already initialized, signal an app activation App Event
            FB.ActivateApp();
            LoginOnStartup();
        }
    }

    private void InitCallback()
    {
        if (FB.IsInitialized)
        {
            // Signal an app activation App Event
            FB.ActivateApp();
            // Continue with Facebook SDK
            // ...
            LoginOnStartup();
        }
        else
        {
            Debug.Log("Failed to Initialize the Facebook SDK");
        }
    }

    private void OnHideUnity(bool isGameShown)
    {
        if (!isGameShown)
        {
            // Pause the game - we will need to hide
            Time.timeScale = 0;
        }
        else
        {
            // Resume the game - we're getting focus again
            Time.timeScale = 1;
        }
    }

    public void LoginOnStartup()
    {
        if (PlayerPrefs.GetInt("FacebookLoggedIn", 0) == 1)
        {
            LoginWithReadPermission();
        }
    }

    public void LoginWithReadPermission()
    {
        var perms = new List<string> { "public_profile", "email" };     //TO-DO: add 'user_friends' after getting permission
        FB.LogInWithReadPermissions(perms, AuthCallback);
    }

    public void LoginWithPublishPermission()
    {
        List<string> permissions = new List<string>() { "publish_actions" };
        FB.LogInWithPublishPermissions(permissions, AuthCallback);
    }

    public void LogOut()
    {
        FB.LogOut();
    }

    private void AuthCallback(ILoginResult result)
    {
        if (FB.IsLoggedIn)
        {
            // AccessToken class will have session details
            var aToken = Facebook.Unity.AccessToken.CurrentAccessToken;
            // Print current access token's User ID
            Debug.Log(aToken.UserId);
            // Print current access token's granted permissions
            foreach (string perm in aToken.Permissions)
            {
                Debug.Log(perm);
            }

            FB.API("/me?fields=first_name", HttpMethod.GET, ShowUserName);
            FB.API("me/picture?type=square&height=128&width=128", HttpMethod.GET, ShowProfilePicture);
            PlayerPrefs.SetInt("FacebookLoggedIn", 1);
            isLoggedIn = true;
            facebookUI.inviteButton.interactable = true;
        }
        else
        {
            Debug.Log("User cancelled Login");
        }
    }

    private void ShowProfilePicture(IGraphResult result)
    {
        if (result.Error == null && result.Texture != null)
        {
            userProfilePic = Sprite.Create(result.Texture, new Rect(0, 0, 128, 128), new Vector2());
            facebookUI.profileImage.sprite = userProfilePic;
        }
        else
        {
            Debug.Log(result.Error);
        }
    }

    private void ShowUserName(IGraphResult result)
    {
        if (result.Error != null)
        {
            Debug.Log(result.Error);
        }
        else
        {
            userName = result.ResultDictionary["first_name"].ToString();
            userName = userName.Split(' ')[0];
            facebookUI.nameText.text = userName;
            playerNameInput.SetPlayerName(userName);
        }
    }

    public void GameRequest()
    {
        FB.AppRequest(
            "Come play this great game!",
            null, null, null, null, null, null,
            GameRequestCallback);
    }

    private void GameRequestCallback(IAppRequestResult result)
    {
        if (result.Cancelled || !String.IsNullOrEmpty(result.Error))
        {
            Debug.Log("Game Request Error: " + result.Error);
        }
        else if (!String.IsNullOrEmpty(result.RequestID))
        {
            // Print identifier of the requested content
            Debug.Log(result.RequestID);
            Debug.Log(result.To);
            Debug.Log(result.RawResult);
        }
        else
        {
            // Game request succeeded without requestID
            Debug.Log("Game Request Success!");
        }
    }

    public void GameRequestToPlay()
    {

    }

    public void Share()
    {
        FB.ShareLink(new Uri("https://developers.facebook.com/"), callback: ShareCallback);
    }

    public void ShareFeed()
    {
        FB.FeedShare(
            string.Empty,
            new Uri("https://developers.facebook.com/"),
            "Hello this is the title",
            "This is the caption",
            "Check out this game",
            new Uri("https://developers.facebook.com/"),
            string.Empty,
            ShareCallback
        );
    }

    private void ShareCallback(IShareResult result)
    {
        if (result.Cancelled || !String.IsNullOrEmpty(result.Error))
        {
            Debug.Log("ShareLink Error: " + result.Error);
        }
        else if (!String.IsNullOrEmpty(result.PostId))
        {
            // Print post identifier of the shared content
            Debug.Log(result.PostId);
        }
        else
        {
            // Share succeeded without postID
            Debug.Log("ShareLink success!");
        }
    }

    public void AppInvite()
    {
        //FB.Mobile.AppInvite(
        //    new Uri("https://#"),
        //    new Uri("http://gamepie.fr/wp-content/uploads/2017/07/18766463_219161298600119_4026107965015456015_o.png"),
        //    InviteCallBack
        //);
    }

    private void InviteCallback(IResult result)
    {
        if (result.Cancelled)
        {
            Debug.Log("Invite Cancelled");
        }
        else if (!string.IsNullOrEmpty(result.Error))
        {
            Debug.Log("Error on invite!");
        }
        else if (!string.IsNullOrEmpty(result.RawResult))
        {
            Debug.Log("Success on Invite");
        }
    }

    public void GetFriends()
    {
        string queryString = "/me/friends?fields=id,first_name,picture.width(128).height(128)&limit=100";
        FB.API(queryString, HttpMethod.GET, GetFriendsCallback);
    }

    private void GetFriendsCallback(IGraphResult result)
    {
        if (result.Error != null)
        {
            Debug.LogError(result.Error);
            return;
        }
        Debug.Log(result.RawResult);

        // Store /me/friends result
        object dataList;
        if (result.ResultDictionary.TryGetValue("data", out dataList))
        {
            var friendsList = (List<object>)dataList;
            foreach (var dict in friendsList)
            {
                string friendsName = ((Dictionary<string, object>)dict)["first_name"].ToString();
                string id = ((Dictionary<string, object>)dict)["id"].ToString();

                FB.API(id + "/picture?width=128&height=128", HttpMethod.GET, delegate (IGraphResult graphResult)
                {
                    if (graphResult.Error != null)
                    {
                        Debug.Log(graphResult.RawResult);
                    }
                    else
                    {
                        Sprite friendProfilePic = Sprite.Create(graphResult.Texture, new Rect(0, 0, 128, 128), new Vector2());
                        //FacebookUser facebookUser = new FacebookUser(friendsName, friendProfilePic, 0);
                        //facebookFriendsList.Add(facebookUser);
                    }
                });
            }
        }
    }

    public void GetFriendsPlayingThisGame()
    {

    }

    public void LogEvent(string eventName, float eventValue, Dictionary<string, object> parameters)
    {
        FB.LogAppEvent(eventName, eventValue, parameters);
    }
}
