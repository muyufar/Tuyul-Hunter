using System;
using UnityEngine;
using GoogleMobileAds.Api;
//using UnityEngine.Advertisements;

// Example script showing how to invoke the Google Mobile Ads Unity plugin.
public class Adcontrol : MonoBehaviour
{
	public static Adcontrol instance;
	private BannerView bannerView;
	private InterstitialAd interstitial;
	private RewardBasedVideoAd rewardBasedVideo;


	private float deltaTime = 0.0f;
	private static string outputMessage = string.Empty;
	public string AppID ="ca-app-pub-3940256099942544~3347511713";
	public string BannerID = "ca-app-pub-3940256099942544/6300978111";
	public string InterstitialID= "ca-app-pub-3940256099942544/1033173712";

    


	public static string OutputMessage
	{
		set { outputMessage = value; }
	}

	public void Awake(){
		if (instance == null)
			instance = this;
		else if(instance != this)
			Destroy (gameObject);
	
	}
	public void Start()
	{



		MobileAds.SetiOSAppPauseOnBackground(true);

		// Initialize the Google Mobile Ads SDK.
		MobileAds.Initialize(AppID);

		// Get singleton reward based video ad reference.
		this.rewardBasedVideo = RewardBasedVideoAd.Instance;

		// RewardBasedVideoAd is a singleton, so handlers should only be registered once.
		this.rewardBasedVideo.OnAdLoaded += this.HandleRewardBasedVideoLoaded;
		this.rewardBasedVideo.OnAdFailedToLoad += this.HandleRewardBasedVideoFailedToLoad;
		this.rewardBasedVideo.OnAdOpening += this.HandleRewardBasedVideoOpened;
		this.rewardBasedVideo.OnAdStarted += this.HandleRewardBasedVideoStarted;
		this.rewardBasedVideo.OnAdRewarded += this.HandleRewardBasedVideoRewarded;
		this.rewardBasedVideo.OnAdClosed += this.HandleRewardBasedVideoClosed;
		this.rewardBasedVideo.OnAdLeavingApplication += this.HandleRewardBasedVideoLeftApplication;
        RequestInterstitial();
        RequestBanner();

        }

		public void Update()
		{
		// Calculate simple moving average for time to render screen. 0.1 factor used as smoothing
		// value.
		this.deltaTime += (Time.deltaTime - this.deltaTime) * 0.1f;
		}


		// Returns an ad request with custom ad targeting.
		public AdRequest CreateAdRequest()
		{
		return new AdRequest.Builder()
		//.AddTestDevice(AdRequest.TestDeviceSimulator)
		//.AddTestDevice("0123456789ABCDEF0123456789ABCDEF")
		//.AddKeyword("game")
		//.SetGender(Gender.Male)
		//.SetBirthday(new DateTime(1985, 1, 1))
		.TagForChildDirectedTreatment(false)
		//.AddExtra("color_bg", "9B30FF")
		.Build();
		}

		public void RequestBanner()
		{
		

		// Clean up banner ad before creating a new one.
		if (this.bannerView != null)
		{
		this.bannerView.Destroy();
		}

		// Create a 320x50 banner at the top of the screen.
		this.bannerView = new BannerView(BannerID, AdSize.SmartBanner, AdPosition.Bottom);

		// Register for ad events.
		this.bannerView.OnAdLoaded += this.HandleAdLoaded;
		this.bannerView.OnAdFailedToLoad += this.HandleAdFailedToLoad;
		this.bannerView.OnAdOpening += this.HandleAdOpened;
		this.bannerView.OnAdClosed += this.HandleAdClosed;
		this.bannerView.OnAdLeavingApplication += this.HandleAdLeftApplication;

		// Load a banner ad.
		this.bannerView.LoadAd(this.CreateAdRequest());
		}

		public void RequestInterstitial()
		{
		// These ad units are configured to always serve test ads.

		// Clean up interstitial ad before creating a new one.
		if (this.interstitial != null)
		{
		this.interstitial.Destroy();
		}

		// Create an interstitial.
		this.interstitial = new InterstitialAd(InterstitialID);

		// Register for ad events.
		this.interstitial.OnAdLoaded += this.HandleInterstitialLoaded;
		this.interstitial.OnAdFailedToLoad += this.HandleInterstitialFailedToLoad;
		this.interstitial.OnAdOpening += this.HandleInterstitialOpened;
		this.interstitial.OnAdClosed += this.HandleInterstitialClosed;
		this.interstitial.OnAdLeavingApplication += this.HandleInterstitialLeftApplication;

		// Load an interstitial ad.
		this.interstitial.LoadAd(this.CreateAdRequest());
		}

