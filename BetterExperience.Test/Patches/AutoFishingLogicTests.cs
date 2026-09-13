using BetterExperience.BConfigManager;
using BetterExperience.BControlManager;
using BetterExperience.BPatchGUI;
using BetterExperience.Patches;
using System.Reflection;
using UnityModBase.HControlSpace;
using UnityModBase.HTranslatorSpace;

namespace BetterExperience.Test.Patches
{
    public class AutoFishingLogicTests
    {
        [Fact]
        public void MoveToward_ReachesTargetWithoutOvershoot()
        {
            Assert.Equal(1.12f, AutoFishingLogic.MoveToward(0f, 10f, 1.12f), 5);
            Assert.Equal(10f, AutoFishingLogic.MoveToward(9.5f, 10f, 1.12f), 5);
            Assert.Equal(0f, AutoFishingLogic.MoveToward(0.4f, 0f, 1.12f), 5);
            Assert.Equal(3f, AutoFishingLogic.MoveToward(3f, 3f, 1.12f), 5);
        }

        [Theory]
        [InlineData(float.NaN, 1f, 1f)]
        [InlineData(1f, float.PositiveInfinity, 1f)]
        [InlineData(1f, 2f, -1f)]
        public void MoveToward_RejectsInvalidValues(float current, float target, float step)
        {
            Assert.Equal(current, AutoFishingLogic.MoveToward(current, target, step));
        }

        [Fact]
        public void HoldAction_WaitsThenAimsThenThrowsWhenClose()
        {
            Assert.Equal(AutoFishingLogic.HoldAction.Wait,
                AutoFishingLogic.DecideHoldAction(49f, true, false, 0f));
            Assert.Equal(AutoFishingLogic.HoldAction.Wait,
                AutoFishingLogic.DecideHoldAction(50f, false, false, 0f));
            Assert.Equal(AutoFishingLogic.HoldAction.Aim,
                AutoFishingLogic.DecideHoldAction(50f, true, false, 0f));
            Assert.Equal(AutoFishingLogic.HoldAction.Wait,
                AutoFishingLogic.DecideHoldAction(50f, true, true, 24f));
            Assert.Equal(AutoFishingLogic.HoldAction.Throw,
                AutoFishingLogic.DecideHoldAction(50f, true, true, 25f));
        }

        [Fact]
        public void WaterAction_HooksAfterBiteWindowAndRetrievesOnAbortOrIgnore()
        {
            Assert.Equal(AutoFishingLogic.WaterAction.Wait,
                AutoFishingLogic.DecideWaterAction(true, false, false, 11f, 0f, 0f, 1));
            Assert.Equal(AutoFishingLogic.WaterAction.Hook,
                AutoFishingLogic.DecideWaterAction(true, false, false, 12f, 0f, 0f, 1));
            Assert.Equal(AutoFishingLogic.WaterAction.Wait,
                AutoFishingLogic.DecideWaterAction(true, false, true, 12f, 0f, 0f, 1));
            Assert.Equal(AutoFishingLogic.WaterAction.Wait,
                AutoFishingLogic.DecideWaterAction(false, true, false, 0f, 59f, 0f, 0));
            Assert.Equal(AutoFishingLogic.WaterAction.Retrieve,
                AutoFishingLogic.DecideWaterAction(false, true, false, 0f, 60f, 0f, 0));
            Assert.Equal(AutoFishingLogic.WaterAction.Retrieve,
                AutoFishingLogic.DecideWaterAction(false, false, false, 0f, 0f, 360f, 0));
            Assert.Equal(AutoFishingLogic.WaterAction.Wait,
                AutoFishingLogic.DecideWaterAction(false, false, false, 0f, 0f, 360f, 1));
        }

        [Theory]
        [InlineData(0f, 0f, 0.115f, 0.04f, 0)]
        [InlineData(0f, 0.5f, 0.115f, 0.04f, 1)]
        [InlineData(0f, -0.5f, 0.115f, 0.04f, -1)]
        [InlineData(0f, 0.04f, 0.115f, 0.04f, 0)]
        [InlineData(0.2f, 0.2f, 0.115f, 0.04f, 0)]
        public void CatcherInput_TracksFishAndRestsNearCenter(
            float catcher, float fish, float range, float margin, int expected)
        {
            Assert.Equal(expected, AutoFishingLogic.ComputeCatcherInput(catcher, fish, range, margin));
        }

