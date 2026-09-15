using BetterExperience.BConfigManager;
using BetterExperience.BLogSpace;
using HarmonyLib;
using nel;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityModBase.HConfigSpace;

namespace BetterExperience.Patches
{
    public partial class HPatches
    {
        /// <summary>
        /// 战斗中快速切换法杖：把战斗内装备法杖的耗时动作改写为原版的立即切换路径，
        /// 库存处理完全复用原版逻辑；立即切换次数沿用游戏的
        /// noel_cane_switchable_in_battle（战斗开始时重置、每次切换递减；
        /// 不限次数时不递减且保持至少 1）。
        /// </summary>
        internal static class CaneQuickSwitch
        {
            private static bool _logged;

            internal static bool QuickSwitchOn => IsOn(ConfigManager.EnableCaneQuickSwitchInBattle);

            internal static bool UnlimitedOn => IsOn(ConfigManager.EnableCaneInstantSwitchUnlimited);

            internal static bool NoInterruptOn => IsOn(ConfigManager.EnableCaneSwitchNoInterrupt);

            /// <summary>当前剩余的立即切换次数；物品管理器未加载时返回 -1。</summary>
            public static int GetInstantSwitchCount()
            {
                return GetIMNG()?.noel_cane_switchable_in_battle ?? -1;
            }

            public static void SetInstantSwitchCount(int count)
            {
                var imng = GetIMNG();
                if (imng == null)
                {
                    BLog.Notice("Item manager not found while setting instant cane switch count.");
                    return;
                }
                imng.noel_cane_switchable_in_battle = Math.Max(0, count);
                BLog.Debug($"Instant cane switch count set. New count: {imng.noel_cane_switchable_in_battle}");
            }

            /// <summary>战斗内的装备命令能否改写为立即切换：需要开关开启且次数仍有富余（或不限次数）。</summary>
            internal static bool ShouldRewriteBattleCommand()
            {
                if (!QuickSwitchOn)
                    return false;
                if (UnlimitedOn)
                    return true;
                return GetInstantSwitchCount() > 0;
            }

            /// <summary>
            /// 战斗开始时游戏按法杖收纳杖内收纳的法杖数量重置次数；按配置覆盖：
            /// 不限次数仅把次数补足到至少 1（不覆盖更高的原版数量），固定次数直接写入。
            /// </summary>
            internal static void ApplyCountConfig(NelItemManager imng)
            {
                if (imng == null)
                    return;
                if (UnlimitedOn)
                    EnsureUnlimitedFloor(imng);
                else if ((ConfigManager.SetCaneInstantSwitchCount?.Value ?? -1) >= 0)
                    imng.noel_cane_switchable_in_battle = ConfigManager.SetCaneInstantSwitchCount.Value;
            }

            /// <summary>不限次数时把立即切换次数补足到至少 1。</summary>
            internal static void EnsureUnlimitedFloor(NelItemManager imng = null)
            {
                if (!UnlimitedOn)
                    return;
                if (imng == null)
                    imng = GetIMNG();
                if (imng != null && imng.noel_cane_switchable_in_battle < 1)
                    imng.noel_cane_switchable_in_battle = 1;
            }

            private static bool IsOn(ConfigEntry<bool> entry)
            {
                return ConfigManager.EnableBetterExperience?.Value == true && entry?.Value == true;
            }

            internal static void LogOnce()
            {
                if (_logged)
                    return;
                BLog.Debug($"{nameof(CaneQuickSwitch)} applied: quick cane switch in battle.");
                _logged = true;
            }
        }

        /// <summary>
        /// 把战斗内装备法杖的命令从“equip_cane_in_battle”（耗时动作）改写为“equip_cane”
        /// （原版立即切换）。之后由原版命令执行器完成全部库存处理并在战斗中递减立即切换次数；
        /// 次数耗尽后自动回退原版的耗时动作。
        /// </summary>
        [HarmonyPatch]
        public class CaneQuickSwitchCommandPatch
        {
            public static bool Prepare()
            {
                return TargetMethod() != null;
            }

            public static MethodBase TargetMethod()
            {
                var type = AccessTools.TypeByName("nel.gm.UiGMCItem");
                return type == null ? null : AccessTools.Method(type, "fnItemCommandKeysPrepare");
            }

