using BetterExperience.HConfigGUI.UI;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BetterExperience.HConfigGUI
{
    public static class ConfigGUI
    {
        private static bool _initialized;
        private static readonly object _lock = new object();

        private static void Initialize()
        {
            lock (_lock)
            {
                if (_initialized)
                    return;

                var go = new GameObject($"{nameof(BetterExperience)}_{nameof(GuiHost)}");
                go.hideFlags = HideFlags.HideAndDontSave;
                Object.DontDestroyOnLoad(go);
                go.AddComponent<GuiHost>();

                _initialized = true;
            }
        }

        [HarmonyPatch(typeof(SceneManager), "Internal_SceneLoaded")]
        public static class LoadScenePatch
        {
            public static void Postfix()
            {
                Initialize();
            }
        }
    }
}
