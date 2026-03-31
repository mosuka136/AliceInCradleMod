using evt;
using HarmonyLib;
using System;

namespace BetterExperience
{
    public class GameAttributePatchManager
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
        public class GameSaveLoadCompletedPatch
        {
            [HarmonyPostfix]
            [HarmonyPatch(typeof(EV), nameof(EV.stack))]
            public static void LoadCompletedPostfix(string _name)
            {
                if (_name != "__INITNEWGAME")
                    return;

                Instance.OnGameSaveLoadCompleted?.Invoke();
            }
        }
    }
}
