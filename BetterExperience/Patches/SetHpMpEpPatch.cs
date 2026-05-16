using BetterExperience.BConfigManager;
using BetterExperience.HClassAttribute;
using HarmonyLib;
using nel;
using System;
using UnityEngine;

namespace BetterExperience.Patches
{
    public partial class HPatches
    {
        /// <summary>
        /// 设置玩家 HP、MP、EP 及最大 HP/MP。
        /// 当前实现直接改写玩家实例私有字段，并调用游戏刷新方法同步 UI 或派生状态。
        /// </summary>
        [HarmonyPatch]
        public class SetHpMpEpPatch
        {
            private static bool _initialized = false;

            [InitializeOnGameBoot]
            public static void Initialize()
            {
                if (_initialized)
                    return;

                GameSaveLoadManager.OnGameSaveLoadCompleted += () =>
                {
                    if (ConfigManager.EnablePreloadPlayerHp.Value)
                    {
                        HLog.Debug($"Applying preloaded HP: {ConfigManager.SetPlayerHp.Value}");
                        SetHp(ConfigManager.SetPlayerHp.Value);
                    }

                    if (ConfigManager.EnablePreloadPlayerMp.Value)
                    {
                        HLog.Debug($"Applying preloaded MP: {ConfigManager.SetPlayerMp.Value}");
                        SetMp(ConfigManager.SetPlayerMp.Value);
                    }

                    if (ConfigManager.EnablePreloadPlayerEp.Value)
                    {
                        HLog.Debug($"Applying preloaded EP: {ConfigManager.SetPlayerEp.Value}");
                        SetEp(ConfigManager.SetPlayerEp.Value);
                    }

                    if (ConfigManager.EnablePreloadPlayerMaxHp.Value)
                    {
                        HLog.Debug($"Applying preloaded max HP: {ConfigManager.SetPlayerMaxHp.Value}");
                        SetMaxHp(ConfigManager.SetPlayerMaxHp.Value);
                    }

                    if (ConfigManager.EnablePreloadPlayerMaxMp.Value)
                    {
                        HLog.Debug($"Applying preloaded max MP: {ConfigManager.SetPlayerMaxMp.Value}");
                        SetMaxMp(ConfigManager.SetPlayerMaxMp.Value);
                    }
                };

                ConfigManager.SetPlayerHp.OnValueChanged += (s, e) =>
                {
                    HLog.Debug($"Player HP config changed: {e}");
                    SetHp(e);
                };
                ConfigManager.SetPlayerMp.OnValueChanged += (s, e) =>
                {
                    HLog.Debug($"Player MP config changed: {e}");
                    SetMp(e);
                };
                ConfigManager.SetPlayerEp.OnValueChanged += (s, e) =>
                {
                    HLog.Debug($"Player EP config changed: {e}");
                    SetEp(e);
                };
                ConfigManager.SetPlayerMaxHp.OnValueChanged += (s, e) =>
                {
                    HLog.Debug($"Player max HP config changed: {e}");
                    SetMaxHp(e);
                };
                ConfigManager.SetPlayerMaxMp.OnValueChanged += (s, e) =>
                {
                    HLog.Debug($"Player max MP config changed: {e}");
                    SetMaxMp(e);
                };

                _initialized = true;
                HLog.Debug("HP/MP/EP patch initialized.");
            }

            public static void SetHp(int hp)
            {
                try
                {
                    if (hp < 0)
                    {
                        HLog.Debug($"Ignored invalid HP value: {hp}");
                        return;
                    }

                    var pr = UnityEngine.Object.FindAnyObjectByType<PR>();
                    if (pr == null)
                    {
                        HLog.Notice("Player instance not found while applying HP.");
                        return;
                    }

                    var prTraverse = Traverse.Create(pr);
                    if (prTraverse == null)
                    {
                        HLog.Notice("Player traverse not found while applying HP.");
                        return;
                    }

                    hp = Mathf.Min(hp, prTraverse.Field("maxhp").GetValue<int>());

                    prTraverse.Field("hp").SetValue(hp);
                    pr.cureHp(0);

                    HLog.Debug($"Player HP set to {hp}");
                }
                catch (Exception ex)
                {
                    HLog.Error($"Unexpected error in {nameof(SetHp)}.", ex);
                }
            }

