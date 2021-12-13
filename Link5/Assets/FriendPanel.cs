using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FriendPanel : MonoBehaviour
{
    public GameObject FriendsPanelHidePosition;
    public GameObject FriendsPanelShowPosition;
    public bool FriendsListAvailable;
    public bool FriendsPanelIsShow;
    public GameObject ContactItemPrefab;
    public GameObject ContactDividerPrefab;
    public Transform ContactsContainer;
    public LeanTweenType Ease;

    private string FRIEND_LIST_KEY = "friends_list";

    private static FriendPanel m_Instance;

    public static FriendPanel Instance { get { return m_Instance; } private set { m_Instance = value; } }

    // Start is called before the first frame update

    private void Awake()
    {
        if(m_Instance == null)
        {
            m_Instance = this;
        }
        else
        {
            GameObject.Destroy(this);
        }
    }

    public void AddFriend(string friendNick, string friendUserId)
    {
        string friendsList = PlayerPrefs.GetString(FRIEND_LIST_KEY, "");
        var friendsArray = friendsList.Split(',');
        bool alreadyAdded = false;
        foreach (string f in friendsArray)
        {
            if(friendUserId == f.Split(':')[1])
            {
                alreadyAdded = true;
                break;
            }
        }
        if(!alreadyAdded)
        {
            StringBuilder stringBuilder = new StringBuilder(friendsList);
            stringBuilder.Append(friendsList).Append(friendNick).Append(':').Append(friendUserId).Append(',');
            PlayerPrefs.SetString(FRIEND_LIST_KEY, stringBuilder.ToString());
        }
    }
    public void DeleteFriend(string friendUserId)
    {
        string friendsList = PlayerPrefs.GetString(FRIEND_LIST_KEY, "");
        List<string> newFriendList = new List<string>();
        var friendsArray = friendsList.Split(',');
        bool alreadyAdded = false;
        foreach (string f in friendsArray)
        {
            if (friendUserId == f.Split(':')[1])
            {
                break;
            }
            else
            {
                newFriendList.Add(f);
                newFriendList.Add(",");
            }
        }
        if (!alreadyAdded)
        {
            PlayerPrefs.SetString(FRIEND_LIST_KEY, newFriendList.ToString());
        }
    }

    public void DeleteAllFriends()
    {
        PlayerPrefs.SetString(FRIEND_LIST_KEY, "");
    }

    private string[] GetFriendIdList()
    {
        string friendsList = PlayerPrefs.GetString(FRIEND_LIST_KEY, "");
        string[] friendsArray = friendsList.Split(',');
        string[] result = new string[friendsArray.Length];
        for (int i = 0; i < friendsArray.Length; i++)
        {
            if (!string.IsNullOrEmpty(friendsArray[i]))
            {
                result[i] = friendsArray[i];
            }
        }
        return friendsArray.Length > 0 ? result : new string[0];
    }

    public void ShowFriendsPanel()
    {
        if (FriendsListAvailable)
        {
            if (FriendsPanelIsShow)
            {
                FriendsPanelIsShow = !FriendsPanelIsShow;
                LeanTween.moveLocalY(gameObject, FriendsPanelHidePosition.transform.localPosition.y, .7f).setEase(Ease);

            }
            else
            {
                FriendsPanelIsShow = !FriendsPanelIsShow;
                LeanTween.moveLocalY(gameObject, FriendsPanelShowPosition.transform.localPosition.y, .7f).setEase(Ease);
            }
        }
    }

    public void ShowFriends()
    {
        string[] Addedfriends = GetFriendIdList();
        foreach(string friend in Addedfriends)
        {
            if (friend == null) continue;
            GameObject contactItem = Instantiate(ContactItemPrefab, ContactsContainer);
            Text tt = contactItem.GetComponentInChildren<Text>();
            string[] friendData = friend.Split(':');
            if (tt != null)
            {
                tt.text = friendData[0];
            }
            Button[] bt = contactItem.GetComponentsInChildren<Button>(true);
            if (bt != null)
            {
                //bt[1].onClick.AddListener(delegate { OnInviteButtonClicked(friendData[1]); });
                //bt[0].onClick.AddListener(delegate { DeleteFriend(friendData[1]); });
                //bt[0].gameObject.SetActive(true);
            }
        }
        //Contacts Divider
        ///Instantiate(ContactDividerPrefab, ContactsContainer);

        foreach (Contact contact in Contacts.ContactsList)
        {
            if (contact.Phones.Count > 0 && !string.IsNullOrEmpty(contact.Phones[0].Number) && !string.IsNullOrEmpty(contact.Name))
            {
                GameObject contactItem = Instantiate(ContactItemPrefab, ContactsContainer);
                ContactItem ContactData = contactItem.GetComponent<ContactItem>();
                ContactData.Name = contact.Name;
                ContactData.Number = contact.Phones[0].Number;
                ContactData.Index = contact.Id;
                Text tt = contactItem.GetComponentInChildren<Text>();
                if (tt != null) tt.text = ContactData.Name;
                Button bt = contactItem.GetComponentInChildren<Button>();
                if (bt != null) bt.onClick.AddListener(delegate { OnInviteButtonClicked(ContactData.Number); });
            }
        }
         FriendsListAvailable = true;
    }

    public void OnInviteButtonClicked(string number)
    {
        Debug.Log("Button Clicked for " + number + "------------");
        string invitation = DeepLinking.Instance.GenerateFriendInvitationLink();
        //Contact contact = Contacts.ContactsList.Find( cont => cont.Id == index);
        var uri = System.Uri.EscapeDataString(invitation);
        //string url = String.Format("sms:{0}?body={1}", number, uri);
        string url = String.Format("whatsapp://send?phone={0}&text={1}", number, uri);
        

#if !UNITY_EDITOR && UNITY_ANDROID
        SendAsIntent(invitation);
#else
        Application.OpenURL(uri);
#endif
        //Application.OpenURL("whatsapp://send?phone=9999878653&text=[hello]");
    }

    private void SendAsIntent(string invitation)
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
}
