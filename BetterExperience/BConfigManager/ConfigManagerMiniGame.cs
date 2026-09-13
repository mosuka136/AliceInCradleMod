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
        public static ConfigEntry<bool> EnableMilkReleaseAssist { get; private set; }
        public static ConfigEntry<bool> EnableMilkTolerance { get; private set; }
        public static ConfigEntry<bool> EnableMilkNoDrain { get; private set; }
        public static ConfigEntry<bool> EnableMilkNoRunAnger { get; private set; }
        public static ConfigEntry<bool> EnableMilkNoTiredLimit { get; private set; }

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
                EnableMilkReleaseAssist = Config.Bind(
                    SectionMiniGame,
                    nameof(EnableMilkReleaseAssist),
                    false,
                    new Translator(chinese: "挤奶：满级自动松手", english: "Milking: auto release at full charge"),
                    new Translator(
                        chinese: "手动按住蓄力时，在进度恰好满级的一帧自动松开，保证满级奶量且不会按过头。",
                        english: "While you hold to charge, it releases the instant the gauge is full for maximum milk and no overhold failure."
                        )
                    );
                EnableMilkTolerance = Config.Bind(
                    SectionMiniGame,
                    nameof(EnableMilkTolerance),
                    false,
                    new Translator(chinese: "挤奶：放宽过压判定", english: "Milking: wider overhold timing"),
                    new Translator(
                        chinese: "大幅延长蓄力满级后允许继续按住的宽限时间，按过头不再轻易失败。",
                        english: "Greatly extends the grace period after the gauge is full, so overholding rarely fails."
                        )
                    );
                EnableMilkNoDrain = Config.Bind(
                    SectionMiniGame,
                    nameof(EnableMilkNoDrain),
                    false,
                    new Translator(chinese: "挤奶：奶量不减", english: "Milking: no milk drain"),
                    new Translator(
                        chinese: "挤奶结束后奶牛不再消耗奶量，同一头牛可反复挤；开场奶牛仍需先吃草积累奶量。",
                        english: "Cows no longer lose milk after being milked, so one cow can be milked repeatedly; they still start empty and must graze first."
                        )
                    );
                EnableMilkNoRunAnger = Config.Bind(
                    SectionMiniGame,
                    nameof(EnableMilkNoRunAnger),
                    false,
                    new Translator(chinese: "挤奶：奔跑不扰牛", english: "Milking: run without disturbing cows"),
                    new Translator(
                        chinese: "玩家奔跑或跳跃经过奶牛身边不再打扰、触怒它们。",
                        english: "Running or jumping past cows no longer disturbs or angers them."
                        )
                    );
                EnableMilkNoTiredLimit = Config.Bind(
                    SectionMiniGame,
                    nameof(EnableMilkNoTiredLimit),
                    false,
                    new Translator(chinese: "挤奶：解除牛疲劳开局限制", english: "Milking: remove cow-tiredness limit"),
                    new Translator(
                        chinese: "原版连续游玩几局后，奶农会以“牛累了”为由不再开启新的一局（疲劳计数累计到 7）。开启后忽略并清零该计数，可无限连续开局。",
                        english: "Vanilla blocks starting a new round after several games because the cows are tired (a fatigue counter reaching 7). This ignores and resets that counter, allowing unlimited consecutive rounds."
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
