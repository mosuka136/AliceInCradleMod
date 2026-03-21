using BetterExperience.ConfigFileSpace;

namespace BetterExperience.BepConfigManager
{
    internal sealed partial class ConfigManager
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
            Config.CreateTable(SectionCane,
                "Cane config notes / 法杖属性配置说明：\n" +
                "1) All SetCane* values use -1 to keep current value.\n" +
                "1) 所有 SetCane* 项设为 -1 表示保持当前值。\n" +
                "2) Value range / 取值范围：\n" +
                "   - Mana consumption efficiency (SetCaneEfficiency): 0-169 / 魔力消耗效率：0-169\n" +
                "   - Properties without a Chinese display-name mapping: >= 0 / 无对应中文命名的属性：>= 0\n" +
                "   - All other cane properties: 0-255 / 其余法杖属性：0-255\n" +
                "\n" +
                "Displayed stats / 面板显示属性计算公式：\n" +
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
                "Raw/internal properties (no Chinese display-name mapping) / 无中文命名映射的内部属性：\n" +
                "- SetCaneNearPower, SetCaneNearShotgunPower, SetCaneStability, SetCaneManaSplashRatio\n" +
                "- SetCaneCastspeedOverhold, SetCaneDrainAfterLock, SetCaneCastspeed, SetCaneMagicPrepareSpeed"
                );
            EnablePreloadCaneSwingSpeed = Config.Bind(
                SectionCane,
                nameof(EnablePreloadCaneSwingSpeed),
                false,
                "Enable preload cane swing speed. When enabled, the cane swing speed will be automatically set once after loading a save, " +
                "using the configured value to override the original cane swing speed.\n" +
                "启用预加载法杖近战攻击速度。开启后，法杖近战攻击速度将在存档读取后自动设置一次，使用设置值覆盖原始的法杖近战攻击速度。"
                );
            EnablePreloadCaneCastSpeed = Config.Bind(
                SectionCane,
                nameof(EnablePreloadCaneCastSpeed),
                false,
                "Enable preload cane cast speed. When enabled, the cane cast speed will be automatically set once after loading a save, " +
                "using the configured value to override the original cane cast speed.\n" +
                "启用预加载法杖咏唱速度。开启后，法杖咏唱速度将在存档读取后自动设置一次，使用设置值覆盖原始的法杖咏唱速度。"
                );
            EnablePreloadCaneBalance = Config.Bind(
                SectionCane,
                nameof(EnablePreloadCaneBalance),
                false,
                "Enable preload cane balance. When enabled, the cane balance will be automatically set once after loading a save, " +
                "using the configured value to override the original cane balance.\n" +
                "启用预加载法杖魔力亲和性。开启后，法杖魔力亲和性将在存档读取后自动设置一次，使用设置值覆盖原始的法杖魔力亲和性。"
                );
            EnablePreloadCaneEfficiency = Config.Bind(
                SectionCane,
                nameof(EnablePreloadCaneEfficiency),
                false,
                "Enable preload cane efficiency. When enabled, the cane efficiency will be automatically set once after loading a save, " +
                "using the configured value to override the original cane efficiency.\n" +
                "启用预加载法杖魔力消耗效率。开启后，法杖魔力消耗效率将在存档读取后自动设置一次，使用设置值覆盖原始的法杖魔力消耗效率。"
                );
            EnablePreloadCaneRetention = Config.Bind(
                SectionCane,
                nameof(EnablePreloadCaneRetention),
                false,
                "Enable preload cane retention. When enabled, the cane retention will be automatically set once after loading a save, " +
                "using the configured value to override the original cane retention.\n" +
                "启用预加载法杖魔力稳定性。开启后，法杖魔力稳定性将在存档读取后自动设置一次，使用设置值覆盖原始的法杖魔力稳定性。"
                );
            EnablePreloadCaneLockOn = Config.Bind(
                SectionCane,
                nameof(EnablePreloadCaneLockOn),
                false,
                "Enable preload cane lock-on. When enabled, the cane lock-on will be automatically set once after loading a save, " +
                "using the configured value to override the original cane lock-on.\n" +
                "启用预加载法杖锁定性能。开启后，法杖锁定性能将在存档读取后自动设置一次，使用设置值覆盖原始的法杖锁定性能。"
                );
            EnablePreloadCaneLongRange = Config.Bind(
                SectionCane,
                nameof(EnablePreloadCaneLongRange),
                false,
                "Enable preload cane long-range attack range. When enabled, the cane long-range attack range will be automatically set once after loading a save, " +
                "using the configured value to override the original cane long-range attack range.\n" +
                "启用预加载法杖射击威力。开启后，法杖射击威力将在存档读取后自动设置一次，使用设置值覆盖原始的法杖射击威力。"
                );
            EnablePreloadCaneShortRange = Config.Bind(
                SectionCane,
                nameof(EnablePreloadCaneShortRange),
                false,
                "Enable preload cane short-range attack range. When enabled, the cane short-range attack range will be automatically set once after loading a save, " +
                "using the configured value to override the original cane short-range attack range.\n" +
                "启用预加载法杖近战威力。开启后，法杖近战威力将在存档读取后自动设置一次，使用设置值覆盖原始的法杖近战威力。"
                );
            EnablePreloadCaneReach = Config.Bind(
                SectionCane,
                nameof(EnablePreloadCaneReach),
                false,
                "Enable preload cane reach. When enabled, the cane reach will be automatically set once after loading a save, " +
                "using the configured value to override the original cane reach.\n" +
                "启用预加载法杖近战攻击距离。开启后，法杖近战攻击距离将在存档读取后自动设置一次，使用设置值覆盖原始的法杖近战攻击距离。"
                );
            EnablePreloadCaneNearPower = Config.Bind(
                SectionCane,
                nameof(EnablePreloadCaneNearPower),
                false,
                "Enable preload cane near power. When enabled, the cane near power will be automatically set once after loading a save, " +
                "using the configured value to override the original cane near power.\n" +
                "启用预加载法杖 near power。开启后，法杖 near power 将在存档读取后自动设置一次，使用设置值覆盖原始的法杖 near power。"
                );
            EnablePreloadCaneNearShotgunPower = Config.Bind(
                SectionCane,
                nameof(EnablePreloadCaneNearShotgunPower),
                false,
                "Enable preload cane near shotgun power. When enabled, the cane near shotgun power will be automatically set once after loading a save, " +
                "using the configured value to override the original cane near shotgun power.\n" +
                "启用预加载法杖 near shotgun power。开启后，法杖 near shotgun power 将在存档读取后自动设置一次，使用设置值覆盖原始的法杖 near shotgun power。"
                );
            EnablePreloadCaneStability = Config.Bind(
                SectionCane,
                nameof(EnablePreloadCaneStability),
                false,
                "Enable preload cane stability. When enabled, the cane stability will be automatically set once after loading a save, " +
                "using the configured value to override the original cane stability.\n" +
                "启用预加载法杖 stability。开启后，法杖 stability 将在存档读取后自动设置一次，使用设置值覆盖原始的法杖 stability。"
                );
            EnablePreloadCaneManaSplashRatio = Config.Bind(
                SectionCane,
                nameof(EnablePreloadCaneManaSplashRatio),
                false,
                "Enable preload cane mana splash ratio. When enabled, the cane mana splash ratio will be automatically set once after loading a save, " +
                "using the configured value to override the original cane mana splash ratio.\n" +
                "启用预加载法杖 mana splash ratio。开启后，法杖 mana splash ratio 将在存档读取后自动设置一次，使用设置值覆盖原始的法杖 mana splash ratio。"
                );
            EnablePreloadCaneCastspeedOverhold = Config.Bind(
                SectionCane,
                nameof(EnablePreloadCaneCastspeedOverhold),
                false,
                "Enable preload cane castspeed overhold. When enabled, the cane castspeed overhold will be automatically set once after loading a save, " +
                "using the configured value to override the original cane castspeed overhold.\n" +
                "启用预加载法杖 castspeed overhold。开启后，法杖 castspeed overhold 将在存档读取后自动设置一次，使用设置值覆盖原始的法杖 castspeed overhold。"
                );
            EnablePreloadCaneDrainAfterLock = Config.Bind(
                SectionCane,
                nameof(EnablePreloadCaneDrainAfterLock),
                false,
                "Enable preload cane drain after lock. When enabled, the cane drain after lock will be automatically set once after loading a save, " +
                "using the configured value to override the original cane drain after lock.\n" +
                "启用预加载法杖 drain after lock。开启后，法杖 drain after lock 将在存档读取后自动设置一次，使用设置值覆盖原始的法杖 drain after lock。"
                );
            EnablePreloadCaneCastspeed = Config.
                Bind(
                SectionCane,
                nameof(EnablePreloadCaneCastspeed),
                false,
                "Enable preload cane castspeed. When enabled, the cane castspeed will be automatically set once after loading a save, " +
                "using the configured value to override the original cane castspeed.\n" +
                "启用预加载法杖 castspeed。开启后，法杖咏唱速度将在存档读取后自动设置一次，使用设置值覆盖原始的法杖咏唱速度。"
                );
            EnablePreloadCaneMagicPrepareSpeed = Config.Bind(
                SectionCane,
                nameof(EnablePreloadCaneMagicPrepareSpeed),
                false,
                "Enable preload cane magic prepare speed. When enabled, the cane magic prepare speed will be automatically set once after loading a save, " +
                "using the configured value to override the original cane magic prepare speed.\n" +
                "启用预加载法杖 magic prepare speed。开启后，法杖魔力准备速度将在存档读取后自动设置一次，使用设置值覆盖原始的法杖魔力准备速度。"
                );
            SetCaneSwingSpeed = Config.Bind(
                SectionCane,
                nameof(SetCaneSwingSpeed),
                -1f,
                "Set cane swing speed. Set to -1 to keep the enhancer slot count at its current value.\n" +
                "设置法杖近战攻击速度。设为 -1 可保持为当前值。"
                );
            SetCaneCastSpeed = Config.Bind(
                SectionCane,
                nameof(SetCaneCastSpeed),
                -1f,
                "Set cane cast speed. Set to -1 to keep the enhancer slot count at its current value.\n" +
                "设置法杖咏唱速度。设为 -1 可保持为当前值。"
                );
            SetCaneBalance = Config.Bind(
                SectionCane,
                nameof(SetCaneBalance),
                -1f,
                "Set cane balance. Set to -1 to keep the enhancer slot count at its current value.\n" +
                "设置法杖魔力亲和性。设为 -1 可保持为当前值。"
                );
            SetCaneEfficiency = Config.Bind(
                SectionCane,
                nameof(SetCaneEfficiency),
                -1f,
                "Set cane efficiency. Set to -1 to keep the enhancer slot count at its current value.\n" +
                "设置法杖魔力消耗效率。设为 -1 可保持为当前值。"
                );
            SetCaneRetention = Config.Bind(
                SectionCane,
                nameof(SetCaneRetention),
                -1f,
                "Set cane retention. Set to -1 to keep the enhancer slot count at its current value.\n" +
                "设置法杖魔力稳定性。设为 -1 可保持为当前值。"
                );
            SetCaneLockOn = Config.Bind(
                SectionCane,
                nameof(SetCaneLockOn),
                -1f,
                "Set cane lock-on. Set to -1 to keep the enhancer slot count at its current value.\n" +
                "设置法杖锁定性能。设为 -1 可保持为当前值。"
                );
            SetCaneLongRange = Config.Bind(
                SectionCane,
                nameof(SetCaneLongRange),
                -1f,
                "Set cane long-range attack range. Set to -1 to keep the enhancer slot count at its current value.\n" +
                "设置法杖射击威力。设为 -1 可保持为当前值。"
                );
            SetCaneShortRange = Config.Bind(
                SectionCane,
                nameof(SetCaneShortRange),
                -1f,
                "Set cane short-range attack range. Set to -1 to keep the enhancer slot count at its current value.\n" +
                "设置法杖近战威力。设为 -1 可保持为当前值。"
                );
            SetCaneReach = Config.Bind(
                SectionCane,
                nameof(SetCaneReach),
                -1f,
                "Set cane reach. Set to -1 to keep the enhancer slot count at its current value.\n" +
                "设置法杖近战攻击距离。设为 -1 可保持为当前值。"
                );
            SetCaneNearPower = Config.Bind(
                SectionCane,
                nameof(SetCaneNearPower),
                -1f,
                "Set cane near power. Set to -1 to keep the enhancer slot count at its current value.\n" +
                "设置法杖 near power。设为 -1 可保持为当前值。"
                );
            SetCaneNearShotgunPower = Config.Bind(
                SectionCane,
                nameof(SetCaneNearShotgunPower),
                -1f,
                "Set cane near shotgun power. Set to -1 to keep the enhancer slot count at its current value.\n" +
                "设置法杖 near shotgun power。设为 -1 可保持为当前值。"
                );
            SetCaneStability = Config.Bind(
                SectionCane,
                nameof(SetCaneStability),
                -1f,
                "Set cane stability. Set to -1 to keep the enhancer slot count at its current value.\n" +
                "设置法杖 stability。设为 -1 可保持为当前值。"
                );
            SetCaneManaSplashRatio = Config.Bind(
                SectionCane,
                nameof(SetCaneManaSplashRatio),
                -1f,
                "Set cane mana splash ratio. Set to -1 to keep the enhancer slot count at its current value.\n" +
                "设置法杖 mana splash ratio。设为 -1 可保持为当前值。"
                );
            SetCaneCastspeedOverhold = Config.Bind(
                SectionCane,
                nameof(SetCaneCastspeedOverhold),
                -1f,
                "Set cane castspeed overhold. Set to -1 to keep the enhancer slot count at its current value.\n" +
                "设置法杖 castspeed overhold。设为 -1 可保持为当前值。"
                );
            SetCaneDrainAfterLock = Config.Bind(
                SectionCane,
                nameof(SetCaneDrainAfterLock),
                -1f,
                "Set cane drain after lock. Set to -1 to keep the enhancer slot count at its current value.\n" +
                "设置法杖 drain after lock。设为 -1 可保持为当前值。"
                );
            SetCaneCastspeed = Config.Bind(
                SectionCane,
                nameof(SetCaneCastspeed),
                -1f,
                "Set cane castspeed. Set to -1 to keep the enhancer slot count at its current value.\n" +
                "设置法杖 castspeed。设为 -1 可保持为当前值。"
                );
            SetCaneMagicPrepareSpeed = Config.Bind(
                SectionCane,
                nameof(SetCaneMagicPrepareSpeed),
                -1f,
                "Set cane magic prepare speed. Set to -1 to keep the enhancer slot count at its current value.\n" +
                "设置法杖 magic prepare speed。设为 -1 可保持为当前值。"
                );
        }
    }
}
