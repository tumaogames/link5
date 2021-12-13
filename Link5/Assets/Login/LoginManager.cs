 
using Google;
 
using Firebase;
using Firebase.Auth;
using Facebook.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using System.Linq;
using Link5;
using UnityEngine.Events;
using Photon.Pun;
using UnityEngine.Networking;
//using AppleAuth;
//using AppleAuth.Native;
using System.Text;

public class LoginManager : MonoBehaviour
{
    public LoginManager instance { get; set; }
    private FirebaseAuth auth;
    public FirebaseUser user;
    GameData gameData;

    public UILoginController uiLogin;

    [Header("Buttons")]
    public Button Friends;
    public GameObject AndroidLogInButton;
    public GameObject IosLogInButton;

    [Header("Inputs")]
    //login
    public InputField userNameInput;
    public InputField psswdInput;

    //singn up
    public InputField userNameInputSignUp;
    public InputField psswdInputSignUp;
    public InputField displayNameInput;
    public Text displayNameText;


    public Text FriendsText;
    public Image profilePhoto;
    public Text infoText;
    public string webClientId;
    public bool googleInit;
    public GoogleSignInConfiguration googleSignInConfiguration;

    public UnityEvent OnSignInActions;
    public UnityEvent OnSignOutActions;
    public UnityEvent OnSingUpFail;
    public UnityEvent OnLoginFail;
    public UnityEvent OnGoogleSignUpFail;
    public UnityEvent OnFacebookSignUpFail;

    [Header("Ios")]
    //public IAppleAuthManager IosLoginManager;
    private string  RawNonce;


    public string isError = "";

    private bool applePlatform;

    public bool ApplePlatform
    {
        private get { return applePlatform; }
        set { applePlatform = value; }
    }



    void Awake()
    {
        instance = this;
        //
        if (!FB.IsInitialized)
        {
            // Initialize the Facebook SDK
            FB.Init(InitCallback, OnHideUnity);
        }
        else
        {
            // Already initialized, signal an app activation App Event
            FB.ActivateApp();
        }

        ApplePlatform = Application.platform == RuntimePlatform.OSXEditor
                 || Application.platform == RuntimePlatform.OSXPlayer
                 || Application.platform == RuntimePlatform.IPhonePlayer;

        //if (!googleInit)
        //{
        //    googleSignInConfiguration = new GoogleSignInConfiguration { WebClientId = webClientId, RequestEmail = true, RequestIdToken = true };
        //    googleInit = true;
        //}
        //CheckFirebaseDependencies();
    }

