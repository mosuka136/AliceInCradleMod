using BetterExperience.HLogSpace;
using BetterExperience.Patches;
using nel;
using System.Reflection;

namespace BetterExperience.Test.Patches
{
    public class BattleStatisticsPatchTests : IDisposable
    {
        private readonly BattleStatisticsStateScope _scope = BattleStatisticsStateScope.Create();

        [Fact]
        public void PlayerInjurySetDamageCounterPostfix_WhenNotInBattle_AddsOnlyTotalDamageAndIgnoresHealing()
        {
            // Arrange
            SetStaticProperty(nameof(HPatches.BattleStatisticsPatch.IsInBattle), false);

            // Act
            HPatches.BattleStatisticsPatch.PlayerInjuryCounterPatch.SetDamageCounterPostfix(-7, 3);

            // Assert
            Assert.Equal(7, HPatches.BattleStatisticsPatch.TotalPlayerInjuryHpCounter);
            Assert.Equal(0, HPatches.BattleStatisticsPatch.TotalPlayerInjuryMpCounter);
            Assert.Equal(0, HPatches.BattleStatisticsPatch.TotalPlayerInjurySingleBattleHpCounter);
            Assert.Equal(0, HPatches.BattleStatisticsPatch.TotalPlayerInjurySingleBattleMpCounter);
            Assert.Empty(HPatches.BattleStatisticsPatch.PlayerInjuryHpCounter);
            Assert.Empty(HPatches.BattleStatisticsPatch.PlayerInjuryMpCounter);
        }

        [Fact]
        public void PlayerInjurySetDamageCounterPostfix_WhenInBattle_AddsTotalAndSingleBattleDamage()
        {
            // Arrange
            SetStaticProperty(nameof(HPatches.BattleStatisticsPatch.IsInBattle), true);

            // Act
            HPatches.BattleStatisticsPatch.PlayerInjuryCounterPatch.SetDamageCounterPostfix(-7, -4);

            // Assert
            Assert.Equal(7, HPatches.BattleStatisticsPatch.TotalPlayerInjuryHpCounter);
            Assert.Equal(4, HPatches.BattleStatisticsPatch.TotalPlayerInjuryMpCounter);
            Assert.Equal(7, HPatches.BattleStatisticsPatch.TotalPlayerInjurySingleBattleHpCounter);
            Assert.Equal(4, HPatches.BattleStatisticsPatch.TotalPlayerInjurySingleBattleMpCounter);
            Assert.Empty(HPatches.BattleStatisticsPatch.PlayerInjurySingleBattleHpCounter);
            Assert.Empty(HPatches.BattleStatisticsPatch.PlayerInjurySingleBattleMpCounter);
        }

        [Fact]
        public void PlayerInjuryApplyDamagePostfix_ClearsStoredAttackSource()
        {
            // Arrange
            SetPrivateStaticField("_objectAttackPlayer", new object());

            // Act
            HPatches.BattleStatisticsPatch.PlayerInjuryCounterPatch.ApplyDamagePostfix();

            // Assert
            Assert.Null(GetPrivateStaticField("_objectAttackPlayer"));
        }

        [Fact]
        public void EnemyInjuryApplyDamagePostfix_ClearsStoredAttackSource()
        {
            // Arrange
            SetPrivateStaticField("_objectAttackEnemy", new object());

            // Act
            HPatches.BattleStatisticsPatch.EnemyInjuryCounterPatch.ApplyDamagePostfix();

            // Assert
            Assert.Null(GetPrivateStaticField("_objectAttackEnemy"));
        }

        public void Dispose()
        {
            _scope.Dispose();
        }

        private static void SetStaticProperty(string propertyName, object value)
        {
            var property = typeof(HPatches.BattleStatisticsPatch).GetProperty(propertyName, BindingFlags.Static | BindingFlags.Public);
            Assert.NotNull(property);
            property.SetValue(null, value);
        }

        private static object GetPrivateStaticField(string fieldName)
        {
            var field = typeof(HPatches.BattleStatisticsPatch).GetField(fieldName, BindingFlags.Static | BindingFlags.NonPublic);
            Assert.NotNull(field);
            return field.GetValue(null);
        }

        private static void SetPrivateStaticField(string fieldName, object value)
        {
            var field = typeof(HPatches.BattleStatisticsPatch).GetField(fieldName, BindingFlags.Static | BindingFlags.NonPublic);
            Assert.NotNull(field);
            field.SetValue(null, value);
        }

        private sealed class BattleStatisticsStateScope : IDisposable
        {
            private static readonly PropertyInfo[] StaticProperties = typeof(HPatches.BattleStatisticsPatch)
                .GetProperties(BindingFlags.Static | BindingFlags.Public)
                .Where(property => property.GetIndexParameters().Length == 0)
                .ToArray();

            private readonly Dictionary<PropertyInfo, object> _originalValues;
            private readonly bool _originalEnableLog;

            private BattleStatisticsStateScope(Dictionary<PropertyInfo, object> originalValues, bool originalEnableLog)
            {
                _originalValues = originalValues;
                _originalEnableLog = originalEnableLog;
            }

            public static BattleStatisticsStateScope Create()
            {
                var originalValues = StaticProperties.ToDictionary(property => property, property => property.GetValue(null));
                var scope = new BattleStatisticsStateScope(originalValues, HLog.EnableLog);

                HLog.EnableLog = false;
                ResetState();

                return scope;
            }

            public void Dispose()
            {
                foreach (var pair in _originalValues)
                {
                    pair.Key.SetValue(null, pair.Value);
                }

                HLog.EnableLog = _originalEnableLog;
            }

            private static void ResetState()
            {
                foreach (var property in StaticProperties)
                {
                    if (property.PropertyType == typeof(int))
                    {
                        property.SetValue(null, 0);
                    }
                    else if (property.PropertyType == typeof(float))
                    {
                        property.SetValue(null, 0f);
                    }
                    else if (property.PropertyType == typeof(bool))
                    {
                        property.SetValue(null, false);
                    }
                    else if (property.PropertyType == typeof(Dictionary<(ENEMYID, ENATTR), int>))
                    {
                        property.SetValue(null, new Dictionary<(ENEMYID, ENATTR), int>());
                    }
                }
            }
        }
    }
}
