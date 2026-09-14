using BetterExperience.BConfigManager;
using BetterExperience.BLogSpace;
using HarmonyLib;
using nel;
using System;

namespace BetterExperience.Patches
{
    public partial class HPatches
    {
        /// <summary>
        /// 锁定危险度：战斗结算不再加危险度，事件 <c>DANGER</c> 不再改写。
        /// 不依赖调试监听器；控制页仍可通过 <see cref="SetDangerLevelPatch"/> 写入。
        /// </summary>
        [HarmonyPatch]
        public class LockDangerLevelPatch
        {
            [HarmonyPostfix]
            [HarmonyPatch(typeof(NightController), nameof(NightController.getReservedObtainableGrade))]
            public static void ReservedGradePostfix(bool consider_debug_lock_danger, ref int __result)
            {
                try
                {
                    __result = WorldStateLock.OverrideReservedObtainableGrade(
                        __result,
                        consider_debug_lock_danger,
                        ConfigManager.EnableLockDangerLevel?.Value == true);
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(ReservedGradePostfix)}", ex);
                }
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(NightController), nameof(NightController.applyDangerousFromEvent))]
            public static bool ApplyDangerousFromEventPrefix()
            {
                try
                {
                    if (!WorldStateLock.ShouldSkipDangerEventWrite(ConfigManager.EnableLockDangerLevel?.Value == true))
                        return true;

                    BLog.Debug($"{nameof(LockDangerLevelPatch)} skipped event danger write.");
                    return false;
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(ApplyDangerousFromEventPrefix)}", ex);
                    return true;
                }
            }
        }
    }
}
