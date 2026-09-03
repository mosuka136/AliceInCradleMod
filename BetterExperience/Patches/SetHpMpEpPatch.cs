using BetterExperience.BConfigManager;
using UnityModBase.HClassAttribute;
using BetterExperience.BLogSpace;
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
                    if (ConfigManager.SetPlayerHp.Value1)
                    {
                        BLog.Debug($"Applying preloaded HP: {ConfigManager.SetPlayerHp.Value2}");
                        SetHp(ConfigManager.SetPlayerHp.Value2);
                    }

                    if (ConfigManager.SetPlayerMp.Value1)
                    {
                        BLog.Debug($"Applying preloaded MP: {ConfigManager.SetPlayerMp.Value2}");
                        SetMp(ConfigManager.SetPlayerMp.Value2);
                    }

                    if (ConfigManager.SetPlayerEp.Value1)
                    {
                        BLog.Debug($"Applying preloaded EP: {ConfigManager.SetPlayerEp.Value2}");
                        SetEp(ConfigManager.SetPlayerEp.Value2);
                    }

                    if (ConfigManager.SetPlayerMaxHp.Value1)
                    {
                        BLog.Debug($"Applying preloaded max HP: {ConfigManager.SetPlayerMaxHp.Value2}");
                        SetMaxHp(ConfigManager.SetPlayerMaxHp.Value2);
                    }

                    if (ConfigManager.SetPlayerMaxMp.Value1)
                    {
                        BLog.Debug($"Applying preloaded max MP: {ConfigManager.SetPlayerMaxMp.Value2}");
                        SetMaxMp(ConfigManager.SetPlayerMaxMp.Value2);
                    }
                };

                _initialized = true;
                BLog.Debug("HP/MP/EP patch initialized.");
            }

            public static void SetHp(int hp)
            {
                try
                {
                    if (hp < 0)
                    {
                        BLog.Debug($"Ignored invalid HP value: {hp}");
                        return;
                    }

                    PR pr;
                    Traverse prTraverse;
                    if (!TryGetPRTraverse(out pr, out prTraverse))
                    {
                        BLog.Notice("Player instance not found while applying HP.");
                        return;
                    }

                    hp = Mathf.Min(hp, prTraverse.Field("maxhp").GetValue<int>());

                    prTraverse.Field("hp").SetValue(hp);
                    pr.cureHp(0);

                    BLog.Debug($"Player HP set to {hp}");
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(SetHp)}.", ex);
                }
            }

            public static void SetMp(int mp)
            {
                try
                {
                    if (mp < 0)
                    {
                        BLog.Debug($"Ignored invalid MP value: {mp}");
                        return;
                    }

                    PR pr;
                    Traverse prTraverse;
                    if (!TryGetPRTraverse(out pr, out prTraverse))
                    {
                        BLog.Notice("Player instance not found while applying MP.");
                        return;
                    }

                    mp = Mathf.Min(mp, prTraverse.Field("maxmp").GetValue<int>());

                    prTraverse.Field("mp").SetValue(mp);
                    pr.cureMp(0);

                    BLog.Debug($"Player MP set to {mp}");
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(SetMp)}.", ex);
                }
            }

            public static void SetEp(int ep)
            {
                try
                {
                    if (ep < 0)
                    {
                        BLog.Debug($"Ignored invalid EP value: {ep}");
                        return;
                    }

                    PR pr;
                    Traverse prTraverse;
                    if (!TryGetPRTraverse(out pr, out prTraverse))
                    {
                        BLog.Notice("Player instance not found while applying EP.");
                        return;
                    }

                    prTraverse.Field("ep").SetValue(ep);
                    // EP 显示不随字段写入自动刷新，需要通知原游戏计数器重算。
                    pr.EpCon.fineCounter();

                    BLog.Debug($"Player EP set to {ep}");
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(SetEp)}.", ex);
                }
            }

            public static void SetMaxHp(int maxHp)
            {
                try
                {
                    if (maxHp <= 0)
                    {
                        BLog.Debug($"Ignored invalid max HP value: {maxHp}");
                        return;
                    }

                    PR pr;
                    Traverse prTraverse;
                    if (!TryGetPRTraverse(out pr, out prTraverse))
                    {
                        BLog.Notice("Player instance not found while applying max HP.");
                        return;
                    }

                    prTraverse.Field("maxhp").SetValue(maxHp);
                    // 最大 HP 变化会影响异常状态相关派生值，先让游戏检查状态再刷新当前 HP。
                    pr.Ser.checkSer();
                    pr.cureHp(0);

                    BLog.Debug($"Player max HP set to {maxHp}");
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(SetMaxHp)}.", ex);
                }
            }

            public static void SetMaxMp(int maxMp)
            {
                try
                {
                    if (maxMp <= 0)
                    {
                        BLog.Debug($"Ignored invalid max MP value: {maxMp}");
                        return;
                    }

                    PR pr;
                    Traverse prTraverse;
                    if (!TryGetPRTraverse(out pr, out prTraverse))
                    {
                        BLog.Notice("Player instance not found while applying max MP.");
                        return;
                    }

                    prTraverse.Field("maxmp").SetValue(maxMp);
                    pr.cureMp(0);

                    BLog.Debug($"Player max MP set to {maxMp}");
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(SetMaxMp)}.", ex);
                }
            }

            public static int GetHp()
            {
                return GetPlayerValue("hp");
            }

            public static int GetMp()
            {
                return GetPlayerValue("mp");
            }

            public static int GetEp()
            {
                return GetPlayerValue("ep");
            }

            public static int GetMaxHp()
            {
                return GetPlayerValue("maxhp");
            }

            public static int GetMaxMp()
            {
                return GetPlayerValue("maxmp");
            }

            private static int GetPlayerValue(string fieldName)
            {
                return GetPRFieldValue(fieldName, -1);
            }
        }
    }
}
