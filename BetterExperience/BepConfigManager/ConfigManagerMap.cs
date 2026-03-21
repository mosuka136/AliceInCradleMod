using BetterExperience.ConfigFileSpace;

namespace BetterExperience.BepConfigManager
{
    internal sealed partial class ConfigManager
    {
        public static ConfigEntry<bool> EnableBetterSaveSite { get; private set; }
        public static ConfigEntry<bool> EnableRemoveLimitInPuppetNpcDefeated { get; private set; }
        public static ConfigEntry<bool> EnableFastTravelAnywhere { get; private set; }
        public static ConfigEntry<bool> EnableWormTrap { get; private set; }
        public static ConfigEntry<bool> EnableMapDamage { get; private set; }
        public static ConfigEntry<bool> EnableDrowning { get; private set; }
        public static ConfigEntry<bool> EnableDarkArea { get; private set; }
        public static ConfigEntry<bool> EnablePreloadDangerLevel { get; private set; }
        public static ConfigEntry<int> SetDangerLevel { get; private set; }

        private const string SectionMap = "Map";

        public static void InitializeMapTrap()
        {
            Config.CreateTable(SectionMap);

            EnableBetterSaveSite = Config.Bind(
                SectionMap,
                nameof(EnableBetterSaveSite),
                false,
                "Enable better save site. It will allow saving anywhere.\n启用更好的存档点功能。允许在任意位置保存。"
                );
            EnableRemoveLimitInPuppetNpcDefeated = Config.Bind(
                SectionMap,
                nameof(EnableRemoveLimitInPuppetNpcDefeated),
                false,
                "Enable the restriction that prevents the Puppet Merchant from spawning before the revenge quest is completed.\n" +
                "启用移除木偶商人在复仇战未完成前无法生成的限制。"
                );
            EnableFastTravelAnywhere = Config.Bind(
                SectionMap,
                nameof(EnableFastTravelAnywhere),
                false,
                "Enable fast travel anywhere. It will allow players to fast travel anywhere on the map. At least one normal transmission must be performed before use.\n" +
                "启用随时快速传送。它将允许玩家在地图上的任何地方快速传送。使用前至少要进行一次正常传送。"
                );
            EnableWormTrap = Config.Bind(
                SectionMap,
                nameof(EnableWormTrap),
                true,
                "Enable worm trap.\n启用虫墙。"
                );
            EnableMapDamage = Config.Bind(
                SectionMap,
                nameof(EnableMapDamage),
                true,
                "Enable map damage, including spikes, thorns, electric shock, and acid. Disabling will prevent taking the above damage.\n" +
                "启用地图伤害，包括地刺，荆棘，电击，酸液。禁用后将不再受到以上伤害"
                );
            EnableDrowning = Config.Bind(
                SectionMap,
                nameof(EnableDrowning),
                true,
                "Enable drowning. Disabling will prevent drowning damage.\n启用溺水。禁用后将不再受到溺水伤害。"
                );
            EnableDarkArea = Config.Bind(
                SectionMap,
                nameof(EnableDarkArea),
                true,
                "Enable dark area. After disabling, specific areas will no longer require the Magic Bug Lantern to illuminate.\n" +
                "启用黑暗区域。禁用后特定区域将不再需要魔荧虫提灯照亮。"
                );
            EnablePreloadDangerLevel = Config.Bind(
                SectionMap,
                nameof(EnablePreloadDangerLevel),
                false,
                "Enable preload danger level. When enabled, the danger level will be automatically set once after loading a save, " +
                "using the configured value to override the original danger level.\n" +
                "启用预加载危险度。开启后，危险度将在存档读取后自动设置一次，使用设置值覆盖原始危险度。"
                );
            SetDangerLevel = Config.Bind(
                SectionMap,
                nameof(SetDangerLevel),
                -1,
                "Set the danger level. It will override the original danger level. Set to -1 to keep the danger level at its current value.\n" +
                "设置危险度。将覆盖原始的危险度。设为 -1 可保持为当前值。"
                );
        }
    }
}
