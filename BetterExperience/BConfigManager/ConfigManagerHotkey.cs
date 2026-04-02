using BetterExperience.HConfigFileSpace;
using BetterExperience.HTranslatorSpace;

namespace BetterExperience.BConfigManager
{
    public partial class ConfigManager
    {
        public static ConfigEntry<string> ConfigUIHotkey { get; private set; }
        public static ConfigEntry<string> ReloadConfigHotkey { get; private set; }
        public static ConfigEntry<string> FlushAllStoreHotkey { get; private set; }
        public static ConfigEntry<string> FlushTextureHotkey { get; private set; }

        private const string SectionHotkey = "Hotkey";

        public static void InitializeHotkey()
        {
            Config.CreateTable(
                SectionHotkey,
                new Translator(chinese: "热键", english: "Hotkey"),
                new Translator(
                    chinese: "热键写法说明：\n" +
                             "1) 单键：F / F1 / Space / Tab\n" +
                             "2) 组合键：Ctrl+Shift+F\n" +
                             "3) 手柄：GamepadStart+GamepadSouth\n" +
                             "4) 备选热键：Ctrl+F, GamepadStart+GamepadSouth（用逗号分隔）\n" +
                             "\n" +
                             "修饰键：\n" +
                             "- Ctrl / Shift / Alt\n" +
                             "- 或：LeftCtrl / RightCtrl / LeftShift / RightShift / LeftAlt / RightAlt\n" +
                             "\n" +
                             "手柄按键名称：\n" +
                             "- GamepadSouth(A/×), GamepadEast(B/○), GamepadWest(X/□), GamepadNorth(Y/△)\n" +
                             "- GamepadStart, GamepadSelect, GamepadLeftShoulder(LB/L1), GamepadRightShoulder(RB/R1)\n" +
                             "- GamepadDpadUp/Down/Left/Right, GamepadLeftStick, GamepadRightStick\n" +
                             "\n" +
                             "示例：\n" +
                             "- Ctrl+F\n" +
                             "- LeftCtrl+RightShift+F\n" +
                             "- GamepadStart+GamepadSouth\n" +
                             "- Ctrl+Shift+F, GamepadStart+GamepadSouth",
                    english: "Hotkey notation guide:\n" +
                             "1) Single key: F / F1 / Space / Tab\n" +
                             "2) Combination: Ctrl+Shift+F\n" +
                             "3) Gamepad: GamepadStart+GamepadSouth\n" +
                             "4) Alternatives: Ctrl+F, GamepadStart+GamepadSouth (comma-separated)\n" +
                             "\n" +
                             "Modifier keys:\n" +
                             "- Ctrl / Shift / Alt\n" +
                             "- Or: LeftCtrl / RightCtrl / LeftShift / RightShift / LeftAlt / RightAlt\n" +
                             "\n" +
                             "Gamepad button names:\n" +
                             "- GamepadSouth(A/×), GamepadEast(B/○), GamepadWest(X/□), GamepadNorth(Y/△)\n" +
                             "- GamepadStart, GamepadSelect, GamepadLeftShoulder(LB/L1), GamepadRightShoulder(RB/R1)\n" +
                             "- GamepadDpadUp/Down/Left/Right, GamepadLeftStick, GamepadRightStick\n" +
                             "\n" +
                             "Examples:\n" +
                             "- Ctrl+F\n" +
                             "- LeftCtrl+RightShift+F\n" +
                             "- GamepadStart+GamepadSouth\n" +
                             "- Ctrl+Shift+F, GamepadStart+GamepadSouth"
                )
                );

            ConfigUIHotkey = Config.Bind(
                SectionHotkey,
                nameof(ConfigUIHotkey),
                "F1",
                new Translator(chinese: "配置界面热键", english: "Config UI Hotkey"),
                new Translator(
                    chinese: "打开配置界面的热键。默认是 F1。",
                    english: "The hotkey to open config UI. Default is F1."
                )
                );
            ReloadConfigHotkey = Config.Bind(
                SectionHotkey,
                nameof(ReloadConfigHotkey),
                "Ctrl+R",
                new Translator(chinese: "重新加载配置热键", english: "Reload Config Hotkey"),
                new Translator(
                    chinese: "重新加载配置的热键。默认值为 Ctrl+R。",
                    english: "The hotkey to reload config. Default is Ctrl+R."
                )
                );
            FlushAllStoreHotkey = Config.Bind(
                SectionHotkey,
                nameof(FlushAllStoreHotkey),
                "F",
                new Translator(chinese: "刷新商店热键", english: "Flush All Store Hotkey"),
                new Translator(
                    chinese: "一键刷新商店的热键。默认值为 F。",
                    english: "The hotkey to flush all store. Default is F."
                )
                );
            FlushTextureHotkey = Config.Bind(
                SectionHotkey,
                nameof(FlushTextureHotkey),
                "Ctrl+T",
                new Translator(chinese: "刷新贴图热键", english: "Flush Texture Hotkey"),
                new Translator(
                    chinese: "一键刷新贴图的热键。默认值为 Ctrl+T。",
                    english: "The hotkey to flush texture. Default is Ctrl+T."
                )
                );
        }
    }
}
