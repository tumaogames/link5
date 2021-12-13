using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
using TMPro;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using UnityEngine.SceneManagement;
using Link5;
using static TutorialManager;
using GoogleMobileAds.Api;
using System.Runtime.InteropServices;
using UnityEngine.Events;

/// <summary>
/// DataContainer for Each Turn's Move
/// </summary>
public class MoveData
{
    public Vector3 ChipPosition { get; set; }
    public GameObject Chip { get; set; }
}

public enum ResultType
{
    None = 0,
    Draw,
    LocalWin,
    LocalLoss
}

[SerializeField]
public enum GameMode
{
    Tutorial = 0,
    SinglePlayer = 1,
    MultiPlayer = 2
}

public enum MatchType
{
    Free,
    Ranked
}

public enum TutorialCondition
{
    LowerTitleClicked,
    HigherTileClicked,
    HigherOrEqualClicked,
    YellowTileClicked,
    NoCondition
}

// the Photon server assigns a ActorNumber (player.ID) to each player, beginning at 1
// for this game, we don't mind the actual number
// this game uses player 0 and 1, so clients need to figure out their number somehow
public class GameManager : MonoBehaviourPunCallbacks, IPunTurnManagerCallbacks
{

#if UNITY_STANDALONE_OSX || UNITY_IOS || UNITY_MAC
    [DllImport("__Internal")]
    private static extern void ShareMatchLink(string message);
#endif

    public static GameManager Instance;
    public AdMobManager AdManager;

    public UIManager UIManager;

    private ITutorialManagerCallbacks TutManager;
    public bool TutorialConditionMeet;
    public TutorialCondition TutorialCondition;
    [SerializeField]
    private float TurnDuration;
    [SerializeField]
    private string currentRoom;
    [SerializeField]
    private InputField CurrentRoomInput;
    public Text currentRoomText;
    public string BetRoomName = "link5BetRoom";

    [SerializeField]
    private PlayerController PlayerController;

    [SerializeField]
    private Image TimerFillImage;

    [SerializeField]
    private TextMeshProUGUI TurnText;

    [SerializeField]
    private TextMeshProUGUI TimeText;

    public Text RemotePlayerText;
    public Image RemotePlayerImage;
    public Text LocalPlayerText;
    public Image LocalPlayerImage;
    [Header("Tutorial")]
    public TutorialManager TutorialPanel;
    public Text TutorialTitle;
    public Text TutorialMessage;
    public float TutorialDelayTime = .5f;
    [Range(0, 10)]
    public int TutorialCurrentStep = 0;
    public Transform ContactsContiener;
    public GameObject ContactItemPrefab;
    public bool FriendsListAvailable = false;
    public RectTransform FriendsPanelShowPosition;
    public RectTransform FriendsPanelHidePosition;
    public GameObject FriendsPanel;
    public bool FriendsPanelIsShow;

    [Header("Rewards")]
    public MatchType MatchType;
    public LeanTweenType testEase;
    private bool MatchCountUpdated = false;

    public GameObject localSelection;   
    public GameObject remoteSelection;
    public bool isGameOver;
    public static bool TutorialMode = false;
    public bool tutorialEndedTurn = false;
    public List<Chip> localChips;
    public List<Chip> remoteChips;

    //[SerializeField]
    private RectTransform DisconnectedPanel;

    public GameMode gameMode;
    private ResultType result;

    public PunTurnManager turnManager;

    // keep track of when we show the results to handle game logic.
    private bool IsShowingResults;
    internal bool isGamePaused;
    public UnityEvent OnPlayerDisconnected;

    private void Awake()
    {
        this.TutManager = GetComponent<TutorialManager>();
        this.AdManager = GameObject.FindObjectOfType<AdMobManager>();
        this.currentRoom = PhotonNetwork.CurrentRoom.Name;
        string MPlayed = PlayerPrefs.GetString("Match Played");
        this.currentRoomText.text = "room Name";
        CurrentRoomInput.text =  this.currentRoom;
        TutorialCondition = TutorialCondition.NoCondition;
        this.TutManager.Init();
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(Instance.gameObject);
        }