    private void CheckFirebaseDependencies()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                if (task.Result == DependencyStatus.Available)
                    auth = FirebaseAuth.DefaultInstance;
                else
                    AddToInformation("Could not resolve all Firebase dependencies: " + task.Result.ToString());
            }
            else
            {
                AddToInformation("Dependency check was not completed. Error : " + task.Exception.Message);
            }
        });
    }

    // Start is called before the first frame update
    void Start()
    {
        InitializeFirebase();
#if UNITY_STANDALONE_WIN || UNITY_ANDROID
        if (!googleInit && !ApplePlatform)
        {
            googleSignInConfiguration = new GoogleSignInConfiguration { WebClientId = webClientId, RequestEmail = true, RequestIdToken = true };
            googleInit = true;
        }
#else
        Debug.LogError("Unable to initialize Google Auth System in this platform. --Disable--"  + Application.platform);
#endif
        CheckFirebaseDependencies();
        DontDestroyOnLoad(this.gameObject);
        gameData = GameData.GetInstance();

        if(ApplePlatform)
        {
            AndroidLogInButton.SetActive(false);
            IosLogInButton.SetActive(true);
        }
    }

    public void OnClickSignUp()
    {
        //uiLogin.setActiveUnlockUI(true);
        StartCoroutine(SignUp(userNameInputSignUp.text,psswdInputSignUp.text, displayNameInput.text));        
    }

    public void OnClickLogin()
    {
        //uiLogin.setActiveUnlockUI(true);
        StartCoroutine(Login(userNameInput.text, psswdInput.text));
    }

    public void OnClickFacebookButton()
    {
        //uiLogin.setActiveUnlockUI(true);
        SignUpWithFacebook();
    }

    private void SignUpWithFacebook()
    {

        List<string> permissions = new List<string>();
        permissions.Add("public_profile");
        permissions.Add("email");
        FB.LogInWithReadPermissions(permissions, AuthCallback);
    }

       
    private void AuthCallback(ILoginResult result)
    {
        if (FB.IsLoggedIn)
        {
            
            List<string> permissions = new List<string>();

            // AccessToken class will have session details
            var aToken = Facebook.Unity.AccessToken.CurrentAccessToken;
            // Print current access token's User ID
            Debug.Log(aToken.TokenString);
            Debug.Log(aToken.UserId);
                
            Firebase.Auth.Credential credential = Firebase.Auth.FacebookAuthProvider.GetCredential(aToken.TokenString);
            auth.SignInWithCredentialAsync(credential).ContinueWith(task => {
                if (task.IsCanceled)
                {
                    Debug.LogError("SignInWithCredentialAsync was canceled.");
                    //uiLogin.ShowWarningPanel("Sign in was canceled...");
                    return;
                }
                if (task.IsFaulted)
                {
                    Debug.LogError("SignInWithCredentialAsync encountered an error: " + task.Exception);
                    string[] messages;
                    messages = task.Exception.ToString().Split(new string[] { "Firebase.FirebaseException:" }, StringSplitOptions.None);
                    uiLogin.warningMsg = messages.Length >= 2 ? messages[2].Split('.')[0] : "";
                    LoginFail();
                    
                    return;
                }

                user = task.Result;
                Debug.LogFormat("User signed in successfully: {0} ({1})",
                        user.DisplayName, user.UserId);
                   
                LoginSuccess();

            });
            // Print current access token's granted permissions
            //foreach(string perms in aToken.Permissions)
            //{
            // Debug.Log(perms);
            //}
            if (isError.Count() > 0)
            {
                ErrorMessages(isError);
                isError = "";
            }
            //uiLogin.setActiveUnlockUI(false);
        }
        else
        {
            Debug.Log("User cancelled login");
            //uiLogin.setActiveUnlockUI(false);

        }
            
    }

    private IEnumerator SignUp(string _email, string _password, string _username)
    {
        uiLogin.uiLockPanel.gameObject.SetActive(true);
        if (_username == "")
        {
            //If the username field is blank show a warning
            uiLogin.warningMsg = "Missing Username";
            OnSingUpFail.Invoke();
        }
        else
        {
            //Call the Firebase auth signin function passing the email and password
            var RegisterTask = auth.CreateUserWithEmailAndPasswordAsync(_email, _password);
            //Wait until the task completes
            yield return new WaitUntil(predicate: () => RegisterTask.IsCompleted);

            if (RegisterTask.Exception != null)
            {
                //If there are errors handle them
                Debug.LogWarning(message: $"Failed to register task with {RegisterTask.Exception}");
                FirebaseException firebaseEx = RegisterTask.Exception.GetBaseException() as FirebaseException;
                AuthError errorCode = (AuthError)firebaseEx.ErrorCode;                
                uiLogin.warningMsg = AuthErrorHandler(errorCode); 
                OnSingUpFail.Invoke();
            }
            else
            {
                //User has now been created
                //Now get the result
                user = RegisterTask.Result;

                if (user != null)
                {
                    //Create a user profile and set the username
                    UserProfile profile = new UserProfile { DisplayName = _username };

                    //Call the Firebase auth update user profile function passing the profile with the username
                    var ProfileTask = user.UpdateUserProfileAsync(profile);
                    //Wait until the task completes
                    yield return new WaitUntil(predicate: () => ProfileTask.IsCompleted);

                    if (ProfileTask.Exception != null)
                    {
                        //If there are errors handle them
                        Debug.LogWarning(message: $"Failed to register task with {ProfileTask.Exception}");
                        FirebaseException firebaseEx = ProfileTask.Exception.GetBaseException() as FirebaseException;
                        AuthError errorCode = (AuthError)firebaseEx.ErrorCode;
                        uiLogin.warningMsg = "Username Set Failed!";
                        OnSingUpFail.Invoke();
                    }
                    else
                    {
                        //Username is now set
                        //Now return to login screen
                        LoginSuccess();
                        uiLogin.warningMsg = "";
                    }
                }
            }
        }
        uiLogin.uiLockPanel.gameObject.SetActive(false);
    }

    private static string AuthErrorHandler(AuthError errorCode)
    {
        string message;
        switch (errorCode)
        {
            case AuthError.MissingEmail:
                message = "Missing Email";
                break;
            case AuthError.MissingPassword:
                message = "Missing Password";
                break;
            case AuthError.WeakPassword:
                message = "Weak Password";
                break;
            case AuthError.EmailAlreadyInUse:
                message = "Email Already In Use";
                break;
            case AuthError.InvalidEmail:
                message = "Invalid Email";
                break;
            case AuthError.WrongPassword:
                message = "The password is invalid or the user does not have a password";
                break;
            case AuthError.UserNotFound:
                message = "User not found";
                break;
            default:
                message = "Auth Failed!";
                break;
        }

        return message;
    }

    private IEnumerator Login(string _email, string _password)
    {
        uiLogin.uiLockPanel.gameObject.SetActive(true);
        //Call the Firebase auth signin function passing the email and password
        var LoginTask = auth.SignInWithEmailAndPasswordAsync(_email, _password);
        //Wait until the task completes
        yield return new WaitUntil(predicate: () => LoginTask.IsCompleted);

        if (LoginTask.Exception != null)
        {
            //If there are errors handle them
            Debug.LogWarning(message: $"Failed to register task with {LoginTask.Exception}");
            FirebaseException firebaseEx = LoginTask.Exception.GetBaseException() as FirebaseException;
            AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

            uiLogin.warningMsg = AuthErrorHandler(errorCode);
            OnSingUpFail.Invoke();
        }
        else
        {
            //User is now logged in
            //Now get the result
            user = LoginTask.Result;
            Debug.LogFormat("User signed in successfully: {0} ({1})", user.DisplayName, user.Email);
            uiLogin.warningMsg = "";
            LoginSuccess();
        }
        uiLogin.uiLockPanel.gameObject.SetActive(false);
    }


    void InitializeFirebase()
    {
        auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
        auth.StateChanged += AuthStateChanged;
        AuthStateChanged(this, null);
    }

    private void InitCallback()
    {
        if (FB.IsInitialized)
        {
            // Signal an app activation App Event
            FB.ActivateApp();
            // Continue with Facebook SDK
            // ...
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

    void AuthStateChanged(object sender, System.EventArgs eventArgs)
    {
        if (auth.CurrentUser != user)
        {
            bool signedIn = user != auth.CurrentUser && auth.CurrentUser != null;
            if (!signedIn && user != null)
            {
                Debug.Log("Signed out " + user.UserId);
            }
            user = auth.CurrentUser;
            if (signedIn)
            {
                Debug.Log("Signed in " + user.UserId);
                LoginSuccess();
               
            }
        }
    }

    public void LoginSuccess()
    {
        displayNameText.text = String.IsNullOrEmpty(user.DisplayName) ? displayNameInput.text : user.DisplayName;       
        GameData.GetInstance().profilePhotoURI = user.PhotoUrl != null ? user.PhotoUrl.ToString() + "?type=large" : String.Empty;
        GameData.GetInstance().RequestProfilePhoto(profilePhoto, GameData.GetInstance().profilePhotoURI);
        PhotonNetwork.NickName = displayNameText.text;
        userNameInput.text = "";
        userNameInputSignUp.text = "";
        psswdInputSignUp.text = "";
        displayNameInput.text = "";
        psswdInput.text = "";
        OnSignInActions.Invoke();
        Debug.Log("Logged in");
    }

    public void LoginFail()
    {        
        uiLogin.ShowWarningPanel(true);
    }

    public void TrySignIn()
    {
        //Login Based on PLatform
        RuntimePlatform platform = Application.platform;
        if (platform == RuntimePlatform.IPhonePlayer )
        {
            //AppleSign();
        }
        else if (platform == RuntimePlatform.Android)
        {
            SignInWithGoogle();
        }
    }

    public static string GenerateRandomAlphaNumericStr(int desiredLength = 32)
    {
        StringBuilder nonce = new StringBuilder(""); // Requires @ top: using System.Text;
        char[] chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789".ToCharArray();
        char singleChar;

        while (nonce.Length < desiredLength)
        {
            singleChar = chars[UnityEngine.Random.Range(0, chars.Length)];
            nonce.Append(singleChar);
        }

        Debug.Log("Generate Nonce: " + nonce.ToString());

        return nonce.ToString();
    }

    //public void AppleSign()
    //{
    //    RawNonce = GenerateRandomAlphaNumericStr();

    //    IosLoginManager = new AppleAuthManager(new PayloadDeserializer());

    //    IosLoginManager.LoginWithAppleId(new AppleAuthLoginArgs(AppleAuth.Enums.LoginOptions.IncludeEmail | AppleAuth.Enums.LoginOptions.IncludeFullName),
    //        AppleSuccessCallBack<AppleAuth.Interfaces.ICredential>,
    //        AppleFailedCallBack<AppleAuth.Interfaces.IAppleError>);

    //}

    private void TryToLogInFireBaseWithAppleCredentials(string l_appleIdToken, string l_rawNonce)
    {
        Firebase.Auth.Credential credential = Firebase.Auth.OAuthProvider.GetCredential("apple.com", l_appleIdToken, l_rawNonce);
        auth.SignInWithCredentialAsync(credential).ContinueWith((task) =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("Attent to login with apple canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("Attent to login with apple failed.");
                string[] messages;
                messages = task.Exception.ToString().Split(new string[] { "Firebase.FirebaseException:" }, StringSplitOptions.None);
                uiLogin.warningMsg = messages.Length >= 2 ? messages[2].Split('.')[0] : "";
                LoginFail();
                return;
            }
            if (task.IsCompleted)
            {
                user = task.Result;
                Debug.LogFormat("Succesfully log in with Apple ID credentials: {0} {1}", user.DisplayName, user.UserId);
            }
            else
            {
                if (task.Exception != null)
                    Debug.LogError("Exception : " + task.Exception.ToString());
            }
        });
    }

    //private void AppleSuccessCallBack<ICredential>(AppleAuth.Interfaces.ICredential Credential)
    //{
    //    Debug.Log("Successfully Connected to Apple Id");
    //    Debug.Log("Your User Name Is: " + Credential.User);
    //    Debug.Log("Full Credential: " + Credential.ToString());
    //    TryToLogInFireBaseWithAppleCredentials(Credential.User, RawNonce);
    //}

    //private void AppleFailedCallBack<IError>(AppleAuth.Interfaces.IAppleError Error)
    //{
    //    Debug.Log("Could not Connected to Apple Id");
    //    Debug.Log("Error Code: " + Error.Code);
    //    Debug.Log("Error Description: " + Error.LocalizedDescription);
    //}

    public void SignInWithGoogle() 
    {
        //uiLogin.setActiveUnlockUI(true);
        StartCoroutine(OnSignIn());
    }
    public void SignOutFromGoogle() { OnSignOut(); }

    private IEnumerator OnSignIn()
    {
        uiLogin.uiLockPanel.gameObject.SetActive(true);
        GoogleSignIn.Configuration = googleSignInConfiguration;
        GoogleSignIn.Configuration.UseGameSignIn = false;
        GoogleSignIn.Configuration.RequestIdToken = true;
        AddToInformation("Calling SignIn");
        var GoogleSingInTask = GoogleSignIn.DefaultInstance.SignIn();    

        //Wait until the task completes
        yield return new WaitUntil(predicate: () => GoogleSingInTask.IsCompleted);
        OnAuthenticationFinished(GoogleSingInTask);
        
 
    }

    private void OnSignOut()
    {
        AddToInformation("Calling SignOut");

        GoogleSignIn.DefaultInstance.SignOut();
    }

    public void OnDisconnect()
    {
        AddToInformation("Calling Disconnect");
 
        GoogleSignIn.DefaultInstance.Disconnect();
 
    }

    internal void OnAuthenticationFinished(Task<GoogleSignInUser> task)
    {
        if(task.Exception != null)
        {
            using (IEnumerator<Exception> enumerator = task.Exception.InnerExceptions.GetEnumerator())
            {
                if (enumerator.MoveNext())
                {
                    GoogleSignIn.SignInException error = (GoogleSignIn.SignInException)enumerator.Current;
                    AddToInformation("Got Error: " + error.Status + " " + error.Message);

                }
                else
                {
                    AddToInformation("Got Unexpected Exception?!?" + task.Exception);
                }
            }
        }

        if (task.IsFaulted)
        {
            using (IEnumerator<Exception> enumerator = task.Exception.InnerExceptions.GetEnumerator())
            {
                if (enumerator.MoveNext())
                {
                    GoogleSignIn.SignInException error = (GoogleSignIn.SignInException)enumerator.Current;
                    AddToInformation("Got Error: " + error.Status + " " + error.Message);

                }
                else
                {
                    AddToInformation("Got Unexpected Exception?!?" + task.Exception);
                }
            }
        }
        else if (task.IsCanceled)
        {
            AddToInformation("Canceled");
        }
        else
        {
            Debug.Log("Welcome: " + task.Result.DisplayName + "!");
            Debug.Log("Email = " + task.Result.Email);
            Debug.Log("Google ID Token = " + task.Result.IdToken);
            Debug.Log("Email = " + task.Result.Email);
            StartCoroutine(SignInWithGoogleOnFirebase(task.Result.IdToken));      
        }
 

    }

    private IEnumerator SignInWithGoogleOnFirebase(string idToken)
    {
 
        Credential credential = GoogleAuthProvider.GetCredential(idToken, null);

        var SignInGoogleCredentialTask = auth.SignInWithCredentialAsync(credential);
        yield return new WaitUntil(predicate: () => SignInGoogleCredentialTask.IsCompleted);
        if (SignInGoogleCredentialTask.Exception != null)
        {
            //If there are errors handle them
            Debug.LogWarning(message: $"Failed to register task with {SignInGoogleCredentialTask.Exception}");
            FirebaseException firebaseEx = SignInGoogleCredentialTask.Exception.GetBaseException() as FirebaseException;
            AuthError errorCode = (AuthError)firebaseEx.ErrorCode;
            uiLogin.warningMsg = AuthErrorHandler(errorCode);
            OnSingUpFail.Invoke();
        }
        else
        {
            //User is now logged in
            //Now get the result
            user = SignInGoogleCredentialTask.Result;
            Debug.LogFormat("User signed in successfully: {0} ({1})", user.DisplayName, user.Email);
            uiLogin.warningMsg = "";
            LoginSuccess();
        }
        
        uiLogin.uiLockPanel.gameObject.SetActive(false);
    }

    public void OnSignInSilently()
    {
        AddToInformation("Calling SignIn Silently");
 
        GoogleSignIn.Configuration = googleSignInConfiguration;
        GoogleSignIn.Configuration.UseGameSignIn = false;
        GoogleSignIn.Configuration.RequestIdToken = true;
        //uiLogin.setActiveUnlockUI(true);
        GoogleSignIn.DefaultInstance.SignInSilently().ContinueWith(OnAuthenticationFinished);
 
    }

    public void OnGamesSignIn()
    {
        AddToInformation("Calling Games SignIn");
        infoText.text = "";
        GoogleSignIn.Configuration = googleSignInConfiguration;
        GoogleSignIn.Configuration.UseGameSignIn = true;
        GoogleSignIn.Configuration.RequestIdToken = false;

        //uiLogin.setActiveUnlockUI(true);
        GoogleSignIn.DefaultInstance.SignIn().ContinueWith(OnAuthenticationFinished);
 

    }

    private void AddToInformation(string str)
    {
        //infoText.text += str;
    }

    public void ErrorMessages(string message)
    {
        string[] messages;            
        messages = message.Split(new string[] { "Firebase.FirebaseException:" }, StringSplitOptions.None);
        uiLogin.warningMsg = messages.Length >= 2 ? messages[2].Split('.')[0] : "";
        LoginFail();
    }


    public Texture2D ToTexture2D(Texture texture)
    {
        return Texture2D.CreateExternalTexture(
            texture.width,
            texture.height,
            TextureFormat.RGB24,
            false, false,
            texture.GetNativeTexturePtr());
    }

    public void SignOut()
    {
        auth.SignOut();
        GameData.GetInstance().profilePhotoURI = null;
        GameData.GetInstance().profilePhoto = null;
        OnSignOutActions.Invoke();
    }

}


    //public void GetFriendsPlayingThisGame()
    //{
    //    string query = "/me/friends";
    //    FB.API(query, HttpMethod.GET, result =>
    //    {
    //        var dictionary = (Dictionary<string, object>)Facebook.MiniJSON.Json.Deserialize(result.RawResult);
    //        var friendsList = (List<object>)dictionary["data"];
    //        string friends = string.Empty;
    //        foreach (var dict in friendsList)
    //            friends += ((Dictionary<string, object>)dict)["name"];

    //            Debug.Log(friends);
    //    });

           
    //}



