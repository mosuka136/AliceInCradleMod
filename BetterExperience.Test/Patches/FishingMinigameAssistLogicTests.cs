using BetterExperience.BConfigManager;
using BetterExperience.Patches;
using nel.mgm.fis;
using System.Reflection;

namespace BetterExperience.Test.Patches
{
    public class FishingMinigameAssistLogicTests
    {
        [Fact]
        public void EasierMarker_KeepsMinMaxSpanAndSlowsJumps()
        {
            var info = new FishData.MKInfo(default);
            float switchSpan = info.t_switch_max - info.t_switch_min;
            float jumpSpan = info.jump01_max - info.jump01_min;
            float originalJumpMin = info.jump01_min;
            float originalSwitchMin = info.t_switch_min;
            float originalReverse = info.switch_another_ratio;

            FishingMinigameAssistLogic.ApplyEasierMarker(ref info);

            Assert.Equal(switchSpan, info.t_switch_max - info.t_switch_min, 5);
            Assert.Equal(jumpSpan, info.jump01_max - info.jump01_min, 5);
            Assert.Equal(originalSwitchMin * FishingMinigameAssistLogic.MarkerSwitchMul, info.t_switch_min, 5);
            Assert.Equal(originalJumpMin * FishingMinigameAssistLogic.MarkerEaseMul, info.jump01_min, 5);
            Assert.Equal(originalReverse * FishingMinigameAssistLogic.MarkerEaseMul, info.switch_another_ratio, 5);
        }

        [Theory]
        [InlineData(0.115f, -1f, 0.115f)]
        [InlineData(0.115f, 1f, 0.115f)]
        [InlineData(0.115f, 2f, 0.23f)]
        [InlineData(0.115f, 0.4f, 0.115f)]
        [InlineData(0.115f, 3.1f, 0.115f)]
        [InlineData(0.115f, float.NaN, 0.115f)]
        public void CatcherRange_ScalesOnlyWhenConfigured(float original, float mul, float expected)
        {
            Assert.Equal(expected, FishingMinigameAssistLogic.ScaleOrKeep(
                original, mul,
                FishingMinigameAssistLogic.CatcherRangeMulMin,
                FishingMinigameAssistLogic.CatcherRangeMulMax), 5);
        }

        [Theory]
        [InlineData(0.01f, -1f, 0.01f)]
        [InlineData(0.01f, 0.1f, 0.1f)]
        [InlineData(0.01f, 0f, 0f)]
        [InlineData(0.01f, 0.3f, 0.01f)]
        public void CatcherMargin_ReplacesOnlyInRange(float original, float configured, float expected)
        {
            Assert.Equal(expected, FishingMinigameAssistLogic.ReplaceOrKeep(
                original, configured,
                FishingMinigameAssistLogic.CatcherMarginMin,
                FishingMinigameAssistLogic.CatcherMarginMax), 5);
        }

        [Theory]
        [InlineData(1.15f, 0.33f, 0.3795f)]
        [InlineData(1.15f, -1f, 1.15f)]
        [InlineData(1.15f, 0.05f, 1.15f)]
        public void MissDamage_ScalesOnlyWhenConfigured(float original, float mul, float expected)
        {
            Assert.Equal(expected, FishingMinigameAssistLogic.ScaleOrKeep(
                original, mul,
                FishingMinigameAssistLogic.MissDamageMulMin,
                FishingMinigameAssistLogic.MissDamageMulMax), 4);
        }

        [Theory]
        [InlineData(6, -1f, 6)]
        [InlineData(6, 2f, 2)]
        [InlineData(6, 0f, 1)]
        [InlineData(3, 9f, 6)]
        [InlineData(4, float.NaN, 4)]
        public void PunchCount_ClampsToOneThroughSix(int original, float configured, int expected)
        {
            Assert.Equal(expected, FishingMinigameAssistLogic.PunchCountOrKeep(original, configured));
        }

        [Fact]
        public void FishingAssist_LivesInConfigNotRuntimeControl()
        {
            Assert.NotNull(typeof(ConfigManager).GetProperty(nameof(ConfigManager.EnableEasierFishMarker)));
            Assert.NotNull(typeof(ConfigManager).GetProperty(nameof(ConfigManager.SetFishingCatcherRangeMul)));
            Assert.NotNull(typeof(ConfigManager).GetProperty(nameof(ConfigManager.SetFishingCatcherMargin)));
            Assert.NotNull(typeof(ConfigManager).GetProperty(nameof(ConfigManager.SetFishingMissDamageMul)));
            Assert.NotNull(typeof(ConfigManager).GetProperty(nameof(ConfigManager.SetFishingPunchNeedCount)));
            Assert.Null(typeof(ConfigManager).GetProperty("EnableBetterFishing"));

            const BindingFlags controlFlags = BindingFlags.NonPublic | BindingFlags.Static;
            Assert.Null(typeof(BControlManager.ControlManager).GetProperty(
                nameof(ConfigManager.EnableEasierFishMarker), controlFlags));
            Assert.Null(typeof(BControlManager.ControlManager).GetProperty(
                nameof(ConfigManager.SetFishingCatcherRangeMul), controlFlags));
            Assert.NotNull(typeof(BControlManager.ControlManager).GetProperty("SetAutoFishing", controlFlags));
        }
    }
}