        switch (PhotonNetwork.OfflineMode)
        {
            case true:
                if (TutorialMode) {
                    gameMode = GameMode.Tutorial;
                    this.StartTutorial(); 
                }else
                {
                    gameMode = GameMode.SinglePlayer;
                    this.StartTurn();
                    PlayerController.StopPlayerInputs = false;
                }
                PhotonNetwork.CurrentRoom.SetTurnTimer();
                break;
            case false:
                gameMode = GameMode.MultiPlayer;
                PlayerController.StopPlayerInputs = false;
                break;
            default:
                break;
        }
    }

    public void Start()
    {
        //this.turnManager = this.gameObject.AddComponent<PunTurnManager>();
        if(Contacts.ContactsList.Count > 0)
        {
            OnContactsLoadDone();
        }
        StartMatch();
    }

    private void StartMatch()
    {

#if UNITY_EDITOR
        Debug.Log("Disconnting Points");
#endif

        this.turnManager.TurnDuration = this.TurnDuration;
        this.turnManager.TurnManagerListener = this;
        GameData.GetInstance().StartGame();
        RefreshUIViews();
    }

    public void Update()
    {

        // for debugging, it's useful to have a few actions tied to keys:
        if (Input.GetKeyUp(KeyCode.L))
        {
            PhotonNetwork.LeaveRoom();
        }
        if (Input.GetKeyUp(KeyCode.C))
        {
            PhotonNetwork.GameVersion = "1.0";
            PhotonNetwork.ConnectUsingSettings();
        }


        if (!PhotonNetwork.InRoom)
        {
            return;
        }

        if (isGameOver)
        {
            if(!MatchCountUpdated)
            {
                RewardManager.Instance.MatchPlayed++;
                MatchCountUpdated = true;
            }
            return;
        }

        // disable the "reconnect panel" if PUN is connected or connecting
        //if (PhotonNetwork.IsConnectedAndReady && this.DisconnectedPanel.gameObject.activeInHierarchy)
        //{
        //    this.DisconnectedPanel.gameObject.SetActive(false);
        //}
        //if (!PhotonNetwork.IsConnectedAndReady /*&& !PhotonNetwork.connecting*/ && !this.DisconnectedPanel.gameObject.activeInHierarchy)
        //{
        //    this.DisconnectedPanel.gameObject.SetActive(true);
        //}


        if (PhotonNetwork.CurrentRoom.PlayerCount > 1)
        {
            float turnEnd = this.turnManager.RemainingSecondsInTurn;

            // check if we ran out of time, in which case we loose
            if (turnEnd < 0f && !IsShowingResults)
            {
                Debug.Log("Calling OnTurnCompleted with turnEnd =" + turnEnd);
                OnTurnCompleted(-1);
                return;
            }

            if (this.TurnText != null)
            {
                this.TurnText.text = this.turnManager.Turn.ToString();
            }

            if (this.turnManager.Turn > 0 && this.TimeText != null && !IsShowingResults)
            {
                int minutes = Mathf.FloorToInt(turnEnd / 60f);
                int seconds = Mathf.FloorToInt(turnEnd - minutes * 60f);
                this.TimeText.text = string.Format("{0:0}:{1:00}", minutes, seconds);
                TimerFillImage.fillAmount = 1f - turnEnd / this.turnManager.TurnDuration;
            }
        }

        this.UpdatePlayerTexts();

        // show local player's selected hand

        // remote player's selection is only shown, when the turn is complete (finished by both)
        if (this.turnManager.IsCompletedByAll)
        {
            
        }
        else
        {
            if (PhotonNetwork.CurrentRoom.PlayerCount < 2)
            {
                
            }

            // if the turn is not completed by all, we use a random image for the remote hand
            else if (this.turnManager.Turn > 0 && !this.turnManager.IsCompletedByAll)
            {
                // alpha of the remote hand is used as indicator if the remote player "is active" and "made a turn"
                Player remote = PhotonNetwork.PlayerListOthers[0];

                if (this.turnManager.GetPlayerFinishedTurn(remote))
                {

                }
                if (remote != null && remote.IsInactive)
                {
                    
                }

                string result = (string)PhotonNetwork.CurrentRoom.CustomProperties["Result"];
                if (result == "LocalWin" && !isGameOver)
                {
                    ShowResultAndGameOver(ResultType.LocalLoss);
                }
            }
        }

    }

    private void ShowAds()
    {
        RewardedAd rewardedAd = new RewardedAd("ca-app-pub-3940256099942544/5224354917");
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
        Debug.Log("Reward Adquired. Resetting free plays");
        RewardManager.Instance.MatchPlayed = 0;
    }

    public void PlayerWrongMove()
    {
        PlayerController.isPlayerTurn = true;
    }

    public void SetRoomProperties(Hashtable data)
    {

        PhotonNetwork.CurrentRoom.SetCustomProperties(data);
    }

