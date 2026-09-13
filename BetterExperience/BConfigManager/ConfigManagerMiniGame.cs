using BetterExperience.BLogSpace;
using System;
using UnityModBase.HClassAttribute;
using UnityModBase.HConfigSpace;
using UnityModBase.HTranslatorSpace;

namespace BetterExperience.BConfigManager
{
    public static partial class ConfigManager
    {
        public static ConfigEntry<bool> EnableEasierFishMarker { get; private set; }
        [EntrySlider(-1f, 3f, 0.05f)]
        public static ConfigEntry<float> SetFishingCatcherRangeMul { get; private set; }
        [EntrySlider(-1f, 0.2f, 0.01f)]
        public static ConfigEntry<float> SetFishingCatcherMargin { get; private set; }
        [EntrySlider(-1f, 2f, 0.01f)]
        public static ConfigEntry<float> SetFishingMissDamageMul { get; private set; }
        [EntrySlider(-1f, 6f, 1f)]
        public static ConfigEntry<float> SetFishingPunchNeedCount { get; private set; }
        public static ConfigEntry<bool> EnableBunNoBodyTouch { get; private set; }
        public static ConfigEntry<bool> EnableBunClearVision { get; private set; }
        public static ConfigEntry<bool> EnableBunEatWhileCarrying { get; private set; }

        private const string SectionMiniGame = "MiniGame";

        public static void InitializeMiniGame()
        {
            try
            {
                Config.CreateTable(SectionMiniGame, new Translator(chinese: "小游戏", english: "Mini Game"));

                EnableEasierFishMarker = Config.Bind(
                    SectionMiniGame,
                    nameof(EnableEasierFishMarker),
                    false,
                    new Translator(chinese: "钓鱼：鱼标更慢更近", english: "Fishing: slower, closer marker"),
                    new Translator(
                        chinese: "收杆时鱼标跳得更慢、更近、更少反向。",
                        english: "During reeling the fish marker jumps slower, shorter and reverses less."
                        )
                    );
                SetFishingCatcherRangeMul = Config.Bind(
                    SectionMiniGame,
                    nameof(SetFishingCatcherRangeMul),
                    -1f,
                    new Translator(chinese: "钓鱼：套圈大小倍率", english: "Fishing: catcher size multiplier"),
                    new Translator(
                        chinese: "乘在竿等级算出的套圈宽度上。-1 不改，1 为原大小，大于 1 更大。",
                        english: "Multiplies the catcher width after rod/rank. -1 leaves it unchanged; 1 is vanilla; above 1 is larger."
                        )
                    );
                SetFishingCatcherMargin = Config.Bind(
                    SectionMiniGame,
                    nameof(SetFishingCatcherMargin),
                    -1f,
                    new Translator(chinese: "钓鱼：套圈判定加宽", english: "Fishing: catcher hit margin"),
                    new Translator(
                        chinese: "额外套圈容差。-1 不改。",
                        english: "Extra catcher forgiveness. -1 leaves it unchanged."
                        )
                    );
                SetFishingMissDamageMul = Config.Bind(
                    SectionMiniGame,
                    nameof(SetFishingMissDamageMul),
                    -1f,
                    new Translator(chinese: "钓鱼：失误掉条倍率", english: "Fishing: miss drain multiplier"),
                    new Translator(
                        chinese: "鱼标离开套圈时掉条速度。-1 不改，1 为原速度，0.33 接近教程。",
                        english: "How fast the gauge drains off-marker. -1 leaves it unchanged; 1 is vanilla; 0.33 is near the tutorial."
                        )
                    );
                SetFishingPunchNeedCount = Config.Bind(
                    SectionMiniGame,
                    nameof(SetFishingPunchNeedCount),
                    -1f,
                    new Translator(chinese: "钓鱼：收杆次数", english: "Fishing: reel-in count"),
                    new Translator(
                        chinese: "满条自动收杆需要的次数，1–6。-1 不改。",
                        english: "Successful auto-reels needed (1–6). -1 leaves the fish's default."
                        )
                    );
                EnableBunNoBodyTouch = Config.Bind(
                    SectionMiniGame,
                    nameof(EnableBunNoBodyTouch),
                    false,
                    new Translator(chinese: "酒吧禁止客人扑人与QTE", english: "Bar: no body grab or QTE"),
                    new Translator(
                        chinese: "兔女郎酒吧中客人不再扑过来，也不会因兴奋度触发钟摆QTE。",
                        english: "In the bunny bar, customers will not grab you and the excitement pendulum QTE will not start."
                        )
                    );
                EnableBunClearVision = Config.Bind(
                    SectionMiniGame,
                    nameof(EnableBunClearVision),
                    false,
                    new Translator(chinese: "酒吧酒款不模糊且不减速", english: "Bar: no blur or slow walk"),
                    new Translator(
                        chinese: "兔女郎酒吧中体力过低时酒款图标不再模糊，走路也不再变慢。",
                        english: "In the bunny bar, low stamina will not blur drink icons or slow your walk."
                        )
                    );
                EnableBunEatWhileCarrying = Config.Bind(
                    SectionMiniGame,
                    nameof(EnableBunEatWhileCarrying),
                    false,
                    new Translator(chinese: "酒吧端酒时也能吃零食", english: "Bar: eat while carrying"),
                    new Translator(
                        chinese: "兔女郎酒吧中端着酒也能按道具键吃零食回体力。",
                        english: "In the bunny bar you can eat snacks to restore stamina while carrying drinks."
                        )
                    );
            }
            catch (Exception ex)
            {
                BLog.Error("Failed to initialize config manager for mini-game.", ex);
            }
        }
    }
}
