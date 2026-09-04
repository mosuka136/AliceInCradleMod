using BetterExperience.BLogSpace;
using System;
using UnityModBase.HClassAttribute;
using UnityModBase.HConfigSpace;
using UnityModBase.HTranslatorSpace;
using static BetterExperience.Patches.HPatches.BetterReelEffectPatch;

namespace BetterExperience.BConfigManager
{
    public static partial class ConfigManager
    {
        // 转轮速度配置中 -1 表示保留游戏默认速度。
        public static ConfigEntry<bool> EnableBetterReelEffect { get; private set; }
        public static ConfigEntry<LuckyBagEffect> SpecifiedLuckyBagEffect { get; private set; }
        public static ConfigEntry<bool> EnableRemoveLimitInTreasureChests { get; private set; }
        [EntrySlider(-0.2f, 1f, 0.01f)]
        public static ConfigEntry<float> SetReelSpeed { get; private set; }

        private const string SectionReel = "Reel";

        /// <summary>
        /// 初始化转轮效果、宝箱数量上限和转轮速度配置。
        /// </summary>
        public static void InitializeReel()
        {
            try
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
                SpecifiedLuckyBagEffect = Config.Bind(
                    SectionReel,
                    nameof(SpecifiedLuckyBagEffect),
                    LuckyBagEffect.Default,
                    new Translator(chinese: "指定福袋效果", english: "Specified Lucky Bag Effect"),
                    new Translator(
                        chinese: "仅影响福袋转轮。指定效果优先于“更好的转轮效果”，无需开启自动择优。品质与数量仍受原有上限控制。",
                        english: "Only affects Lucky Bag reels. Takes priority over Better Reel Effect and works independently of it. Existing grade and count limits still apply."
                        )
                    );
                EnableRemoveLimitInTreasureChests = Config.Bind(
                    SectionReel,
                    nameof(EnableRemoveLimitInTreasureChests),
                    false,
                    new Translator(chinese: "启用移除宝箱数量上限", english: "Enable Remove Limit In Treasure Chests"),
                    new Translator(
                        chinese: "启用移除宝箱99物品数量上限，必须在游戏启动前设置。",
                        english: "Enable removal of the 99-item limit in treasure chests, must be set before launching the game."
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
            catch (Exception ex)
            {
                BLog.Error("Failed to initialize config manager for reel.", ex);
            }
        }
    }
}
