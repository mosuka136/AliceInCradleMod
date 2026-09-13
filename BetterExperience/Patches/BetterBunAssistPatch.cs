using BetterExperience.BConfigManager;
using BetterExperience.BLogSpace;
using HarmonyLib;
using m2d;
using nel;
using nel.mgm.bun;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityModBase.HConfigSpace;

namespace BetterExperience.Patches
{
    public partial class HPatches
    {
        /// <summary>
        /// 兔女郎酒吧手玩辅助：每次调用都读配置文件，不缓存到小游戏实例上，
        /// 因此读档、换图或重新进入酒吧后只要配置仍开启就会继续生效。
        /// </summary>
        internal static class BetterBunAssist
        {
            private static readonly MethodInfo QuitAttack = AccessTools.Method(
                typeof(M2BunEventItem),
                "quitAttackExecute",
                new[] { typeof(bool) });
            private static readonly FieldInfo StatusCarrying = AccessTools.Field(typeof(UiBunStatus), "carrying");
            private static readonly FieldInfo RunnerBun = AccessTools.Field(typeof(BunPrRunner), "Bun");

            private static bool _loggedNoBodyTouch;
            private static bool _loggedClearVision;
            private static bool _loggedEatWhileCarrying;

            internal static bool NoBodyTouch()
            {
                return IsOn(ConfigManager.EnableBunNoBodyTouch);
            }

            internal static bool ClearVision()
            {
                return IsOn(ConfigManager.EnableBunClearVision);
            }

            internal static bool EatWhileCarrying()
            {
                return IsOn(ConfigManager.EnableBunEatWhileCarrying);
            }

            internal static void StopBodyTouch(M2BunEventItem mob)
            {
                if (mob == null || !mob.isAttackMoving() || QuitAttack == null)
                    return;
                QuitAttack.Invoke(mob, new object[] { false });
            }

            public static bool CanCure(UiBunStatus status)
            {
                if (EatWhileCarrying())
                {
                    LogEatWhileCarryingOnce();
                    return true;
                }
                return status != null && status.can_cure;
            }

            internal static void HideCarryBlockOnSnackUi(UiBunStatus status)
            {
                if (!EatWhileCarrying() || status == null || StatusCarrying == null)
                    return;
                if (!(bool)StatusCarrying.GetValue(status))
                    return;
                StatusCarrying.SetValue(status, false);
                status.need_redraw = true;
            }

            internal static void KeepTrayPoseWhileEating(BunPrRunner runner, PR pr)
            {
                if (!EatWhileCarrying() || runner == null || pr == null || !runner.isEatingInputting())
                    return;
                var bun = RunnerBun == null ? null : RunnerBun.GetValue(runner) as BUN;
                if (bun == null || !bun.carrying)
                    return;
                pr.getAnimator()?.setPose("stand_sv");
            }

            internal static void LogNoBodyTouchOnce()
            {
                if (_loggedNoBodyTouch)
                    return;
                BLog.Debug($"{nameof(BetterBunAssist)} no body touch / QTE applied.");
                _loggedNoBodyTouch = true;
            }

            internal static void LogClearVisionOnce()
            {
                if (_loggedClearVision)
                    return;
                BLog.Debug($"{nameof(BetterBunAssist)} clear vision applied.");
                _loggedClearVision = true;
            }

            internal static void LogEatWhileCarryingOnce()
            {
                if (_loggedEatWhileCarrying)
                    return;
                BLog.Debug($"{nameof(BetterBunAssist)} eat while carrying applied.");
                _loggedEatWhileCarrying = true;
            }

            private static bool IsOn(ConfigEntry<bool> entry)
            {
                return ConfigManager.EnableBetterExperience?.Value == true && entry?.Value == true;
            }
        }

