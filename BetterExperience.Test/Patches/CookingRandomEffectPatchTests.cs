using BetterExperience.BPatchGUI;
using BetterExperience.Patches;
using HarmonyLib;
using nel;
using System.Reflection;
using System.Reflection.Emit;
using UnityModBase.HTranslatorSpace;

namespace BetterExperience.Test.Patches
{
    public class CookingRandomEffectPatchTests
    {
        [Fact]
        public void LayoutPatch_CurrentGame_InsertsOneConditionalAdjustmentWithoutChangingOriginal()
        {
            var original = ReadLayoutInstructions();
            var before = original.Select(i => (i.opcode, i.operand)).ToArray();

            Assert.True(CookingRandomEffectPatch.TryReserveButtonRow(original, out var rewritten));

            Assert.Equal(original.Count + 2, rewritten.Count);
            Assert.Equal(before, original.Select(i => (i.opcode, i.operand)));
            Assert.Single(rewritten.Where(i => i.opcode == OpCodes.Call && i.operand?.ToString().Contains("AdjustConfirmOffset") == true));
        }

        [Theory]
        [InlineData("offset")]
        [InlineData("duplicate")]
        [InlineData("tab")]
        public void LayoutPatch_UnsupportedGameShape_LeavesInstructionsUnchanged(string change)
        {
            var original = ReadLayoutInstructions();
            if (change == "offset")
                original.RemoveAll(i => i.opcode == OpCodes.Ldc_R4 && Equals(i.operand, -60f));
            else if (change == "duplicate")
                original.Add(new CodeInstruction(OpCodes.Ldc_R4, -60f));
            else
                original.RemoveAll(i => i.opcode == OpCodes.Ldstr && Equals(i.operand, "Cmd-bottom"));

            Assert.False(CookingRandomEffectPatch.TryReserveButtonRow(original, out var rewritten));
            Assert.Equal(original, rewritten);
        }

        [Theory]
        [InlineData(LanguageType.Chinese, "重新随机", "蘑菇随机效果（预览）")]
        [InlineData(LanguageType.English, "Reroll Effects", "Mushroom Effects (Preview)")]
        public void CookingLabels_FollowConfiguredLanguage(LanguageType language, string button, string preview)
        {
            var original = Translator.DefaultLanguage;
            try
            {
                Translator.DefaultLanguage = language;
                Assert.Equal(button, TranslatorResource.CookingReroll.ToString());
                Assert.Equal(preview, TranslatorResource.CookingRandomPreview.ToString());
            }
            finally
            {
                Translator.DefaultLanguage = original;
            }
        }

        // 游戏自带的 Harmony 依赖 Mono/.NET Framework 内部 API，不能在 net8 测试宿主中初始化。
        // 直接解码实际游戏方法的 IL，验证生产 Transpiler 的匹配和变换，不在测试进程安装 detour。
        private static List<CodeInstruction> ReadLayoutInstructions()
        {
            var method = typeof(UiCraftBase).GetMethod("initCmd", BindingFlags.Instance | BindingFlags.NonPublic);
            var codes = typeof(OpCodes).GetFields(BindingFlags.Static | BindingFlags.Public)
                .Where(field => field.FieldType == typeof(OpCode))
                .Select(field => (OpCode)field.GetValue(null)).ToDictionary(code => (ushort)code.Value);
            using var stream = new MemoryStream(method.GetMethodBody().GetILAsByteArray());
            using var reader = new BinaryReader(stream);
            var result = new List<CodeInstruction>();
            while (stream.Position < stream.Length)
            {
                ushort value = reader.ReadByte();
                if (value == 0xfe)
                    value = (ushort)(0xfe00 | reader.ReadByte());
                var code = codes[value];
                object operand = code.OperandType switch
                {
                    OperandType.InlineNone => null,
                    OperandType.ShortInlineI or OperandType.ShortInlineBrTarget => reader.ReadSByte(),
                    OperandType.ShortInlineVar => reader.ReadByte(),
                    OperandType.InlineVar => reader.ReadUInt16(),
                    OperandType.InlineI or OperandType.InlineBrTarget => reader.ReadInt32(),
                    OperandType.InlineI8 => reader.ReadInt64(),
                    OperandType.ShortInlineR => reader.ReadSingle(),
                    OperandType.InlineR => reader.ReadDouble(),
                    OperandType.InlineString => method.Module.ResolveString(reader.ReadInt32()),
                    OperandType.InlineField => method.Module.ResolveField(reader.ReadInt32()),
                    OperandType.InlineMethod => method.Module.ResolveMethod(reader.ReadInt32()),
                    OperandType.InlineType or OperandType.InlineTok => method.Module.ResolveMember(reader.ReadInt32()),
                    OperandType.InlineSwitch => Enumerable.Range(0, reader.ReadInt32()).Select(_ => reader.ReadInt32()).ToArray(),
                    _ => throw new NotSupportedException(code.OperandType.ToString())
                };
                result.Add(new CodeInstruction(code, operand));
            }
            return result;
        }
    }
}
