using HarmonyLib;
using nel;
using System;

namespace BetterExperience
{
    public class OnSiteProtectionManager
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
        public class RecoveryGameSaveData
        {
            [HarmonyPrefix]
            [HarmonyPatch(typeof(COOK), nameof(COOK.createBinary))]
            public static void SaveGamePrefix()
            {
                Instance.OnSiteProtectionActivated?.Invoke();
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(SVD), nameof(SVD.saveBinary))]
            public static void SaveGamePostfix()
            {
                Instance.OnSiteProtectionCompleted?.Invoke();
            }
        }
    }
}
