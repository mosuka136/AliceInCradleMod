using BetterExperience.BConfigManager;
using BetterExperience.BControlManager;
using System.Reflection;
using UnityModBase.HClassAttribute;
using UnityModBase.HConfigSpace;
using UnityModBase.HControlSpace;

namespace BetterExperience.Test
{
    public class ConfigControlParityTests
    {
        private static readonly string[] ExpectedPreloadEntryNames = new[]
        {
            // Player (12)
            nameof(ConfigManager.SetBackpackCapacity),
            nameof(ConfigManager.SetBottleHolderCount),
            nameof(ConfigManager.SetBombHolderSlotCount),
            nameof(ConfigManager.SetItemReelHolderSlotCount),
            nameof(ConfigManager.SetPlayerHp),
            nameof(ConfigManager.SetPlayerMp),
            nameof(ConfigManager.SetPlayerEp),
            nameof(ConfigManager.SetPlayerMaxHp),
            nameof(ConfigManager.SetPlayerMaxMp),
            nameof(ConfigManager.SetPlayerMaxSatiety),
            nameof(ConfigManager.SetOverChargeSlotCount),
            nameof(ConfigManager.SetEnhancerSlotCount),

            // Cane (18)
            nameof(ConfigManager.SetCaneSwingSpeed),
            nameof(ConfigManager.SetCaneCastSpeed),
            nameof(ConfigManager.SetCaneBalance),
            nameof(ConfigManager.SetCaneEfficiency),
            nameof(ConfigManager.SetCaneRetention),
            nameof(ConfigManager.SetCaneLockOn),
            nameof(ConfigManager.SetCaneLongRange),
            nameof(ConfigManager.SetCaneShortRange),
            nameof(ConfigManager.SetCaneReach),
            nameof(ConfigManager.SetCaneNearPower),
            nameof(ConfigManager.SetCaneNearShotgunPower),
            nameof(ConfigManager.SetCaneStability),
            nameof(ConfigManager.SetCaneManaSplashRatio),
            nameof(ConfigManager.SetCaneCastspeedOverhold),
            nameof(ConfigManager.SetCaneDrainAfterLock),
            nameof(ConfigManager.SetCaneCastspeed),
            nameof(ConfigManager.SetCaneMagicPrepareSpeed),
            nameof(ConfigManager.SetCaneHolderSlotCount),

            // Map (1)
            nameof(ConfigManager.SetDangerLevel),

            // Weather (6)
            nameof(ConfigManager.SetWeatherWind),
            nameof(ConfigManager.SetWeatherThunder),
            nameof(ConfigManager.SetWeatherMist),
            nameof(ConfigManager.SetWeatherDrought),
            nameof(ConfigManager.SetWeatherDenseMist),
            nameof(ConfigManager.SetWeatherPlague),

            // Currency (6)
            nameof(ConfigManager.SetCurrencyGoldCount),
            nameof(ConfigManager.SetCurrencyCraftsCount),
            nameof(ConfigManager.SetCurrencyJuiceCount),
            nameof(ConfigManager.SetCurrencyBarScoreCount),
            nameof(ConfigManager.SetGuildPoint),
            nameof(ConfigManager.SetFoodTicketCount)
        };

        // 只存在于实时控制、不落盘的会话开关；不要求对应 ConfigEntry<bool, T>。
        private static readonly string[] ExpectedControlOnlyEntryNames = new[]
        {
            nameof(ControlManager.SetAutoFishing),
            nameof(ControlManager.SetAutoMilk),
            nameof(ControlManager.SetAutoBunServe),
            nameof(ControlManager.SetCaneInstantSwitchCount),
            nameof(ControlManager.SetKillEnemies),
            nameof(ControlManager.SetKillEnemiesOnSpawn),
            nameof(ControlManager.SetEggCategory),
            nameof(ControlManager.SetEggCount),
            nameof(ControlManager.SummonWanderingNpcKind),
            nameof(ControlManager.SummonWanderingNpc),
            nameof(ControlManager.GiveKind),
            nameof(ControlManager.GiveFilter),
            nameof(ControlManager.GiveChoices),
            nameof(ControlManager.GiveItemGrade),
            nameof(ControlManager.GiveItemCount),
            nameof(ControlManager.GiveItem),
            nameof(ControlManager.PortraitFilter),
            nameof(ControlManager.PortraitPoses),
            nameof(ControlManager.PortraitPresets),
            nameof(ControlManager.PortraitStates),
            nameof(ControlManager.PortraitAdditional),
            nameof(ControlManager.ApplyPortrait),
            nameof(ControlManager.LockPortrait)
        };

        [Fact]
        public void PreloadConfigEntries_ExactlyMatchRuntimeControls()
        {
            // Arrange
            var expectedNames = ExpectedPreloadEntryNames.OrderBy(name => name).ToArray();

            // Act
            var preloadProperties = typeof(ConfigManager)
                .GetProperties(BindingFlags.Public | BindingFlags.Static)
                .Where(property => IsGenericType(property.PropertyType, typeof(ConfigEntry<,>)))
                .Where(property => property.PropertyType.GetGenericArguments()[0] == typeof(bool))
                .OrderBy(property => property.Name)
                .ToArray();
            var controlProperties = typeof(ControlManager)
                .GetProperties(BindingFlags.NonPublic | BindingFlags.Static)
                .Where(property => IsGenericType(property.PropertyType, typeof(ControlEntry<>)))
                .OrderBy(property => property.Name)
                .ToArray();

            var expectedControlNames = ExpectedPreloadEntryNames
                .Concat(ExpectedControlOnlyEntryNames)
                .OrderBy(name => name)
                .ToArray();

            // Assert
            Assert.Equal(43, ExpectedPreloadEntryNames.Length);
            Assert.Equal(expectedNames, preloadProperties.Select(property => property.Name).ToArray());
            Assert.Equal(expectedControlNames, controlProperties.Select(property => property.Name).ToArray());

            foreach (var preloadProperty in preloadProperties)
            {
                var valueType = preloadProperty.PropertyType.GetGenericArguments()[1];
                var controlProperty = controlProperties.Single(property => property.Name == preloadProperty.Name);

                Assert.Equal(2, preloadProperty.GetCustomAttribute<EntryGuiAttribute>()?.Count);
                Assert.Equal(typeof(ControlEntry<>).MakeGenericType(valueType), controlProperty.PropertyType);
            }
        }

        private static bool IsGenericType(Type type, Type genericTypeDefinition)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == genericTypeDefinition;
        }
    }
}
