using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using Photon.Pun;
using TMPro;
using ExitGames.Client.Photon;
using GoogleMobileAds.Api;
using Link5;
using System.Text;
using System;

/// <summary>
/// Connection Manager. Connect, join a random room or create one if none or all full.
/// </summary>
public class ConnectionManager : MonoBehaviourPunCallbacks , ILobbyCallbacks
{
    [SerializeField]
    private UILoginController m_LoginController;
    private const int RoomNameMaxLength = 10;
    public static ConnectionManager Instance;
    private AdMobManager AdManager;
    public MatchType matchType;
    [SerializeField]
    private InputField RequestedNameRoom;
    public string RoomName;
    public bool tryToJoinNamedRoom = false;
    public FriendPanel m_FriendPanel;

    #region Private Serializable Fields

    [Tooltip("The Ui Panel to let the user enter name, connect and play")]
    [SerializeField]
    private GameObject controlPanel;

    [Tooltip("The Ui Text to inform the user about the connection progress")]
    [SerializeField]
    private TextMeshProUGUI feedbackText;

    [Tooltip("The maximum number of players per room")]
    [SerializeField]
    private byte maxPlayersPerRoom = 2;

    [Tooltip("The UI Loader Anime")]
    [SerializeField]
    private GameObject loaderAnime;

    [SerializeField]
    private TypedLobby FreeGamesLobby;
    private RoomOptions PublicRoomOption;
    private RoomOptions PrivateRoomOption;

    public bool isPrivateRoom; 
    public bool isFreeGame;

    private bool IsConnectingToRoom;

    public bool IsRandomMatch { get; private set; }

    public bool IsFreeGame
    {
        get { return isFreeGame; }
        set { isFreeGame = value; }
    }

    public bool IsPrivateRoom
    {
        get { return isPrivateRoom;  }
        set { isPrivateRoom = value;  }
    }

    #endregion

    #region Private Fields
    /// <summary>
    /// Keep track of the current process. Since connection is asynchronous and is based on several callbacks from Photon, 
    /// we need to keep track of this to properly adjust the behavior when we receive call back by Photon.
    /// Typically this is used for the OnConnectedToMaster() callback.
    /// </summary>
    bool isConnecting;

    /// <summary>
    /// Previous room name, to rejoin room if player gets disconnected
    /// </summary>
    string previousRoom;

    /// <summary>
    /// This client's version number. Users are separated from each other by gameVersion (which allows you to make breaking changes).
    /// </summary>
    string gameVersion = "1";

    #endregion

    #region MonoBehaviour CallBacks

