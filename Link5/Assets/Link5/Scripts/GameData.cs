//#define NOT_DISCOUNT_POINTS
using Firebase.Auth;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Runtime.CompilerServices;

namespace Link5
{
    public class GameData : MonoBehaviour
    {
        [SerializeField]
        private const double PROFIT = 0.10;
        private static GameData m_instance;
        public GameObject m_gameObject;
        public FirebaseAuth auth;
        public FirebaseUser user;
        public string profilePhotoURI;
        public Sprite profilePhoto;
        public Sprite defaultProfilePhoto;
        public Sprite requestingImg;
        public bool googleInit;
        public bool isRequesting;
        public Text pointsText;
        public double gamePoints = 150;
        [SerializeField]
        private double betPoints = 50;
        public enum RemoteProfile
        {
            Default, 
            Requesting,
            Retrieved
        };
        public RemoteProfile remoteProfile;
        public static readonly string POINTS_REMAINING_KEY;

        public double BetPoints { get { return betPoints; } private set { betPoints = 50; } }
        private void Awake()
        {
            GameData gameData = GameObject.FindObjectOfType<GameData>();
            if (gameData.gameObject == this.gameObject)
            {
                m_instance = gameData;
                DontDestroyOnLoad(m_gameObject);
#if NOT_DISCOUNT_POINTS
                if (PlayerPrefs.HasKey(POINTS_REMAINING_KEY))
                {
                    gamePoints = PlayerPrefs.GetFloat(POINTS_REMAINING_KEY);
                }
#endif
                pointsText.text = gamePoints.ToString();
            }
            else
            {
                GameObject.Destroy(this.gameObject);
            }
        }

        private void Start()
        {
            remoteProfile = RemoteProfile.Default;
        }

        public static GameData GetInstance()
        {
            return m_instance;
            
        }

        public IEnumerator GetProfilePhoto(Image image, string uri)
        {
            isRequesting = true;
            using (UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(uri))
            {
                // Request and wait for the desired page.
                yield return webRequest.SendWebRequest();

                
                while (!(webRequest.downloadProgress == 1))
                {
                    yield return webRequest;
                }
                if (webRequest.isNetworkError)
                {
                    Debug.Log(": Error: " + webRequest.error);
                }
                else
                {
                    Texture2D webTexture = ((DownloadHandlerTexture)webRequest.downloadHandler).texture as Texture2D;
                    if(webTexture != null)
                    {
                        if(profilePhoto == null)
                            profilePhoto = SpriteFromTexture2D(webTexture);
                        if(image != null)
                        image.sprite = SpriteFromTexture2D(webTexture);
                        if (remoteProfile == RemoteProfile.Requesting) remoteProfile = RemoteProfile.Retrieved;
                    }                    
                }
            }
            isRequesting = false;
        }

        Sprite SpriteFromTexture2D(Texture2D texture)
        {

            return Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100.0f);
        }

        public void RequestProfilePhoto(Image image, string uri)
        {
            image.sprite = requestingImg;
            if (!String.IsNullOrEmpty(uri))
                StartCoroutine(GetProfilePhoto(image, uri));
            else
                image.sprite = defaultProfilePhoto;
        }

        public void IncreasePoints(double points)
        {
            gamePoints += points;
            pointsText.text = gamePoints.ToString(); 
        }

        public void WonGame()
        {
            if(!ConnectionManager.Instance.IsFreeGame)
            {
                gamePoints += (BetPoints * 2);
            }
        }

        //start a ranked game, discont points.
        public void StartGame()
        {
            if(!ConnectionManager.Instance.IsFreeGame)
            {
                //gamePoints -= BetPoints;
            }
        }

        public void RefundPoints()
        {
            if(!ConnectionManager.Instance.isFreeGame)
            {
                gamePoints += BetPoints;
            }
        }

        public RewardManager GetRewardManager()
        {
            return GetComponent<RewardManager>();
        }

        private void OnApplicationQuit()
        {
            PlayerPrefs.SetFloat(POINTS_REMAINING_KEY, (float) gamePoints);
        }
    }
}
