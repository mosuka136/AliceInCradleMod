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
        /// 设置当前玩家法杖的显示属性或内部属性。
        /// 部分方法接收的是面板显示值，需要按游戏显示公式反推到 <see cref="PrCaneEquip"/> 内部字段。
        /// </summary>
        [HarmonyPatch]
        public class SetCaneAttributePatch
        {
            // PR.Skill 中当前装备法杖字段名，游戏类未公开强类型访问器时通过 Harmony Traverse 读写。
            private const string EqCane = "EqCane";

            private static bool _initialized = false;

            [InitializeOnGameBoot]
            public static void Initialize()
            {
                if (_initialized)
                    return;

                GameSaveLoadManager.OnGameSaveLoadCompleted += () =>
                {
                    if (ConfigManager.SetCaneSwingSpeed.Value1)
                        SetSwingSpeed(ConfigManager.SetCaneSwingSpeed.Value2);

                    if (ConfigManager.SetCaneCastSpeed.Value1)
                        SetCastSpeed(ConfigManager.SetCaneCastSpeed.Value2);

                    if (ConfigManager.SetCaneBalance.Value1)
                        SetBalance(ConfigManager.SetCaneBalance.Value2);

                    if (ConfigManager.SetCaneEfficiency.Value1)
                        SetEfficiency(ConfigManager.SetCaneEfficiency.Value2);

                    if (ConfigManager.SetCaneRetention.Value1)
                        SetRetention(ConfigManager.SetCaneRetention.Value2);

                    if (ConfigManager.SetCaneLockOn.Value1)
                        SetLockOn(ConfigManager.SetCaneLockOn.Value2);

                    if (ConfigManager.SetCaneLongRange.Value1)
                        SetLongRange(ConfigManager.SetCaneLongRange.Value2);

                    if (ConfigManager.SetCaneShortRange.Value1)
                        SetShortRange(ConfigManager.SetCaneShortRange.Value2);

                    if (ConfigManager.SetCaneReach.Value1)
                        SetReach(ConfigManager.SetCaneReach.Value2);

                    if (ConfigManager.SetCaneNearPower.Value1)
                        SetNearPower(ConfigManager.SetCaneNearPower.Value2);

                    if (ConfigManager.SetCaneNearShotgunPower.Value1)
                        SetNearShotgunPower(ConfigManager.SetCaneNearShotgunPower.Value2);

                    if (ConfigManager.SetCaneStability.Value1)
                        SetStability(ConfigManager.SetCaneStability.Value2);

                    if (ConfigManager.SetCaneManaSplashRatio.Value1)
                        SetManaSplashRatio(ConfigManager.SetCaneManaSplashRatio.Value2);

                    if (ConfigManager.SetCaneCastspeedOverhold.Value1)
                        SetCastspeedOverhold(ConfigManager.SetCaneCastspeedOverhold.Value2);

                    if (ConfigManager.SetCaneDrainAfterLock.Value1)
                        SetDrainAfterLock(ConfigManager.SetCaneDrainAfterLock.Value2);

                    if (ConfigManager.SetCaneCastspeed.Value1)
                        SetCastspeed(ConfigManager.SetCaneCastspeed.Value2);

                    if (ConfigManager.SetCaneMagicPrepareSpeed.Value1)
                        SetMagicPrepareSpeed(ConfigManager.SetCaneMagicPrepareSpeed.Value2);
                };

                _initialized = true;

                BLog.Debug("Cane attribute patch initialized.");
            }

            public static void SetSwingSpeed(float speed)
            {
                try
                {
                    if (speed < 0f)
                    {
                        BLog.Debug($"Ignored invalid swing speed: {speed}");
                        return;
                    }

                    var skill = GetPRSkillTraverse();
                    if (skill == null)
                    {
                        BLog.Notice("Failed to find PR skill for setting cane swing speed.");
                        return;
                    }

                    var cane = skill.Field(EqCane).GetValue<PrCaneEquip>();
                    cane.near_punch_speed = speed / 50f;
                    skill.Field(EqCane).SetValue(cane);

                    BLog.Debug($"Cane swing speed set to: {speed}");
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(SetSwingSpeed)}.", ex);
                }
            }

            public static void SetCastSpeed(float speed)
            {
                try
                {
                    if (speed < 0f)
                    {
                        BLog.Debug($"Ignored invalid caneCast speed: {speed}");
                        return;
                    }

                    var skill = GetPRSkillTraverse();
                    if (skill == null)
                    {
                        BLog.Notice("Failed to find PR skill for setting cane swing speed.");
                        return;
                    }

                    var cane = skill.Field(EqCane).GetValue<PrCaneEquip>();
                    // Cast Speed 面板值由 castspeed、magic_prepare_speed 和 castspeed_overhold 共同决定。
                    var dividend = 50f * (0.33f * (cane.magic_prepare_speed - 1f) + 1f) * (0.25f * (cane.castspeed_overhold - 1f) + 1f);
                    if (dividend == 0f)
                    {
                        BLog.Notice($"Calculated dividend for cast speed is zero, cannot set cast speed. (magic_prepare_speed: {cane.magic_prepare_speed}, castspeed_overhold: {cane.castspeed_overhold})");
                        return;
                    }
                    cane.castspeed = speed / dividend;
                    skill.Field(EqCane).SetValue(cane);

                    BLog.Debug($"Cane cast speed set to: {cane.castspeed})");
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(SetCastSpeed)}.", ex);
                }
            }

            public static void SetBalance(float balance)
            {
                try
                {
                    if (balance < 0f)
                    {
                        BLog.Debug($"Ignored invalid cane balance: {balance}");
                        return;
                    }

                    var skill = GetPRSkillTraverse();
                    if (skill == null)
                    {
                        BLog.Notice("Failed to find PR skill for setting cane swing speed.");
                        return;
                    }

                    var cane = skill.Field(EqCane).GetValue<PrCaneEquip>();
                    cane.neutral = balance / 60f;
                    skill.Field(EqCane).SetValue(cane);

                    BLog.Debug($"Cane balance set to: {balance}");
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(SetBalance)}.", ex);
                }
            }

            public static void SetEfficiency(float efficiency)
            {
                try
                {
                    if (efficiency < 0f)
                    {
                        BLog.Debug($"Ignored invalid cane efficiency: {efficiency}");
                        return;
                    }

                    var skill = GetPRSkillTraverse();
                    if (skill == null)
                    {
                        BLog.Notice("Failed to find PR skill for setting cane swing speed.");
                        return;
                    }

                    var cane = skill.Field(EqCane).GetValue<PrCaneEquip>();
                    // Efficiency 显示值是 mp_use_ratio 的分段函数，这里按原公式反解。
                    if (efficiency == 0f)
                        cane.mp_use_ratio = 12f;
                    else if (efficiency <= 65f)
                        cane.mp_use_ratio = Mathf.Sqrt(65f / efficiency);
                    else if (efficiency <= 169f)
                        cane.mp_use_ratio = (float)((169.0 - efficiency) / 104.0);
                    else
                        cane.mp_use_ratio = 0f;
                    skill.Field(EqCane).SetValue(cane);

                    BLog.Debug($"Cane efficiency set to: {efficiency} (mp_use_ratio: {cane.mp_use_ratio})");
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(SetEfficiency)}.", ex);
                }
            }

            public static void SetRetention(float retention)
            {
                try
                {
                    if (retention < 0f)
                    {
                        BLog.Debug($"Ignored invalid cane retention: {retention}");
                        return;
                    }

                    var skill = GetPRSkillTraverse();
                    if (skill == null)
                    {
                        BLog.Notice("Failed to find PR skill for setting cane swing speed.");
                        return;
                    }

                    var cane = skill.Field(EqCane).GetValue<PrCaneEquip>();
                    var dividend = 55f * cane.mana_splash_ratio * (0.75f * (cane.castspeed_overhold - 1f) + 1f) * (0.5f * (cane.drain_after_lock - 1f) + 1f);
                    if (dividend == 0f)
                    {
                        BLog.Notice($"Calculated dividend for cane retention is zero, cannot set retention. (mana_splash_ratio: {cane.mana_splash_ratio}, castspeed_overhold: {cane.castspeed_overhold}, drain_after_lock: {cane.drain_after_lock})");
                        return;
                    }
                    // Retention 面板值还依赖其他法杖内部属性，因此只改写 stability 分量。
                    cane.stability = retention / dividend;
                    skill.Field(EqCane).SetValue(cane);

                    BLog.Debug($"Cane retention set to: {retention} (stability: {cane.stability})");
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(SetRetention)}.", ex);
                }
            }

            public static void SetLockOn(float lockOn)
            {
                try
                {
                    if (lockOn < 0f)
                    {
                        BLog.Debug($"Ignored invalid cane lock-on: {lockOn}");
                        return;
                    }

                    var skill = GetPRSkillTraverse();
                    if (skill == null)
                    {
                        BLog.Notice("Failed to find PR skill for setting cane swing speed.");
                        return;
                    }

                    var cane = skill.Field(EqCane).GetValue<PrCaneEquip>();
                    cane.lockon_power = lockOn / 50f;
                    skill.Field(EqCane).SetValue(cane);

                    BLog.Debug($"Cane lock-on set to: {lockOn}");
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(SetLockOn)}.", ex);
                }
            }

            public static void SetLongRange(float range)
            {
                try
                {
                    if (range < 0f)
                    {
                        BLog.Debug($"Ignored invalid cane long range: {range}");
                        return;
                    }

                    var skill = GetPRSkillTraverse();
                    if (skill == null)
                    {
                        BLog.Notice("Failed to find PR skill for setting cane swing speed.");
                        return;
                    }

                    var cane = skill.Field(EqCane).GetValue<PrCaneEquip>();
                    cane.far_power = range / 46f;
                    skill.Field(EqCane).SetValue(cane);

                    BLog.Debug($"Cane long range set to: {range}");
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(SetLongRange)}.", ex);
                }
            }

            public static void SetShortRange(float range)
            {
                try
                {
                    if (range < 0f)
                    {
                        BLog.Debug($"Ignored invalid cane short range: {range}");
                        return;
                    }

                    var skill = GetPRSkillTraverse();
                    if (skill == null)
                    {
                        BLog.Notice("Failed to find PR skill for setting cane swing speed.");
                        return;
                    }

                    var cane = skill.Field(EqCane).GetValue<PrCaneEquip>();
                    // Short Range 由 near_power 与 near_shotgun_power 共同决定，保持 shotgun 分量不变。
                    var dividend = 55f * cane.near_shotgun_power;
                    if (dividend == 0f)
                    {
                        BLog.Notice($"Calculated dividend for cane short range is zero, cannot set short range. (near_shotgun_power: {cane.near_shotgun_power})");
                        return;
                    }
                    cane.near_power = 4f * (range / dividend - 1f) + 1f;
                    skill.Field(EqCane).SetValue(cane);

                    BLog.Debug($"Cane short range set to: {range} (near_power: {cane.near_power})");
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(SetShortRange)}.", ex);
                }
            }

            public static void SetReach(float reach)
            {
                try
                {
                    if (reach < 0f)
                    {
                        BLog.Debug($"Ignored invalid cane reach: {reach}");
                        return;
                    }

                    var skill = GetPRSkillTraverse();
                    if (skill == null)
                    {
                        BLog.Notice("Failed to find PR skill for setting cane swing speed.");
                        return;
                    }

                    var cane = skill.Field(EqCane).GetValue<PrCaneEquip>();
                    cane.near_reach = reach / 50f;
                    skill.Field(EqCane).SetValue(cane);

                    BLog.Debug($"Cane reach set to: {reach}");
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(SetReach)}.", ex);
                }
            }

            public static void SetNearPower(float nearPower)
            {
                try
                {
                    if (nearPower < 0f)
                    {
                        BLog.Debug($"Ignored invalid cane near power: {nearPower}");
                        return;
                    }

                    var skill = GetPRSkillTraverse();
                    if (skill == null)
                    {
                        BLog.Notice("Failed to find PR skill for setting cane swing speed.");
                        return;
                    }

                    var cane = skill.Field(EqCane).GetValue<PrCaneEquip>();
                    cane.near_power = nearPower;
                    skill.Field(EqCane).SetValue(cane);

                    BLog.Debug($"Cane near power set to: {nearPower}");
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(SetNearPower)}.", ex);
                }
            }

            public static void SetNearShotgunPower(float nearShotgunPower)
            {
                try
                {
                    if (nearShotgunPower < 0f)
                    {
                        BLog.Debug($"Ignored invalid cane near shotgun power: {nearShotgunPower}");
                        return;
                    }

                    var skill = GetPRSkillTraverse();
                    if (skill == null)
                    {
                        BLog.Notice("Failed to find PR skill for setting cane swing speed.");
                        return;
                    }

                    var cane = skill.Field(EqCane).GetValue<PrCaneEquip>();
                    cane.near_shotgun_power = nearShotgunPower;
                    skill.Field(EqCane).SetValue(cane);

                    BLog.Debug($"Cane near shotgun power set to: {nearShotgunPower}");
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(SetNearShotgunPower)}.", ex);
                }
            }

            public static void SetStability(float stability)
            {
                try
                {
                    if (stability < 0f)
                    {
                        BLog.Debug($"Ignored invalid cane stability: {stability}");
                        return;
                    }

                    var skill = GetPRSkillTraverse();
                    if (skill == null)
                    {
                        BLog.Notice("Failed to find PR skill for setting cane swing speed.");
                        return;
                    }

                    var cane = skill.Field(EqCane).GetValue<PrCaneEquip>();
                    cane.stability = stability;
                    skill.Field(EqCane).SetValue(cane);

                    BLog.Debug($"Cane stability set to: {stability}");
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(SetStability)}.", ex);
                }
            }

            public static void SetManaSplashRatio(float manaSplashRatio)
            {
                try
                {
                    if (manaSplashRatio < 0f)
                    {
                        BLog.Debug($"Ignored invalid cane mana splash ratio: {manaSplashRatio}");
                        return;
                    }

                    var skill = GetPRSkillTraverse();
                    if (skill == null)
                    {
                        BLog.Notice("Failed to find PR skill for setting cane swing speed.");
                        return;
                    }

                    var cane = skill.Field(EqCane).GetValue<PrCaneEquip>();
                    cane.mana_splash_ratio = manaSplashRatio;
                    skill.Field(EqCane).SetValue(cane);

                    BLog.Debug($"Cane mana splash ratio set to: {manaSplashRatio}");
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(SetManaSplashRatio)}.", ex);
                }
            }

            public static void SetCastspeedOverhold(float castspeedOverhold)
            {
                try
                {
                    if (castspeedOverhold < 0f)
                    {
                        BLog.Debug($"Ignored invalid cane castspeed overhold: {castspeedOverhold}");
                        return;
                    }

                    var skill = GetPRSkillTraverse();
                    if (skill == null)
                    {
                        BLog.Notice("Failed to find PR skill for setting cane swing speed.");
                        return;
                    }

                    var cane = skill.Field(EqCane).GetValue<PrCaneEquip>();
                    cane.castspeed_overhold = castspeedOverhold;
                    skill.Field(EqCane).SetValue(cane);

                    BLog.Debug($"Cane castspeed overhold set to: {castspeedOverhold}");
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(SetCastspeedOverhold)}.", ex);
                }
            }

            public static void SetDrainAfterLock(float drainAfterLock)
            {
                try
                {
                    if (drainAfterLock < 0f)
                    {
                        BLog.Debug($"Ignored invalid cane drain after lock: {drainAfterLock}");
                        return;
                    }

                    var skill = GetPRSkillTraverse();
                    if (skill == null)
                    {
                        BLog.Notice("Failed to find PR skill for setting cane swing speed.");
                        return;
                    }

                    var cane = skill.Field(EqCane).GetValue<PrCaneEquip>();
                    cane.drain_after_lock = drainAfterLock;
                    skill.Field(EqCane).SetValue(cane);

                    BLog.Debug($"Cane drain after lock set to: {drainAfterLock}");
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(SetDrainAfterLock)}.", ex);
                }
            }

            public static void SetCastspeed(float speed)
            {
                try
                {
                    if (speed < 0f)
                    {
                        BLog.Debug($"Ignored invalid cane cast speed: {speed}");
                        return;
                    }

                    var skill = GetPRSkillTraverse();
                    if (skill == null)
                    {
                        BLog.Notice("Failed to find PR skill for setting cane swing speed.");
                        return;
                    }

                    var cane = skill.Field(EqCane).GetValue<PrCaneEquip>();
                    cane.castspeed = speed;
                    skill.Field(EqCane).SetValue(cane);

                    BLog.Debug($"Cane cast speed set to: {speed}");
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(SetCastspeed)}.", ex);
                }
            }

            public static void SetMagicPrepareSpeed(float magicPrepareSpeed)
            {
                try
                {
                    if (magicPrepareSpeed < 0f)
                    {
                        BLog.Debug($"Ignored invalid cane magic prepare speed: {magicPrepareSpeed}");
                        return;
                    }

                    var skill = GetPRSkillTraverse();
                    if (skill == null)
                    {
                        BLog.Notice("Failed to find PR skill for setting cane swing speed.");
                        return;
                    }

                    var cane = skill.Field(EqCane).GetValue<PrCaneEquip>();
                    cane.magic_prepare_speed = magicPrepareSpeed;
                    skill.Field(EqCane).SetValue(cane);

                    BLog.Debug($"Cane magic prepare speed set to: {magicPrepareSpeed}");
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(SetMagicPrepareSpeed)}.", ex);
                }
            }

            public static float GetSwingSpeed()
            {
                return GetCaneValue(cane => 50f * cane.near_punch_speed);
            }

            public static float GetCastSpeed()
            {
                return GetCaneValue(cane =>
                    50f * cane.castspeed *
                    (0.33f * cane.magic_prepare_speed + 0.67f) *
                    (0.25f * cane.castspeed_overhold + 0.75f));
            }

            public static float GetBalance()
            {
                return GetCaneValue(cane => 60f * cane.neutral);
            }

            public static float GetEfficiency()
            {
                return GetCaneValue(cane => cane.mp_use_ratio < 1f
                    ? 169f - 104f * cane.mp_use_ratio
                    : 65f / (cane.mp_use_ratio * cane.mp_use_ratio));
            }

            public static float GetRetention()
            {
                return GetCaneValue(cane =>
                    55f * cane.stability * cane.mana_splash_ratio *
                    (0.75f * cane.castspeed_overhold + 0.25f) *
                    (0.5f * cane.drain_after_lock + 0.5f));
            }

            public static float GetLockOn()
            {
                return GetCaneValue(cane => 50f * cane.lockon_power);
            }

            public static float GetLongRange()
            {
                return GetCaneValue(cane => 46f * cane.far_power);
            }

            public static float GetShortRange()
            {
                return GetCaneValue(cane =>
                    55f * (0.25f * cane.near_power + 0.75f) * cane.near_shotgun_power);
            }

            public static float GetReach()
            {
                return GetCaneValue(cane => 50f * cane.near_reach);
            }

            public static float GetNearPower()
            {
                return GetCaneValue(cane => cane.near_power);
            }

            public static float GetNearShotgunPower()
            {
                return GetCaneValue(cane => cane.near_shotgun_power);
            }

            public static float GetStability()
            {
                return GetCaneValue(cane => cane.stability);
            }

            public static float GetManaSplashRatio()
            {
                return GetCaneValue(cane => cane.mana_splash_ratio);
            }

            public static float GetCastspeedOverhold()
            {
                return GetCaneValue(cane => cane.castspeed_overhold);
            }

            public static float GetDrainAfterLock()
            {
                return GetCaneValue(cane => cane.drain_after_lock);
            }

            public static float GetCastspeed()
            {
                return GetCaneValue(cane => cane.castspeed);
            }

            public static float GetMagicPrepareSpeed()
            {
                return GetCaneValue(cane => cane.magic_prepare_speed);
            }

            private static float GetCaneValue(Func<PrCaneEquip, float> valueSelector)
            {
                var skill = GetPRSkillTraverse();
                if (skill == null)
                    return -1f;

                var cane = skill.Field(EqCane).GetValue<PrCaneEquip>();
                return valueSelector(cane);
            }

        }
    }
}
