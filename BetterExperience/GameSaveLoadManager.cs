using evt;
using HarmonyLib;
using System;

namespace BetterExperience
{
    public static class GameSaveLoadManager
    {
        public static event Action OnGameSaveLoadCompleted;

        [HarmonyPatch]
        public class GameSaveLoadPatch
        {
            [HarmonyPostfix]
            [HarmonyPatch(typeof(EV), nameof(EV.stack))]
            public static void LoadCompletedPostfix(string _name)
            {
                if (_name != "__INITNEWGAME")
                    return;

                HLog.Info("Detected game save/load completion event.");

                try
                {
                    OnGameSaveLoadCompleted?.Invoke();
                }
                catch (Exception ex)
                {
                    HLog.Error("An error occurred while invoking OnGameSaveLoadCompleted event.", ex);
                }
            }
        }
    }
}
