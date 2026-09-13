using BetterExperience.BLogSpace;
using System;
using UnityModBase.HConfigSpace;
using UnityModBase.HTranslatorSpace;

namespace BetterExperience.BConfigManager
{
    public static partial class ConfigManager
    {
        public static ConfigEntry<bool> EnableBetterFishing { get; private set; }

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
            }
            catch (Exception ex)
            {
                BLog.Error("Failed to initialize config manager for mini-game.", ex);
            }
        }
    }
}
