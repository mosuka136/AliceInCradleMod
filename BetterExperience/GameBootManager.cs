using BetterExperience.HClassAttribute;
using HarmonyLib;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BetterExperience
{
    public static class GameBootManager
    {
        private static bool _initialized = false;
        private static object _lock = new object();

        public static event Action OnGameBoot;

        [HarmonyPatch(typeof(SceneManager), "Internal_SceneLoaded")]
        public class LoadScenePatch
        {
            public static void Prefix()
            {
                lock (_lock)
                {
                    if (_initialized)
                        return;

                    var types = ClassHelper.GetRegisterOnGameBootClasses(typeof(GameBootManager).Assembly);
                    foreach (var type in types)
                    {
                        RegisterComponentOnGameBoot(type);
                    }

                    var methods = ClassHelper.GetInitializeOnGameBootMethods(typeof(GameBootManager).Assembly);
                    foreach (var method in methods)
                    {
                        OnGameBoot += () => method.Invoke(null, null);
                    }
                }
            }

            public static void Postfix()
            {
                lock (_lock)
                {
                    if (_initialized)
                        return;

                    try
                    {
                        OnGameBoot?.Invoke();
                    }
                    catch (Exception ex)
                    {
                        HLog.Error("An error occurred while invoking OnGameBoot event.", ex);
                    }

                    _initialized = true;
                }
            }
        }

        public static void RegisterComponentOnGameBoot(Type type)
        {
            if (!typeof(Component).IsAssignableFrom(type))
            {
                HLog.Warn($"Type {type.FullName} is not a Component, cannot register for game boot.");
                return;
            }

            OnGameBoot += () =>
            {
                var go = new GameObject($"{nameof(BetterExperience)}_{type.Name}")
                {
                    hideFlags = HideFlags.HideAndDontSave
                };
                UnityEngine.Object.DontDestroyOnLoad(go);
                go.AddComponent(type);
            };
        }
    }
}
