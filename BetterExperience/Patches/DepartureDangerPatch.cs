using BetterExperience.BConfigManager;
using BetterExperience.BLogSpace;
using HarmonyLib;
using nel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityModBase.HConfigSpace;
using XX;

namespace BetterExperience.Patches
{
    public partial class HPatches
    {
        /// <summary>
        /// 出门危险度滑条：把原版 <c>X.Mx(0, num3 - 1)</c> 换成按曾达最高危险度计算的档位。
        /// 关闭配置时返回原值，滑条仍最多到第 3 天夜间（41）。
        /// </summary>
        [HarmonyPatch]
        public static class DepartureDangerPatch
        {
            private static readonly MethodInfo MxInt = typeof(X).GetMethod(
                nameof(X.Mx),
                new[] { typeof(int), typeof(int) });
            private static readonly FieldInfo SliderMaxField = typeof(DsnDataSlider).GetField("mx");
            private static readonly FieldInfo ReachedField = typeof(WholeMapItem).GetField(
                nameof(WholeMapItem.reached_night_level));
            private static readonly MethodInfo ApplyMethod = typeof(DepartureDangerPatch).GetMethod(
                nameof(ApplySliderMax),
                BindingFlags.Static | BindingFlags.NonPublic);

            private static bool _available;
            private static bool _logged;

            internal static bool UnlockOn => IsOn(ConfigManager.EnableDepartureMaxReachedDanger);

            [HarmonyCleanup]
            private static void Cleanup(Exception __exception)
            {
                if (__exception == null)
                    return;
                _available = false;
                BLog.Error("Departure max-reached danger disabled: Harmony registration failed.", __exception);
            }

            [HarmonyTranspiler]
            [HarmonyPatch(
                typeof(UiDangerLevelInitBox),
                nameof(UiDangerLevelInitBox.Init),
                new[] { typeof(WholeMapItem), typeof(List<string>), typeof(M2LpMapTransferWarp) })]
            private static IEnumerable<CodeInstruction> InitTranspiler(IEnumerable<CodeInstruction> instructions)
            {
                var original = new List<CodeInstruction>(instructions);
                _available = TryRewriteSliderMax(original, out var rewritten);
                if (!_available)
                    BLog.Warn("Departure max-reached danger disabled: UiDangerLevelInitBox.Init did not match the supported game version.");
                return rewritten;
            }

            internal static bool TryRewriteSliderMax(
                IList<CodeInstruction> original,
                out List<CodeInstruction> rewritten)
            {
                rewritten = original.ToList();
                if (MxInt == null || SliderMaxField == null || ReachedField == null || ApplyMethod == null)
                    return false;

                int at = -1;
                for (int i = 0; i + 6 < original.Count; i++)
                {
                    if (!IsSliderMaxAssignment(original, i))
                        continue;
                    if (at >= 0)
                        return false;
                    at = i;
                }
                if (at < 0)
                    return false;

                rewritten.InsertRange(at, new[]
                {
                    new CodeInstruction(OpCodes.Ldarg_1),
                    new CodeInstruction(OpCodes.Ldfld, ReachedField),
                    new CodeInstruction(OpCodes.Conv_I4)
                });
                rewritten.Insert(at + 3 + 5, new CodeInstruction(OpCodes.Call, ApplyMethod));
                return true;
            }

            internal static int ApplySliderMax(int reachedNightLevel, int vanillaMax)
            {
                try
                {
                    if (!_available)
                        return vanillaMax;
                    int resolved = DepartureDangerLogic.ResolveSliderMax(
                        UnlockOn,
                        reachedNightLevel,
                        vanillaMax);
                    if (resolved != vanillaMax && !_logged)
                    {
                        BLog.Debug($"{nameof(DepartureDangerPatch)} slider max {vanillaMax} -> {resolved} (reached {reachedNightLevel}).");
                        _logged = true;
                    }
                    return resolved;
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(ApplySliderMax)}", ex);
                    return vanillaMax;
                }
            }

            private static bool IsSliderMaxAssignment(IList<CodeInstruction> codes, int i)
            {
                return codes[i].opcode == OpCodes.Ldc_I4_0
                    && IsLdloc(codes[i + 1])
                    && codes[i + 2].opcode == OpCodes.Ldc_I4_1
                    && codes[i + 3].opcode == OpCodes.Sub
                    && codes[i + 4].opcode == OpCodes.Call
                    && Equals(codes[i + 4].operand, MxInt)
                    && codes[i + 5].opcode == OpCodes.Conv_R4
                    && codes[i + 6].opcode == OpCodes.Stfld
                    && Equals(codes[i + 6].operand, SliderMaxField);
            }

            private static bool IsLdloc(CodeInstruction instruction)
            {
                return instruction.opcode == OpCodes.Ldloc
                    || instruction.opcode == OpCodes.Ldloc_S
                    || instruction.opcode == OpCodes.Ldloc_0
                    || instruction.opcode == OpCodes.Ldloc_1
                    || instruction.opcode == OpCodes.Ldloc_2
                    || instruction.opcode == OpCodes.Ldloc_3;
            }

            private static bool IsOn(ConfigEntry<bool> entry)
            {
                return ConfigManager.EnableBetterExperience?.Value == true && entry?.Value == true;
            }
        }
    }
}
