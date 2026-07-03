using BetterExperience.BConfigManager;
using UnityModBase.HClassAttribute;
using BetterExperience.BLogSpace;
using nel;
using System;
using UnityModBase;

namespace BetterExperience.Patches
{
    public partial class HPatches
    {
        /// <summary>
        /// 通过热键触发全部商店刷新。
        /// 该补丁不挂接游戏方法，而是在游戏启动后订阅全局帧更新轮询热键。
        /// </summary>
        public class FlushStorePatch
        {
            private static bool _initialized = false;

            [InitializeOnGameBoot]
            public static void Initialize()
            {
                if (_initialized)
                    return;

                FrameUpdateManager.OnFrameUpdate += Update;

                _initialized = true;
            }

            public static void Update()
            {
                if (ConfigManager.EnableFlushAllStore?.Value != true)
                    return;

                if (ConfigManager.FlushAllStoreHotkey?.Value?.WasPressedThisFrame() == true)
                {
                    try
                    {
                        StoreManager.FlushAll();
                        BLog.Info("Flushed all store!");
                    }
                    catch (Exception ex)
                    {
                        BLog.Error($"Unexpected error in {nameof(FlushStorePatch)}", ex);
                    }
                }
            }
        }
    }
}
