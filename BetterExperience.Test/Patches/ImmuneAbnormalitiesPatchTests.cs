using BetterExperience.BConfigManager;
using BetterExperience.Patches;
using m2d;
using nel;
using System.Collections.Generic;
using System.Reflection;
using UnityModBase.HConfigSpace;

namespace BetterExperience.Test.Patches
{
    public class ImmuneAbnormalitiesPatchTests
    {
        private static readonly SER[] NewStatuses =
        {
            SER.HP_REDUCE,
            SER.POISON,
            SER.MILKY,
            SER.STRONG_HOLD,
            SER.EATEN,
            SER.FAINTED,
            SER.WORM_TRAPPED,
            SER.DEF_DOWN,
            SER.FORBIDDEN_ORGASM,
            SER.ORGASM_INITIALIZE,
            SER.ORGASM_STACK,
            SER.COCOON,
            SER.DEATH
        };

        [Fact]
        public void ConfigMap_IncludesNewIndividualStatuses()
        {
            var map = GetConfigMap();
            foreach (var ser in NewStatuses)
                Assert.True(map.ContainsKey(ser), ser.ToString());
        }

        [Theory]
        [InlineData(nameof(ConfigManager.EnableImmuneAbnormalityHpReduce))]
        [InlineData(nameof(ConfigManager.EnableImmuneAbnormalityPoison))]
        [InlineData(nameof(ConfigManager.EnableImmuneAbnormalityMilky))]
        [InlineData(nameof(ConfigManager.EnableImmuneAbnormalityStrongHold))]
        [InlineData(nameof(ConfigManager.EnableImmuneAbnormalityEaten))]
        [InlineData(nameof(ConfigManager.EnableImmuneAbnormalityFainted))]
        [InlineData(nameof(ConfigManager.EnableImmuneAbnormalityWormTrapped))]
        [InlineData(nameof(ConfigManager.EnableImmuneAbnormalityDefDown))]
        [InlineData(nameof(ConfigManager.EnableImmuneAbnormalityForbiddenOrgasm))]
        [InlineData(nameof(ConfigManager.EnableImmuneAbnormalityOrgasmInitialize))]
        [InlineData(nameof(ConfigManager.EnableImmuneAbnormalityOrgasmStack))]
        [InlineData(nameof(ConfigManager.EnableImmuneAbnormalityCocoon))]
        [InlineData(nameof(ConfigManager.EnableImmuneAbnormalityDeath))]
        public void NewImmuneSwitches_ArePersistentBoolConfigEntries(string propertyName)
        {
            var property = typeof(ConfigManager).GetProperty(propertyName);
            Assert.NotNull(property);
            Assert.Equal(typeof(ConfigEntry<bool>), property.PropertyType);
        }

        private static IDictionary<SER, Func<bool>> GetConfigMap()
        {
            var field = typeof(HPatches.ImmuneAbnormalitiesPatch).GetField(
                "ImmuneAbnormalityConfigMap",
                BindingFlags.NonPublic | BindingFlags.Static);
            Assert.NotNull(field);
            return (IDictionary<SER, Func<bool>>)field.GetValue(null);
        }
    }
}
