using BetterExperience.BLogSpace;
using System;
using UnityModBase.HClassAttribute;
using UnityModBase.HConfigSpace;
using UnityModBase.HTranslatorSpace;

namespace BetterExperience.BConfigManager
{
    public static partial class ConfigManager
    {
        // 法杖配置同时暴露面板显示属性和内部原始属性；所有 SetCane* 使用 -1 表示不覆盖当前装备值。
        [EntryGui(2)]
        [EntrySlider(1, -1f, 255f, 0.1f)]
        public static ConfigEntry<bool, float> SetCaneSwingSpeed { get; private set; }
        [EntryGui(2)]
        [EntrySlider(1, -1f, 255f, 0.1f)]
        public static ConfigEntry<bool, float> SetCaneCastSpeed { get; private set; }
        [EntryGui(2)]
        [EntrySlider(1, -1f, 255f, 0.1f)]
        public static ConfigEntry<bool, float> SetCaneBalance { get; private set; }
        [EntryGui(2)]
        [EntrySlider(1, -1f, 169f, 0.1f)]
        public static ConfigEntry<bool, float> SetCaneEfficiency { get; private set; }
        [EntryGui(2)]
        [EntrySlider(1, -1f, 255f, 0.1f)]
        public static ConfigEntry<bool, float> SetCaneRetention { get; private set; }
        [EntryGui(2)]
        [EntrySlider(1, -1f, 255f, 0.1f)]
        public static ConfigEntry<bool, float> SetCaneLockOn { get; private set; }
        [EntryGui(2)]
        [EntrySlider(1, -1f, 255f, 0.1f)]
        public static ConfigEntry<bool, float> SetCaneLongRange { get; private set; }
        [EntryGui(2)]
        [EntrySlider(1, -1f, 255f, 0.1f)]
        public static ConfigEntry<bool, float> SetCaneShortRange { get; private set; }
        [EntryGui(2)]
        [EntrySlider(1, -1f, 255f, 0.1f)]
        public static ConfigEntry<bool, float> SetCaneReach { get; private set; }
        [EntryGui(2)]
        [EntrySlider(1, -1f, 255f, 0.1f)]
        public static ConfigEntry<bool, float> SetCaneNearPower { get; private set; }
        [EntryGui(2)]
        [EntrySlider(1, -1f, 255f, 0.1f)]
        public static ConfigEntry<bool, float> SetCaneNearShotgunPower { get; private set; }
        [EntryGui(2)]
        [EntrySlider(1, -1f, 255f, 0.1f)]
        public static ConfigEntry<bool, float> SetCaneStability { get; private set; }
        [EntryGui(2)]
        [EntrySlider(1, -1f, 255f, 0.1f)]
        public static ConfigEntry<bool, float> SetCaneManaSplashRatio { get; private set; }
        [EntryGui(2)]
        [EntrySlider(1, -1f, 255f, 0.1f)]
        public static ConfigEntry<bool, float> SetCaneCastspeedOverhold { get; private set; }
        [EntryGui(2)]
        [EntrySlider(1, -1f, 255f, 0.1f)]
        public static ConfigEntry<bool, float> SetCaneDrainAfterLock { get; private set; }
        [EntryGui(2)]
        [EntrySlider(1, -1f, 255f, 0.1f)]
        public static ConfigEntry<bool, float> SetCaneCastspeed { get; private set; }
        [EntryGui(2)]
        [EntrySlider(1, -1f, 255f, 0.1f)]
        public static ConfigEntry<bool, float> SetCaneMagicPrepareSpeed { get; private set; }

        private const string SectionCane = "Cane";

