using BetterExperience.BConfigManager;
using BetterExperience.BControlManager;
using BetterExperience.Patches;
using System.Reflection;

namespace BetterExperience.Test.Patches
{
    public class WorldStateLockTests
    {
        [Theory]
        [InlineData(false, false, false)]
        [InlineData(true, false, true)]
        [InlineData(false, true, true)]
        [InlineData(true, true, true)]
        public void AllowsNightThunderTravel_OrsTheTwoSwitches(
            bool fastTravelAnywhere,
            bool allowNightTravel,
            bool expected)
        {
            Assert.Equal(
                expected,
                WorldStateLock.AllowsNightThunderTravel(fastTravelAnywhere, allowNightTravel));
        }

        [Theory]
        [InlineData(null, false, null)]
        [InlineData(null, true, null)]
        [InlineData("Alert_cannot_fast_travel", false, "Alert_cannot_fast_travel")]
        [InlineData("Alert_cannot_fast_travel", true, null)]
        [InlineData("Alert_bench_execute_scenario_locked", false, "Alert_bench_execute_scenario_locked")]
        [InlineData("Alert_bench_execute_scenario_locked", true, "Alert_bench_execute_scenario_locked")]
        [InlineData("other", true, "other")]
        public void OverrideCantFastTravel_OnlyClearsNightThunderAlert(
            string original,
            bool allowNightThunder,
            string expected)
        {
            Assert.Equal(expected, WorldStateLock.OverrideCantFastTravel(original, allowNightThunder));
        }

        [Theory]
        [InlineData(null, true, true, null)]
        [InlineData("Alert_cannot_fast_travel", true, true, null)]
        [InlineData("Alert_bench_execute_scenario_locked", true, true, "Alert_bench_execute_scenario_locked")]
        [InlineData(null, false, true, "Alert_cannot_fast_travel")]
        [InlineData("Alert_cannot_fast_travel", false, true, "Alert_cannot_fast_travel")]
        [InlineData("Alert_bench_execute_scenario_locked", false, true, "Alert_bench_execute_scenario_locked")]
        [InlineData(null, false, false, null)]
        public void ApplyNightTravelAssist_ReblocksNightWhenAssistIsOff(
            string original,
            bool assistOn,
            bool vanillaBlocks,
            string expected)
        {
            Assert.Equal(
                expected,
                WorldStateLock.ApplyNightTravelAssist(original, assistOn, vanillaBlocks));
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(true, true)]
        public void ShouldSkipWeatherShuffle_FollowsLockSwitch(bool lockWeather, bool expected)
        {
            Assert.Equal(expected, WorldStateLock.ShouldSkipWeatherShuffle(lockWeather));
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(true, true)]
        public void ShouldSkipDangerEventWrite_FollowsLockSwitch(bool lockDanger, bool expected)
        {
            Assert.Equal(expected, WorldStateLock.ShouldSkipDangerEventWrite(lockDanger));
        }

        [Theory]
        [InlineData(3, false, false, 3)]
        [InlineData(3, false, true, 3)]
        [InlineData(3, true, false, 3)]
        [InlineData(3, true, true, 0)]
        [InlineData(0, true, true, 0)]
        public void OverrideReservedObtainableGrade_ZerosOnlyWhenBattleLockApplies(
            int original,
            bool considerLock,
            bool lockDanger,
            int expected)
        {
            Assert.Equal(
                expected,
                WorldStateLock.OverrideReservedObtainableGrade(original, considerLock, lockDanger));
        }

        [Theory]
        [InlineData(false, 0, false, false)]
        [InlineData(true, 0, false, false)]
        [InlineData(true, 1, false, true)]
        [InlineData(false, 8, false, false)]
        [InlineData(false, 0, true, true)]
        [InlineData(true, 1, true, true)]
        public void VanillaNightOrThunderBlocksTravel_MatchesOriginalCantFastTravelRule(
            bool isNight,
            int battleCount,
            bool hasThunder,
            bool expected)
        {
            Assert.Equal(
                expected,
                WorldStateLock.VanillaNightOrThunderBlocksTravel(isNight, battleCount, hasThunder));
        }

        [Theory]
        [InlineData(false, true, true, false, true)]
        [InlineData(true, false, true, false, true)]
        [InlineData(true, true, false, false, true)]
        [InlineData(true, true, true, true, true)]
        [InlineData(true, true, true, false, false)]
        public void ShouldPersistFreeTravelAnalyzed_OnlyBlocksUnlockCausedByAssist(
            bool settingTrue,
            bool assistOn,
            bool vanillaBlocks,
            bool currentUnlocked,
            bool expected)
        {
            Assert.Equal(
                expected,
                WorldStateLock.ShouldPersistFreeTravelAnalyzed(
                    settingTrue,
                    assistOn,
                    vanillaBlocks,
                    currentUnlocked));
        }

        [Theory]
        [InlineData(true, false, true, true, true)]
        [InlineData(false, true, true, true, true)]
        [InlineData(false, false, true, true, false)]
        [InlineData(true, true, false, true, false)]
        [InlineData(true, false, true, false, false)]
        public void ComputeMenuFastTravelAllowed_RequiresCommandHandleAndBenchOrAnywhere(
            bool onBench,
            bool anywhere,
            bool commandEnabled,
            bool mapCanHandle,
            bool expected)
        {
            Assert.Equal(
                expected,
                WorldStateLock.ComputeMenuFastTravelAllowed(onBench, anywhere, commandEnabled, mapCanHandle));
        }

        [Fact]
        public void MapAndWeatherLockSwitches_ArePersistentBoolConfigEntries()
        {
            Assert.Equal(
                typeof(UnityModBase.HConfigSpace.ConfigEntry<bool>),
                typeof(ConfigManager).GetProperty(nameof(ConfigManager.EnableAllowNightTravel))?.PropertyType);
            Assert.Equal(
                typeof(UnityModBase.HConfigSpace.ConfigEntry<bool>),
                typeof(ConfigManager).GetProperty(nameof(ConfigManager.EnableLockDangerLevel))?.PropertyType);
            Assert.Equal(
                typeof(UnityModBase.HConfigSpace.ConfigEntry<bool>),
                typeof(ConfigManager).GetProperty(nameof(ConfigManager.EnableLockWeather))?.PropertyType);

            Assert.Null(typeof(ControlManager).GetProperty(
                nameof(ConfigManager.EnableAllowNightTravel),
                BindingFlags.NonPublic | BindingFlags.Static));
            Assert.Null(typeof(ControlManager).GetProperty(
                nameof(ConfigManager.EnableLockDangerLevel),
                BindingFlags.NonPublic | BindingFlags.Static));
            Assert.Null(typeof(ControlManager).GetProperty(
                nameof(ConfigManager.EnableLockWeather),
                BindingFlags.NonPublic | BindingFlags.Static));
        }
    }
}
