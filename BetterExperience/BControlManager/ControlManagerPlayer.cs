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
        internal static ControlEntry<int> SetBackpackCapacity { get; private set; }
        internal static ControlEntry<int> SetBottleHolderCount { get; private set; }
        internal static ControlEntry<int> SetBombHolderSlotCount { get; private set; }
        internal static ControlEntry<int> SetItemReelHolderSlotCount { get; private set; }
        internal static ControlEntry<int> SetPlayerHp { get; private set; }
        internal static ControlEntry<int> SetPlayerMp { get; private set; }
        internal static ControlEntry<int> SetPlayerEp { get; private set; }
        internal static ControlEntry<int> SetPlayerMaxHp { get; private set; }
        internal static ControlEntry<int> SetPlayerMaxMp { get; private set; }
        internal static ControlEntry<int> SetPlayerMaxSatiety { get; private set; }
        internal static ControlEntry<int> SetOverChargeSlotCount { get; private set; }
        internal static ControlEntry<int> SetEnhancerSlotCount { get; private set; }

        private const string SectionPlayer = "Player";

        /// <summary>
        /// 创建玩家控制表并绑定玩家状态条目。
        /// </summary>
        internal static void InitializePlayer()
        {
            try
            {
                BService.Control.CreateTable(
                    SectionPlayer,
                    new Translator(chinese: "玩家", english: "Player"),
                    new Translator(
                        chinese: "查看和修改当前游戏中的玩家状态。",
                        english: "View and modify player state in the current game."
                        )
                    );

                SetBackpackCapacity = Bind(
                    SectionPlayer,
                    nameof(SetBackpackCapacity),
                    HPatches.SetBackpackCapacityPatch.GetBackpackCapacity,
                    HPatches.SetBackpackCapacityPatch.SetBackpackCapacity,
                    new Translator(chinese: "设置背包容量", english: "Set Backpack Capacity"),
                    new Translator(
                        chinese: "设置当前背包容量。",
                        english: "Set the current backpack capacity."
                        ),
                    new UiSliderMetadata(0f, 200f, 1f)
                    );
                SetBottleHolderCount = Bind(
                    SectionPlayer,
                    nameof(SetBottleHolderCount),
                    HPatches.SetBottleHolderCountPatch.GetBottleHolderCount,
                    HPatches.SetBottleHolderCountPatch.SetBottleHolderCount,
                    new Translator(chinese: "设置空瓶收纳数量", english: "Set Bottle Holder Count"),
                    new Translator(
                        chinese: "设置当前空瓶收纳槽位数量。",
                        english: "Set the current bottle holder count."
                        ),
                    new UiSliderMetadata(0f, 50f, 1f)
                    );
                SetBombHolderSlotCount = Bind(
                    SectionPlayer,
                    nameof(SetBombHolderSlotCount),
                    HPatches.SetBombHolderSlotCountPatch.GetBombHolderSlotCount,
                    HPatches.SetBombHolderSlotCountPatch.SetBombHolderSlotCount,
                    new Translator(chinese: "设置手雷收纳槽数量", english: "Set Bomb Holder Slot Count"),
                    new Translator(
                        chinese: "设置当前手雷收纳槽数量。",
                        english: "Set the current bomb holder slot count."
                        ),
                    new UiSliderMetadata(0f, 20f, 1f)
                    );
                SetItemReelHolderSlotCount = Bind(
                    SectionPlayer,
                    nameof(SetItemReelHolderSlotCount),
                    HPatches.SetItemReelHolderSlotCountPatch.GetItemReelHolderSlotCount,
                    HPatches.SetItemReelHolderSlotCountPatch.SetItemReelHolderSlotCount,
                    new Translator(chinese: "设置宝箱收纳槽数量", english: "Set Item Reel Holder Slot Count"),
                    new Translator(
                        chinese: "设置当前宝箱收纳槽数量。",
                        english: "Set the current item reel holder slot count."
                        ),
                    new UiSliderMetadata(0f, 20f, 1f)
                    );
                SetPlayerHp = BindPlayerValue(
                    nameof(SetPlayerHp),
                    HPatches.SetHpMpEpPatch.GetHp,
                    HPatches.SetHpMpEpPatch.SetHp,
                    new Translator(chinese: "设置玩家 HP", english: "Set Player HP"),
                    0f
                    );
                SetPlayerMp = BindPlayerValue(
                    nameof(SetPlayerMp),
                    HPatches.SetHpMpEpPatch.GetMp,
                    HPatches.SetHpMpEpPatch.SetMp,
                    new Translator(chinese: "设置玩家 MP", english: "Set Player MP"),
                    0f
                    );
                SetPlayerEp = BindPlayerValue(
                    nameof(SetPlayerEp),
                    HPatches.SetHpMpEpPatch.GetEp,
                    HPatches.SetHpMpEpPatch.SetEp,
                    new Translator(chinese: "设置玩家 EP", english: "Set Player EP"),
                    0f
                    );
                SetPlayerMaxHp = BindPlayerValue(
                    nameof(SetPlayerMaxHp),
                    HPatches.SetHpMpEpPatch.GetMaxHp,
                    HPatches.SetHpMpEpPatch.SetMaxHp,
                    new Translator(chinese: "设置玩家最大 HP", english: "Set Player Max HP"),
                    1f
                    );
                SetPlayerMaxMp = BindPlayerValue(
                    nameof(SetPlayerMaxMp),
                    HPatches.SetHpMpEpPatch.GetMaxMp,
                    HPatches.SetHpMpEpPatch.SetMaxMp,
                    new Translator(chinese: "设置玩家最大 MP", english: "Set Player Max MP"),
                    1f
                    );
                SetPlayerMaxSatiety = Bind(
                    SectionPlayer,
                    nameof(SetPlayerMaxSatiety),
                    HPatches.SetMaxSatietyPatch.GetMaxSatiety,
                    HPatches.SetMaxSatietyPatch.SetMaxSatiety,
                    new Translator(chinese: "设置玩家最大饱食度", english: "Set Player Max Satiety"),
                    new Translator(
                        chinese: "设置当前玩家最大饱食度。",
                        english: "Set the current player max satiety."
                        ),
                    new UiSliderMetadata(1f, 100f, 1f)
                    );
                SetOverChargeSlotCount = Bind(
                    SectionPlayer,
                    nameof(SetOverChargeSlotCount),
                    HPatches.SetOverChargeSlotCountPatch.GetOverChargeSlotCount,
                    HPatches.SetOverChargeSlotCountPatch.SetOverChargeSlotCount,
                    new Translator(chinese: "设置过充插槽数量", english: "Set Over Charge Slot Count"),
                    new Translator(
                        chinese: "设置当前过充插槽数量。",
                        english: "Set the current overcharge slot count."
                        ),
                    new UiSliderMetadata(0f, 10f, 1f)
                    );
                SetEnhancerSlotCount = Bind(
                    SectionPlayer,
                    nameof(SetEnhancerSlotCount),
                    HPatches.SetEnhancerSlotCountPatch.GetEnhancerSlotCount,
                    HPatches.SetEnhancerSlotCountPatch.SetEnhancerSlotCount,
                    new Translator(chinese: "设置强化插槽数量", english: "Set Enhancer Slot Count"),
                    new Translator(
                        chinese: "设置当前强化插槽数量。",
                        english: "Set the current enhancer slot count."
                        ),
                    new UiSliderMetadata(0f, 20f, 1f)
                    );
            }
            catch (Exception ex)
            {
                BLog.Error("Failed to initialize control manager for player.", ex);
            }
        }

        /// <summary>
        /// 绑定玩家 HP/MP/EP 等数值属性，统一使用步进 1 的滑杆；
        /// 滑杆下限取对应 Set* 方法接受的最小有效值（当前值 0、最大值 1）。
        /// </summary>
        private static ControlEntry<int> BindPlayerValue(
            string key,
            Func<int> valueGetter,
            Action<int> valueSetter,
            Translator name,
            float sliderMin)
        {
            return Bind(
                SectionPlayer,
                key,
                valueGetter,
                valueSetter,
                name,
                new Translator(
                    chinese: "设置当前玩家属性值。",
                    english: "Set the current player attribute value."
                    ),
                new UiSliderMetadata(sliderMin, 1000f, 1f)
                );
        }
    }
}
