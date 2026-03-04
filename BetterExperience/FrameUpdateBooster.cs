using HarmonyLib;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BetterExperience
{
    internal sealed class FrameUpdateBooster
    {
        private FrameUpdateBooster()
        {

        }

        public static FrameUpdateBooster Instance { get; } = new FrameUpdateBooster();

        public event Action OnFrameUpdate;

        private bool _initialized = false;

        public void Awake()
        {
            if (_initialized)
                return;

            var go = new GameObject($"{nameof(BetterExperience)}_{nameof(FrameUpdateBooster)}");
            go.hideFlags = HideFlags.HideAndDontSave;
            UnityEngine.Object.DontDestroyOnLoad(go);
            go.AddComponent<Updater>();

            _initialized = true;

            HLog.Info("FrameUpdateBooster initialized.");
        }

        [HarmonyPatch(typeof(SceneManager), "Internal_SceneLoaded")]
        private class LoadScenePatch
        {
            private static void Postfix()
            {
                if (Instance._initialized)
                    return;

                Instance.Awake();
            }
        }

        private class Updater : MonoBehaviour
        {
            private void Update()
            {
                Instance.OnFrameUpdate?.Invoke();
            }
        }
    }
}
