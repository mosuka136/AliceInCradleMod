using BetterExperience.BLogSpace;
using BetterExperience.BPatchGUI;
using nel;
using System;
using UnityModBase.HTranslatorSpace;

namespace BetterExperience.Patches
{
    public partial class HPatches
    {
        /// <summary>
        /// 把选中的流浪商人安排到当前地图。
        /// 与原版调试命令 debugFlush 使用相同序列：清空游走状态、出现率设为 1、
        /// 把所在地图固定为当前地图；下一次加载该地图时必定在长椅点出现。
        /// 剧情未解锁或地图黑名单仍按原版判定生效。
        /// </summary>
        public static class WanderingNpcForcePatch
        {
            internal static readonly object NoticeOwner = new object();

            private static SummonWanderingNpcKind _kind;

            internal static SummonWanderingNpcKind GetSummonKind() => _kind;

            internal static void SetSummonKind(SummonWanderingNpcKind value)
            {
                _kind = value;
            }

            internal static bool GetSummon() => false;

            internal static void SetSummon(bool value)
            {
                if (!value)
                    return;

                Summon(_kind);
            }

            internal static void Summon(SummonWanderingNpcKind kind)
            {
                try
                {
                    var m2d = GetM2D();
                    if (!WanderingNpcForceLogic.ShouldSummon(m2d?.WDR != null && m2d.curMap != null))
                    {
                        Notice(new Translator(chinese: "未进入游戏或尚未读档。", english: "Not in game or no save loaded."));
                        return;
                    }

                    var npc = m2d.WDR.Get(WanderingNpcForceLogic.ToGameType(kind));
                    npc.flush();
                    npc.appear_ratio = 1f;
                    npc.setToHere();

                    BLog.Debug($"{nameof(WanderingNpcForcePatch)} applied for {kind}.");
                    Notice(new Translator(
                        chinese: "已把商人安排到当前地图，切换地图后出现；剧情未开放或黑名单地图除外。",
                        english: "The merchant is assigned to the current map and appears after changing maps, except story-locked maps."
                        ));
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(Summon)}.", ex);
                }
            }

            private static void Notice(Translator message)
            {
                NoticeGUI.Show(message, owner: NoticeOwner);
            }
        }
    }
}