            [HarmonyPostfix]
            public static void Postfix(ref List<string> __result)
            {
                try
                {
                    var commands = __result;
                    var index = commands?.IndexOf("equip_cane_in_battle") ?? -1;
                    if (index < 0)
                        return;
                    // 不限次数时先补足次数，菜单标签与门槛判定立即恢复到至少 1。
                    CaneQuickSwitch.EnsureUnlimitedFloor();
                    if (!CaneQuickSwitch.ShouldRewriteBattleCommand())
                        return;
                    commands[index] = "equip_cane";
                    CaneQuickSwitch.LogOnce();
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(CaneQuickSwitchCommandPatch)} postfix", ex);
                }
            }
        }

        /// <summary>
        /// 战斗开始（或无边界解除）时按配置覆盖立即切换次数的重置值。
        /// </summary>
        [HarmonyPatch]
        public class CaneQuickSwitchCountPatch
        {
            [HarmonyPostfix]
            [HarmonyPatch(typeof(NelItemManager), nameof(NelItemManager.fineCaneSwitchCountInBattle))]
            public static void Postfix(NelItemManager __instance)
            {
                try
                {
                    if (ConfigManager.EnableBetterExperience?.Value != true)
                        return;
                    CaneQuickSwitch.ApplyCountConfig(__instance);
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(CaneQuickSwitchCountPatch)} postfix", ex);
                }
            }
        }

        /// <summary>
        /// 原版在战斗中执行立即切换后递减次数（fnItemCommandExecuted 的 equip_cane 分支）。
        /// 不限次数时在方法前后快照并恢复该字段：次数不减少，且始终至少为 1。
        /// </summary>
        [HarmonyPatch]
        public class CaneQuickSwitchKeepCountPatch
        {
            public static bool Prepare()
            {
                return TargetMethod() != null;
            }

            public static MethodBase TargetMethod()
            {
                var type = AccessTools.TypeByName("nel.gm.UiGMCItem");
                return type == null ? null : AccessTools.Method(type, "fnItemCommandExecuted");
            }

            [HarmonyPrefix]
            public static void Prefix(out int __state)
            {
                __state = int.MinValue;
                try
                {
                    __state = CaneQuickSwitch.GetInstantSwitchCount();
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(CaneQuickSwitchKeepCountPatch)} prefix", ex);
                }
            }

            [HarmonyPostfix]
            public static void Postfix(int __state)
            {
                try
                {
                    if (!CaneQuickSwitch.UnlimitedOn)
                        return;
                    var imng = GetIMNG();
                    if (imng == null)
                        return;
                    var kept = Math.Max(__state, 1);
                    if (imng.noel_cane_switchable_in_battle < kept)
                        imng.noel_cane_switchable_in_battle = kept;
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(CaneQuickSwitchKeepCountPatch)} postfix", ex);
                }
            }
        }

        /// <summary>
        /// 切换法杖的耗时动作期间完全免疫伤害：所有伤害入口最终都汇聚到
        /// M2PrADmg.applyDamage 的 6 参核心重载，在其之前返回 0 可保证
        /// 不受伤、不硬直、不被打断，法杖也不会残留在地上。
        /// </summary>
        [HarmonyPatch]
        public class CaneQuickSwitchNoInterruptPatch
        {
            public static bool Prepare()
            {
                return TargetMethod() != null;
            }

            public static MethodBase TargetMethod()
            {
                foreach (var method in AccessTools.GetDeclaredMethods(typeof(M2PrADmg)))
                {
                    if (method.Name == "applyDamage" && method.GetParameters().Length == 6)
                        return method;
                }
                return null;
            }

            [HarmonyPrefix]
            public static bool Prefix(M2PrADmg __instance, ref int __result)
            {
                try
                {
                    if (!CaneQuickSwitch.NoInterruptOn)
                        return true;
                    if (__instance.Pr?.SpRunner is NoelCaneEquipSwitcher)
                    {
                        __result = 0;
                        return false;
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(CaneQuickSwitchNoInterruptPatch)} prefix", ex);
                    return true;
                }
            }
        }
    }
}