            public static void SetMp(int mp)
            {
                try
                {
                    if (mp < 0)
                    {
                        HLog.Debug($"Ignored invalid MP value: {mp}");
                        return;
                    }

                    var pr = UnityEngine.Object.FindAnyObjectByType<PR>();
                    if (pr == null)
                    {
                        HLog.Notice("Player instance not found while applying MP.");
                        return;
                    }

                    var prTraverse = Traverse.Create(pr);
                    if (prTraverse == null)
                    {
                        HLog.Notice("Player traverse not found while applying MP.");
                        return;
                    }

                    mp = Mathf.Min(mp, prTraverse.Field("maxmp").GetValue<int>());

                    prTraverse.Field("mp").SetValue(mp);
                    pr.cureMp(0);

                    HLog.Debug($"Player MP set to {mp}");
                }
                catch (Exception ex)
                {
                    HLog.Error($"Unexpected error in {nameof(SetMp)}.", ex);
                }
            }

            public static void SetEp(int ep)
            {
                try
                {
                    if (ep < 0)
                    {
                        HLog.Debug($"Ignored invalid EP value: {ep}");
                        return;
                    }

                    var pr = UnityEngine.Object.FindAnyObjectByType<PR>();
                    if (pr == null)
                    {
                        HLog.Notice("Player instance not found while applying EP.");
                        return;
                    }

                    var prTraverse = Traverse.Create(pr);
                    if (prTraverse == null)
                    {
                        HLog.Notice("Player traverse not found while applying EP.");
                        return;
                    }

                    prTraverse.Field("ep").SetValue(ep);
                    // EP 显示不随字段写入自动刷新，需要通知原游戏计数器重算。
                    pr.EpCon.fineCounter();

                    HLog.Debug($"Player EP set to {ep}");
                }
                catch (Exception ex)
                {
                    HLog.Error($"Unexpected error in {nameof(SetEp)}.", ex);
                }
            }

            public static void SetMaxHp(int maxHp)
            {
                try
                {
                    if (maxHp <= 0)
                    {
                        HLog.Debug($"Ignored invalid max HP value: {maxHp}");
                        return;
                    }

                    var pr = UnityEngine.Object.FindAnyObjectByType<PR>();
                    if (pr == null)
                    {
                        HLog.Notice("Player instance not found while applying max HP.");
                        return;
                    }

                    var prTraverse = Traverse.Create(pr);
                    if (prTraverse == null)
                    {
                        HLog.Notice("Player traverse not found while applying max HP.");
                        return;
                    }

                    prTraverse.Field("maxhp").SetValue(maxHp);
                    // 最大 HP 变化会影响异常状态相关派生值，先让游戏检查状态再刷新当前 HP。
                    pr.Ser.checkSer();
                    pr.cureHp(0);

                    HLog.Debug($"Player max HP set to {maxHp}");
                }
                catch (Exception ex)
                {
                    HLog.Error($"Unexpected error in {nameof(SetMaxHp)}.", ex);
                }
            }

            public static void SetMaxMp(int maxMp)
            {
                try
                {
                    if (maxMp <= 0)
                    {
                        HLog.Debug($"Ignored invalid max MP value: {maxMp}");
                        return;
                    }

                    var pr = UnityEngine.Object.FindAnyObjectByType<PR>();
                    if (pr == null)
                    {
                        HLog.Notice("Player instance not found while applying max MP.");
                        return;
                    }

                    var prTraverse = Traverse.Create(pr);
                    if (prTraverse == null)
                    {
                        HLog.Notice("Player traverse not found while applying max MP.");
                        return;
                    }

                    prTraverse.Field("maxmp").SetValue(maxMp);
                    pr.cureMp(0);

                    HLog.Debug($"Player max MP set to {maxMp}");
                }
                catch (Exception ex)
                {
                    HLog.Error($"Unexpected error in {nameof(SetMaxMp)}.", ex);
                }
            }
        }
    }
}
