using BepInEx.Configuration;

namespace BetterExperience.BepConfigManager
{
    internal sealed partial class ConfigManager
    {
        public static ConfigEntry<bool> EnableWormTrap { get; private set; }
        public static ConfigEntry<bool> EnableMapDamage { get; private set; }
        public static ConfigEntry<bool> EnableDrowning { get; private set; }
        public static ConfigEntry<bool> EnableDarkArea { get; private set; }

        private const string SectionMapTrap = "MapTrap";

        public static void InitializeMapTrap()
        {
            var Config = BetterExperience.Instance.Config;

            EnableWormTrap = Config.Bind(
                SectionMapTrap,
                nameof(EnableWormTrap),
                true,
                "Enable worm trap.\n启用虫墙。"
                );
            EnableMapDamage = Config.Bind(
                SectionMapTrap,
                nameof(EnableMapDamage),
                true,
                "Enable map damage, including spikes, thorns, electric shock, and acid. Disabling will prevent taking the above damage.\n" +
                "启用地图伤害，包括地刺，荆棘，电击，酸液。禁用后将不再受到以上伤害"
                );
            EnableDrowning = Config.Bind(
                SectionMapTrap,
                nameof(EnableDrowning),
                true,
                "Enable drowning. Disabling will prevent drowning damage.\n启用溺水。禁用后将不再受到溺水伤害。"
                );
            EnableDarkArea = Config.Bind(
                SectionMapTrap,
                nameof(EnableDarkArea),
                true,
                "Enable dark area. After disabling, specific areas will no longer require the Magic Bug Lantern to illuminate.\n" +
                "启用黑暗区域。禁用后特定区域将不再需要魔荧虫提灯照亮。"
                );
        }
    }
}
