using HarmonyLib;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BetterExperience
{
    public static class FrameUpdateManager
    {
        public static event Action OnFrameUpdate;

        private static bool _initialized = false;
        private static readonly object _lock = new object();

        public static void Initialize()
        {
            lock (_lock)
            {
                if (_initialized)
                    return;

                var go = new GameObject($"{nameof(BetterExperience)}_{nameof(FrameUpdateManager)}");
                go.hideFlags = HideFlags.HideAndDontSave;
                UnityEngine.Object.DontDestroyOnLoad(go);
                go.AddComponent<Updater>();

                _initialized = true;
            }
        }

        [HarmonyPatch(typeof(SceneManager), "Internal_SceneLoaded")]
        public class LoadScenePatch
        {
            public static void Postfix()
            {
                if (_initialized)
                    return;

                Initialize();
            }
        }

        public class Updater : MonoBehaviour
        {
            private void Update()
            {
                OnFrameUpdate?.Invoke();
            }
        }
    }
}
