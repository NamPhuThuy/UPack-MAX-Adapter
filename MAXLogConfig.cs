using UnityEngine;

namespace NamPhuThuy.MAXAdapter
{
    
    public static class MAXLogConfig
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void DisableMaxLogs()
        {
            // Optional: also ensure verbose logging is not enabled.
            MaxSdk.SetVerboseLogging(false);
        }
    }
}