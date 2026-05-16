using BetterExperience.BConfigManager;
using HarmonyLib;
using nel;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using XX;

namespace BetterExperience.Patches
{
    public partial class HPatches
    {
        /// <summary>
        /// 移除宝箱转轮效果中的 99 个物品数量上限。
        /// 该补丁使用 transpiler 精确替换限制计数的 IL 片段，目标模式找不到时会保留原指令。
        /// </summary>
        [HarmonyPatch(typeof(ReelExecuter), "applyEffectToIK")]
        public class RemoveLimitInTreasureChestsPatch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                try
                {
                    if (!ConfigManager.EnableRemoveLimitInTreasureChests.Value)
                        return instructions;

                    var ikRowField = AccessTools.Field(typeof(ReelExecuter), "IKRow");
                    var countField = AccessTools.Field(typeof(NelItemEntry), "count");

                    var mnMethod = AccessTools.Method(typeof(X), "Mn", new[] { typeof(int), typeof(int) });

                    var matcher = new CodeMatcher(instructions);

                    matcher.MatchForward(false,
                        new CodeMatch(OpCodes.Ldarg_0),
                        new CodeMatch(OpCodes.Ldfld, ikRowField),
                        new CodeMatch(OpCodes.Ldarg_0),
                        new CodeMatch(OpCodes.Ldfld, ikRowField),
                        new CodeMatch(OpCodes.Ldfld, countField),
                        new CodeMatch(ci => ci.LoadsConstant(99)),       // 匹配 ldc.i4.s 99
                        new CodeMatch(ci => (ci.opcode == OpCodes.Call) && Equals(ci.operand, mnMethod)),
                        new CodeMatch(OpCodes.Stfld, countField)
                    );

                    if (!matcher.IsValid)
                    {
                        HLog.Error("Pattern not found: IKRow.count = X.Mn(this.IKRow.count, 99)");
                        return matcher.InstructionEnumeration();
                    }

                    for (int k = 0; k < 8; k++)
                        matcher.SetAndAdvance(OpCodes.Nop, null);

                    HLog.Debug($"{nameof(RemoveLimitInTreasureChestsPatch)} applied.");
                    return matcher.InstructionEnumeration();
                }
                catch (Exception ex)
                {
                    HLog.Error($"Unexpected error in {nameof(RemoveLimitInTreasureChestsPatch)}.", ex);
                    return instructions;
                }
            }
        }
    }
}
