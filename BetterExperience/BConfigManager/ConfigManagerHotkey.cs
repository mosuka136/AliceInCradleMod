using BetterExperience.BLogSpace;
using System;
using UnityModBase.HConfigSpace;
using UnityModBase.HotkeyManager;
using UnityModBase.HProvider;
using UnityModBase.HTranslatorSpace;

namespace BetterExperience.BConfigManager
{
    public static partial class ConfigManager
    {
        // 热键配置使用 Hotkey 自定义适配器序列化，默认 UnityProvider 实例在初始化时统一创建。
        public static ConfigEntry<Hotkey> FlushAllStoreHotkey { get; private set; }
        public static ConfigEntry<Hotkey> FlushTextureHotkey { get; private set; }
        public static ConfigEntry<Hotkey> MouseTeleportHotkey { get; private set; }
        public static ConfigEntry<Hotkey> ToggleNoclipHotkey { get; private set; }

        private const string SectionHotkey = "Hotkey";

        /// <summary>
        /// 初始化全局热键配置。
        /// </summary>
        public static void InitializeHotkey()
        {
            try
            {
                Config.CreateTable(SectionHotkey, new Translator(chinese: "热键", english: "Hotkey"));

                var unityService = UnityProvider.Instance;

                MouseTeleportHotkey = Config.Bind(
                    SectionHotkey,
                    nameof(MouseTeleportHotkey),
                    new Hotkey("Ctrl+G", unityService),
                    new Translator(chinese: "鼠标传送热键", english: "Mouse Teleport Hotkey"),
                    new Translator(
                        chinese: "传送到鼠标指向的位置，默认 Ctrl+G。",
                        english: "Teleport to the mouse position. Default: Ctrl+G."
                        )
                    );
                ToggleNoclipHotkey = Config.Bind(
                    SectionHotkey,
                    nameof(ToggleNoclipHotkey),
                    new Hotkey("Ctrl+N", unityService),
                    new Translator(chinese: "切换穿墙热键", english: "Toggle Noclip Hotkey"),
                    new Translator(
                        chinese: "切换穿墙飞行，默认 Ctrl+N。",
                        english: "Toggle noclip flight. Default: Ctrl+N."
                        )
                    );
                FlushAllStoreHotkey = Config.Bind(
                    SectionHotkey,
                    nameof(FlushAllStoreHotkey),
                    new Hotkey("F", unityService),
                    new Translator(chinese: "刷新商店热键", english: "Flush All Store Hotkey"),
                    new Translator(
                        chinese: "一键刷新商店的热键。默认值为 F。",
                        english: "The hotkey to flush all store. Default is F."
                        )
                    );
                FlushTextureHotkey = Config.Bind(
                    SectionHotkey,
                    nameof(FlushTextureHotkey),
                    new Hotkey("Ctrl+T", unityService),
                    new Translator(chinese: "刷新贴图热键", english: "Flush Texture Hotkey"),
                    new Translator(
                        chinese: "一键刷新贴图的热键。默认值为 Ctrl+T。",
                        english: "The hotkey to flush texture. Default is Ctrl+T."
                        )
                    );
            }
            catch (Exception ex)
            {
                BLog.Error("Failed to initialize config manager for hotkey.", ex);
            }
        }
    }
}
