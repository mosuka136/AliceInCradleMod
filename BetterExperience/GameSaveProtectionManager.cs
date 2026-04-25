using HarmonyLib;
using nel;
using System;

namespace BetterExperience
{
    public static class GameSaveProtectionManager
    {
        public static event Action OnSavingActivated;

        public static event Action OnSavingCompleted;

        [HarmonyPatch]
        public class GameSaveProtectionPatch
        {
            [HarmonyPrefix]
            [HarmonyPatch(typeof(COOK), nameof(COOK.createBinary))]
            public static void SaveGamePrefix()
            {
                OnSavingActivated?.Invoke();
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(SVD), nameof(SVD.saveBinary))]
            public static void SaveGamePostfix()
            {
                OnSavingCompleted?.Invoke();
            }
        }
    }
}
