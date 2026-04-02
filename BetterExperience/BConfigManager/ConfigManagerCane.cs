using BetterExperience.HConfigFileSpace;
using BetterExperience.HTranslatorSpace;

namespace BetterExperience.BConfigManager
{
    public partial class ConfigManager
    {
        public static ConfigEntry<bool> EnablePreloadCaneSwingSpeed { get; private set; }
        public static ConfigEntry<bool> EnablePreloadCaneCastSpeed { get; private set; }
        public static ConfigEntry<bool> EnablePreloadCaneBalance { get; private set; }
        public static ConfigEntry<bool> EnablePreloadCaneEfficiency { get; private set; }
        public static ConfigEntry<bool> EnablePreloadCaneRetention { get; private set; }
        public static ConfigEntry<bool> EnablePreloadCaneLockOn { get; private set; }
        public static ConfigEntry<bool> EnablePreloadCaneLongRange { get; private set; }
        public static ConfigEntry<bool> EnablePreloadCaneShortRange { get; private set; }
        public static ConfigEntry<bool> EnablePreloadCaneReach { get; private set; }
        public static ConfigEntry<bool> EnablePreloadCaneNearPower { get; private set; }
        public static ConfigEntry<bool> EnablePreloadCaneNearShotgunPower { get; private set; }
        public static ConfigEntry<bool> EnablePreloadCaneStability { get; private set; }
        public static ConfigEntry<bool> EnablePreloadCaneManaSplashRatio { get; private set; }
        public static ConfigEntry<bool> EnablePreloadCaneCastspeedOverhold { get; private set; }
        public static ConfigEntry<bool> EnablePreloadCaneDrainAfterLock { get; private set; }
        public static ConfigEntry<bool> EnablePreloadCaneCastspeed { get; private set; }
        public static ConfigEntry<bool> EnablePreloadCaneMagicPrepareSpeed { get; private set; }
        public static ConfigEntry<float> SetCaneSwingSpeed { get; private set; }
        public static ConfigEntry<float> SetCaneCastSpeed { get; private set; }
        public static ConfigEntry<float> SetCaneBalance { get; private set; }
        public static ConfigEntry<float> SetCaneEfficiency { get; private set; }
        public static ConfigEntry<float> SetCaneRetention { get; private set; }
        public static ConfigEntry<float> SetCaneLockOn { get; private set; }
        public static ConfigEntry<float> SetCaneLongRange { get; private set; }
        public static ConfigEntry<float> SetCaneShortRange { get; private set; }
        public static ConfigEntry<float> SetCaneReach { get; private set; }
        public static ConfigEntry<float> SetCaneNearPower { get; private set; }
        public static ConfigEntry<float> SetCaneNearShotgunPower { get; private set; }
        public static ConfigEntry<float> SetCaneStability { get; private set; }
        public static ConfigEntry<float> SetCaneManaSplashRatio { get; private set; }
        public static ConfigEntry<float> SetCaneCastspeedOverhold { get; private set; }
        public static ConfigEntry<float> SetCaneDrainAfterLock { get; private set; }
        public static ConfigEntry<float> SetCaneCastspeed { get; private set; }
        public static ConfigEntry<float> SetCaneMagicPrepareSpeed { get; private set; }

        private const string SectionCane = "Cane";