#region TurnManager Callbacks
    /// <summary>Called when a turn begins (Master Client set a new Turn number).</summary>
    public void OnTurnBegins(int turn)
    {
        UIManager.SetTurnStatusText("Turn " + turn + " Begins");

        PlayerController.isPlayerTurn = true;
        IsShowingResults = false;
    }

    public void OnTurnCompleted(int obj)
    {
        //UIManager.SetTurnStatusText("Turn " + turnManager.Turn + " Completed");

        //this.CalculateWinAndLoss();
        this.UpdateScores();
        this.OnEndTurn();
    }

    // when a player moved (but did not finish the turn)
    public void OnPlayerMove(Player photonPlayer, int turn, object move)
    {
        AutoPlayerMove();
        chipPlaced = true;
    }

    // when a player made the last/final move in a turn
    public void OnPlayerFinished(Player photonPlayer, int turn, object move)
    {
        UIManager.SetTurnStatusText(photonPlayer.NickName + " Finished Turn!");

        if (photonPlayer.IsLocal)
        {
            switch (gameMode)
            {
                case GameMode.Tutorial:
                    ShowTutorialPanel();
                    break;
                case GameMode.SinglePlayer:
                    AutoPlayerMove();
                    break;
                case GameMode.MultiPlayer:
                    PlayerController.isPlayerTurn = false;
                    string result = (string)PhotonNetwork.CurrentRoom.CustomProperties["Result"];
                    Debug.Log(result);
                    break;
                default:
                    break;
            }
        }
        else
        {
            Vector3 remotePosition = (Vector3)PhotonNetwork.CurrentRoom.CustomProperties["RemoteMove"];
            PlayerController.SpawnChip(Instance.remoteSelection, remotePosition, false);
            Tile remoteTile = BoardManager.Instance.FindTileByPosition(remotePosition);
            remoteTile.isOccupied = true;
        }

        Debug.Log(photonPlayer.NickName + " Finished Turn!");
    }
    private Coroutine movedCoroutine;
    private IEnumerator MovedSinglePlayer()
    {
        yield return new WaitUntil(() =>
            TutorialMode && tutorialEndedTurn);

        yield return new WaitForSeconds(TutorialDelayTime);
        tutorialEndedTurn = false;
        AutoPlayerMove();
    }

    public void WaitUserMove()
    {
        if (movedCoroutine != null)
            StopAllCoroutines();
        movedCoroutine = StartCoroutine(MovedSinglePlayer());
    }

    private void AutoPlayerMove()
    {
        Vector3 aiPosition = BoardManager.Instance.FindRandomTilePosition();
        PlayerController.SpawnChip(Instance.remoteSelection, aiPosition, false);
        Tile aiTile = BoardManager.Instance.FindTileByPosition(aiPosition);
        aiTile.isOccupied = true;
        MoveData moveData = new MoveData();
        moveData.ChipPosition = aiPosition;
        MakeTurn(gameMode == GameMode.Tutorial ? 2 : 1, moveData);
    }

    public void TutorialModalNextButtonClick()
    {
        if(TutManager.CurrentStep() == 0)
        {
            this.StartTurn();
            TutManager.NextStep();
            ShowTutorialPanel();
            return;
        }else if (TutManager.CurrentStep() == 1 || TutManager.CurrentStep() == 2)
        {
            TutorialConditionMeet = true;
        }
        else
        {
            PlayerController.isPlayerTurn = true;
            PlayerController.StopPlayerInputs = false;
            SetTextFieldForTutorialPanel();
        }
        TutorialCurrentStep++;
        if(TutorialConditionMeet)
        {
            TutManager.NextStep();
            TutorialConditionMeet = false;
            TutorialCondition = SetCurrentTutorialCondition(TutManager.CurrentStep());
        }
        PlayerController.StopPlayerInputs = false;
        TutorialPanel.ShowPanel(false);
    }

    private TutorialCondition SetCurrentTutorialCondition(int currentTutorialStep)
    {
        if (currentTutorialStep == 4)
            return TutorialCondition.YellowTileClicked;
        return TutorialCondition.NoCondition;
    }

    public void ShowTutorialPanel(int MessageIndex = 0)
    {
        MessageStep Message = TutManager.GetMessage(MessageIndex);
        if(Message != null)
        {
            SetTextFieldForTutorialPanel(Message.Title, Message.Content);
            PlayerController.StopPlayerInputs = true;
            TutorialPanel.ShowPanel();
        }else
        {
            WaitUserMove();
        }
    }

    private void SetTextFieldForTutorialPanel(string Title = "", string Message = "")
    {
        TutorialTitle.text = Title;
        TutorialMessage.text = Message;
    }

    public void OnTurnTimeEnds(int obj)
    {
        // not yet implemented.
        PlayerController.isPlayerTurn = false;
        UIManager.SetTurnStatusText("Time Out. Game Over!");
    }

    private void UpdateScores()
    {
        if (this.result == ResultType.LocalWin)
        {
            PhotonNetwork.LocalPlayer.AddScore(1);   // this is an extension method for PhotonPlayer. you can see it's implementatin
        }
    }
