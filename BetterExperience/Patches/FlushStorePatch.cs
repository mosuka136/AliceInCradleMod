using BetterExperience.BConfigManager;
using BetterExperience.HClassAttribute;
using nel;

namespace BetterExperience.Patches
{
    public partial class HPatches
    {
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
                if (!ConfigManager.EnableFlushAllStore.Value)
                    return;

                if (ConfigManager.FlushAllStoreHotkey.Value.WasPressedThisFrame())
                {
                    StoreManager.FlushAll();
                    HLog.Info("Flushed all store!");
                }
            }
        }
    }
}
