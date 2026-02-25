using BepInEx.Configuration;

namespace BetterExperience.BepConfigManager
{
    internal sealed partial class ConfigManager
    {
        private static ConfigEntry<string> HotkeyReadme { get; set; }
        public static ConfigEntry<string> ReloadConfigHotkey { get; private set; }
        public static ConfigEntry<string> FlushAllStoreHotkey { get; private set; }

        private const string SectionHotkey = "Hotkey";

        public static void InitializeHotkey()
        {
            var Config = BetterExperience.Instance.Config;

            HotkeyReadme = Config.Bind(
                SectionHotkey,
                "_README",
                "",
                new ConfigDescription(
                    "Hotkey notation guide:\n" +
                    "热键写法说明：\n" +
                    "1) Single key:F / F1 / Space / Tab\n" +
                    "1) 单键：F / F1 / Space / Tab\n" +
                    "2) Combination:Ctrl+Shift+F\n" +
                    "2) 组合键：Ctrl+Shift+F\n" +
                    "3) Gamepad:GamepadStart+GamepadSouth\n" +
                    "3) 手柄：GamepadStart+GamepadSouth\n" +
                    "4) Alternatives:Ctrl+F, GamepadStart+GamepadSouth(comma-separated)\n" +
                    "4) 备选热键：Ctrl+F, GamepadStart+GamepadSouth（用逗号分隔）\n" +
                    "\n" +
                    "Modifier keys:\n" +
                    "修饰键：\n" +
                    "- Ctrl / Shift / Alt\n" +
                    "- Ctrl / Shift / Alt\n" +
                    "- Or:LeftCtrl / RightCtrl / LeftShift / RightShift / LeftAlt / RightAlt\n" +
                    "- 或：LeftCtrl / RightCtrl / LeftShift / RightShift / LeftAlt / RightAlt\n" +
                    "\n" +
                    "Gamepad button names:\n" +
                    "手柄按键名称：\n" +
                    "- GamepadSouth(A/×), GamepadEast(B/○), GamepadWest(X/□), GamepadNorth(Y/△)\n" +
                    "- GamepadSouth(A/×), GamepadEast(B/○), GamepadWest(X/□), GamepadNorth(Y/△)\n" +
                    "- GamepadStart, GamepadSelect, GamepadLeftShoulder(LB/L1), GamepadRightShoulder(RB/R1)\n" +
                    "- GamepadStart, GamepadSelect, GamepadLeftShoulder(LB/L1), GamepadRightShoulder(RB/R1)\n" +
                    "- GamepadDpadUp/Down/Left/Right, GamepadLeftStick, GamepadRightStick\n" +
                    "- GamepadDpadUp/Down/Left/Right, GamepadLeftStick, GamepadRightStick\n" +
                    "\n" +
                    "Examples:\n" +
                    "示例：\n" +
                    "- Ctrl+F\n" +
                    "- LeftCtrl+RightShift+F\n" +
                    "- GamepadStart+GamepadSouth\n" +
                    "- Ctrl+Shift+F, GamepadStart+GamepadSouth\n" +
                    "\n"
                ));
            ReloadConfigHotkey = Config.Bind(
                SectionHotkey,
                nameof(ReloadConfigHotkey),
                "Ctrl+R",
                "The hotkey to reload config. Default is Ctrl+R.\n重新加载配置的热键。默认值为 Ctrl+R。"
                );
            FlushAllStoreHotkey = Config.Bind(
                SectionHotkey,
                nameof(FlushAllStoreHotkey),
                "F",
                "The hotkey to flush all store. Default is F.\n一键刷新商店的热键。默认值为 F。"
                );
        }
    }
}
