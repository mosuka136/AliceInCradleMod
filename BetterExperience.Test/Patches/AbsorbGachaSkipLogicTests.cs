using BetterExperience.BConfigManager;
using BetterExperience.BControlManager;
using BetterExperience.Patches;
using nel;
using System.Reflection;
using UnityModBase.HConfigSpace;

namespace BetterExperience.Test.Patches
{
    public class AbsorbGachaSkipLogicTests
    {
        [Theory]
        [InlineData(false, PrGachaItem.TYPE.REP, false, false, false, false)]
        [InlineData(true, PrGachaItem.TYPE.CANNOT_RELEASE, false, false, false, false)]
        [InlineData(true, PrGachaItem.TYPE.REP, true, false, false, false)]
        [InlineData(true, PrGachaItem.TYPE.PENDULUM, false, true, false, false)]
        [InlineData(true, PrGachaItem.TYPE.SEQUENCE, false, false, true, false)]
        [InlineData(true, PrGachaItem.TYPE.REP, false, false, false, true)]
        [InlineData(true, PrGachaItem.TYPE.HOLD, false, false, false, true)]
        [InlineData(true, PrGachaItem.TYPE.PENDULUM, false, false, false, true)]
        [InlineData(true, PrGachaItem.TYPE.PENDULUM_ONNIE, false, false, false, true)]
        [InlineData(true, PrGachaItem.TYPE.REP_AFTER_ORGASM, false, false, false, true)]
        public void ShouldFinish_SkipsAbsorbEscapeOnly(
            bool skipOn,
            PrGachaItem.TYPE type,
            bool evAssign,
            bool isMasturbate,
            bool isEventGacha,
            bool expected)
        {
            Assert.Equal(
                expected,
                AbsorbGachaSkipLogic.ShouldFinish(skipOn, type, evAssign, isMasturbate, isEventGacha));
        }

        [Theory]
        [InlineData(0, 0f)]
        [InlineData(100, 100f)]
        [InlineData(500, 500f)]
        public void FinishedCount_MatchesNeedCountUnits(int count0, float expected)
        {
            Assert.Equal(expected, AbsorbGachaSkipLogic.FinishedCount(count0));
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(true, true)]
        public void ShouldTreatRunAsComplete_FollowsSkipEligibility(bool skipEligible, bool expected)
        {
            Assert.Equal(expected, AbsorbGachaSkipLogic.ShouldTreatRunAsComplete(skipEligible));
        }

        [Theory]
        [InlineData(false, true, true, false)]
        [InlineData(true, false, true, false)]
        [InlineData(true, true, false, false)]
        [InlineData(true, true, true, true)]
        public void ShouldForceContainerRelease_RequiresReadySkipGacha(
            bool skipOn,
            bool anySkipEligibleUseable,
            bool allUseableReleaseable,
            bool expected)
        {
            Assert.Equal(
                expected,
                AbsorbGachaSkipLogic.ShouldForceContainerRelease(
                    skipOn, anySkipEligibleUseable, allUseableReleaseable));
        }

        [Fact]
        public void AssistFlag_IsPersistentConfigNotSessionControl()
        {
            var config = typeof(ConfigManager).GetProperty(
                nameof(ConfigManager.EnableAbsorbGachaSkip),
                BindingFlags.Public | BindingFlags.Static);
            Assert.NotNull(config);
            Assert.Equal(typeof(ConfigEntry<bool>), config.PropertyType);
            Assert.Null(typeof(ControlManager).GetProperty(
                "SetAbsorbGachaSkip",
                BindingFlags.NonPublic | BindingFlags.Static));
        }
    }
}
