using BetterExperience.BLogSpace;
using System;
using UnityModBase.HConfigSpace;
using UnityModBase.HTranslatorSpace;

namespace BetterExperience.BConfigManager
{
    public static partial class ConfigManager
    {
        public static ConfigEntry<bool> EnableBetterFishing { get; private set; }
        public static ConfigEntry<bool> EnableBunNoBodyTouch { get; private set; }
        public static ConfigEntry<bool> EnableBunClearVision { get; private set; }
        public static ConfigEntry<bool> EnableBunEatWhileCarrying { get; private set; }

        private const string SectionMiniGame = "MiniGame";

        public static void InitializeMiniGame()
        {
            try
            {
                Config.CreateTable(SectionMiniGame, new Translator(chinese: "小游戏", english: "Mini Game"));

                EnableBetterFishing = Config.Bind(
                    SectionMiniGame,
                    nameof(EnableBetterFishing),
                    false,
                    new Translator(chinese: "启用更好的钓鱼", english: "Enable Better Fishing"),
                    new Translator(
                        chinese: "启用更好的钓鱼。它将允许玩家更容易地钓到鱼。",
                        english: "Enable better fishing. It will allow players to catch fish more easily."
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
