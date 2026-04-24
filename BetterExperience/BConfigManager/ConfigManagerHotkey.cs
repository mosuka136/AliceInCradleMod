using BetterExperience.HProvider;
using BetterExperience.HConfigSpace;
using BetterExperience.HotkeyManager;
using BetterExperience.HTranslatorSpace;

namespace BetterExperience.BConfigManager
{
    public partial class ConfigManager
    {
        public static ConfigEntry<Hotkey> ConfigUIHotkey { get; private set; }
        public static ConfigEntry<Hotkey> ReloadConfigHotkey { get; private set; }
        public static ConfigEntry<Hotkey> FlushAllStoreHotkey { get; private set; }
        public static ConfigEntry<Hotkey> FlushTextureHotkey { get; private set; }

        private const string SectionHotkey = "Hotkey";

        public static void InitializeHotkey()
        {
            Config.CreateTable(
                SectionHotkey,
                new Translator(chinese: "热键", english: "Hotkey"),
                new Translator(
                    chinese: "热键写法说明：\n" +
                             "1) 单键：F / F1 / Space / Tab\n" +
                             "2) 键盘组合键：修饰键在前，主键在最后，例如 Ctrl+Shift+F\n" +
                             "3) 手柄组合键：手柄按键用 + 连接，例如 GamepadStart+GamepadA\n" +
                             "4) 备选热键：Ctrl+F, GamepadStart+GamepadA（用逗号分隔）\n" +
                             "5) 同一组组合键不能混用键盘和手柄，例如 Ctrl+GamepadA 不支持\n" +
                             "\n" +
                             "键盘修饰键：\n" +
                             "- Ctrl（或 Control）/ Shift / Alt\n" +
                             "- LeftCtrl（或 LCtrl）/ RightCtrl（或 RCtrl）\n" +
                             "- LeftShift（或 LShift）/ RightShift（或 RShift）\n" +
                             "- LeftAlt（或 LAlt）/ RightAlt（或 RAlt）\n" +
                             "\n" +
                             "手柄按键名称（配置写回时会规范化为以下名称）：\n" +
                             "- GamepadA / GamepadB / GamepadX / GamepadY\n" +
                             "- GamepadStart / GamepadBack / GamepadLB / GamepadRB\n" +
                             "- GamepadDpadUp / GamepadDpadDown / GamepadDpadLeft / GamepadDpadRight\n" +
                             "- GamepadLS / GamepadRS\n" +
                             "- 也接受别名：South / East / West / North、Cross / Circle / Square / Triangle、Select / LeftShoulder / RightShoulder / LeftStick / RightStick\n" +
                             "- Gamepad 前缀可省略，例如 A 与 GamepadA 等价\n" +
                             "\n" +
                             "示例：\n" +
                             "- Ctrl+F\n" +
                             "- LeftCtrl+RightShift+F\n" +
                             "- GamepadStart+GamepadA\n" +
                             "- Ctrl+Shift+F, GamepadStart+GamepadA",
                    english: "Hotkey notation guide:\n" +
                             "1) Single key: F / F1 / Space / Tab\n" +
                             "2) Keyboard chord: modifiers first, main key last, e.g. Ctrl+Shift+F\n" +
                             "3) Gamepad chord: join gamepad buttons with +, e.g. GamepadStart+GamepadA\n" +
                             "4) Alternatives: Ctrl+F, GamepadStart+GamepadA (comma-separated)\n" +
                             "5) Keyboard and gamepad cannot be mixed in the same chord, e.g. Ctrl+GamepadA is not supported\n" +
                             "\n" +
                             "Keyboard modifiers:\n" +
                             "- Ctrl (or Control) / Shift / Alt\n" +
                             "- LeftCtrl (or LCtrl) / RightCtrl (or RCtrl)\n" +
                             "- LeftShift (or LShift) / RightShift (or RShift)\n" +
                             "- LeftAlt (or LAlt) / RightAlt (or RAlt)\n" +
                             "\n" +
                             "Gamepad button names (config values are normalized to these names when written back):\n" +
                             "- GamepadA / GamepadB / GamepadX / GamepadY\n" +
                             "- GamepadStart / GamepadBack / GamepadLB / GamepadRB\n" +
                             "- GamepadDpadUp / GamepadDpadDown / GamepadDpadLeft / GamepadDpadRight\n" +
                             "- GamepadLS / GamepadRS\n" +
                             "- Also accepts aliases: South / East / West / North, Cross / Circle / Square / Triangle, Select / LeftShoulder / RightShoulder / LeftStick / RightStick\n" +
                             "- The Gamepad prefix is optional, e.g. A and GamepadA are equivalent\n" +
                             "\n" +
                             "Examples:\n" +
                             "- Ctrl+F\n" +
                             "- LeftCtrl+RightShift+F\n" +
                             "- GamepadStart+GamepadA\n" +
                             "- Ctrl+Shift+F, GamepadStart+GamepadA"
                )
                );

            var unityService = new UnityProvider();

            ConfigUIHotkey = Config.Bind(
                SectionHotkey,
                nameof(ConfigUIHotkey),
                new Hotkey("F1", unityService),
                new Translator(chinese: "配置界面热键", english: "Config UI Hotkey"),
                new Translator(
                    chinese: "打开配置界面的热键。默认是 F1。",
                    english: "The hotkey to open config UI. Default is F1."
                )
                );
            ReloadConfigHotkey = Config.Bind(
                SectionHotkey,
                nameof(ReloadConfigHotkey),
                new Hotkey("Ctrl+R", unityService),
                new Translator(chinese: "重新加载配置热键", english: "Reload Config Hotkey"),
                new Translator(
                    chinese: "重新加载配置的热键。默认值为 Ctrl+R。",
                    english: "The hotkey to reload config. Default is Ctrl+R."
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
    }
}
