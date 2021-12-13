using UnityEngine;
using System;
using Photon.Pun;
using Firebase;
using Firebase.DynamicLinks;

public class DeepLinking : MonoBehaviour
{
    public static DeepLinking Instance { get; private set; }
    public string DeepLinkUrl;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            //Application.deepLinkActivated += OnDeepLinkActivated;
            if (!String.IsNullOrEmpty(Application.absoluteURL))
            {
                // Cold start and Application.absoluteURL not null so process Deep Link.
                OnDeepLinkActivated(Application.absoluteURL);
            }
            // Initialize DeepLink Manager global variable.
            else DeepLinkUrl = "[none]";
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDeepLinkActivated(string url)
    {
        // Update DeepLink Manager global variable, so URL can be accessed from anywhere.
        DeepLinkUrl = url;

        // Decode the URL to determine action. 
        // In this example, the app expects a link formatted like this:
        // unitydl://mylink?scene1
        string action = url.Split("?"[0])[1].Split('&')[0];
        string query = new Uri(url).Query;
        bool validScene;
        switch (action)
        {
            case "friend":
                Debug.Log("Friend Invitation received.");
                string[] queryParams = query.Split('&');
                string userId = "";
                string nick = "";
                foreach(string qry in queryParams)
                {
                    if (qry.StartsWith("id"))
                    {
                        userId = qry.Split('=')[1];
                    }
                    if (qry.StartsWith("nick"))
                    {
                        nick = System.Uri.UnescapeDataString(qry.Split('=')[1]);
                    }
                }
                if (!String.IsNullOrEmpty(userId) || !String.IsNullOrEmpty(nick))
                    FriendPanel.Instance.AddFriend(nick, userId);
                else
                    Debug.LogError("Can't add friend");
                break;
            case "game":
                Debug.Log("Game Invitation received");
                break;
            case "room":
                Debug.Log("Room Invitation Received.");
                break;
            default:
                validScene = false;
                break;
        }
    }

    public string GenerateFriendInvitationLink()
    {
        string UserNickName = System.Uri.EscapeDataString(PhotonNetwork.LocalPlayer.NickName);
        string roomId = PhotonNetwork.CurrentRoom.Name;
        string toLinkt5 = "";
        if (!string.IsNullOrEmpty(roomId))
        {
            var components = new Firebase.DynamicLinks.DynamicLinkComponents(
                new System.Uri($"https://linkfivegame.tbltech.xyz/index.html?room={roomId}"),
                // The dynamic link URI prefix.
                "https://link5.page.link")
                {
                    IOSParameters = new Firebase.DynamicLinks.IOSParameters("com.tbl.link5"),
                    AndroidParameters = new Firebase.DynamicLinks.AndroidParameters(
                $"com.tbl.linkfive")
                };


            //components.Link = new Uri($"https://www.link5.page.link?friend&id={UserId}&nick={UserNickName}");
            Uri link5Uri = components.LongDynamicLink;
            toLinkt5 = link5Uri.ToString();
        }

        return toLinkt5;
    }
                                              
    public string GenerateRoomInvitationLink()
    {
        if(PhotonNetwork.InRoom)
        {
            string RoomName = PhotonNetwork.CurrentRoom.Name;

            return $"unitydl://link5?room&gameroom={RoomName}";
        }
        return "";
    }
                              
    void Start()
    {
        DynamicLinks.DynamicLinkReceived += OnDynamicLink;
    }
                       
    // Display the dynamic link received by the application.
    void OnDynamicLink(object sender, EventArgs args)
    {
        var dynamicLinkEventArgs = args as ReceivedDynamicLinkEventArgs;
        Debug.LogFormat("Received dynamic link {0}",
                        dynamicLinkEventArgs.ReceivedDynamicLink.Url.OriginalString);
        string[] field = dynamicLinkEventArgs.ReceivedDynamicLink.Url.PathAndQuery.Split('?')[1].Split('=');
        if(field != null && !string.IsNullOrEmpty(field[0]) && !string.IsNullOrEmpty(field[1]))
        {
            switch ( field[0] ) {
                case "room":
                    ConnectionManager.Instance.ConnectToRoom(field[1]);
                    break;
            }
        }
    }
}

