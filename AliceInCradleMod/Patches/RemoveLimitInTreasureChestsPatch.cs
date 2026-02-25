using BetterExperience.BepConfigManager;
using HarmonyLib;
using nel;
using System.Collections.Generic;
using System.Reflection.Emit;
using XX;

namespace BetterExperience.Patches
{
    internal partial class HPatches
    {
        [HarmonyPatch(typeof(ReelExecuter), "applyEffectToIK")]
        private class RemoveLimitInTreasureChestsPatch
        {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
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

                HLog.Info("Patched: skipped IKRow.count = X.Mn(IKRow.count, 99)");
                return matcher.InstructionEnumeration();
            }
        }
    }
}
