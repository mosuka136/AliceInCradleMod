using BetterExperience.ConfigFileSpace;

namespace BetterExperience.BepConfigManager
{
    internal sealed partial class ConfigManager
    {
        public static ConfigEntry<bool> EnableBetterReelEffect { get; private set; }
        public static ConfigEntry<bool> EnableRemoveLimitInTreasureChests { get; private set; }
        public static ConfigEntry<float> SetReelSpeed { get; private set; }

        private const string SectionReel = "Reel";

        public static void InitializeReel()
        {
            Config.CreateTable(SectionReel);

            EnableBetterReelEffect = Config.Bind(
                SectionReel,
                nameof(EnableBetterReelEffect),
                false,
                "Enable better reel effect.\n启用更好的转轮效果。"
                );
            EnableRemoveLimitInTreasureChests = Config.Bind(
                SectionReel,
                nameof(EnableRemoveLimitInTreasureChests),
                false,
                "Enable removal of the 99-item limit in treasure chests.\n启用移除宝箱99物品数量上限。"
                );
            SetReelSpeed = Config.Bind(
                SectionReel,
                nameof(SetReelSpeed),
                -1f,
                "Set reel speed. Set a value between 0 and 1 to adjust the wheel speed. The larger the value, the slower the speed.\n" +
                "设置转轮速度。设为 0 和 1 之间的值可调节转轮速度。数值越大速度越慢。"
                );
        }
    }
}
