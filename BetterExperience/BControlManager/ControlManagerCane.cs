using BetterExperience.BLogSpace;
using BetterExperience.Patches;
using System;
using UnityModBase.HControlSpace;
using UnityModBase.HGuiSpace;
using UnityModBase.HTranslatorSpace;

namespace BetterExperience.BControlManager
{
    internal static partial class ControlManager
    {
        internal static ControlEntry<float> SetCaneSwingSpeed { get; private set; }
        internal static ControlEntry<float> SetCaneCastSpeed { get; private set; }
        internal static ControlEntry<float> SetCaneBalance { get; private set; }
        internal static ControlEntry<float> SetCaneEfficiency { get; private set; }
        internal static ControlEntry<float> SetCaneRetention { get; private set; }
        internal static ControlEntry<float> SetCaneLockOn { get; private set; }
        internal static ControlEntry<float> SetCaneLongRange { get; private set; }
        internal static ControlEntry<float> SetCaneShortRange { get; private set; }
        internal static ControlEntry<float> SetCaneReach { get; private set; }
        internal static ControlEntry<float> SetCaneNearPower { get; private set; }
        internal static ControlEntry<float> SetCaneNearShotgunPower { get; private set; }
        internal static ControlEntry<float> SetCaneStability { get; private set; }
        internal static ControlEntry<float> SetCaneManaSplashRatio { get; private set; }
        internal static ControlEntry<float> SetCaneCastspeedOverhold { get; private set; }
        internal static ControlEntry<float> SetCaneDrainAfterLock { get; private set; }
        internal static ControlEntry<float> SetCaneCastspeed { get; private set; }
        internal static ControlEntry<float> SetCaneMagicPrepareSpeed { get; private set; }
        internal static ControlEntry<int> SetCaneInstantSwitchCount { get; private set; }
        internal static ControlEntry<int> SetCaneHolderSlotCount { get; private set; }

        private const string SectionCane = "Cane";

