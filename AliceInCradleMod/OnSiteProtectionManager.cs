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

        public event Action OnSiteProtectionCompleted;

        [HarmonyPatch]
        private class RecoveryGameSaveData
        {
            [HarmonyPrefix]
            [HarmonyPatch(typeof(COOK), nameof(COOK.createBinary))]
            private static void SaveGamePrefix()
            {
                Instance.OnSiteProtectionActivated?.Invoke();
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(SVD), nameof(SVD.saveBinary))]
            private static void SaveGamePostfix()
            {
                Instance.OnSiteProtectionCompleted?.Invoke();
            }
        }
    }
}