        public static void InitializeCane()
        {
            Config.CreateTable(
                SectionCane,
                new Translator(chinese: "法杖", english: "Cane"),
                new Translator(
                    chinese: "法杖属性配置说明：\n" +
                             "1) 所有 SetCane* 项设为 -1 表示保持当前值。\n" +
                             "2) 取值范围：\n" +
                             "   - 魔力消耗效率 (SetCaneEfficiency)：0-169\n" +
                             "   - 无对应中文命名的属性：>= 0\n" +
                             "   - 其余法杖属性：0-255\n" +
                             "\n" +
                             "面板显示属性计算公式：\n" +
                             "- 近战攻击速度 (Swing Speed): 50 * near_punch_speed\n" +
                             "- 近战攻击距离 (Reach): 50 * near_reach\n" +
                             "- 近战威力 (Short Range): 55 * (0.25 * near_power + 0.75) * near_shotgun_power\n" +
                             "- 射击威力 (Long Range): 46 * far_power\n" +
                             "- 锁定性能 (Lock-On): 50 * lockon_power\n" +
                             "- 魔力稳定性 (Retention): 55 * stability * mana_splash_ratio * (0.75 * castspeed_overhold + 0.25) * (0.5 * drain_after_lock + 0.5)\n" +
                             "- 魔力消耗效率 (Efficiency): mp_use_ratio < 1 ? (169 - 104 * mp_use_ratio) : 65 / (mp_use_ratio * mp_use_ratio)\n" +
                             "- 魔力亲和性 (Balance): 60 * neutral\n" +
                             "- 咏唱速度 (Cast Speed): 50 * castspeed * (0.33 * magic_prepare_speed + 0.67) * (0.25 * castspeed_overhold + 0.75)\n" +
                             "\n" +
                             "无中文命名映射的内部属性：\n" +
                             "- SetCaneNearPower, SetCaneNearShotgunPower, SetCaneStability, SetCaneManaSplashRatio\n" +
                             "- SetCaneCastspeedOverhold, SetCaneDrainAfterLock, SetCaneCastspeed, SetCaneMagicPrepareSpeed",
                    english: "Cane config notes:\n" +
                             "1) All SetCane* values use -1 to keep current value.\n" +
                             "2) Value range:\n" +
                             "   - Mana consumption efficiency (SetCaneEfficiency): 0-169\n" +
                             "   - Properties without a Chinese display-name mapping: >= 0\n" +
                             "   - All other cane properties: 0-255\n" +
                             "\n" +
                             "Displayed stats:\n" +
                             "- Swing Speed: 50 * near_punch_speed\n" +
                             "- Reach: 50 * near_reach\n" +
                             "- Short Range: 55 * (0.25 * near_power + 0.75) * near_shotgun_power\n" +
                             "- Long Range: 46 * far_power\n" +
                             "- Lock-On: 50 * lockon_power\n" +
                             "- Retention: 55 * stability * mana_splash_ratio * (0.75 * castspeed_overhold + 0.25) * (0.5 * drain_after_lock + 0.5)\n" +
                             "- Efficiency: mp_use_ratio < 1 ? (169 - 104 * mp_use_ratio) : 65 / (mp_use_ratio * mp_use_ratio)\n" +
                             "- Balance: 60 * neutral\n" +
                             "- Cast Speed: 50 * castspeed * (0.33 * magic_prepare_speed + 0.67) * (0.25 * castspeed_overhold + 0.75)\n" +
                             "\n" +
                             "Raw/internal properties (no Chinese display-name mapping):\n" +
                             "- SetCaneNearPower, SetCaneNearShotgunPower, SetCaneStability, SetCaneManaSplashRatio\n" +
                             "- SetCaneCastspeedOverhold, SetCaneDrainAfterLock, SetCaneCastspeed, SetCaneMagicPrepareSpeed"
                )
                );
            EnablePreloadCaneSwingSpeed = Config.Bind(
                SectionCane,
                nameof(EnablePreloadCaneSwingSpeed),
                false,
                new Translator(chinese: "预加载近战攻击速度", english: "Preload Cane Swing Speed"),
                new Translator(
                    chinese: "启用预加载法杖近战攻击速度。开启后，法杖近战攻击速度将在存档读取后自动设置一次，使用设置值覆盖原始的法杖近战攻击速度。",
                    english: "Enable preload cane swing speed. When enabled, the cane swing speed will be automatically set once after loading a save, " +
                    "using the configured value to override the original cane swing speed."
                )
                );
            EnablePreloadCaneCastSpeed = Config.Bind(
                SectionCane,
                nameof(EnablePreloadCaneCastSpeed),
                false,
                new Translator(chinese: "预加载咏唱速度", english: "Preload Cane Cast Speed"),
                new Translator(
                    chinese: "启用预加载法杖咏唱速度。开启后，法杖咏唱速度将在存档读取后自动设置一次，使用设置值覆盖原始的法杖咏唱速度。",
                    english: "Enable preload cane cast speed. When enabled, the cane cast speed will be automatically set once after loading a save, " +
                    "using the configured value to override the original cane cast speed."
                )
                );
            EnablePreloadCaneBalance = Config.Bind(
                SectionCane,
                nameof(EnablePreloadCaneBalance),
                false,
                new Translator(chinese: "预加载魔力亲和性", english: "Preload Cane Balance"),
                new Translator(
                    chinese: "启用预加载法杖魔力亲和性。开启后，法杖魔力亲和性将在存档读取后自动设置一次，使用设置值覆盖原始的法杖魔力亲和性。",
                    english: "Enable preload cane balance. When enabled, the cane balance will be automatically set once after loading a save, " +
                    "using the configured value to override the original cane balance."
                )
                );
            EnablePreloadCaneEfficiency = Config.Bind(
                SectionCane,
                nameof(EnablePreloadCaneEfficiency),
                false,
                new Translator(chinese: "预加载魔力消耗效率", english: "Preload Cane Efficiency"),
                new Translator(
                    chinese: "启用预加载法杖魔力消耗效率。开启后，法杖魔力消耗效率将在存档读取后自动设置一次，使用设置值覆盖原始的法杖魔力消耗效率。",
                    english: "Enable preload cane efficiency. When enabled, the cane efficiency will be automatically set once after loading a save, " +
                    "using the configured value to override the original cane efficiency."
                )
                );
            EnablePreloadCaneRetention = Config.Bind(
                SectionCane,
                nameof(EnablePreloadCaneRetention),
                false,
                new Translator(chinese: "预加载魔力稳定性", english: "Preload Cane Retention"),
                new Translator(
                    chinese: "启用预加载法杖魔力稳定性。开启后，法杖魔力稳定性将在存档读取后自动设置一次，使用设置值覆盖原始的法杖魔力稳定性。",
                    english: "Enable preload cane retention. When enabled, the cane retention will be automatically set once after loading a save, " +
                    "using the configured value to override the original cane retention."
                )
                );
            EnablePreloadCaneLockOn = Config.Bind(
                SectionCane,
                nameof(EnablePreloadCaneLockOn),
                false,
                new Translator(chinese: "预加载锁定性能", english: "Preload Cane Lock-On"),
                new Translator(
                    chinese: "启用预加载法杖锁定性能。开启后，法杖锁定性能将在存档读取后自动设置一次，使用设置值覆盖原始的法杖锁定性能。",
                    english: "Enable preload cane lock-on. When enabled, the cane lock-on will be automatically set once after loading a save, " +
                    "using the configured value to override the original cane lock-on."
                )
                );
            EnablePreloadCaneLongRange = Config.Bind(
                SectionCane,
                nameof(EnablePreloadCaneLongRange),
                false,
                new Translator(chinese: "预加载射击威力", english: "Preload Cane Long Range"),
                new Translator(
                    chinese: "启用预加载法杖射击威力。开启后，法杖射击威力将在存档读取后自动设置一次，使用设置值覆盖原始的法杖射击威力。",
                    english: "Enable preload cane long-range attack range. When enabled, the cane long-range attack range will be automatically set once after loading a save, " +
                    "using the configured value to override the original cane long-range attack range."
                )
                );
            EnablePreloadCaneShortRange = Config.Bind(
                SectionCane,
                nameof(EnablePreloadCaneShortRange),
                false,
                new Translator(chinese: "预加载近战威力", english: "Preload Cane Short Range"),
                new Translator(
                    chinese: "启用预加载法杖近战威力。开启后，法杖近战威力将在存档读取后自动设置一次，使用设置值覆盖原始的法杖近战威力。",
                    english: "Enable preload cane short-range attack range. When enabled, the cane short-range attack range will be automatically set once after loading a save, " +
                    "using the configured value to override the original cane short-range attack range."
                )
                );
            EnablePreloadCaneReach = Config.Bind(
                SectionCane,
                nameof(EnablePreloadCaneReach),
                false,
                new Translator(chinese: "预加载近战攻击距离", english: "Preload Cane Reach"),
                new Translator(
                    chinese: "启用预加载法杖近战攻击距离。开启后，法杖近战攻击距离将在存档读取后自动设置一次，使用设置值覆盖原始的法杖近战攻击距离。",
                    english: "Enable preload cane reach. When enabled, the cane reach will be automatically set once after loading a save, " +
                    "using the configured value to override the original cane reach."
                )
                );
            EnablePreloadCaneNearPower = Config.Bind(
                SectionCane,
                nameof(EnablePreloadCaneNearPower),
                false,
                new Translator(chinese: "预加载 Near Power", english: "Preload Cane Near Power"),
                new Translator(
                    chinese: "启用预加载法杖 near power。开启后，法杖 near power 将在存档读取后自动设置一次，使用设置值覆盖原始的法杖 near power。",
                    english: "Enable preload cane near power. When enabled, the cane near power will be automatically set once after loading a save, " +
                    "using the configured value to override the original cane near power."
                )
                );
            EnablePreloadCaneNearShotgunPower = Config.Bind(
                SectionCane,
                nameof(EnablePreloadCaneNearShotgunPower),
                false,
                new Translator(chinese: "预加载 Near Shotgun Power", english: "Preload Cane Near Shotgun Power"),
                new Translator(
                    chinese: "启用预加载法杖 near shotgun power。开启后，法杖 near shotgun power 将在存档读取后自动设置一次，使用设置值覆盖原始的法杖 near shotgun power。",
                    english: "Enable preload cane near shotgun power. When enabled, the cane near shotgun power will be automatically set once after loading a save, " +
                    "using the configured value to override the original cane near shotgun power."
                )
                );
            EnablePreloadCaneStability = Config.Bind(
                SectionCane,
                nameof(EnablePreloadCaneStability),
                false,
                new Translator(chinese: "预加载 Stability", english: "Preload Cane Stability"),
                new Translator(
                    chinese: "启用预加载法杖 stability。开启后，法杖 stability 将在存档读取后自动设置一次，使用设置值覆盖原始的法杖 stability。",
                    english: "Enable preload cane stability. When enabled, the cane stability will be automatically set once after loading a save, " +
                    "using the configured value to override the original cane stability."
                )
                );
            EnablePreloadCaneManaSplashRatio = Config.Bind(
                SectionCane,
                nameof(EnablePreloadCaneManaSplashRatio),
                false,
                new Translator(chinese: "预加载 Mana Splash Ratio", english: "Preload Cane Mana Splash Ratio"),
                new Translator(
                    chinese: "启用预加载法杖 mana splash ratio。开启后，法杖 mana splash ratio 将在存档读取后自动设置一次，使用设置值覆盖原始的法杖 mana splash ratio。",
                    english: "Enable preload cane mana splash ratio. When enabled, the cane mana splash ratio will be automatically set once after loading a save, " +
                    "using the configured value to override the original cane mana splash ratio."
                )
                );
            EnablePreloadCaneCastspeedOverhold = Config.Bind(
                SectionCane,
                nameof(EnablePreloadCaneCastspeedOverhold),
                false,
                new Translator(chinese: "预加载 Castspeed Overhold", english: "Preload Cane Castspeed Overhold"),
                new Translator(
                    chinese: "启用预加载法杖 castspeed overhold。开启后，法杖 castspeed overhold 将在存档读取后自动设置一次，使用设置值覆盖原始的法杖 castspeed overhold。",
                    english: "Enable preload cane castspeed overhold. When enabled, the cane castspeed overhold will be automatically set once after loading a save, " +
                    "using the configured value to override the original cane castspeed overhold."
                )
                );
            EnablePreloadCaneDrainAfterLock = Config.Bind(
                SectionCane,
                nameof(EnablePreloadCaneDrainAfterLock),
                false,
                new Translator(chinese: "预加载 Drain After Lock", english: "Preload Cane Drain After Lock"),
                new Translator(
                    chinese: "启用预加载法杖 drain after lock。开启后，法杖 drain after lock 将在存档读取后自动设置一次，使用设置值覆盖原始的法杖 drain after lock。",
                    english: "Enable preload cane drain after lock. When enabled, the cane drain after lock will be automatically set once after loading a save, " +
                    "using the configured value to override the original cane drain after lock."
                )
                );
            EnablePreloadCaneCastspeed = Config.
                Bind(
                SectionCane,
                nameof(EnablePreloadCaneCastspeed),
                false,
                new Translator(chinese: "预加载 Castspeed", english: "Preload Cane Castspeed"),
                new Translator(
                    chinese: "启用预加载法杖 castspeed。开启后，法杖咏唱速度将在存档读取后自动设置一次，使用设置值覆盖原始的法杖咏唱速度。",
                    english: "Enable preload cane castspeed. When enabled, the cane castspeed will be automatically set once after loading a save, " +
                    "using the configured value to override the original cane castspeed."
                )
                );
            EnablePreloadCaneMagicPrepareSpeed = Config.Bind(
                SectionCane,
                nameof(EnablePreloadCaneMagicPrepareSpeed),
                false,
                new Translator(chinese: "预加载 Magic Prepare Speed", english: "Preload Cane Magic Prepare Speed"),
                new Translator(
                    chinese: "启用预加载法杖 magic prepare speed。开启后，法杖魔力准备速度将在存档读取后自动设置一次，使用设置值覆盖原始的法杖魔力准备速度。",
                    english: "Enable preload cane magic prepare speed. When enabled, the cane magic prepare speed will be automatically set once after loading a save, " +
                    "using the configured value to override the original cane magic prepare speed."
                )
                );
            SetCaneSwingSpeed = Config.Bind(
                SectionCane,
                nameof(SetCaneSwingSpeed),
                -1f,
                new Translator(chinese: "设置近战攻击速度", english: "Set Cane Swing Speed"),
                new Translator(
                    chinese: "设置法杖近战攻击速度。设为 -1 可保持为当前值。",
                    english: "Set cane swing speed. Set to -1 to keep the enhancer slot count at its current value."
                )
                );
            SetCaneCastSpeed = Config.Bind(
                SectionCane,
                nameof(SetCaneCastSpeed),
                -1f,
                new Translator(chinese: "设置咏唱速度", english: "Set Cane Cast Speed"),
                new Translator(
                    chinese: "设置法杖咏唱速度。设为 -1 可保持为当前值。",
                    english: "Set cane cast speed. Set to -1 to keep the enhancer slot count at its current value."
                )
                );
            SetCaneBalance = Config.Bind(
                SectionCane,
                nameof(SetCaneBalance),
                -1f,
                new Translator(chinese: "设置魔力亲和性", english: "Set Cane Balance"),
                new Translator(
                    chinese: "设置法杖魔力亲和性。设为 -1 可保持为当前值。",
                    english: "Set cane balance. Set to -1 to keep the enhancer slot count at its current value."
                )
                );
            SetCaneEfficiency = Config.Bind(
                SectionCane,
                nameof(SetCaneEfficiency),
                -1f,
                new Translator(chinese: "设置魔力消耗效率", english: "Set Cane Efficiency"),
                new Translator(
                    chinese: "设置法杖魔力消耗效率。设为 -1 可保持为当前值。",
                    english: "Set cane efficiency. Set to -1 to keep the enhancer slot count at its current value."
                )
                );
            SetCaneRetention = Config.Bind(
                SectionCane,
                nameof(SetCaneRetention),
                -1f,
                new Translator(chinese: "设置魔力稳定性", english: "Set Cane Retention"),
                new Translator(
                    chinese: "设置法杖魔力稳定性。设为 -1 可保持为当前值。",
                    english: "Set cane retention. Set to -1 to keep the enhancer slot count at its current value."
                )
                );
            SetCaneLockOn = Config.Bind(
                SectionCane,
                nameof(SetCaneLockOn),
                -1f,
                new Translator(chinese: "设置锁定性能", english: "Set Cane Lock-On"),
                new Translator(
                    chinese: "设置法杖锁定性能。设为 -1 可保持为当前值。",
                    english: "Set cane lock-on. Set to -1 to keep the enhancer slot count at its current value."
                )
                );
            SetCaneLongRange = Config.Bind(
                SectionCane,
                nameof(SetCaneLongRange),
                -1f,
                new Translator(chinese: "设置射击威力", english: "Set Cane Long Range"),
                new Translator(
                    chinese: "设置法杖射击威力。设为 -1 可保持为当前值。",
                    english: "Set cane long-range attack range. Set to -1 to keep the enhancer slot count at its current value."
                )
                );
            SetCaneShortRange = Config.Bind(
                SectionCane,
                nameof(SetCaneShortRange),
                -1f,
                new Translator(chinese: "设置近战威力", english: "Set Cane Short Range"),
                new Translator(
                    chinese: "设置法杖近战威力。设为 -1 可保持为当前值。",
                    english: "Set cane short-range attack range. Set to -1 to keep the enhancer slot count at its current value."
                )
                );
            SetCaneReach = Config.Bind(
                SectionCane,
                nameof(SetCaneReach),
                -1f,
                new Translator(chinese: "设置近战攻击距离", english: "Set Cane Reach"),
                new Translator(
                    chinese: "设置法杖近战攻击距离。设为 -1 可保持为当前值。",
                    english: "Set cane reach. Set to -1 to keep the enhancer slot count at its current value."
                )
                );
            SetCaneNearPower = Config.Bind(
                SectionCane,
                nameof(SetCaneNearPower),
                -1f,
                new Translator(chinese: "设置 Near Power", english: "Set Cane Near Power"),
                new Translator(
                    chinese: "设置法杖 near power。设为 -1 可保持为当前值。",
                    english: "Set cane near power. Set to -1 to keep the enhancer slot count at its current value."
                )
                );
            SetCaneNearShotgunPower = Config.Bind(
                SectionCane,
                nameof(SetCaneNearShotgunPower),
                -1f,
                new Translator(chinese: "设置 Near Shotgun Power", english: "Set Cane Near Shotgun Power"),
                new Translator(
                    chinese: "设置法杖 near shotgun power。设为 -1 可保持为当前值。",
                    english: "Set cane near shotgun power. Set to -1 to keep the enhancer slot count at its current value."
                )
                );
            SetCaneStability = Config.Bind(
                SectionCane,
                nameof(SetCaneStability),
                -1f,
                new Translator(chinese: "设置 Stability", english: "Set Cane Stability"),
                new Translator(
                    chinese: "设置法杖 stability。设为 -1 可保持为当前值。",
                    english: "Set cane stability. Set to -1 to keep the enhancer slot count at its current value."
                )
                );
            SetCaneManaSplashRatio = Config.Bind(
                SectionCane,
                nameof(SetCaneManaSplashRatio),
                -1f,
                new Translator(chinese: "设置 Mana Splash Ratio", english: "Set Cane Mana Splash Ratio"),
                new Translator(
                    chinese: "设置法杖 mana splash ratio。设为 -1 可保持为当前值。",
                    english: "Set cane mana splash ratio. Set to -1 to keep the enhancer slot count at its current value."
                )
                );
            SetCaneCastspeedOverhold = Config.Bind(
                SectionCane,
                nameof(SetCaneCastspeedOverhold),
                -1f,
                new Translator(chinese: "设置 Castspeed Overhold", english: "Set Cane Castspeed Overhold"),
                new Translator(
                    chinese: "设置法杖 castspeed overhold。设为 -1 可保持为当前值。",
                    english: "Set cane castspeed overhold. Set to -1 to keep the enhancer slot count at its current value."
                )
                );
            SetCaneDrainAfterLock = Config.Bind(
                SectionCane,
                nameof(SetCaneDrainAfterLock),
                -1f,
                new Translator(chinese: "设置 Drain After Lock", english: "Set Cane Drain After Lock"),
                new Translator(
                    chinese: "设置法杖 drain after lock。设为 -1 可保持为当前值。",
                    english: "Set cane drain after lock. Set to -1 to keep the enhancer slot count at its current value."
                )
                );
            SetCaneCastspeed = Config.Bind(
                SectionCane,
                nameof(SetCaneCastspeed),
                -1f,
                new Translator(chinese: "设置 Castspeed", english: "Set Cane Castspeed"),
                new Translator(
                    chinese: "设置法杖 castspeed。设为 -1 可保持为当前值。",
                    english: "Set cane castspeed. Set to -1 to keep the enhancer slot count at its current value."
                )
                );
            SetCaneMagicPrepareSpeed = Config.Bind(
                SectionCane,
                nameof(SetCaneMagicPrepareSpeed),
                -1f,
                new Translator(chinese: "设置 Magic Prepare Speed", english: "Set Cane Magic Prepare Speed"),
                new Translator(
                    chinese: "设置法杖 magic prepare speed。设为 -1 可保持为当前值。",
                    english: "Set cane magic prepare speed. Set to -1 to keep the enhancer slot count at its current value."
                )
                );
        }
    }
}