		public void RequestRewardBasedVideo()
		{
		#if UNITY_EDITOR
		string adUnitId = "unused";
		#elif UNITY_ANDROID
		string adUnitId = "ca-app-pub-3940256099942544/5224354917";
		#elif UNITY_IPHONE
		string adUnitId = "ca-app-pub-3940256099942544/1712485313";
		#else
		string adUnitId = "unexpected_platform";
		#endif

		this.rewardBasedVideo.LoadAd(this.CreateAdRequest(), adUnitId);
		}

		public void ShowInterstitial()
		{
		
		this.interstitial.Show();
        RequestInterstitial();

		}

		public void ShowRewardBasedVideo()
		{
		if (this.rewardBasedVideo.IsLoaded())
		{
		this.rewardBasedVideo.Show();
		}
		else
		{
		//	Advertisement.Show ();
		MonoBehaviour.print("Reward based video ad is not ready yet");
		}
		}

		#region Banner callback handlers

		public void HandleAdLoaded(object sender, EventArgs args)
		{
		MonoBehaviour.print("HandleAdLoaded event received");
		}

		public void HandleAdFailedToLoad(object sender, AdFailedToLoadEventArgs args)
		{
		MonoBehaviour.print("HandleFailedToReceiveAd event received with message: " + args.Message);
		}

		public void HandleAdOpened(object sender, EventArgs args)
		{
		MonoBehaviour.print("HandleAdOpened event received");
		}

		public void HandleAdClosed(object sender, EventArgs args)
		{
		MonoBehaviour.print("HandleAdClosed event received");
		}

		public void HandleAdLeftApplication(object sender, EventArgs args)
		{
		MonoBehaviour.print("HandleAdLeftApplication event received");
		}

		#endregion

		#region Interstitial callback handlers

		public void HandleInterstitialLoaded(object sender, EventArgs args)
		{
		MonoBehaviour.print("HandleInterstitialLoaded event received");
		}

		public void HandleInterstitialFailedToLoad(object sender, AdFailedToLoadEventArgs args)
		{
		MonoBehaviour.print(
		"HandleInterstitialFailedToLoad event received with message: " + args.Message);
		}

		public void HandleInterstitialOpened(object sender, EventArgs args)
		{
		MonoBehaviour.print("HandleInterstitialOpened event received");
		}

		public void HandleInterstitialClosed(object sender, EventArgs args)
		{
		MonoBehaviour.print("HandleInterstitialClosed event received");
		}

		public void HandleInterstitialLeftApplication(object sender, EventArgs args)
		{
		MonoBehaviour.print("HandleInterstitialLeftApplication event received");
		}

		#endregion

		#region RewardBasedVideo callback handlers

		public void HandleRewardBasedVideoLoaded(object sender, EventArgs args)
		{
		MonoBehaviour.print("HandleRewardBasedVideoLoaded event received");
		}

		public void HandleRewardBasedVideoFailedToLoad(object sender, AdFailedToLoadEventArgs args)
		{
		MonoBehaviour.print(
		"HandleRewardBasedVideoFailedToLoad event received with message: " + args.Message);
		}

		public void HandleRewardBasedVideoOpened(object sender, EventArgs args)
		{
		MonoBehaviour.print("HandleRewardBasedVideoOpened event received");
		}

		public void HandleRewardBasedVideoStarted(object sender, EventArgs args)
		{
		MonoBehaviour.print("HandleRewardBasedVideoStarted event received");
		}

		public void HandleRewardBasedVideoClosed(object sender, EventArgs args)
		{
		MonoBehaviour.print("HandleRewardBasedVideoClosed event received");
		}

		public void HandleRewardBasedVideoRewarded(object sender, Reward args)
		{
		string type = args.Type;
		double amount = args.Amount;
		MonoBehaviour.print(
		"HandleRewardBasedVideoRewarded event received for " + amount.ToString() + " " + type);
		}

		public void HandleRewardBasedVideoLeftApplication(object sender, EventArgs args)
		{
		MonoBehaviour.print("HandleRewardBasedVideoLeftApplication event received");
		}

    #endregion

    public void HideBanner() {
        this.bannerView.Hide();
    }
    public void ShowBanner()
    {
        this.bannerView.Show();
    }




}