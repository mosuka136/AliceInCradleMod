using BetterExperience.BLogSpace;
using BetterExperience.Patches;
using System;
using System.Collections.Generic;
using UnityModBase.HControlSpace;
using UnityModBase.HGuiSpace;
using UnityModBase.HTranslatorSpace;

namespace BetterExperience.BControlManager
{
    internal static partial class ControlManager
    {
        internal static ControlEntry<GiveCatalogKind> GiveKind { get; private set; }
        internal static ControlEntry<string> GiveFilter { get; private set; }
        internal static ControlEntry<List<(string Display, bool Selected)>> GiveChoices { get; private set; }
        internal static ControlEntry<int> GiveItemGrade { get; private set; }
        internal static ControlEntry<int> GiveItemCount { get; private set; }
        internal static ControlEntry<bool> GiveItem { get; private set; }

        private const string SectionDebug = "Debug";

        /// <summary>
        /// 创建给予控制表并绑定给予/解锁条目。
        /// </summary>
        internal static void InitializeGive()
        {
            try
            {
                BService.Control.CreateTable(
                    SectionDebug,
                    new Translator(chinese: "给予", english: "Give"),
                    new Translator(
                        chinese: "从列表中选择物品、技能或配方后给予或解锁。未读档时无效。",
                        english: "Choose an item, skill or recipe from the list, then give or unlock it. Does nothing until a save is loaded."
                        )
                    );

                GiveKind = Bind(
                    SectionDebug,
                    nameof(GiveKind),
                    HPatches.DebugGive.GetKind,
                    HPatches.DebugGive.SetKind,
                    new Translator(chinese: "给予目录", english: "Give Catalog"),
                    new Translator(
                        chinese: "选择物品、技能或配方目录。切换后请在下面的列表中勾选一项。",
                        english: "Choose the item, skill or recipe catalog, then tick one entry in the list below."
                        ),
                    policy: ControlUpdatePolicy.Never
                    );
                GiveFilter = Bind(
                    SectionDebug,
                    nameof(GiveFilter),
                    HPatches.DebugGive.GetFilter,
                    HPatches.DebugGive.SetFilter,
                    new Translator(chinese: "名称筛选", english: "Name Filter"),
                    new Translator(
                        chinese: "按显示名称筛选列表，可留空。",
                        english: "Filter the list by display name. Leave empty to show all."
                        ),
                    policy: ControlUpdatePolicy.Never
                    );
                GiveChoices = Bind(
                    SectionDebug,
                    nameof(GiveChoices),
                    HPatches.DebugGive.GetChoices,
                    HPatches.DebugGive.SetChoices,
                    new Translator(chinese: "选择条目", english: "Select Entry"),
                    new Translator(
                        chinese: "展开后勾选要给予或解锁的条目。勾选另一项会切换选择，取消勾选即清空。读档后打开本页才会列出游戏数据。",
                        english: "Expand and tick the entry to give or unlock. Ticking another entry switches the selection; unticking clears it. Open this page after loading a save so the game data is listed."
                        ),
                    policy: ControlUpdatePolicy.WhenVisibleEverySecond
                    );
                GiveItemGrade = Bind(
                    SectionDebug,
                    nameof(GiveItemGrade),
                    HPatches.DebugGive.GetGiveItemGrade,
                    HPatches.DebugGive.SetGiveItemGrade,
                    new Translator(chinese: "给予物品品级", english: "Give Item Grade"),
                    new Translator(
                        chinese: "仅物品目录有效。品级 0–4。独立品级物品会强制为 0。",
                        english: "Items only. Grade 0–4. Individual-grade items are forced to 0."
                        ),
                    new UiSliderMetadata(0f, 4f, 1f),
                    ControlUpdatePolicy.Never
                    );
                GiveItemCount = Bind(
                    SectionDebug,
                    nameof(GiveItemCount),
                    HPatches.DebugGive.GetGiveItemCount,
                    HPatches.DebugGive.SetGiveItemCount,
                    new Translator(chinese: "给予物品数量", english: "Give Item Count"),
                    new Translator(
                        chinese: "仅物品目录有效。一次给予 1–99。",
                        english: "Items only. Give 1–99 at a time."
                        ),
                    new UiSliderMetadata(1f, 99f, 1f),
                    ControlUpdatePolicy.Never
                    );
                GiveItem = BindPulse(
                    SectionDebug,
                    nameof(GiveItem),
                    HPatches.DebugGive.SetGive,
                    new Translator(chinese: "给予或解锁", english: "Give or Unlock"),
                    new Translator(
                        chinese: "打开后按当前目录和勾选项执行：物品掉在脚边并拾取，技能直接解锁，配方会揭示并给予配方物品。",
                        english: "Turn on to apply the ticked entry: items drop at your feet, skills unlock, recipes are revealed and granted."
                        )
                    );
            }
            catch (Exception ex)
            {
                BLog.Error("Failed to initialize control manager for give.", ex);
            }
        }
    }
}
