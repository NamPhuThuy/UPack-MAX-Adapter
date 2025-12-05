namespace NamPhuThuy.MAXAdapter
{
    using System;
    using System.Collections;
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.SceneManagement;

    public class MAXAdapter : Singleton<MAXAdapter>
    {
        [Header("SCRIPTABLE OBJECT")]
        [SerializeField] private IntVariable currentLevel;

        [Header("MAX Ad Units")]
#if UNITY_ANDROID
        [SerializeField] private string maxSdkKey =
            "YOUR_MAX_SDK_KEY_ANDROID";
        [SerializeField] private string interstitialAdUnitId = "YOUR_INTER_AD_UNIT_ID_ANDROID";
        [SerializeField] private string rewardedAdUnitId    = "YOUR_REWARDED_AD_UNIT_ID_ANDROID";
        [SerializeField] private string bannerAdUnitId      = "YOUR_BANNER_AD_UNIT_ID_ANDROID";
        [SerializeField] private string mrecAdUnitId        = "YOUR_MREC_AD_UNIT_ID_ANDROID";
#elif UNITY_IOS
        [SerializeField] private string maxSdkKey =
            "YOUR_MAX_SDK_KEY_IOS";
        [SerializeField] private string interstitialAdUnitId = "YOUR_INTER_AD_UNIT_ID_IOS";
        [SerializeField] private string rewardedAdUnitId    = "YOUR_REWARDED_AD_UNIT_ID_IOS";
        [SerializeField] private string bannerAdUnitId      = "YOUR_BANNER_AD_UNIT_ID_IOS";
        [SerializeField] private string mrecAdUnitId        = "YOUR_MREC_AD_UNIT_ID_IOS";
#else
        [SerializeField] private string maxSdkKey           = "unused";
        [SerializeField] private string interstitialAdUnitId = "unused";
        [SerializeField] private string rewardedAdUnitId    = "unused";
        [SerializeField] private string bannerAdUnitId      = "unused";
        [SerializeField] private string mrecAdUnitId        = "unused";
#endif

        [Header("Runtime")]
        public float countdownAds = int.MaxValue;

        private bool _isInitialized;
        private bool _isShowingAds;

        // Interstitial
        private bool _isLoadingInterstitial;
        private UnityAction _actionInterstitialClose;

        // Rewarded
        private UnityAction _actionRewardVideo;
        private UnityAction _actionRewardNotLoaded;
        private UnityAction _actionRewardClose;
        private ActionWatchVideo _actionWatchVideo;
        private bool _isVideoDone;

        // Banner
        private bool _maxBannerCreated;

        // MRec
        private bool _mrecCreated;

        private DateTime _adsStartTime;
        private DateTime _oldTime = DateTime.MinValue;

        #region MonoBehaviour

        private void Awake()
        {
            SceneManager.sceneLoaded += OnSceneSwitched;

            StartCoroutine(InitMax());
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneSwitched;
        }

        private void Update()
        {
            if (!_isInitialized)
                return;

            countdownAds += Time.unscaledDeltaTime;
        }

        private void OnSceneSwitched(Scene scene, LoadSceneMode mode)
        {
            // Scene change specific logic if needed
        }

        #endregion

        #region Init

        private IEnumerator InitMax()
        {
            // User consent or privacy settings should be configured before InitializeSdk
            MaxSdk.SetHasUserConsent(DataUtility.Load("User Consent", true));
            MaxSdk.SetSdkKey(maxSdkKey);

            bool completed = false;
            MaxSdkCallbacks.OnSdkInitializedEvent += config =>
            {
                InitializeInterstitial();
                InitializeRewarded();
                InitializeBanner();
                InitializeMRecAds();

                _isInitialized = true;
                completed = true;
            };

            MaxSdk.InitializeSdk();

            // wait until MAX reports initialized
            while (!completed)
            {
                yield return null;
            }
        }

        #endregion

        #region Interstitial

        private void InitializeInterstitial()
        {
            MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += OnInterstitialLoadedEvent;
            MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += OnInterstitialLoadFailedEvent;
            MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += OnInterstitialDisplayFailedEvent;
            MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += OnInterstitialHiddenEvent;
            MaxSdkCallbacks.Interstitial.OnAdClickedEvent += OnInterstitialClickedEvent;
            MaxSdkCallbacks.Interstitial.OnAdDisplayedEvent += OnInterstitialDisplayedEvent;
            MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent += OnAdRevenuePaidEvent;

            RequestInterstitial();
        }

        private void RequestInterstitial()
        {
            if (_isLoadingInterstitial)
                return;

            _isLoadingInterstitial = true;
            MaxSdk.LoadInterstitial(interstitialAdUnitId);
            GameController.Instance.AnalyticsController.LogInterLoad();
        }

        private void OnInterstitialLoadedEvent(string adUnitId, MaxSdk.AdInfo adInfo)
        {
            _isLoadingInterstitial = false;
            GameController.Instance.AnalyticsController.LogInterReady();
        }

        private void OnInterstitialLoadFailedEvent(string adUnitId, MaxSdk.ErrorInfo errorInfo)
        {
            _isLoadingInterstitial = false;
            _actionInterstitialClose?.Invoke();
            _actionInterstitialClose = null;

            Invoke(nameof(RequestInterstitial), 3f);
        }

        private void OnInterstitialDisplayFailedEvent(string adUnitId, MaxSdk.ErrorInfo errorInfo,
            MaxSdk.AdInfo adInfo)
        {
            _isLoadingInterstitial = false;
            _actionInterstitialClose?.Invoke();
            _actionInterstitialClose = null;

            RequestInterstitial();
        }

        private void OnInterstitialHiddenEvent(string adUnitId, MaxSdk.AdInfo adInfo)
        {
            _isLoadingInterstitial = false;
            Time.timeScale = 1f;

            _actionRewardVideo?.Invoke();
            _actionRewardVideo = null;

            _actionInterstitialClose?.Invoke();
            _actionInterstitialClose = null;

            countdownAds = 0f;

            RequestInterstitial();
            Invoke(nameof(RefreshCloseAdsFlag), 1f);

            var adDuration = DateTime.Now - AdsDurationObserver.startTime;
            if (adDuration.TotalSeconds < 1000)
                AdsDurationObserver.duration += adDuration.TotalSeconds;
        }

        private void OnInterstitialDisplayedEvent(string adUnitId, MaxSdk.AdInfo adInfo)
        {
            _isLoadingInterstitial = false;
            Time.timeScale = 0f;
        }

        private void OnInterstitialClickedEvent(string adUnitId, MaxSdk.AdInfo adInfo)
        {
            GameController.Instance.AnalyticsController.LogInterClick();
            _isLoadingInterstitial = false;
        }

        public bool IsLoadedInterstitial()
        {
            return MaxSdk.IsInterstitialReady(interstitialAdUnitId);
        }

        public bool ShowInterstitial(bool isShowImmediatly = false, string actionWatchLog = "other",
            UnityAction actionIniterClose = null, UnityAction actionIniterShow = null,
            string level = null, bool isInGame = true, bool isShowAdBreak = true)
        {
            if (UserData.IsRemoveAds)
            {
                actionIniterClose?.Invoke();
                return false;
            }

            if (!RemoteConfigController.GetBoolConfig(FirebaseConfig.ENABLE_INTERSTITIAL, false))
            {
                actionIniterClose?.Invoke();
                return false;
            }

            var segment = GameController.Instance.segmentController.GetCurrentSegment();
            bool passRules =
                (UserData.CurrentLevel > segment.adsData.config.interstitials.minimumLevel &&
                 countdownAds > segment.adsData.config.interstitials.minimumInterval)
                || isShowImmediatly;

            if (!passRules)
            {
                actionIniterClose?.Invoke();
                return false;
            }

            if (isShowAdBreak)
            {
                AdsBreakScreen.Instance.Show(() =>
                {
                    ShowInterstitialInternal(isShowImmediatly, actionWatchLog, actionIniterClose, isInGame);
                });
            }
            else
            {
                ShowInterstitialInternal(isShowImmediatly, actionWatchLog, actionIniterClose, isInGame);
            }

            return true;
        }

        private void ShowInterstitialInternal(bool isShowImmediatly, string actionWatchLog,
            UnityAction actionIniterClose, bool isInGame)
        {
            actionIniterClose += () =>
            {
                AdsBreakScreen.Instance.Hide();
            };

            if (!IsLoadedInterstitial())
            {
                actionIniterClose?.Invoke();
                RequestInterstitial();
                return;
            }

            _isShowingAds = true;
            _oldTime = DateTime.Now;

            _actionInterstitialClose = actionIniterClose;

            AdsDurationObserver.startTime = DateTime.Now;

            MaxSdk.ShowInterstitial(interstitialAdUnitId, actionWatchLog);

            countdownAds = 0;
            GameController.Instance.AnalyticsController.LogInterShow(actionWatchLog);

            UserData.NumberOfAdsInDay += 1;
            UserData.NumberOfAdsInPlay += 1;
        }

        private void RefreshCloseAdsFlag()
        {
            _isShowingAds = false;
        }

        public void ShowInterAutoClaim(string actionWatchLog = "other",
            UnityAction actionIniterClose = null, UnityAction actionFail = null)
        {
            ShowInterstitialInternal(true, actionWatchLog, actionIniterClose, true);
        }

        #endregion

        #region Rewarded

        private void InitializeRewarded()
        {
            MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += OnRewardedAdLoadedEvent;
            MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += OnRewardedAdLoadFailedEvent;
            MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += OnRewardedAdDisplayFailedEvent;
            MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent += OnRewardedAdDisplayedEvent;
            MaxSdkCallbacks.Rewarded.OnAdClickedEvent += OnRewardedAdClickedEvent;
            MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += OnRewardedAdHiddenEvent;
            MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += OnRewardedAdReceivedRewardEvent;
            MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += OnAdRevenuePaidEvent;

            LoadRewardedAd();
        }

        private void LoadRewardedAd()
        {
            MaxSdk.LoadRewardedAd(rewardedAdUnitId);
        }

        private void OnRewardedAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            GameController.Instance.AnalyticsController.LogVideoRewardReady();
        }

        private void OnRewardedAdLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            Invoke(nameof(LoadRewardedAd), 15f);
            GameController.Instance.AnalyticsController.LogVideoRewardLoadFail(
                _actionWatchVideo.ToString(), errorInfo.Message);
        }

        private void OnRewardedAdDisplayFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo,
            MaxSdkBase.AdInfo adInfo)
        {
            _isVideoDone = false;
            LoadRewardedAd();
        }

        private void OnRewardedAdDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            _isVideoDone = false;
        }

        private void OnRewardedAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            _isVideoDone = true;
            GameController.Instance.AnalyticsController.LogClickToVideoReward(_actionWatchVideo.ToString());
        }

        private void OnRewardedAdHiddenEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            _actionRewardClose?.Invoke();
            _actionRewardClose = null;
            _actionRewardVideo = null;

            LoadRewardedAd();
            Invoke(nameof(RefreshCloseAdsFlag), 1f);

            var adDuration = DateTime.Now - AdsDurationObserver.startTime;
            if (adDuration.TotalSeconds < 1000)
                AdsDurationObserver.duration += adDuration.TotalSeconds;
        }

        private void OnRewardedAdReceivedRewardEvent(string adUnitId, MaxSdk.Reward reward,
            MaxSdkBase.AdInfo adInfo)
        {
            _isVideoDone = true;
            _actionRewardVideo?.Invoke();
            _actionRewardVideo = null;

            countdownAds = 0;
            GameController.Instance.AnalyticsController.LogVideoRewardShowDone(_actionWatchVideo.ToString());
        }

        public bool IsLoadedVideoReward()
        {
            bool result = MaxSdk.IsRewardedAdReady(rewardedAdUnitId);
            if (!result)
            {
                RequestInterstitial();
            }

            return result;
        }

        public bool ShowVideoReward(UnityAction actionReward, UnityAction actionNotLoadedVideo,
            UnityAction actionClose, ActionWatchVideo actionType = ActionWatchVideo.None,
            string adWhere = "")
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                actionNotLoadedVideo?.Invoke();
                GameController.Instance.AnalyticsController.LogWatchVideo(
                    actionType, true, false, UserData.CurrentLevel.ToString());
                return false;
            }

            _actionWatchVideo = actionType;
            GameController.Instance.AnalyticsController.LogRequestVideoReward(actionType.ToString());
            GameController.Instance.AnalyticsController.LogVideoRewardEligible();

            if (IsLoadedVideoReward())
            {
                _isShowingAds = true;
                countdownAds = 0;
                _oldTime = DateTime.Now;

                _actionRewardNotLoaded = actionNotLoadedVideo;
                _actionRewardClose = actionClose;
                _actionRewardVideo = actionReward;

                AdsDurationObserver.startTime = DateTime.Now;

                MaxSdk.ShowRewardedAd(rewardedAdUnitId, actionType.ToString());

                GameController.Instance.AnalyticsController.LogWatchVideo(
                    actionType, true, true, UserData.CurrentLevel.ToString());
                GameController.Instance.AnalyticsController.LogVideoRewardShow(_actionWatchVideo.ToString());
                return true;
            }

            // fallback to interstitial if no rewarded ad
            if (IsLoadedInterstitial())
            {
                _actionRewardNotLoaded = actionNotLoadedVideo;
                _actionRewardClose = actionClose;
                _actionRewardVideo = actionReward;

                ShowInterstitial(true, actionType.ToString(), () => { });
                GameController.Instance.AnalyticsController.LogWatchVideo(
                    actionType, true, true, UserData.CurrentLevel.ToString());

                countdownAds = 0;
                return true;
            }

            actionNotLoadedVideo?.Invoke();
            GameController.Instance.AnalyticsController.LogWatchVideo(
                actionType, false, true, UserData.CurrentLevel.ToString());
            return false;
        }

        #endregion

        #region Banner

        private void InitializeBanner()
        {
            MaxSdkCallbacks.Banner.OnAdLoadedEvent += OnBannerAdLoadedEvent;
            MaxSdkCallbacks.Banner.OnAdLoadFailedEvent += OnBannerAdLoadFailedEvent;
            MaxSdkCallbacks.Banner.OnAdClickedEvent += OnBannerAdClickedEvent;
            MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += OnAdRevenuePaidEvent;

            MaxSdk.CreateBanner(bannerAdUnitId, MaxSdkBase.BannerPosition.BottomCenter);
            MaxSdk.SetBannerPlacement(bannerAdUnitId, "MY_BANNER_PLACEMENT");
            MaxSdk.SetBannerBackgroundColor(bannerAdUnitId, Color.clear);
            MaxSdk.SetBannerExtraParameter(bannerAdUnitId, "adaptive_banner", "true");

            _maxBannerCreated = true;
            ShowBanner();
        }

        private void OnBannerAdLoadedEvent(string adUnitId, MaxSdk.AdInfo adInfo)
        {
            // nothing special, but hook kept for debug
        }

        private void OnBannerAdClickedEvent(string adUnitId, MaxSdk.AdInfo adInfo)
        {
            // tracking if needed
        }

        private void OnBannerAdLoadFailedEvent(string adUnitId, MaxSdk.ErrorInfo errorInfo)
        {
            StartCoroutine(ReloadBannerCoroutine());
        }

        private IEnumerator ReloadBannerCoroutine()
        {
            yield return new WaitForSeconds(0.3f);
            ShowBanner();
        }

        public void ShowBanner()
        {
            if (UserData.IsRemoveAds)
                return;

            if (!GamePersistentVariable.isShowBanner)
                return;

            if (SceneManager.GetActiveScene().name == GameConstants.LOADING_SCENE)
                return;

            if (!_maxBannerCreated)
                return;

            MaxSdk.ShowBanner(bannerAdUnitId);
        }

        public void HideBanner()
        {
            if (!_maxBannerCreated)
                return;

            MaxSdk.HideBanner(bannerAdUnitId);
        }

        public void DestroyBanner()
        {
            if (!_maxBannerCreated)
                return;

            MaxSdk.DestroyBanner(bannerAdUnitId);
            _maxBannerCreated = false;
        }

        #endregion

        #region MRec

        private void InitializeMRecAds()
        {
            MaxSdk.CreateMRec(mrecAdUnitId, MaxSdkBase.AdViewPosition.TopCenter);

            MaxSdkCallbacks.MRec.OnAdLoadedEvent += OnMRecAdLoadedEvent;
            MaxSdkCallbacks.MRec.OnAdLoadFailedEvent += OnMRecAdLoadFailedEvent;
            MaxSdkCallbacks.MRec.OnAdClickedEvent += OnMRecAdClickedEvent;
            MaxSdkCallbacks.MRec.OnAdExpandedEvent += OnMRecAdExpandedEvent;
            MaxSdkCallbacks.MRec.OnAdCollapsedEvent += OnMRecAdCollapsedEvent;
            MaxSdkCallbacks.MRec.OnAdRevenuePaidEvent += OnMRecAdRevenuePaidEvent;

            _mrecCreated = true;
        }

        public void ShowMRec()
        {
            if (UserData.IsRemoveAds)
                return;

            if (!GamePersistentVariable.remoteConfigIsShowMREC)
                return;

            if (!GamePersistentVariable.doubleInterAdsConfig.isEnableMrec)
                return;

            if (!_mrecCreated)
                return;

            MaxSdk.ShowMRec(mrecAdUnitId);
        }

        public void HideMRec()
        {
            if (!_mrecCreated)
                return;

            MaxSdk.HideMRec(mrecAdUnitId);
        }

        public void DestroyMRec()
        {
            if (!_mrecCreated)
                return;

            MaxSdk.DestroyMRec(mrecAdUnitId);
            _mrecCreated = false;
        }

        private void OnMRecAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) { }

        private void OnMRecAdLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo) { }

        private void OnMRecAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) { }

        private void OnMRecAdExpandedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) { }

        private void OnMRecAdCollapsedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) { }

        private void OnMRecAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            try
            {
                if (adInfo == null) return;
                if (adInfo.Revenue < 0) return;

                double revenue = adInfo.Revenue;

                AppFlyerGplay.LogRevenue(adInfo.NetworkName, "max", adInfo.AdUnitIdentifier,
                    adInfo.AdFormat, revenue, "USD");

                AnalyticsController.TryLogAdsRevenue(
                    "ad_impression", currentLevel.Value, adInfo.NetworkName, adInfo.AdUnitIdentifier,
                    adInfo.AdFormat, revenue, "USD");

                AnalyticsController.TryLogAdsRevenue(
                    "ad_revenue_sdk", currentLevel.Value, adInfo.NetworkName.ToLower(),
                    adInfo.AdUnitIdentifier.ToLower(), adInfo.AdFormat.ToLower(),
                    revenue, "USD");
            }
            catch (Exception)
            {
                // ignore
            }
        }

        #endregion

        #region Revenue callback (shared)

        private void OnAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            if (adInfo == null) return;
            if (adInfo.Revenue < 0) return;

            double revenue = adInfo.Revenue;

            AppFlyerGplay.LogRevenue(adInfo.NetworkName, "max", adInfo.AdUnitIdentifier,
                adInfo.AdFormat, revenue, "USD");

            AnalyticsController.LogAdsRevenue("ad_impression", currentLevel.Value,
                adInfo.NetworkName, adInfo.AdUnitIdentifier, adInfo.AdFormat, revenue, "USD");

            AnalyticsController.LogAdsRevenue("ad_revenue_sdk", currentLevel.Value,
                adInfo.NetworkName.ToLower(), adInfo.AdUnitIdentifier.ToLower(),
                adInfo.AdFormat.ToLower(), revenue, "USD");
        }

        #endregion

        #region Public helpers

        public void ResetAdsCooldown()
        {
            countdownAds = 0f;
        }

        #endregion
    }
}