    /// <summary>
    /// MonoBehaviour method called on GameObject by Unity during early initialization phase.
    /// </summary>
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(this.gameObject);
        }
        else 
        {
            Instance = null;
            Destroy(this.gameObject);
        }

        if (loaderAnime == null)
        {
            Debug.LogError("<Color=Red><b>Missing</b></Color> loaderAnime Reference.", this);
        }
        // #Critical
        // this makes sure we can use PhotonNetwork.LoadLevel() on the master client and all clients in the same room sync their level automatically
        PhotonNetwork.AutomaticallySyncScene = true;
        AdManager = GameObject.FindObjectOfType<AdMobManager>();
        m_LoginController = GetComponent<UILoginController>();
    }

    private void Start()
    {
        PublicRoomOption = new RoomOptions { IsVisible = true, MaxPlayers = this.maxPlayersPerRoom };
        PrivateRoomOption = new RoomOptions { IsVisible = false, MaxPlayers = this.maxPlayersPerRoom };

        FreeGamesLobby = new TypedLobby("Free", LobbyType.Default);
        PermissionManager.RequestPermissons();
        IsConnectingToRoom = false;
#if NOT
        if(PermissionManager.HasPermissions()) 
        {
         int getSDKInt()
            {
                using (var version = new AndroidJavaClass("android.os.Build$VERSION"))
                    return version.GetStatic<int>("SDK_INT");
            }
        
            if(getSDKInt() < 29)
            {
                Contacts.LoadContactList(() =>
                {
                    m_FriendPanel.FriendsListAvailable = true;
                    m_FriendPanel.ShowFriends();
                }, (str) => { Debug.Log(str); });
            }
        }
        else 
        {
            PermissionManager.ShowSettings();
        }
#endif
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Start the connection process. 
    /// - If already connected, we attempt joining a random room
    /// - if not yet connected, Connect this application instance to Photon Cloud Network
    /// </summary>
    private void Connect()
    {
        // we want to make sure the log is clear everytime we connect, we might have several failed attempted if connection failed.
        feedbackText.gameObject.SetActive(true);
        feedbackText.text = "";

        // keep track of the will to join a room, because when we come back from the game we will get a callback that we are connected, so we need to know what to do then
        isConnecting = true;

        // hide the Play button for visual consistency
        controlPanel.SetActive(false);

        // start the loader animation for visual effect.
        if (loaderAnime != null)
        {
            loaderAnime.SetActive(true);
        }


        // we check if we are connected or not, we join if we are , else we initiate the connection to the server.
        if (PhotonNetwork.IsConnected)
        {
            LogFeedback("Joining Room...");
            // #Critical we need at this point to attempt joining a Random Room. If it fails, we'll get notified in OnJoinRandomFailed() and we'll create one.
            PhotonNetwork.JoinRandomRoom();
            //PhotonNetwork.JoinOrCreateRoom("", PrivateRoom(), TypedLobby.Default);
        }
        else
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                LogFeedback("No Internet Access...");
                return;
            }
            LogFeedback("Connecting...");

            // #Critical, we must first and foremost connect to Photon Online Server.
            PhotonNetwork.GameVersion = this.gameVersion;
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    internal void ConnectToRoom(string roomName)
    {
        if(!IsConnectingToRoom)
        {
            m_LoginController.ChangeActivePanel("startPanel");
            tryToJoinNamedRoom = true;
            IsConnectingToRoom = true;
            RequestedNameRoom.text = roomName;
            this.RoomName = roomName;
            Connect();
        }
    }

    /// <summary>
    /// Sets the Online/Offline Game Mode
    /// </summary>
    /// <param name="gameMode"></param>
    public void SetGameMode(int gameMode)
    {
        switch ((GameMode) gameMode)
        {
            case GameMode.Tutorial:
                PhotonNetwork.OfflineMode = true;
                GameManager.TutorialMode = true;
                isFreeGame = true;
                break;
            case GameMode.SinglePlayer:
                PhotonNetwork.OfflineMode = true;
                GameManager.TutorialMode = false;
                isFreeGame = true;
                break;
            case GameMode.MultiPlayer:
                PhotonNetwork.OfflineMode = false;
                break;
        }
    }

    /// <summary>
    /// Logs the feedback in the UI view for the player, as opposed to inside the Unity Editor for the developer.
    /// </summary>
    /// <param name="message">Message.</param>
    void LogFeedback(string message)
    {
        // we do not assume there is a feedbackText defined.
        if (feedbackText == null)
        {
            Debug.LogError("TEst");
            return;
        }

        // add new messages as a new line and at the bottom of the log.
        feedbackText.text += System.Environment.NewLine + message;
    }

    #endregion

    #region MonoBehaviourPunCallbacks CallBacks
    // below, we implement some callbacks of PUN
    // you can find PUN's callbacks in the class MonoBehaviourPunCallbacks


    /// <summary>
    /// Called after the connection to the master is established and authenticated
    /// </summary>
    public override void OnConnectedToMaster()
    {
        // we don't want to do anything if we are not attempting to join a room. 
        // this case where isConnecting is false is typically when you lost or quit the game, when this level is loaded, OnConnectedToMaster will be called, in that case
        // we don't want to do anything.
        if (isConnecting)
        {
            // after timeout: re-join "old" room (if one is known)
            if (!string.IsNullOrEmpty(this.previousRoom))
            {
                Debug.Log("ReJoining Previous Room: " + this.previousRoom);
                PhotonNetwork.RejoinRoom(this.previousRoom);
                this.previousRoom = null;       // we only will try to re-join once. if this fails, we will get into a random/new room
            }
            else if (IsFreeGame)
            {
                ConnectToFreeLobby();
                return;
            }
            else if (this.tryToJoinNamedRoom)
            {

                PhotonNetwork.JoinRoom(RequestedNameRoom.text);
            }
            else
            {
                LogFeedback("Searching for Random Room");
                Debug.Log("OnConnectedToMaster() was called by PUN. Now this client is connected and could join a room.\n Calling: PhotonNetwork.JoinRandomRoom(); Operation will fail if no room found");
                PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable() { { "UserProfilePic", Link5.GameData.GetInstance().profilePhotoURI } });
                // #Critical: The first we try to do is to join a potential existing room. If there is, good, else, we'll be called back with OnJoinRandomFailed()
                PhotonNetwork.JoinRandomRoom();
            }
        }

    }

    /// <summary>
    /// Called when a JoinRandom() call failed. The parameter provides ErrorCode and message.
    /// </summary>
    /// <remarks>
    /// Most likely all rooms are full or no rooms are available. <br/>
    /// </remarks>
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        LogFeedback("Creating new Room");
        Debug.Log("OnJoinRandomFailed() was called by PUN. No random room available, so we create one.\nCalling: PhotonNetwork.CreateRoom");

        // #Critical: we failed to join a random room, maybe none exists or they are all full. No worries, we create a new room.
        string l_roomName = LoginManager.GenerateRandomAlphaNumericStr(RoomNameMaxLength).ToLower();
        PhotonNetwork.JoinOrCreateRoom(
            l_roomName,
            isPrivateRoom ? PrivateRoomOption : PublicRoomOption,
            TypedLobby.Default); 
    }

    public override void OnRoomListUpdate(System.Collections.Generic.List<RoomInfo> roomInfos)
    {
        Debug.Log(roomInfos);
    }

    private Coroutine LobbyConnectionRoutine;
    private bool SuccessfullyConnectedToFreeLobby = false;
    private void ConnectToFreeLobby()
    {
        if(LobbyConnectionRoutine != null)
        {
            StopCoroutine(LobbyConnectionRoutine);
            LobbyConnectionRoutine = null;
        }
        LobbyConnectionRoutine = StartCoroutine(ConnectoLobbyRoutine());
    }
    private System.Collections.IEnumerator ConnectoLobbyRoutine()
    {
        yield return new WaitWhile(() => PhotonNetwork.NetworkClientState != ClientState.ConnectedToMasterServer);
        PhotonNetwork.JoinLobby(FreeGamesLobby);
        yield return new WaitWhile(() => !SuccessfullyConnectedToFreeLobby);

        yield return null;
    }

    public override void OnJoinedLobby()
    {
        base.OnJoinedLobby();
        SuccessfullyConnectedToFreeLobby = false;
        if (this.tryToJoinNamedRoom)
        {
            PhotonNetwork.JoinRoom(RequestedNameRoom.text);
        }
        else
        {
            LogFeedback("Searching for Random Room on Lobby: " + PhotonNetwork.CurrentLobby.Name);
            Debug.Log("OnConnectedToMaster() was called by PUN. Now this client is connected and could join a room.\n Calling: PhotonNetwork.JoinRandomRoom(); Operation will fail if no room found");
            PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable() { { "UserProfilePic", Link5.GameData.GetInstance().profilePhotoURI } });
            // #Critical: The first we try to do is to join a potential existing room. If there is, good, else, we'll be called back with OnJoinRandomFailed()
            PhotonNetwork.JoinRandomRoom();
        }
        PhotonNetwork.JoinRandomRoom(); //Try to Join a Random Room. if fail will create a new room in OnJoinRandomRoomFailed Callback
    }


    /// <summary>
    /// Called after disconnecting from the Photon server.
    /// </summary>
    public override void OnDisconnected(DisconnectCause cause)
    {
        LogFeedback("OnDisconnected " + cause);
        Debug.Log("Disconnected");

        // #Critical: we failed to connect or got disconnected. There is not much we can do. Typically, a UI system should be in place to let the user attemp to connect again.
        if (loaderAnime != null)
        {
            loaderAnime.SetActive(false);
        }

        isConnecting = false;
        controlPanel.SetActive(true);

    }

    /// <summary>
    /// Called when entering a room (by creating or joining it). Called on all clients (including the Master Client).
    /// </summary>
    /// <remarks>
    /// This method is commonly used to instantiate player characters.
    /// If a match has to be started "actively", you can call an [PunRPC](@ref PhotonView.RPC) triggered by a user's button-press or a timer.
    ///
    /// When this is called, you can usually already access the existing players in the room via PhotonNetwork.PlayerList.
    /// Also, all custom properties should be already available as Room.customProperties. Check Room..PlayerCount to find out if
    /// enough players are in the room to start playing.
    /// </remarks>
    public override void OnJoinedRoom()
    {
        LogFeedback("JoinedRoom");
        Debug.Log("OnJoinedRoom() called by PUN. Now this client is in a room.\nFrom here on, your game would be running.");

        // #Critical: We only load if we are the first player, else we rely on  PhotonNetwork.AutomaticallySyncScene to sync our instance scene.
        if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
        {
            // #Critical
            // Load the Room Level. 
            SceneManager.LoadScene("GameScene");
            this.previousRoom = PhotonNetwork.CurrentRoom.Name;
            PlayerPrefs.SetString("CurrentRoom", this.previousRoom);
            PlayerPrefs.SetString("Match Played", GameData.GetInstance().GetRewardManager().MatchPlayed.ToString());
        }
        else
        {
            LogFeedback("Waiting for Another Player");
            Debug.Log("Waiting for another player");
        }
    }

    /// <summary>
    /// Called when a JoinRoom() call failed. The parameter provides ErrorCode and message.
    /// </summary>
    /// <param name="returnCode"></param>
    /// <param name="message"></param>
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        LogFeedback("OnJoinRandomFailed.Creating a new Room");
        Debug.Log("OnJoinRandomFailed() was called by PUN. No random room or named room exist in this lobby available");
        string roomName = LoginManager.GenerateRandomAlphaNumericStr(RoomNameMaxLength);
        if (tryToJoinNamedRoom)
        {
            roomName = RequestedNameRoom.text.Length > 10 ? RequestedNameRoom.text : roomName; //try to create it with passed name, or with random one
            RequestedNameRoom.text = "";
            PublicRoomOption.IsVisible = false;
        } 
        else
        {
            PublicRoomOption.IsVisible = true;
        }
        PhotonNetwork.JoinOrCreateRoom(
            roomName,
            isPrivateRoom ? PrivateRoomOption : PublicRoomOption,
            IsFreeGame ? FreeGamesLobby : TypedLobby.Default
            );
        this.previousRoom = null;
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log("Other player arrived");

        if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
        {
            SceneManager.LoadScene("GameScene");
            this.previousRoom = PhotonNetwork.CurrentRoom.Name;
        }
        if(PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            TakeBet();
        }
    }

    private void TakeBet() 
    {
        GameData.GetInstance().StartGame();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log("Other player disconnected! isInactive: " + otherPlayer.IsInactive);
    }
    #endregion
    private RewardedAd rewardedAd;

    private void ShowAd()
    {
        rewardedAd = new RewardedAd("ca-app-pub-3940256099942544/5224354917");
        // Called when an ad request has successfully loaded.
        rewardedAd.OnAdLoaded += AdManager.HandleRewardedAdLoaded;
        // Called when an ad request failed to load.
        rewardedAd.OnAdFailedToLoad += AdManager.HandleRewardedAdFailedToLoad;
        // Called when an ad is shown.
        rewardedAd.OnAdOpening += AdManager.HandleRewardedAdOpening;
        // Called when an ad request failed to show.
        rewardedAd.OnAdFailedToShow += AdManager.HandleRewardedAdFailedToShow;
        // Called when the user should be rewarded for interacting with the ad.
        rewardedAd.OnUserEarnedReward += this.HandleUserEarnedReward;
        // Called when the ad is closed.
        rewardedAd.OnAdClosed += AdManager.HandleRewardedAdClosed;

        // Create an empty ad request.
        AdRequest request = new AdRequest.Builder().Build();
        // Load the rewarded ad with the request.
        rewardedAd.LoadAd(request);

    }

    private void HandleUserEarnedReward(object sender, Reward args)
    {
        Debug.Log("Ad for Free Plays");
        GameData.GetInstance().GetRewardManager().MatchPlayed = -1;
        Connect();
    }
    public void TryJoinOrCreateRoom(bool random)
    {
        //isFreeGame = false;
        IsRandomMatch = random;

        if (IsFreeGame)
        {
            if (RewardManager.Instance.MaxFreeGames >= RewardManager.Instance.MatchPlayed)
            {
                isFreeGame = true;
                SetupAndConnect();
            }
            else
            {
                //ShowAds.
                AdManager.RequestRewards(HandleUserEarnedReward);
            }
        }
        else //Bet match
        {
            isFreeGame = false;
            if (GameData.GetInstance().gamePoints >= GameData.GetInstance().BetPoints)
            {
                SetupAndConnect();
            }
            else
            {
                //Show BuyPoints Panel??
                m_LoginController.ShowPointsPanel(true);
            }
        }

        void SetupAndConnect()
        {
            if (RequestedNameRoom.text.Length > 0)
            {
                if (RequestedNameRoom.text.Length < 10)
                {
                    ShowRoomNameToolTip();
                } 
                else
                {
                    this.tryToJoinNamedRoom = true;
                    if (PhotonNetwork.NetworkClientState == ClientState.Disconnected || PhotonNetwork.NetworkClientState == ClientState.PeerCreated)
                    {
                        Connect();
                    }
                    else if (PhotonNetwork.NetworkClientState == ClientState.Disconnecting)
                    {
                        PhotonNetwork.Disconnect();
                        Connect();
                    }
                    else if (PhotonNetwork.NetworkClientState == ClientState.ConnectedToMasterServer)
                    {
                        OnConnectedToMaster();
                    }
                }
            }
            else
            {
                this.tryToJoinNamedRoom = false;
                Connect();
            }
        }
    }

    public void ShowRoomNameToolTip()
    {
        m_LoginController.ShowAndHideRoomNameToolTip();
    }
}