#endregion

#region Core Gameplay Methods
    /// <summary>Call to start the turn (only the Master Client will send this).</summary>
    public void StartTurn()
    {
        Debug.Log("StartTurn");
        if (PhotonNetwork.IsMasterClient)
        {
            this.turnManager.BeginTurn();
        }
    }

    private void StartTutorial() 
    {
        PlayerController.StopPlayerInputs = true;
        RemotePlayerText.text = "Tutorial";
        Sprite resourcesSprite =  Resources.Load<Sprite>("Sprites/unknowuser");
        RemotePlayerImage.sprite = resourcesSprite;
        ShowTutorialPanel();
    }

    public void MakeTurn(int selection, MoveData moveData)
    {
        PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "RemoteMove", moveData.ChipPosition } });
        PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "Result", "None" } });
        this.StartCoroutine(CalculateResult());
        this.turnManager.SendMove((byte)selection, true);
    }

    public void OnReceivedTurn()
    {
        Debug.Log("Turn Received");
    }

    public void OnEndTurn()
    {
        Debug.Log("Turn Ended");
        this.StartTurn();
    }

    public IEnumerator CalculateResult()
    {
        //yield return new WaitForSeconds(1.50f);

        IsShowingResults = true;
        ScannerManager.Instance.Scan();
        yield return new WaitForSeconds(0);
    }

    public void ShowResultAndGameOver(ResultType resultType)
    {
        isGameOver = true;
        Hashtable data = new Hashtable();

        switch (resultType)
        {
            case ResultType.LocalWin:
                data.Add("Result", "LocalWin");
                PhotonNetwork.CurrentRoom.SetCustomProperties(data);
                UIManager.ShowGameOverUI(resultType);
                GameData.GetInstance().WonGame();
                break;
            case ResultType.LocalLoss:
                data.Add("Result", "LocalLose");
                PhotonNetwork.CurrentRoom.SetCustomProperties(data);
                UIManager.ShowGameOverUI(resultType);
                break;
            default:
                break;
        }
    }

    public void EndGame()
    {
        UIManager.SetTurnStatusText("Turn " + turnManager.Turn + " Completed");
    }

    private void CalculateWinAndLoss()
    {
        this.result = ResultType.Draw;
    }

    private void UpdatePlayerTexts()
    {
        Player remote = null;
        if (PhotonNetwork.PlayerListOthers.Length > 0)
        {
            remote = PhotonNetwork.PlayerListOthers[0]; 
        }
        Player local = PhotonNetwork.LocalPlayer;
                   
        if (remote != null)
        {
            this.RemotePlayerText.text = remote.NickName;
            this.RemotePlayerImage.rectTransform.parent.GetComponent<Image>().color = Color.black;
            if(GameData.GetInstance().remoteProfile == GameData.RemoteProfile.Default)
            {
                GameData.GetInstance().remoteProfile = GameData.RemoteProfile.Requesting;
                GameData.GetInstance().RequestProfilePhoto(this.RemotePlayerImage, (String)remote.CustomProperties["UserProfilePic"]);
            }
        }
        else
        {
            TimerFillImage.fillAmount = 0;
            this.RemotePlayerText.text = gameMode == GameMode.Tutorial ? "Tutorial" : "Disconnected";
            UIManager.SetTurnStatusText("Waiting for Opponent");
        }

        if (local != null)
        {
            this.LocalPlayerText.text = local.NickName;
            if (FacebookManager.Instance && FacebookManager.Instance.isLoggedIn)
            {
                
            }

            this.LocalPlayerImage.sprite = GameData.GetInstance().profilePhoto;
            this.LocalPlayerImage.rectTransform.parent.GetComponent<Image>().color = Color.black;
        }
    }

    private void RefreshUIViews()
    {
        if (isGameOver)
        {
            return;
        }

        UIManager.RefreshConnectionPanel(PhotonNetwork.InRoom && PhotonNetwork.CurrentRoom.PlayerCount == 1);
    }
    private bool chipPlaced;

    public void DisconnectPlayer()
    {
        this.RemotePlayerImage = null;
        if(!chipPlaced) // Only refund points is not chip have been place.
        {
            GameData.GetInstance().RefundPoints();
        }
        OnPlayerDisconnected.Invoke();
        PhotonNetwork.Disconnect();
    }
