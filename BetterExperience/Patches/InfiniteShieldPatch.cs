using BetterExperience.BConfigManager;
using HarmonyLib;
using nel;
using System;

namespace BetterExperience.Patches
{
    public partial class HPatches
    {
        [HarmonyPatch]
        public class InfiniteShieldPatch
        {
            [HarmonyPatch(typeof(M2Shield), "canGuard")]
            public static void Prefix(M2Shield __instance, ref bool __result)
            {
                try
                {
                    if (!ConfigManager.EnableInfiniteShield.Value)
                        return;

                    __instance.cure();

                    HLog.Debug($"{nameof(InfiniteShieldPatch)} applied.");
                }
                catch (Exception ex)
                {
                    HLog.Error($"Unexpected error in {nameof(InfiniteShieldPatch)}", ex);
                }
            }
        }
    }
}
