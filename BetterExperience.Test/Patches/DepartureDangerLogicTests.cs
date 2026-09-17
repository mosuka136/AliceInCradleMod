using BetterExperience.BConfigManager;
using BetterExperience.BControlManager;
using BetterExperience.Patches;
using HarmonyLib;
using nel;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityModBase.HConfigSpace;
using XX;

namespace BetterExperience.Test.Patches
{
    public class DepartureDangerLogicTests
    {
        [Theory]
        [InlineData(0, 0)]
        [InlineData(1, 5)]
        [InlineData(2, 9)]
        [InlineData(3, 16)]
        [InlineData(8, 41)]
        [InlineData(9, 48)]
        [InlineData(14, 73)]
        [InlineData(15, 80)]
        [InlineData(30, 160)]
        public void NightLevelFromSlider_MatchesVanillaSlots(int val, int expected)
        {
            Assert.Equal(expected, DepartureDangerLogic.NightLevelFromSlider(val));
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(4, 0)]
        [InlineData(5, 1)]
        [InlineData(9, 2)]
        [InlineData(15, 2)]
        [InlineData(16, 3)]
        [InlineData(41, 8)]
        [InlineData(47, 8)]
        [InlineData(48, 9)]
        [InlineData(80, 15)]
        [InlineData(160, 30)]
        [InlineData(200, 30)]
        public void MaxSliderValue_UsesHighestSlotAtOrBelowReached(int reached, int expected)
        {
            Assert.Equal(expected, DepartureDangerLogic.MaxSliderValue(reached));
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(15, 0)]
        [InlineData(16, 2)]
        [InlineData(31, 2)]
        [InlineData(32, 5)]
        [InlineData(47, 5)]
        [InlineData(48, 8)]
        [InlineData(64, 8)]
        [InlineData(160, 8)]
        public void VanillaMaxSliderValue_CapsAtDayThreeNight(int reached, int expected)
        {
            Assert.Equal(expected, DepartureDangerLogic.VanillaMaxSliderValue(reached));
        }

        [Theory]
        [InlineData(false, 80, 8, 8)]
        [InlineData(true, 80, 8, 15)]
        [InlineData(true, 41, 5, 8)]
        [InlineData(true, 16, 2, 3)]
        public void ResolveSliderMax_UnlocksReachedSlot(
            bool unlock,
            int reached,
            int vanillaMax,
            int expected)
        {
            Assert.Equal(
                expected,
                DepartureDangerLogic.ResolveSliderMax(unlock, reached, vanillaMax));
        }

        [Fact]
        public void MaxSliderValue_NeverExceedsReachedNightLevel()
        {
            for (int reached = 0; reached <= DepartureDangerLogic.MeterCap; reached++)
            {
                int val = DepartureDangerLogic.MaxSliderValue(reached);
                Assert.True(
                    DepartureDangerLogic.NightLevelFromSlider(val) <= reached,
                    $"reached {reached} produced night {DepartureDangerLogic.NightLevelFromSlider(val)}");
            }
        }

        [Fact]
        public void AssistFlag_IsPersistentConfigNotSessionControl()
        {
            var config = typeof(ConfigManager).GetProperty(
                nameof(ConfigManager.EnableDepartureMaxReachedDanger),
                BindingFlags.Public | BindingFlags.Static);
            Assert.NotNull(config);
            Assert.Equal(typeof(ConfigEntry<bool>), config.PropertyType);
            Assert.Null(typeof(ControlManager).GetProperty(
                "SetDepartureMaxReachedDanger",
                BindingFlags.NonPublic | BindingFlags.Static));
        }

        [Fact]
        public void Init_ContainsVanillaSliderMaxPatternBytes()
        {
            var method = typeof(UiDangerLevelInitBox).GetMethod(
                nameof(UiDangerLevelInitBox.Init),
                new[] { typeof(WholeMapItem), typeof(List<string>), typeof(M2LpMapTransferWarp) });
            Assert.NotNull(method);
            var il = method.GetMethodBody().GetILAsByteArray();
            bool found = false;
            for (int i = 0; i < il.Length - 4; i++)
            {
                if (il[i] == 0x16 && il[i + 1] == 0x11 && il[i + 3] == 0x17 && il[i + 4] == 0x59)
                {
                    found = true;
                    break;
                }
            }
            Assert.True(found, "UiDangerLevelInitBox.Init no longer contains ldc.i4.0 / ldloc.s / ldc.i4.1 / sub for the slider max.");
        }

        [Fact]
        public void InitPatch_SliderMaxAssignment_InsertsReachedMaxCall()
        {
            var original = SliderMaxAssignment();
            var before = original.Select(i => (i.opcode, i.operand)).ToArray();

            Assert.True(HPatches.DepartureDangerPatch.TryRewriteSliderMax(original, out var rewritten));

            Assert.Equal(original.Count + 4, rewritten.Count);
            Assert.Equal(before, original.Select(i => (i.opcode, i.operand)));
            Assert.Single(rewritten.Where(i =>
                i.opcode == OpCodes.Call && i.operand?.ToString().Contains("ApplySliderMax") == true));
            Assert.Equal(OpCodes.Ldarg_1, rewritten[0].opcode);
            Assert.Equal(
                typeof(WholeMapItem).GetField(nameof(WholeMapItem.reached_night_level)),
                rewritten[1].operand);
        }

        [Fact]
        public void InitPatch_MissingAssignment_LeavesInstructionsUnchanged()
        {
            var original = SliderMaxAssignment();
            original.RemoveAll(i => i.opcode == OpCodes.Stfld);
            Assert.False(HPatches.DepartureDangerPatch.TryRewriteSliderMax(original, out var rewritten));
            Assert.Equal(original, rewritten);
        }

        [Fact]
        public void InitPatch_DuplicateAssignment_LeavesInstructionsUnchanged()
        {
            var original = SliderMaxAssignment();
            original.AddRange(SliderMaxAssignment());
            Assert.False(HPatches.DepartureDangerPatch.TryRewriteSliderMax(original, out var rewritten));
            Assert.Equal(original, rewritten);
        }

        private static List<CodeInstruction> SliderMaxAssignment()
        {
            var mxInt = typeof(X).GetMethod(nameof(X.Mx), new[] { typeof(int), typeof(int) });
            return new List<CodeInstruction>
            {
                new CodeInstruction(OpCodes.Ldc_I4_0),
                new CodeInstruction(OpCodes.Ldloc_S, (byte)8),
                new CodeInstruction(OpCodes.Ldc_I4_1),
                new CodeInstruction(OpCodes.Sub),
                new CodeInstruction(OpCodes.Call, mxInt),
                new CodeInstruction(OpCodes.Conv_R4),
                new CodeInstruction(OpCodes.Stfld, typeof(DsnDataSlider).GetField("mx"))
            };
        }
    }
}