        /// <summary>
        /// 禁止客人扑人和兴奋度钟摆 QTE。
        /// </summary>
        [HarmonyPatch]
        public class BetterBunNoBodyTouchPatch
        {
            [HarmonyPrefix]
            [HarmonyPatch(typeof(M2BunEventItem), "runBodyTouch")]
            public static bool RunBodyTouchPrefix(M2BunEventItem __instance)
            {
                try
                {
                    if (!BetterBunAssist.NoBodyTouch())
                        return true;
                    BetterBunAssist.StopBodyTouch(__instance);
                    BetterBunAssist.LogNoBodyTouchOnce();
                    return false;
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(BetterBunNoBodyTouchPatch)}.{nameof(RunBodyTouchPrefix)}", ex);
                    return true;
                }
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(BunPrRunner), "applyDamage")]
            public static bool ApplyDamagePrefix(ref bool __result)
            {
                try
                {
                    if (!BetterBunAssist.NoBodyTouch())
                        return true;
                    __result = false;
                    BetterBunAssist.LogNoBodyTouchOnce();
                    return false;
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(BetterBunNoBodyTouchPatch)}.{nameof(ApplyDamagePrefix)}", ex);
                    return true;
                }
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(BunPrRunner), nameof(BunPrRunner.checkInitGacha))]
            public static bool CheckInitGachaPrefix(BunPrRunner __instance)
            {
                try
                {
                    if (!BetterBunAssist.NoBodyTouch())
                        return true;
                    __instance.quitAbsorbForce();
                    BetterBunAssist.LogNoBodyTouchOnce();
                    return false;
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(BetterBunNoBodyTouchPatch)}.{nameof(CheckInitGachaPrefix)}", ex);
                    return true;
                }
            }
        }

        /// <summary>
        /// 体力低时酒款图标不模糊、走路不变慢。
        /// </summary>
        [HarmonyPatch]
        public class BetterBunClearVisionPatch
        {
            [HarmonyPostfix]
            [HarmonyPatch(typeof(UiBunStatus), "get_current_hp_thresh_blur_cocktail")]
            public static void BlurCocktailPostfix(ref bool __result)
            {
                try
                {
                    if (!BetterBunAssist.ClearVision())
                        return;
                    __result = false;
                    BetterBunAssist.LogClearVisionOnce();
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(BetterBunClearVisionPatch)}.{nameof(BlurCocktailPostfix)}", ex);
                }
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(UiBunStatus), "get_current_hp_thresh_slow_walk")]
            public static void SlowWalkThreshPostfix(ref bool __result)
            {
                try
                {
                    if (!BetterBunAssist.ClearVision())
                        return;
                    __result = false;
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(BetterBunClearVisionPatch)}.{nameof(SlowWalkThreshPostfix)}", ex);
                }
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(BunPrRunner), nameof(BunPrRunner.setWalkSlow))]
            public static void SetWalkSlowPrefix(ref bool slow)
            {
                try
                {
                    if (!BetterBunAssist.ClearVision())
                        return;
                    slow = false;
                    BetterBunAssist.LogClearVisionOnce();
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(BetterBunClearVisionPatch)}.{nameof(SetWalkSlowPrefix)}", ex);
                }
            }
        }

        /// <summary>
        /// 端着酒也能吃零食：打开 UI 划线，并把 can_cure 属性改成允许。
        /// </summary>
        [HarmonyPatch]
        public class BetterBunEatWhileCarryingPatch
        {
            [HarmonyPrefix]
            [HarmonyPatch(typeof(UiBunStatus), nameof(UiBunStatus.can_cure), MethodType.Getter)]
            public static bool CanCurePrefix(ref bool __result)
            {
                try
                {
                    if (!BetterBunAssist.EatWhileCarrying())
                        return true;
                    __result = true;
                    BetterBunAssist.LogEatWhileCarryingOnce();
                    return false;
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(BetterBunEatWhileCarryingPatch)}.{nameof(CanCurePrefix)}", ex);
                    return true;
                }
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(UiBunStatus), nameof(UiBunStatus.run))]
            public static void StatusRunPostfix(UiBunStatus __instance)
            {
                try
                {
                    BetterBunAssist.HideCarryBlockOnSnackUi(__instance);
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(BetterBunEatWhileCarryingPatch)}.{nameof(StatusRunPostfix)}", ex);
                }
            }
        }

        /// <summary>
        /// 吃零食判定在 runPreSPPR 里调用 can_cure；这里直接替换调用，避免属性补丁没打上。
        /// 端着酒吃的时候保持 stand_sv，托盘不会因为换成 stand_wait_normal 而消失。
        /// </summary>
        [HarmonyPatch]
        public class BetterBunEatWhileCarryingInputPatch
        {
            public static MethodBase TargetMethod()
            {
                MethodBase method = null;
                var map = typeof(BunPrRunner).GetInterfaceMap(typeof(ISpecialPrRunner));
                for (int i = 0; i < map.InterfaceMethods.Length; i++)
                {
                    if (map.InterfaceMethods[i].Name == nameof(ISpecialPrRunner.runPreSPPR))
                    {
                        method = map.TargetMethods[i];
                        break;
                    }
                }
                return method
                    ?? AccessTools.Method(typeof(BunPrRunner), "nel.ISpecialPrRunner.runPreSPPR")
                    ?? AccessTools.DeclaredMethod(typeof(BunPrRunner), nameof(ISpecialPrRunner.runPreSPPR))
                    ?? throw new InvalidOperationException("BunPrRunner.runPreSPPR was not found.");
            }

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var canCure = AccessTools.PropertyGetter(typeof(UiBunStatus), nameof(UiBunStatus.can_cure));
                var autoCanCure = AccessTools.Method(typeof(BetterBunAssist), nameof(BetterBunAssist.CanCure));
                if (canCure == null || autoCanCure == null)
                    throw new InvalidOperationException("UiBunStatus.can_cure was not found.");

                var result = new List<CodeInstruction>(instructions);
                int replaced = 0;
                foreach (var instruction in result)
                {
                    if (instruction.opcode != OpCodes.Call && instruction.opcode != OpCodes.Callvirt)
                        continue;
                    if (!Equals(instruction.operand, canCure))
                        continue;
                    instruction.operand = autoCanCure;
                    replaced++;
                }
                if (replaced != 1)
                    throw new InvalidOperationException(
                        $"Expected one can_cure call in BunPrRunner.runPreSPPR, found {replaced}.");
                BLog.Debug($"{nameof(BetterBunEatWhileCarryingInputPatch)} transpiler replaced can_cure calls: {replaced}.");
                return result;
            }

            public static void Postfix(BunPrRunner __instance, PR Pr)
            {
                try
                {
                    BetterBunAssist.KeepTrayPoseWhileEating(__instance, Pr);
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(BetterBunEatWhileCarryingInputPatch)}", ex);
                }
            }
        }
    }
}
