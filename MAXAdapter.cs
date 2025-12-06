/*
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

namespace NamPhuThuy.MAXAdapter
{
    
    public class MAXAdapter : MonoBehaviour
    {
        #region Private Serializable Fields

        #endregion

        #region Private Fields

        #endregion

        #region MonoBehaviour Callbacks

        void Start()
        {
            
        }

        void Update()
        {
            
        }

        #endregion

        #region Private Methods
        #endregion

        #region Public Methods
        #endregion

        #region Editor Methods

        public void ResetValues()
        {
            
        }

        #endregion
    }
}
*/
