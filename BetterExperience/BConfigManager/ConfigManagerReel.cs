using BetterExperience.HConfigFileSpace;
using BetterExperience.HTranslatorSpace;

namespace BetterExperience.BConfigManager
{
    public partial class ConfigManager
    {
        public static ConfigEntry<bool> EnableBetterReelEffect { get; private set; }
        public static ConfigEntry<bool> EnableRemoveLimitInTreasureChests { get; private set; }
        public static ConfigEntry<float> SetReelSpeed { get; private set; }

        private const string SectionReel = "Reel";

        public static void InitializeReel()
        {
            Config.CreateTable(SectionReel, new Translator(chinese: "转轮", english: "Reel"));

            EnableBetterReelEffect = Config.Bind(
                SectionReel,
                nameof(EnableBetterReelEffect),
                false,
                new Translator(chinese: "启用更好的转轮效果", english: "Enable Better Reel Effect"),
                new Translator(
                    chinese: "启用更好的转轮效果。",
                    english: "Enable better reel effect."
                )
                );
            EnableRemoveLimitInTreasureChests = Config.Bind(
                SectionReel,
                nameof(EnableRemoveLimitInTreasureChests),
                false,
                new Translator(chinese: "启用移除宝箱数量上限", english: "Enable Remove Limit In Treasure Chests"),
                new Translator(
                    chinese: "启用移除宝箱99物品数量上限。",
                    english: "Enable removal of the 99-item limit in treasure chests."
                )
                );
            SetReelSpeed = Config.Bind(
                SectionReel,
                nameof(SetReelSpeed),
                -1f,
                new Translator(chinese: "设置转轮速度", english: "Set Reel Speed"),
                new Translator(
                    chinese: "设置转轮速度。设为 0 和 1 之间的值可调节转轮速度。数值越大速度越慢。",
                    english: "Set reel speed. Set a value between 0 and 1 to adjust the wheel speed. The larger the value, the slower the speed."
                )
                );
        }
    }
}
