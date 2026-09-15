using BetterExperience.BConfigManager;
using BetterExperience.BControlManager;
using BetterExperience.Patches;
using nel.mgm.dojo;
using System.Reflection;
using UnityModBase.HConfigSpace;

namespace BetterExperience.Test.Patches
{
    public class DojoAssistLogicTests
    {
        [Theory]
        [InlineData(0, 2)]
        [InlineData(1, 0)]
        [InlineData(2, 1)]
        [InlineData(3, 2)]
        [InlineData(-1, 1)]
        public void WinningHand_BeatsEnemy(int enemy, int expected)
        {
            int winning = DojoAssistLogic.WinningHand(enemy);
            Assert.Equal(expected, winning);
            Assert.Equal(1, DjRPC.checkWin(winning, ((enemy % 3) + 3) % 3));
        }

        [Theory]
        [InlineData(0, 0, false, 0)]
        [InlineData(1, 0, false, 1)]
        [InlineData(0, 0, true, 2)]
        [InlineData(1, 2, true, 1)]
        public void ResolvePlayerHand_CorrectsOnlyWhenAsked(int pressed, int enemy, bool correct, int expected)
        {
            Assert.Equal(expected, DojoAssistLogic.ResolvePlayerHand(pressed, enemy, correct));
        }

        [Theory]
        [InlineData(0f, false, 0f)]
        [InlineData(20f, false, 20f)]
        [InlineData(20f, true, DjGM.hand_fast_time - 0.01f)]
        [InlineData(-20f, true, -DjGM.hand_slow_time + 0.01f)]
        [InlineData(5f, true, 5f)]
        [InlineData(-5f, true, -5f)]
        [InlineData(50f, true, 50f)]
        [InlineData(-50f, true, -50f)]
        [InlineData(1100f, true, 1100f)]
        [InlineData(2000f, true, 2000f)]
        [InlineData(14f, true, DjGM.hand_fast_time - 0.01f)]
        [InlineData(-14f, true, -14f)]
        public void AdjustTiming_ClampsWideWindowEdgesAndSkipsAppear(float t, bool wider, float expected)
        {
            Assert.Equal(expected, DojoAssistLogic.AdjustTiming(t, wider), 5);
        }

        [Fact]
        public void WideWindow_IsThreeTimesVanilla()
        {
            Assert.Equal(DjGM.hand_fast_time, DojoAssistLogic.VanillaFastWindow);
            Assert.Equal(DjGM.hand_slow_time, DojoAssistLogic.VanillaSlowWindow);
            Assert.Equal(DjGM.hand_fast_time * 3f, DojoAssistLogic.WideFastWindow);
            Assert.Equal(DjGM.HAND__NEXT_CALC, DojoAssistLogic.HandNextCalc);
            Assert.Equal(DjGM.HAND__BEAT_BITS, DojoAssistLogic.HandBeatBits);
        }

        [Theory]
        [InlineData(0f, DjGM.HAND__NEXT_CALC, true)]
        [InlineData(-14f, DjGM.HAND__NEXT_CALC, true)]
        [InlineData(-14.01f, DjGM.HAND__NEXT_CALC, false)]
        [InlineData(0.01f, DjGM.HAND__NEXT_CALC, false)]
        [InlineData(0f, 0u, false)]
        [InlineData(0f, DjGM.HAND__NEXT_CALC | DjGM.HAND_B_RK, false)]
        [InlineData(0f, DjGM.HAND__NEXT_CALC | DjGM.HAND__FAST, false)]
        [InlineData(1100f, DjGM.HAND__NEXT_CALC, false)]
        [InlineData(-5f, DjGM.HAND__NEXT_CALC | DjGM.HAND__NEXT_HAND, true)]
        public void ShouldAutoHit_OnlyAfterGoWithinVanillaSlowWindow(float t, uint bits, bool expected)
        {
            Assert.Equal(expected, DojoAssistLogic.ShouldAutoHit(t, bits));
        }

        [Theory]
        [InlineData(nameof(ConfigManager.EnableDojoWiderTiming))]
        [InlineData(nameof(ConfigManager.EnableDojoAutoHit))]
        public void AssistFlags_ArePersistentConfigNotSessionControls(string name)
        {
            var config = typeof(ConfigManager).GetProperty(name, BindingFlags.Public | BindingFlags.Static);
            Assert.NotNull(config);
            Assert.Equal(typeof(ConfigEntry<bool>), config.PropertyType);

            Assert.Null(typeof(ControlManager).GetProperty(
                "Set" + name.Substring("Enable".Length),
                BindingFlags.NonPublic | BindingFlags.Static));
        }
    }
}
