using BetterExperience.HClassAttribute;
using HarmonyLib;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BetterExperience
{
    /// <summary>
    /// 在游戏首次完成场景加载时执行一次性启动注册。
    /// 该管理器通过 Harmony 挂接 Unity 场景加载流程，将带有启动特性的组件和初始化方法延迟到游戏环境可用后创建/执行。
    /// 它不负责插件 <c>Awake</c> 阶段的基础设施初始化，也不会在后续场景切换时重复运行。
    /// </summary>
    public static class GameBootManager
    {
        // SceneManager 的内部回调可能在启动期间被多次触发，使用锁和标记保证注册与执行只发生一次。
        private static bool _initialized = false;
        private static object _lock = new object();

        /// <summary>
        /// 游戏启动阶段的一次性回调集合。
        /// 订阅者应假设它只在首次场景加载完成后触发一次，并且运行在 Unity 主线程。
        /// </summary>
        public static event Action OnGameBoot;

        /// <summary>
        /// 挂接 Unity 内部场景加载回调，用于把注册阶段和实际执行阶段拆开。
        /// Prefix 收集要执行的初始化项，Postfix 在场景加载完成后触发，避免过早访问尚未建立的 Unity 对象。
        /// </summary>
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

        /// <summary>
        /// 将一个 Unity 组件类型注册为游戏启动后创建的常驻对象。
        /// </summary>
        /// <param name="type">必须派生自 <see cref="Component"/>；非法类型只记录警告，不抛出异常。</param>
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
