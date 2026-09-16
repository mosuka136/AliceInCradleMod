using BetterExperience.BLogSpace;
using System;
using UnityModBase.HClassAttribute;
using UnityModBase.HConfigSpace;
using UnityModBase.HTranslatorSpace;

namespace BetterExperience.BConfigManager
{
    public static partial class ConfigManager
    {
        // 地图配置主要影响地图交互限制和环境伤害；SetDangerLevel 为双值“读档后预加载”项，Value2 的 -1 表示不覆盖当前危险度。
        public static ConfigEntry<bool> EnableBetterSaveSite { get; private set; }
        public static ConfigEntry<bool> EnableRemoveLimitInPuppetNpcDefeated { get; private set; }
        public static ConfigEntry<bool> EnableFastTravelAnywhere { get; private set; }
        public static ConfigEntry<bool> EnableAllowNightTravel { get; private set; }
        public static ConfigEntry<bool> EnableWormTrap { get; private set; }
        public static ConfigEntry<bool> EnableMapDamage { get; private set; }
        public static ConfigEntry<bool> EnableDrowning { get; private set; }
        public static ConfigEntry<bool> EnableDarkArea { get; private set; }
        public static ConfigEntry<bool> EnableLockDangerLevel { get; private set; }
        public static ConfigEntry<bool> EnableAlwaysShowWanderingNpcOnMap { get; private set; }
        public static ConfigEntry<bool> EnableWanderingNpcAlwaysAppear { get; private set; }
        public static ConfigEntry<bool> EnableBattleEnemyPreview { get; private set; }
        public static ConfigEntry<bool> EnableNightSummonerAlwaysOpen { get; private set; }
        [EntryGui(2)]
        [EntrySlider(1, -1f, 160f, 1f)]
        public static ConfigEntry<bool, int> SetDangerLevel { get; private set; }

        private const string SectionMap = "Map";

