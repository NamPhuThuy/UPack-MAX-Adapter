namespace NamPhuThuy.MAXAdapter
{
    public static partial class MAXConst
    {
#if UNITY_ANDROID
        public static string MaxSdkKey = "YOUR_MAX_SDK_KEY_ANDROID";
        public static string InterstitialAdUnitId = "YOUR_INTER_AD_UNIT_ID_ANDROID";
        public static string RewardedAdUnitId    = "YOUR_REWARDED_AD_UNIT_ID_ANDROID";
        public static string BannerAdUnitId      = "YOUR_BANNER_AD_UNIT_ID_ANDROID";
        public static string MrecAdUnitId        = "YOUR_MREC_AD_UNIT_ID_ANDROID";
#elif UNITY_IOS
        public static string maxSdkKey = "YOUR_MAX_SDK_KEY_IOS";
        public static string interstitialAdUnitId = "YOUR_INTER_AD_UNIT_ID_IOS";
        public static string rewardedAdUnitId    = "YOUR_REWARDED_AD_UNIT_ID_IOS";
        public static string bannerAdUnitId      = "YOUR_BANNER_AD_UNIT_ID_IOS";
        public static string mrecAdUnitId        = "YOUR_MREC_AD_UNIT_ID_IOS";
#else
        public static string maxSdkKey           = "unused";
        public static string interstitialAdUnitId = "unused";
        public static string rewardedAdUnitId    = "unused";
        public static string bannerAdUnitId      = "unused";
        public static string mrecAdUnitId        = "unused";
#endif
    }
}