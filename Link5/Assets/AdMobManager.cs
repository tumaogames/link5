using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleMobileAds.Api;
using System;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Link5;

public class AdMobManager : MonoBehaviour
{
    public Text adStatus;

    //string App_ID = "XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX";

    [SerializeField]
    private string Banner_AD_ID = "ca-app-pub-3940256099942544/6300978111";
    [SerializeField]
    private string Interstitial_AD_ID = "ca-app-pub-3940256099942544/1033173712";
    [SerializeField]
    private string Video_AD_ID = "ca-app-pub-3940256099942544/5224354917";

    private BannerView bannerView;
    private InterstitialAd interstital;
    private RewardBasedVideoAd rewardBasedVideo;
    private RewardedAd rewardedAd;

    //FOR DEBUG
    public Text DebugText;

    public string Banner_AD_ID1 { get => Banner_AD_ID2; set => Banner_AD_ID2 = value; }
    public string Banner_AD_ID2 { get => Banner_AD_ID; set => Banner_AD_ID = value; }

    void Start()
    {
        //MobileAds.Initialize(App_ID);
        MobileAds.Initialize(initStatus => 
        {
            Debug.Log("AdMob Initialized");
            //RequestBanner();
            //RequestInterstitial();
            //RequestRewardBasedVideo();
            //RequestRewardsAd();
        });
    }

    private void RequestBanner()
    {

        // Create a 320x50 banner at the top of the screen.
        this.bannerView = new BannerView(Banner_AD_ID, AdSize.Banner, AdPosition.BottomRight);

        // Called when an ad request has successfully loaded.
        this.bannerView.OnAdLoaded += this.HandleOnAdLoaded;
        // Called when an ad request failed to load.
        this.bannerView.OnAdFailedToLoad += this.HandleOnAdFailedToLoad;
        // Called when an ad is clicked.
        this.bannerView.OnAdOpening += this.HandleOnAdOpened;
        // Called when the user returned from the app after an ad click.
        this.bannerView.OnAdClosed += this.HandleOnAdClosed;
        // Called when the ad click caused the user to leave the application.
        this.bannerView.OnAdLeavingApplication += this.HandleOnAdLeavingApplication;
    }


    public void ShowBannerAD()
    {

        // Create an empty ad request.
        AdRequest request = new AdRequest.Builder().Build();

        // Load the banner with the request.
        this.bannerView.LoadAd(request);

    }

    public void RequestInterstitial()
    {
        // Initialize an InterstitialAd.
        this.interstital = new InterstitialAd(Interstitial_AD_ID);

        // Called when an ad request has successfully loaded.
        this.interstital.OnAdLoaded += HandleOnAdLoaded;
        // Called when an ad request failed to load.
        this.interstital.OnAdFailedToLoad += HandleOnAdFailedToLoad;
        // Called when an ad is shown.
        this.interstital.OnAdOpening += HandleOnAdOpened;
        // Called when the ad is closed.
        this.interstital.OnAdClosed += HandleOnAdClosed;
        // Called when the ad click caused the user to leave the application.
        this.interstital.OnAdLeavingApplication += HandleOnAdLeavingApplication;

        AdRequest request = new AdRequest.Builder().Build();
        this.interstital.LoadAd(request);
    }


    public void ShowInterstitialAd()
    {
        if (this.interstital.IsLoaded())
        {
            this.interstital.Show();
        }
    }

    public void RequestRewardBasedVideo()
    {
        rewardBasedVideo = RewardBasedVideoAd.Instance;

        // Create an empty ad request.
        AdRequest request = new AdRequest.Builder().Build();
        // Load the rewarded video ad with the request.
        this.rewardBasedVideo.LoadAd(request, Video_AD_ID);
    }

    public void RequestRewardsAd()
    {
        this.rewardedAd = new RewardedAd("ca-app-pub-3940256099942544/5224354917");

        // Called when an ad request has successfully loaded.
        this.rewardedAd.OnAdLoaded += HandleRewardedAdLoaded;
        // Called when an ad request failed to load.
        this.rewardedAd.OnAdFailedToLoad += HandleRewardedAdFailedToLoad;
        // Called when an ad is shown.
        this.rewardedAd.OnAdOpening += HandleRewardedAdOpening;
        // Called when an ad request failed to show.
        this.rewardedAd.OnAdFailedToShow += HandleRewardedAdFailedToShow;
        // Called when the user should be rewarded for interacting with the ad.
        this.rewardedAd.OnUserEarnedReward += HandleUserEarnedReward;
        // Called when the ad is closed.
        this.rewardedAd.OnAdClosed += HandleRewardedAdClosed;        

        // Create an empty ad request.
        AdRequest request = new AdRequest.Builder().Build();
        // Load the rewarded ad with the request.
        this.rewardedAd.LoadAd(request);
    }

