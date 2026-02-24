using HarmonyLib;
using nel;
using System;

namespace BetterExperience
{
    internal class GameAttributePatchManager
    {
        private GameAttributePatchManager()
        {
            
        }

        public static GameAttributePatchManager Instance { get; } = new GameAttributePatchManager();

        static GameAttributePatchManager()
        {
            
        }

        public event Action OnGameSaveLoadCompleted;

        [HarmonyPatch]
        private class GameSaveLoadCompletedPatch
        {
            [HarmonyPostfix]
            [HarmonyPatch(typeof(COOK), nameof(COOK.initGameScene))]
            private static void LoadCompletedPostfix()
            {
                Instance.OnGameSaveLoadCompleted?.Invoke();
            }
        }
    }
}