        [Fact]
        public void CatcherInput_MovesWhenFishLeavesTheRing()
        {
            // range+margin=0.155, 中心松手半径约 0.054；鱼在圈外必须给方向。
            Assert.Equal(1, AutoFishingLogic.ComputeCatcherInput(-0.2f, 0.2f, 0.115f, 0.04f));
            Assert.Equal(-1, AutoFishingLogic.ComputeCatcherInput(0.3f, -0.2f, 0.115f, 0.04f));
        }

        [Theory]
        [InlineData(float.NaN, 0f, 0.115f, 0.04f)]
        [InlineData(0f, float.PositiveInfinity, 0.115f, 0.04f)]
        public void CatcherInput_InvalidNumbersProduceNoInput(float catcher, float fish, float range, float margin)
        {
            Assert.Equal(0, AutoFishingLogic.ComputeCatcherInput(catcher, fish, range, margin));
        }

        [Fact]
        public void ResultConfirm_WaitsForExperienceAnimation()
        {
            Assert.False(AutoFishingLogic.ShouldConfirmResult(true, true, true, true, true, true, 1060f));
            Assert.False(AutoFishingLogic.ShouldConfirmResult(true, true, true, true, true, false, 1059f));
            Assert.False(AutoFishingLogic.ShouldConfirmResult(true, false, true, true, true, false, 1060f));
            Assert.False(AutoFishingLogic.ShouldConfirmResult(false, true, true, true, true, false, 1060f));
            Assert.True(AutoFishingLogic.ShouldConfirmResult(true, true, true, true, true, false, 1060f));
        }

        [Fact]
        public void AutoFishingSwitch_IsSessionControlWithoutConfigOrHotkey()
        {
            var control = typeof(ControlManager).GetProperty(
                nameof(ControlManager.SetAutoFishing),
                BindingFlags.NonPublic | BindingFlags.Static);
            Assert.NotNull(control);
            Assert.Equal(typeof(ControlEntry<bool>), control.PropertyType);

            Assert.Null(typeof(ConfigManager).GetProperty(nameof(ControlManager.SetAutoFishing)));
            Assert.Null(typeof(ConfigManager).GetProperty("EnableAutoFishing"));
            Assert.Null(typeof(ConfigManager).GetProperty("ToggleAutoFishingHotkey"));
        }

        [Theory]
        [InlineData(LanguageType.Chinese, "自动钓鱼已开启：开始钓鱼后将自动抛竿、起竿并收杆。", "自动钓鱼已关闭。", "自动钓鱼中")]
        [InlineData(LanguageType.English, "Auto fishing on: after you start, it will cast, hook and reel.", "Auto fishing off.", "Auto fishing active")]
        public void AutoFishingLabels_FollowConfiguredLanguage(
            LanguageType language, string enabled, string disabled, string active)
        {
            var original = Translator.DefaultLanguage;
            try
            {
                Translator.DefaultLanguage = language;
                Assert.Equal(enabled, TranslatorResource.AutoFishingEnabled.ToString());
                Assert.Equal(disabled, TranslatorResource.AutoFishingDisabled.ToString());
                Assert.Equal(active, TranslatorResource.AutoFishingActive.ToString());
            }
            finally
            {
                Translator.DefaultLanguage = original;
            }
        }

        [Fact]
        public void CloseEnough_UsesSquaredDistance()
        {
            Assert.True(AutoFishingLogic.IsCloseEnough(0f, 0f, 3f, 4f, 25f));
            Assert.False(AutoFishingLogic.IsCloseEnough(0f, 0f, 6f, 0f, 25f));
            Assert.False(AutoFishingLogic.IsCloseEnough(0f, 0f, float.NaN, 0f, 25f));
        }
    }
}