#endregion

#region Handling Of Buttons
    public void OnClickConnect()
    {
        PhotonNetwork.ConnectUsingSettings();
        //PhotonHandler.StopFallbackSendAckThread();  // this is used in the demo to timeout in background!
    }

    public void OnClickReConnectAndRejoin()
    {
        PhotonNetwork.ReconnectAndRejoin();
        //PhotonHandler.StopFallbackSendAckThread();  // this is used in the demo to timeout in background!
    }
#endregion

#region MonobehaviourPunCallbacks
    public override void OnLeftRoom()
    {
        Debug.Log("OnLeftRoom()");
        RefreshUIViews();
    }

    public override void OnJoinedRoom()
    {
        RefreshUIViews();
        if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            if (this.turnManager.Turn == 0)
            {
                // when the room has two players, start the first turn (later on, joining players won't trigger a turn)
                UIManager.StopAllCoroutines();
                this.StartTurn();
                PhotonNetwork.CurrentRoom.SetTurnTimer();
            }
        }
        else
        {
            Debug.Log("Waiting for another player");
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        RefreshUIViews();
        if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            if (this.turnManager.Turn == 0)
            {
                // when the room has two players, start the first turn (later on, joining players won't trigger a turn)
                UIManager.StopAllCoroutines();
                this.StartTurn();
                PhotonNetwork.CurrentRoom.SetTurnTimer();
            }
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log("Other player disconnected! isInactive: " + otherPlayer.IsInactive);
        UIManager.SetFeedbackText(otherPlayer.NickName + " Disconnected!" + System.Environment.NewLine + 
            "Waiting for Opponents");
        //GameData.GetInstance().WonGame();
        RefreshUIViews();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log("Player disconnected!: " + cause.ToString());
        RefreshUIViews();
        if (PhotonNetwork.LocalPlayer.IsLocal)
        {
            if(PhotonNetwork.CurrentLobby != null) // We are in a Lobby, on disconnect Logout from Lobby
            {
                DisconnectFromFreeLobby();
            }
            PlayerPrefs.SetFloat(GameData.POINTS_REMAINING_KEY, (float) GameData.GetInstance().gamePoints);
            SceneManager.LoadScene("MainMenuScene");
        }
    }
    private Coroutine DisconnectFromLobbyCoroutine;

    internal void DisconnectFromFreeLobby()
    {
        if (DisconnectFromLobbyCoroutine != null)
        {
            StopCoroutine(DisconnectFromLobbyCoroutine);
            DisconnectFromLobbyCoroutine = null;
        }
        DisconnectFromLobbyCoroutine = StartCoroutine(DisconnectFromLobby());
    }
    public IEnumerator DisconnectFromLobby()
    {
        PhotonNetwork.LeaveLobby();
        yield return null;
    }