        /// <summary>
        /// 创建法杖控制表并绑定法杖属性条目。
        /// </summary>
        internal static void InitializeCane()
        {
            try
            {
                BService.Control.CreateTable(
                    SectionCane,
                    new Translator(chinese: "法杖", english: "Cane"),
                    new Translator(
                        chinese: "查看和修改当前装备法杖的属性。",
                        english: "View and modify attributes of the currently equipped cane."
                        )
                    );

                SetCaneSwingSpeed = BindCane(
                    nameof(SetCaneSwingSpeed),
                    HPatches.SetCaneAttributePatch.GetSwingSpeed,
                    HPatches.SetCaneAttributePatch.SetSwingSpeed,
                    new Translator(chinese: "设置近战攻击速度", english: "Set Cane Swing Speed"),
                    255f
                    );
                SetCaneCastSpeed = BindCane(
                    nameof(SetCaneCastSpeed),
                    HPatches.SetCaneAttributePatch.GetCastSpeed,
                    HPatches.SetCaneAttributePatch.SetCastSpeed,
                    new Translator(chinese: "设置咏唱速度", english: "Set Cane Cast Speed"),
                    255f
                    );
                SetCaneBalance = BindCane(
                    nameof(SetCaneBalance),
                    HPatches.SetCaneAttributePatch.GetBalance,
                    HPatches.SetCaneAttributePatch.SetBalance,
                    new Translator(chinese: "设置魔力亲和性", english: "Set Cane Balance"),
                    255f
                    );
                SetCaneEfficiency = BindCane(
                    nameof(SetCaneEfficiency),
                    HPatches.SetCaneAttributePatch.GetEfficiency,
                    HPatches.SetCaneAttributePatch.SetEfficiency,
                    new Translator(chinese: "设置魔力消耗效率", english: "Set Cane Efficiency"),
                    169f
                    );
                SetCaneRetention = BindCane(
                    nameof(SetCaneRetention),
                    HPatches.SetCaneAttributePatch.GetRetention,
                    HPatches.SetCaneAttributePatch.SetRetention,
                    new Translator(chinese: "设置魔力稳定性", english: "Set Cane Retention"),
                    255f
                    );
                SetCaneLockOn = BindCane(
                    nameof(SetCaneLockOn),
                    HPatches.SetCaneAttributePatch.GetLockOn,
                    HPatches.SetCaneAttributePatch.SetLockOn,
                    new Translator(chinese: "设置锁定性能", english: "Set Cane Lock-On"),
                    255f
                    );
                SetCaneLongRange = BindCane(
                    nameof(SetCaneLongRange),
                    HPatches.SetCaneAttributePatch.GetLongRange,
                    HPatches.SetCaneAttributePatch.SetLongRange,
                    new Translator(chinese: "设置射击威力", english: "Set Cane Long Range"),
                    255f
                    );
                SetCaneShortRange = BindCane(
                    nameof(SetCaneShortRange),
                    HPatches.SetCaneAttributePatch.GetShortRange,
                    HPatches.SetCaneAttributePatch.SetShortRange,
                    new Translator(chinese: "设置近战威力", english: "Set Cane Short Range"),
                    255f
                    );
                SetCaneReach = BindCane(
                    nameof(SetCaneReach),
                    HPatches.SetCaneAttributePatch.GetReach,
                    HPatches.SetCaneAttributePatch.SetReach,
                    new Translator(chinese: "设置近战攻击距离", english: "Set Cane Reach"),
                    255f
                    );
                SetCaneNearPower = BindCane(
                    nameof(SetCaneNearPower),
                    HPatches.SetCaneAttributePatch.GetNearPower,
                    HPatches.SetCaneAttributePatch.SetNearPower,
                    new Translator(chinese: "设置 Near Power", english: "Set Cane Near Power"),
                    255f
                    );
                SetCaneNearShotgunPower = BindCane(
                    nameof(SetCaneNearShotgunPower),
                    HPatches.SetCaneAttributePatch.GetNearShotgunPower,
                    HPatches.SetCaneAttributePatch.SetNearShotgunPower,
                    new Translator(chinese: "设置 Near Shotgun Power", english: "Set Cane Near Shotgun Power"),
                    255f
                    );
                SetCaneStability = BindCane(
                    nameof(SetCaneStability),
                    HPatches.SetCaneAttributePatch.GetStability,
                    HPatches.SetCaneAttributePatch.SetStability,
                    new Translator(chinese: "设置 Stability", english: "Set Cane Stability"),
                    255f
                    );
                SetCaneManaSplashRatio = BindCane(
                    nameof(SetCaneManaSplashRatio),
                    HPatches.SetCaneAttributePatch.GetManaSplashRatio,
                    HPatches.SetCaneAttributePatch.SetManaSplashRatio,
                    new Translator(chinese: "设置 Mana Splash Ratio", english: "Set Cane Mana Splash Ratio"),
                    255f
                    );
                SetCaneCastspeedOverhold = BindCane(
                    nameof(SetCaneCastspeedOverhold),
                    HPatches.SetCaneAttributePatch.GetCastspeedOverhold,
                    HPatches.SetCaneAttributePatch.SetCastspeedOverhold,
                    new Translator(chinese: "设置 Castspeed Overhold", english: "Set Cane Castspeed Overhold"),
                    255f
                    );
                SetCaneDrainAfterLock = BindCane(
                    nameof(SetCaneDrainAfterLock),
                    HPatches.SetCaneAttributePatch.GetDrainAfterLock,
                    HPatches.SetCaneAttributePatch.SetDrainAfterLock,
                    new Translator(chinese: "设置 Drain After Lock", english: "Set Cane Drain After Lock"),
                    255f
                    );
                SetCaneCastspeed = BindCane(
                    nameof(SetCaneCastspeed),
                    HPatches.SetCaneAttributePatch.GetCastspeed,
                    HPatches.SetCaneAttributePatch.SetCastspeed,
                    new Translator(chinese: "设置 Castspeed", english: "Set Cane Castspeed"),
                    255f
                    );
                SetCaneMagicPrepareSpeed = BindCane(
                    nameof(SetCaneMagicPrepareSpeed),
                    HPatches.SetCaneAttributePatch.GetMagicPrepareSpeed,
                    HPatches.SetCaneAttributePatch.SetMagicPrepareSpeed,
                    new Translator(chinese: "设置 Magic Prepare Speed", english: "Set Cane Magic Prepare Speed"),
                    255f
                    );
                SetCaneInstantSwitchCount = Bind(
                    SectionCane,
                    nameof(SetCaneInstantSwitchCount),
                    HPatches.CaneQuickSwitch.GetInstantSwitchCount,
                    HPatches.CaneQuickSwitch.SetInstantSwitchCount,
                    new Translator(chinese: "设置当前立即切换次数", english: "Set Current Instant Switch Count"),
                    new Translator(
                        chinese: "设置当前剩余的立即切换法杖次数。",
                        english: "Set the remaining instant cane switch charges."
                        ),
                    new UiSliderMetadata(0f, 99f, 1f)
                    );
                SetCaneHolderSlotCount = Bind(
                    SectionCane,
                    nameof(SetCaneHolderSlotCount),
                    HPatches.SetCaneHolderSlotCountPatch.GetCaneHolderSlotCount,
                    HPatches.SetCaneHolderSlotCountPatch.SetCaneHolderSlotCount,
                    new Translator(chinese: "设置法杖收纳槽数量", english: "Set Cane Holder Slot Count"),
                    new Translator(
                        chinese: "设置当前法杖收纳杖的槽位数量。",
                        english: "Set the current cane holder slot count."
                        ),
                    new UiSliderMetadata(0f, 20f, 1f)
                    );
            }
            catch (Exception ex)
            {
                BLog.Error("Failed to initialize control manager for cane.", ex);
            }
        }

        /// <summary>
        /// 绑定法杖属性，统一使用下限 0（各 Set* 方法拒绝负值）、步进 0.1 的滑杆；上限因属性而异（多数为 255，魔力消耗效率为 169）。
        /// </summary>
        private static ControlEntry<float> BindCane(
            string key,
            Func<float> valueGetter,
            Action<float> valueSetter,
            Translator name,
            float sliderMax)
        {
            return Bind(
                SectionCane,
                key,
                valueGetter,
                valueSetter,
                name,
                new Translator(
                    chinese: "设置当前装备法杖的属性值。",
                    english: "Set an attribute of the currently equipped cane."
                    ),
                new UiSliderMetadata(0f, sliderMax, 0.1f)
                );
        }
    }
}
