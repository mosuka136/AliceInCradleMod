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
                HLog.Info("Detected game saving activation.");

                try
                {
                    OnSavingActivated?.Invoke();
                }
                catch (Exception ex)
                {
                    HLog.Error("An error occurred while invoking OnSavingActivated event.", ex);
                }
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(SVD), nameof(SVD.saveBinary))]
            public static void SaveGamePostfix()
            {
                HLog.Info("Detected game saving completion.");

                try
                {
                    OnSavingCompleted?.Invoke();
                }
                catch (Exception ex)
                {
                    HLog.Error("An error occurred while invoking OnSavingCompleted event.", ex);
                }
            }
        }
    }
}