    public void RequestRewards(EventHandler<Reward> OnLoad)
    {
        MobileAds.Initialize(initStatus =>
        {
            Debug.Log("AdMob Initialized");
            //RequestBanner();
            //RequestInterstitial();
            //RequestRewardBasedVideo();
            RequestRewardsAd(OnLoad);
        });
    }

    private void RequestRewardsAd(EventHandler<Reward> OnLoad)
    {
        this.rewardedAd = new RewardedAd("ca-app-pub-3940256099942544/5224354917");

        // Called when an ad request has successfully loaded.
        this.rewardedAd.OnAdLoaded += HandleRewardedAdLoaded;
        // Called when an ad request failed to load.
        this.rewardedAd.OnAdFailedToLoad += HandleRewardedAdFailedToLoad;
        // Called when an ad is shown.
        this.rewardedAd.OnAdOpening += HandleRewardedAdOpening;
        // Called when an ad request failed to show.
        this.rewardedAd.OnAdFailedToShow += HandleRewardedAdFailedToShow;
        // Called when the user should be rewarded for interacting with the ad.
        this.rewardedAd.OnUserEarnedReward += OnLoad;
        // Called when the ad is closed.
        this.rewardedAd.OnAdClosed += HandleRewardedAdClosed;

        // Create an empty ad request.
        AdRequest request = new AdRequest.Builder().Build();
        // Load the rewarded ad with the request.
        this.rewardedAd.LoadAd(request);
    }

    public void ShowVideoRewardAd()
    {
        if (rewardBasedVideo.IsLoaded())
        {
            rewardBasedVideo.Show();
        }
    }

    public void UserChoseToWatchAd()
    {
        if (this.rewardedAd.IsLoaded())
        {
            this.rewardedAd.Show();
        }
    }

    // FOR EVENTS AND DELEGATES FOR ADS
    public void HandleOnAdLoaded(object sender, EventArgs args)
    {
        //adStatus.text = "Ad Loaded";

        //if (this.interstital.IsLoaded())
        //{
        //    this.interstital.Show();
        //}
    }

    public void HandleOnAdFailedToLoad(object sender, AdFailedToLoadEventArgs args)
    {
        Debug.Log("Ad Failed To Load");
        DebugText.text = "Ad Failed To Load";
    }

    public void HandleOnAdOpened(object sender, EventArgs args)
    {
        Debug.Log("HandleAdOpened event received");
        DebugText.text = "HandleAdOpened event received";
    }

    public void HandleOnAdClosed(object sender, EventArgs args)
    {
        Debug.Log("HandleAdOpened event received");
        DebugText.text = "HandleAdOpened event received";
    }

    public void HandleOnAdLeavingApplication(object sender, EventArgs args)
    {
        Debug.Log("HandleAdLeavingApplication event received");
        DebugText.text = "HandleAdLeavingApplication event received";
    }

    public void HandleRewardedAdLoaded(object sender, EventArgs args)
    {
        Debug.LogError("HandleRewardedAdLoaded event received");
        //DebugText.text = "HandleRewardedAdLoaded event received";
        if (this.rewardedAd.IsLoaded())
        {
            this.rewardedAd.Show();
        }
    }

    public void HandleRewardedAdFailedToLoad(object sender, AdErrorEventArgs args)
    {
        DebugText.text = 
            "HandleRewardedAdFailedToLoad event received with message: "
                             + args.Message;
    }

    public void HandleRewardedAdOpening(object sender, EventArgs args)
    {
        DebugText.text = "HandleRewardedAdOpening event received";
    }

    public void HandleRewardedAdFailedToShow(object sender, AdErrorEventArgs args)
    {
        DebugText.text = 
            "HandleRewardedAdFailedToShow event received with message: " + args.Message;
    }

    public void HandleRewardedAdClosed(object sender, EventArgs args)
    {
        DebugText.text = "HandleRewardedAdClosed event received";
    }

    public void HandleUserEarnedReward(object sender, Reward args)
    {
        string type = args.Type;
        double amount = args.Amount;
        GameData.GetInstance().IncreasePoints(amount);
        Debug.Log("Recieve " + amount);

        DebugText.text = "Recieve " + amount;
    }

}