        /// <summary>
        /// 初始化法杖属性配置。
        /// 表说明中保留了面板显示值到内部字段的换算公式，补丁写入时需要依赖这些公式反推原始属性。
        /// </summary>
        public static void InitializeCane()
        {
            try
            {
                Config.CreateTable(
                    SectionCane,
                    new Translator(chinese: "法杖", english: "Cane"),
                    new Translator(
                    chinese: "法杖属性配置说明：\n" +
                             "1） 所有 SetCane* 项设为 -1 表示保持当前值。\n" +
                             "2） 取值范围：\n" +
                             "   - 魔力消耗效率（SetCaneEfficiency）：0-169\n" +
                             "   - 无对应中文命名的属性：>= 0\n" +
                             "   - 其余法杖属性：0-255\n" +
                             "\n" +
                             "面板显示属性计算公式：\n" +
                             "- 近战攻击速度（Swing Speed）：50 * near_punch_speed\n" +
                             "- 近战攻击距离（Reach）：50 * near_reach\n" +
                             "- 近战威力（Short Range）：55 * (0.25 * near_power + 0.75) * near_shotgun_power\n" +
                             "- 射击威力（Long Range）：46 * far_power\n" +
                             "- 锁定性能（Lock-On）：50 * lockon_power\n" +
                             "- 魔力稳定性（Retention）：55 * stability * mana_splash_ratio * (0.75 * castspeed_overhold + 0.25) * (0.5 * drain_after_lock + 0.5)\n" +
                             "- 魔力消耗效率（Efficiency）：mp_use_ratio < 1 ? (169 - 104 * mp_use_ratio) : 65 / (mp_use_ratio * mp_use_ratio)\n" +
                             "- 魔力亲和性（Balance）：60 * neutral\n" +
                             "- 咏唱速度（Cast Speed）：50 * castspeed * (0.33 * magic_prepare_speed + 0.67) * (0.25 * castspeed_overhold + 0.75)\n" +
                             "\n" +
                             "无中文命名映射的内部属性：\n" +
                             "- SetCaneNearPower、SetCaneNearShotgunPower、SetCaneStability、SetCaneManaSplashRatio\n" +
                             "- SetCaneCastspeedOverhold、SetCaneDrainAfterLock、SetCaneCastspeed、SetCaneMagicPrepareSpeed",
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
                SetCaneSwingSpeed = BindPreloadValue(
                    SectionCane,
                    nameof(SetCaneSwingSpeed),
                    -1f,
                    new Translator(chinese: "设置近战攻击速度", english: "Set Cane Swing Speed"),
                    new Translator(
                        chinese: "设置法杖近战攻击速度。",
                        english: "Set cane swing speed."
                    )
                    );
                SetCaneCastSpeed = BindPreloadValue(
                    SectionCane,
                    nameof(SetCaneCastSpeed),
                    -1f,
                    new Translator(chinese: "设置咏唱速度", english: "Set Cane Cast Speed"),
                    new Translator(
                        chinese: "设置法杖咏唱速度。",
                        english: "Set cane cast speed."
                    )
                    );
                SetCaneBalance = BindPreloadValue(
                    SectionCane,
                    nameof(SetCaneBalance),
                    -1f,
                    new Translator(chinese: "设置魔力亲和性", english: "Set Cane Balance"),
                    new Translator(
                        chinese: "设置法杖魔力亲和性。",
                        english: "Set cane balance."
                    )
                    );
                SetCaneEfficiency = BindPreloadValue(
                    SectionCane,
                    nameof(SetCaneEfficiency),
                    -1f,
                    new Translator(chinese: "设置魔力消耗效率", english: "Set Cane Efficiency"),
                    new Translator(
                        chinese: "设置法杖魔力消耗效率。",
                        english: "Set cane efficiency."
                    )
                    );
                SetCaneRetention = BindPreloadValue(
                    SectionCane,
                    nameof(SetCaneRetention),
                    -1f,
                    new Translator(chinese: "设置魔力稳定性", english: "Set Cane Retention"),
                    new Translator(
                        chinese: "设置法杖魔力稳定性。",
                        english: "Set cane retention."
                    )
                    );
                SetCaneLockOn = BindPreloadValue(
                    SectionCane,
                    nameof(SetCaneLockOn),
                    -1f,
                    new Translator(chinese: "设置锁定性能", english: "Set Cane Lock-On"),
                    new Translator(
                        chinese: "设置法杖锁定性能。",
                        english: "Set cane lock-on."
                    )
                    );
                SetCaneLongRange = BindPreloadValue(
                    SectionCane,
                    nameof(SetCaneLongRange),
                    -1f,
                    new Translator(chinese: "设置射击威力", english: "Set Cane Long Range"),
                    new Translator(
                        chinese: "设置法杖射击威力。",
                        english: "Set cane long-range attack range."
                    )
                    );
                SetCaneShortRange = BindPreloadValue(
                    SectionCane,
                    nameof(SetCaneShortRange),
                    -1f,
                    new Translator(chinese: "设置近战威力", english: "Set Cane Short Range"),
                    new Translator(
                        chinese: "设置法杖近战威力。",
                        english: "Set cane short-range attack range."
                    )
                    );
                SetCaneReach = BindPreloadValue(
                    SectionCane,
                    nameof(SetCaneReach),
                    -1f,
                    new Translator(chinese: "设置近战攻击距离", english: "Set Cane Reach"),
                    new Translator(
                        chinese: "设置法杖近战攻击距离。",
                        english: "Set cane reach."
                    )
                    );
                SetCaneNearPower = BindPreloadValue(
                    SectionCane,
                    nameof(SetCaneNearPower),
                    -1f,
                    new Translator(chinese: "设置 Near Power", english: "Set Cane Near Power"),
                    new Translator(
                        chinese: "设置法杖 near power。",
                        english: "Set cane near power."
                    )
                    );
                SetCaneNearShotgunPower = BindPreloadValue(
                    SectionCane,
                    nameof(SetCaneNearShotgunPower),
                    -1f,
                    new Translator(chinese: "设置 Near Shotgun Power", english: "Set Cane Near Shotgun Power"),
                    new Translator(
                        chinese: "设置法杖 near shotgun power。",
                        english: "Set cane near shotgun power."
                    )
                    );
                SetCaneStability = BindPreloadValue(
                    SectionCane,
                    nameof(SetCaneStability),
                    -1f,
                    new Translator(chinese: "设置 Stability", english: "Set Cane Stability"),
                    new Translator(
                        chinese: "设置法杖 stability。",
                        english: "Set cane stability."
                    )
                    );
                SetCaneManaSplashRatio = BindPreloadValue(
                    SectionCane,
                    nameof(SetCaneManaSplashRatio),
                    -1f,
                    new Translator(chinese: "设置 Mana Splash Ratio", english: "Set Cane Mana Splash Ratio"),
                    new Translator(
                        chinese: "设置法杖 mana splash ratio。",
                        english: "Set cane mana splash ratio."
                    )
                    );
                SetCaneCastspeedOverhold = BindPreloadValue(
                    SectionCane,
                    nameof(SetCaneCastspeedOverhold),
                    -1f,
                    new Translator(chinese: "设置 Castspeed Overhold", english: "Set Cane Castspeed Overhold"),
                    new Translator(
                        chinese: "设置法杖 castspeed overhold。",
                        english: "Set cane castspeed overhold."
                    )
                    );
                SetCaneDrainAfterLock = BindPreloadValue(
                    SectionCane,
                    nameof(SetCaneDrainAfterLock),
                    -1f,
                    new Translator(chinese: "设置 Drain After Lock", english: "Set Cane Drain After Lock"),
                    new Translator(
                        chinese: "设置法杖 drain after lock。",
                        english: "Set cane drain after lock."
                    )
                    );
                SetCaneCastspeed = BindPreloadValue(
                    SectionCane,
                    nameof(SetCaneCastspeed),
                    -1f,
                    new Translator(chinese: "设置 Castspeed", english: "Set Cane Castspeed"),
                    new Translator(
                        chinese: "设置法杖 castspeed。",
                        english: "Set cane castspeed."
                    )
                    );
                SetCaneMagicPrepareSpeed = BindPreloadValue(
                    SectionCane,
                    nameof(SetCaneMagicPrepareSpeed),
                    -1f,
                    new Translator(chinese: "设置 Magic Prepare Speed", english: "Set Cane Magic Prepare Speed"),
                    new Translator(
                        chinese: "设置法杖 magic prepare speed。",
                        english: "Set cane magic prepare speed."
                    )
                    );
            }
            catch (Exception ex)
            {
                BLog.Error("Failed to initialize config manager for cane.", ex);
            }
        }
    }
}
