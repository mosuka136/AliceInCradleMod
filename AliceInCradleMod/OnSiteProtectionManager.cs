using HarmonyLib;
using nel;
using System;

namespace BetterExperience
{
    internal class OnSiteProtectionManager
    {
        public static OnSiteProtectionManager Instance { get; private set; }

        private OnSiteProtectionManager()
        {

        }

        static OnSiteProtectionManager()
        {
            Instance = new OnSiteProtectionManager();
        }

        public event Action OnSiteProtectionActivated;

        [HarmonyPatch]
        private class RecoveryGameSaveData
        {
            [HarmonyPrefix]
            [HarmonyPatch(typeof(COOK), "createBinary")]
            private static void SaveGamePrefix()
            {
                OnSiteProtectionManager.Instance.OnSiteProtectionActivated?.Invoke();
            }
        }
    }
}
