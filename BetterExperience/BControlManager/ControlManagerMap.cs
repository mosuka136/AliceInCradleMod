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
        internal static ControlEntry<int> SetDangerLevel { get; private set; }
        internal static ControlEntry<SummonWanderingNpcKind> SummonWanderingNpcKind { get; private set; }
        internal static ControlEntry<bool> SummonWanderingNpc { get; private set; }

        private const string SectionMap = "Map";

        /// <summary>
        /// 创建地图控制表并绑定地图状态条目。
        /// </summary>
        internal static void InitializeMap()
        {
            try
            {
                BService.Control.CreateTable(
                    SectionMap,
                    new Translator(chinese: "地图", english: "Map"),
                    new Translator(
                        chinese: "查看和修改当前游戏中的地图状态。",
                        english: "View and modify map state in the current game."
                        )
                    );

                SetDangerLevel = Bind(
                    SectionMap,
                    nameof(SetDangerLevel),
                    HPatches.SetDangerLevelPatch.GetDangerLevel,
                    HPatches.SetDangerLevelPatch.SetDangerLevel,
                    new Translator(chinese: "设置危险度", english: "Set Danger Level"),
                    new Translator(
                        chinese: "设置当前游戏的危险度。",
                        english: "Set the danger level in the current game."
                        ),
                    new UiSliderMetadata(0f, 160f, 1f)
                    );
                SummonWanderingNpcKind = Bind(
                    SectionMap,
                    nameof(SummonWanderingNpcKind),
                    HPatches.WanderingNpcForcePatch.GetSummonKind,
                    HPatches.WanderingNpcForcePatch.SetSummonKind,
                    new Translator(chinese: "传唤NPC", english: "Summon NPC"),
                    new Translator(
                        chinese: "选择要传唤的NPC：南丁格尔、咖啡师、提尔德或木偶商人。",
                        english: "Choose the NPC to summon: Nightingale, Coffee Maker, Tilde or Puppet."
                        ),
                    policy: ControlUpdatePolicy.Never
                    );
                SummonWanderingNpc = BindPulse(
                    SectionMap,
                    nameof(SummonWanderingNpc),
                    HPatches.WanderingNpcForcePatch.SetSummon,
                    new Translator(chinese: "传唤到当前地图", english: "Summon To Current Map"),
                    new Translator(
                        chinese: "把所选商人安排到当前地图，切换地图后在长椅点必定出现；剧情未开放或黑名单地图除外。",
                        english: "Assign the selected merchant to the current map; it always appears at a bench after changing maps, except story-locked maps."
                        )
                    );
            }
            catch (Exception ex)
            {
                BLog.Error("Failed to initialize control manager for map.", ex);
            }
        }
    }
}