#endregion

    public void OnContactsLoadDone()
    {
        foreach( Contact contact in Contacts.ContactsList)
        {
            if(contact.Phones.Count > 0 && !String.IsNullOrEmpty(contact.Phones[0].Number) && !String.IsNullOrEmpty(contact.Name))
            {
                GameObject contactItem = Instantiate(ContactItemPrefab, ContactsContiener);
                ContactItem ContactData = contactItem.GetComponent<ContactItem>();
                ContactData.Name = contact.Name;
                ContactData.Number = contact.Phones[0].Number;
                ContactData.Index = contact.Id;
                Text tt = contactItem.GetComponentInChildren<Text>();
                    if(tt != null) tt.text = ContactData.Name;
                Button bt = contactItem.GetComponentInChildren<Button>();
                if(bt != null) bt.onClick.AddListener(delegate { OnInviteButtonClicked(ContactData.Number); });
            }

        }
        FriendsListAvailable = true;
    }

    void OnContactsFailed(string reason)
    {
        Debug.LogError("Contacts load failed " + reason);
    }

    public void OnInviteButtonClicked(string number)
    {
        Debug.Log("Button Clicked for " + number + "------------");
        string invitation = DeepLinking.Instance.GenerateFriendInvitationLink();
        //Contact contact = Contacts.ContactsList.Find( cont => cont.Id == index);
        if (invitation == "") return;
        var uri = System.Uri.EscapeDataString(invitation);
        //string url = String.Format("sms:{0}?body={1}", number, uri);
        //string url = String.Format("whatsapp://send?phone={0}&text={1}", number, uri);

#if !UNITY_EDITOR && UNITY_ANDROID
        SendAsAndroidIntent(invitation);
#elif UNITY_STANDALONE_OSX || UNITY_IOS || UNITY_MAC
        SendiOSText(uri);
#endif
        //Application.OpenURL("whatsapp://send?phone=9999878653&text=[hello]");
    }

#if UNITY_STANDALONE_OSX || UNITY_IOS || UNITY_MAC
    private void SendiOSText(string message)
    {
        Debug.Log("Trying to shared Link with iOS");
        ShareMatchLink(message);
    }
#endif

#if !UNITY_EDITOR && UNITY_ANDROID
    private void SendAsAndroidIntent(string invitation)
    {
        AndroidJavaClass intentClass = new
                 AndroidJavaClass("android.content.Intent");
        AndroidJavaObject intentObject = new
                         AndroidJavaObject("android.content.Intent");

        //set action to that intent object   
        intentObject.Call<AndroidJavaObject>
        ("setAction", intentClass.GetStatic<string>("ACTION_SEND"));

        string shareSubject = "I challenge you to beat me on Link5" +
                   "Fire Block";
        string shareMessage = "I challenge you to beat me on Link5 " + "Play now or download the app from the link below.\nCheers\n\n" + invitation;
        //set the type as text and put extra subject and text to share
        intentObject.Call<AndroidJavaObject>("setType", "text/plain");
        intentObject.Call<AndroidJavaObject>("putExtra",
                    intentClass.GetStatic<string>("EXTRA_SUBJECT"), shareSubject);
        intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_TEXT"), shareMessage);

        AndroidJavaClass unity = new
                AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject currentActivity =
                     unity.GetStatic<AndroidJavaObject>("currentActivity");

        //call createChooser method of activity class     
        AndroidJavaObject chooser =
                intentClass.CallStatic<AndroidJavaObject>("createChooser",
                             intentObject, "Share your high score");
        currentActivity.Call("startActivity", chooser);
    }
#endif
                           
    public void ShowFriendsPanel()
    {
        if (FriendsListAvailable)
        {
            if(FriendsPanelIsShow)
            {
                FriendsPanelIsShow = !FriendsPanelIsShow;
                LeanTween.moveLocalY(FriendsPanel, FriendsPanelHidePosition.transform.localPosition.y, .7f).setEase(testEase);

            } else
            {
                FriendsPanelIsShow = !FriendsPanelIsShow;
                LeanTween.moveLocalY(FriendsPanel, FriendsPanelShowPosition.transform.localPosition.y, .7f).setEase(testEase);
            }
        }
    }
}