        /// <summary>
        /// 初始化地图、陷阱、快速传送和危险度相关配置。
        /// </summary>
        public static void InitializeMapTrap()
        {
            try
            {
                Config.CreateTable(SectionMap, new Translator(chinese: "地图", english: "Map"));

                EnableBetterSaveSite = Config.Bind(
                    SectionMap,
                    nameof(EnableBetterSaveSite),
                    false,
                    new Translator(chinese: "启用更好的存档点", english: "Enable Better Save Site"),
                    new Translator(
                        chinese: "启用更好的存档点功能。允许在任意位置保存。",
                        english: "Enable better save site. It will allow saving anywhere."
                        )
                    );
                EnableRemoveLimitInPuppetNpcDefeated = Config.Bind(
                    SectionMap,
                    nameof(EnableRemoveLimitInPuppetNpcDefeated),
                    false,
                    new Translator(chinese: "启用移除木偶商人限制", english: "Enable Remove Limit In Puppet NPC Defeated"),
                    new Translator(
                        chinese: "移除木偶商人在复仇战未完成前无法生成的限制。",
                        english: "Remove the restriction that prevents the Puppet Merchant from spawning before the revenge quest is completed."
                        )
                    );
                EnableFastTravelAnywhere = Config.Bind(
                    SectionMap,
                    nameof(EnableFastTravelAnywhere),
                    false,
                    new Translator(chinese: "启用随时快速传送", english: "Enable Fast Travel Anywhere"),
                    new Translator(
                        chinese: "启用随时快速传送。允许不坐椅子传送，并解除夜间与雷暴对快传的限制。剧情锁定的传送不可用。",
                        english: "Enable fast travel anywhere. Allows travel without sitting on a bench and lifts the night and thunder restriction. Scenario-locked travel stays blocked."
                        )
                    );
                EnableAllowNightTravel = Config.Bind(
                    SectionMap,
                    nameof(EnableAllowNightTravel),
                    false,
                    new Translator(chinese: "启用夜间椅子传送", english: "Enable Night Bench Travel"),
                    new Translator(
                        chinese: "允许在夜间或雷暴时从椅子快速传送。",
                        english: "Allow bench fast travel at night or during thunder."
                        )
                    );
                EnableWormTrap = Config.Bind(
                    SectionMap,
                    nameof(EnableWormTrap),
                    true,
                    new Translator(chinese: "启用虫墙", english: "Enable Worm Trap"),
                    new Translator(
                        chinese: "启用虫墙。",
                        english: "Enable worm trap."
                        )
                    );
                EnableMapDamage = Config.Bind(
                    SectionMap,
                    nameof(EnableMapDamage),
                    true,
                    new Translator(chinese: "启用地图伤害", english: "Enable Map Damage"),
                    new Translator(
                        chinese: "启用地图伤害，包括地刺、荆棘、电击、酸液。禁用后将不再受到以上伤害。",
                        english: "Enable map damage, including spikes, thorns, electric shock, and acid. Disabling will prevent taking the above damage."
                        )
                    );
                EnableDrowning = Config.Bind(
                    SectionMap,
                    nameof(EnableDrowning),
                    true,
                    new Translator(chinese: "启用溺水", english: "Enable Drowning"),
                    new Translator(
                        chinese: "启用溺水。禁用后将不再受到溺水伤害。",
                        english: "Enable drowning. Disabling will prevent drowning damage."
                        )
                    );
                EnableDarkArea = Config.Bind(
                    SectionMap,
                    nameof(EnableDarkArea),
                    true,
                    new Translator(chinese: "启用黑暗区域", english: "Enable Dark Area"),
                    new Translator(
                        chinese: "启用黑暗区域。禁用后特定区域将不再需要魔荧虫提灯照亮。",
                        english: "Enable dark area. After disabling, specific areas will no longer require the Magic Bug Lantern to illuminate."
                        )
                    );
                EnableLockDangerLevel = Config.Bind(
                    SectionMap,
                    nameof(EnableLockDangerLevel),
                    false,
                    new Translator(chinese: "启用危险度锁定", english: "Enable Lock Danger Level"),
                    new Translator(
                        chinese: "锁定当前危险度。战斗结算和事件不再改变危险度。旅馆休息等玩家主动操作仍可能重置。",
                        english: "Lock the current danger level. Battle results and events will not change it. Player actions such as hotel rest may still reset it."
                        )
                    );
                EnableAlwaysShowWanderingNpcOnMap = Config.Bind(
                    SectionMap,
                    nameof(EnableAlwaysShowWanderingNpcOnMap),
                    false,
                    new Translator(chinese: "启用商人地图常显", english: "Enable Always Show Merchants On Map"),
                    new Translator(
                        chinese: "世界地图上始终显示流浪商人位置：南丁格尔不再要求携带铃铛，咖啡师、提尔德、木偶商人以各自颜色的范围圈标出。剧情未解锁的商人不显示。",
                        english: "Always show wandering merchants on the world map: Nightingale no longer requires her bell, and the Coffee Maker, Tilde and Puppet are marked with colored range circles. Story-locked merchants stay hidden."
                        )
                    );
                EnableWanderingNpcAlwaysAppear = Config.Bind(
                    SectionMap,
                    nameof(EnableWanderingNpcAlwaysAppear),
                    false,
                    new Translator(chinese: "启用商人必定出现", english: "Enable Merchants Always Appear"),
                    new Translator(
                        chinese: "流浪商人在其游走范围内必定出现，不再受原版概率影响；单地图单商人与剧情地图限制保持不变。",
                        english: "Wandering merchants always appear within their walking range instead of the vanilla chance. One merchant per map and story-map restrictions are unchanged."
                        )
                    );
                EnableBattleEnemyPreview = Config.Bind(
                    SectionMap,
                    nameof(EnableBattleEnemyPreview),
                    false,
                    new Translator(chinese: "启用战斗点详细魔物预览", english: "Enable Detailed Battle Enemy Preview"),
                    new Translator(
                        chinese: "在战斗点现场显示魔物种类、污染体、强化属性及数量范围；动态增援的数量可能无法确定。",
                        english: "Show enemy kinds, contamination, attributes and count ranges at battle points. Dynamic reinforcements may have unknown counts."
                        )
                    );
                EnableNightSummonerAlwaysOpen = Config.Bind(
                    SectionMap,
                    nameof(EnableNightSummonerAlwaysOpen),
                    false,
                    new Translator(chinese: "启用夜间战斗点常开", english: "Enable Night Summoners Always Open"),
                    new Translator(
                        chinese: "夜间限定的战斗点白天也显示为开启并可进入。",
                        english: "Night-only summoner circles appear open and enterable during the day."
                        )
                    );
                SetDangerLevel = BindPreloadValue(
                    SectionMap,
                    nameof(SetDangerLevel),
                    -1,
                    new Translator(chinese: "设置危险度", english: "Set Danger Level"),
                    new Translator(
                        chinese: "设置危险度。将覆盖原始的危险度。",
                        english: "Set the danger level."
                        )
                    );
            }
            catch (Exception ex)
            {
                BLog.Error("Failed to initialize config manager for map.", ex);
            }
        }
    }
}
