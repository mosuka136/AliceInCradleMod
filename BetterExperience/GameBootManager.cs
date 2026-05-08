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
                    int registeredComponentCount = 0;
                    foreach (var type in types)
                    {
                        registeredComponentCount++;
                        RegisterComponentOnGameBoot(type);
                    }

                    var methods = ClassHelper.GetInitializeOnGameBootMethods(typeof(GameBootManager).Assembly);
                    int registeredMethodCount = 0;
                    foreach (var method in methods)
                    {
                        registeredMethodCount++;
                        HLog.Debug($"Register game boot initializer: {method.DeclaringType.FullName}.{method.Name}");
                        OnGameBoot += () =>
                        {
                            try
                            {
                                HLog.Debug($"Invoke game boot initializer: {method.DeclaringType.FullName}.{method.Name}");
                                method.Invoke(null, null);
                            }
                            catch (Exception ex)
                            {
                                HLog.Error($"Failed to invoke game boot initializer: {method.DeclaringType.FullName}.{method.Name}", ex);
                                throw;
                            }
                        };
                    }

                    HLog.Info($"Game boot registration completed. Components={registeredComponentCount}, Initializers={registeredMethodCount}");
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
                    HLog.Info("Game boot initialization completed.");
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

            HLog.Debug($"Register game boot component: {type.FullName}");
            OnGameBoot += () =>
            {
                try
                {
                    var go = new GameObject($"{nameof(BetterExperience)}_{type.Name}")
                    {
                        hideFlags = HideFlags.HideAndDontSave
                    };
                    UnityEngine.Object.DontDestroyOnLoad(go);
                    go.AddComponent(type);
                    HLog.Debug($"Created game boot component: {type.FullName}");
                }
                catch (Exception ex)
                {
                    HLog.Error($"Failed to create game boot component: {type.FullName}", ex);
                    throw;
                }
            };
        }
    }
}
