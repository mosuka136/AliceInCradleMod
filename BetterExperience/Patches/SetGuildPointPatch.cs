using BetterExperience.BConfigManager;
using BetterExperience.BLogSpace;
using BetterExperience.BPatchGUI;
using nel;
using System;
using UnityEngine;
using UnityModBase.HClassAttribute;
using UnityModBase.HTranslatorSpace;

namespace BetterExperience.Patches
{
    public partial class HPatches
    {
        /// <summary>
        /// 一次性设置工会积分；积分也决定工会等级，由原生存档保存。
        /// 工会界面缓存积分且动画会回写，必须等相关对象销毁后才能修改。
        /// </summary>
        public static class SetGuildPointPatch
        {
            private static bool _initialized;
            private static readonly object NoticeOwner = new object();

            internal enum ApplyResult
            {
                Applied,
                Ignored,
                Unavailable,
                GuildNotInitialized,
                InterfaceOpen
            }

            [InitializeOnGameBoot]
            public static void Initialize()
            {
                if (_initialized)
                    return;

                GameSaveLoadManager.OnGameSaveLoadCompleted += () =>
                {
                    if (ConfigManager.SetGuildPoint.Value1)
                        SetGuildPoint(ConfigManager.SetGuildPoint.Value2);
                };
                _initialized = true;
            }

            public static int GetGuildPoint() => GetGuildPoint(GetM2D()?.GUILD);

            internal static int GetGuildPoint(GuildManager guild) => guild?.gq_point ?? -1;

            public static void SetGuildPoint(int point)
            {
                if (point < 0)
                    return;

                try
                {
                    var result = Apply(GetM2D()?.GUILD, point, HasGuildInterface);
                    switch (result)
                    {
                        case ApplyResult.Unavailable:
                            NoticeGUI.Show(new Translator("游戏尚未加载，无法设置工会积分。", "Load a game before setting guild points."), owner: NoticeOwner);
                            break;
                        case ApplyResult.GuildNotInitialized:
                            NoticeGUI.Show(new Translator("工会尚未开启，无法设置工会积分。", "Initialize the guild before setting guild points."), owner: NoticeOwner);
                            break;
                        case ApplyResult.InterfaceOpen:
                            NoticeGUI.Show(new Translator("请关闭工会任务及积分商店界面后重试。", "Close the guild quest and point shop screens, then try again."), owner: NoticeOwner);
                            break;
                        case ApplyResult.Applied:
                            NoticeGUI.Clear(NoticeOwner);
                            BLog.Debug($"Guild points set to {Math.Min(point, GuildManager.GQ_POINT_MAX)}.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    BLog.Error("Failed to set guild points.", ex);
                    NoticeGUI.Show(new Translator("设置工会积分失败，请查看日志。", "Failed to set guild points. Check the log."), owner: NoticeOwner);
                }
            }

            // 仅在提交时扫描；包括暂时隐藏的界面，避免结算或奖励动画覆盖新值。
            private static bool HasGuildInterface()
            {
                return UnityEngine.Object.FindObjectsByType<FillBlockGQPointBox>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length != 0
                    || UnityEngine.Object.FindObjectsByType<UiItemStoreGRank>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length != 0;
            }

            internal static ApplyResult Apply(GuildManager guild, int point, Func<bool> hasGuildInterface)
            {
                if (point < 0)
                    return ApplyResult.Ignored;
                if (guild == null)
                    return ApplyResult.Unavailable;
                if (guild.gq_point < 0)
                    return ApplyResult.GuildNotInitialized;
                if (hasGuildInterface())
                    return ApplyResult.InterfaceOpen;

                guild.gq_point = Math.Min(point, GuildManager.GQ_POINT_MAX);
                return ApplyResult.Applied;
            }
        }
    }
}
