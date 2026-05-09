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
        [HarmonyPatch]
        public class SetCaneAttributePatch
        {
            private const string EqCane = "EqCane";

            private static bool _initialized = false;

            [InitializeOnGameBoot]
            public static void Initialize()
            {
                if (_initialized)
                    return;

                GameSaveLoadManager.OnGameSaveLoadCompleted += () =>
                {
                    if (ConfigManager.EnablePreloadCaneSwingSpeed.Value)
                        SetSwingSpeed(ConfigManager.SetCaneSwingSpeed.Value);

                    if (ConfigManager.EnablePreloadCaneCastSpeed.Value)
                        SetCastSpeed(ConfigManager.SetCaneCastSpeed.Value);

                    if (ConfigManager.EnablePreloadCaneBalance.Value)
                        SetBalance(ConfigManager.SetCaneBalance.Value);

                    if (ConfigManager.EnablePreloadCaneEfficiency.Value)
                        SetEfficiency(ConfigManager.SetCaneEfficiency.Value);

                    if (ConfigManager.EnablePreloadCaneRetention.Value)
                        SetRetention(ConfigManager.SetCaneRetention.Value);

                    if (ConfigManager.EnablePreloadCaneLockOn.Value)
                        SetLockOn(ConfigManager.SetCaneLockOn.Value);

                    if (ConfigManager.EnablePreloadCaneLongRange.Value)
                        SetLongRange(ConfigManager.SetCaneLongRange.Value);

                    if (ConfigManager.EnablePreloadCaneShortRange.Value)
                        SetShortRange(ConfigManager.SetCaneShortRange.Value);

                    if (ConfigManager.EnablePreloadCaneReach.Value)
                        SetReach(ConfigManager.SetCaneReach.Value);

                    if (ConfigManager.EnablePreloadCaneNearPower.Value)
                        SetNearPower(ConfigManager.SetCaneNearPower.Value);

                    if (ConfigManager.EnablePreloadCaneNearShotgunPower.Value)
                        SetNearShotgunPower(ConfigManager.SetCaneNearShotgunPower.Value);

                    if (ConfigManager.EnablePreloadCaneStability.Value)
                        SetStability(ConfigManager.SetCaneStability.Value);

                    if (ConfigManager.EnablePreloadCaneManaSplashRatio.Value)
                        SetManaSplashRatio(ConfigManager.SetCaneManaSplashRatio.Value);

                    if (ConfigManager.EnablePreloadCaneCastspeedOverhold.Value)
                        SetCastspeedOverhold(ConfigManager.SetCaneCastspeedOverhold.Value);

                    if (ConfigManager.EnablePreloadCaneDrainAfterLock.Value)
                        SetDrainAfterLock(ConfigManager.SetCaneDrainAfterLock.Value);

                    if (ConfigManager.EnablePreloadCaneCastspeed.Value)
                        SetCastspeed(ConfigManager.SetCaneCastspeed.Value);

                    if (ConfigManager.EnablePreloadCaneMagicPrepareSpeed.Value)
                        SetMagicPrepareSpeed(ConfigManager.SetCaneMagicPrepareSpeed.Value);
                };

                ConfigManager.SetCaneSwingSpeed.OnValueChanged += (s, e) => SetSwingSpeed(e);
                ConfigManager.SetCaneCastSpeed.OnValueChanged += (s, e) => SetCastSpeed(e);
                ConfigManager.SetCaneBalance.OnValueChanged += (s, e) => SetBalance(e);
                ConfigManager.SetCaneEfficiency.OnValueChanged += (s, e) => SetEfficiency(e);
                ConfigManager.SetCaneRetention.OnValueChanged += (s, e) => SetRetention(e);
                ConfigManager.SetCaneLockOn.OnValueChanged += (s, e) => SetLockOn(e);
                ConfigManager.SetCaneLongRange.OnValueChanged += (s, e) => SetLongRange(e);
                ConfigManager.SetCaneShortRange.OnValueChanged += (s, e) => SetShortRange(e);
                ConfigManager.SetCaneReach.OnValueChanged += (s, e) => SetReach(e);
                ConfigManager.SetCaneNearPower.OnValueChanged += (s, e) => SetNearPower(e);
                ConfigManager.SetCaneNearShotgunPower.OnValueChanged += (s, e) => SetNearShotgunPower(e);
                ConfigManager.SetCaneStability.OnValueChanged += (s, e) => SetStability(e);
                ConfigManager.SetCaneManaSplashRatio.OnValueChanged += (s, e) => SetManaSplashRatio(e);
                ConfigManager.SetCaneCastspeedOverhold.OnValueChanged += (s, e) => SetCastspeedOverhold(e);
                ConfigManager.SetCaneDrainAfterLock.OnValueChanged += (s, e) => SetDrainAfterLock(e);
                ConfigManager.SetCaneCastspeed.OnValueChanged += (s, e) => SetCastspeed(e);
                ConfigManager.SetCaneMagicPrepareSpeed.OnValueChanged += (s, e) => SetMagicPrepareSpeed(e);

                _initialized = true;

                HLog.Debug("Cane attribute patch initialized.");
            }

            public static void SetSwingSpeed(float speed)
            {
                try
                {
                    if (speed < 0f)
                    {
                        HLog.Debug($"Ignored invalid swing speed: {speed}");
                        return;
                    }

                    var skill = GetM2PrSkill();
                    if (skill == null)
                    {
                        HLog.Notice("Failed to find PR skill for setting cane swing speed.");
                        return;
                    }

                    var cane = skill.Field(EqCane).GetValue<PrCaneEquip>();
                    cane.near_punch_speed = speed / 50f;
                    skill.Field(EqCane).SetValue(cane);

                    HLog.Debug($"Cane swing speed set to: {speed}");
                }
                catch (Exception ex)
                {
                    HLog.Error($"Unexpected error in {nameof(SetSwingSpeed)}.", ex);
                }
            }

            public static void SetCastSpeed(float speed)
            {
                try
                {
                    if (speed < 0f)
                    {
                        HLog.Debug($"Ignored invalid caneCast speed: {speed}");
                        return;
                    }

                    var skill = GetM2PrSkill();
                    if (skill == null)
                    {
                        HLog.Notice("Failed to find PR skill for setting cane swing speed.");
                        return;
                    }

                    var cane = skill.Field(EqCane).GetValue<PrCaneEquip>();
                    cane.castspeed = speed / (50f * (0.33f * (cane.magic_prepare_speed - 1f) + 1f) * (0.25f * (cane.castspeed_overhold - 1f) + 1f));
                    skill.Field(EqCane).SetValue(cane);

                    HLog.Debug($"Cane cast speed set to: {cane.castspeed})");
                }
                catch (Exception ex)
                {
                    HLog.Error($"Unexpected error in {nameof(SetCastSpeed)}.", ex);
                }
            }

            public static void SetBalance(float balance)
            {
                try
                {
                    if (balance < 0f)
                    {
                        HLog.Debug($"Ignored invalid cane balance: {balance}");
                        return;
                    }

                    var skill = GetM2PrSkill();
                    if (skill == null)
                    {
                        HLog.Notice("Failed to find PR skill for setting cane swing speed.");
                        return;
                    }

                    var cane = skill.Field(EqCane).GetValue<PrCaneEquip>();
                    cane.neutral = balance / 60f;
                    skill.Field(EqCane).SetValue(cane);

                    HLog.Debug($"Cane balance set to: {balance}");
                }
                catch (Exception ex)
                {
                    HLog.Error($"Unexpected error in {nameof(SetBalance)}.", ex);
                }
            }

            public static void SetEfficiency(float efficiency)
            {
                try
                {
                    if (efficiency < 0f)
                    {
                        HLog.Debug($"Ignored invalid cane efficiency: {efficiency}");
                        return;
                    }

                    var skill = GetM2PrSkill();
                    if (skill == null)
                    {
                        HLog.Notice("Failed to find PR skill for setting cane swing speed.");
                        return;
                    }

                    var cane = skill.Field(EqCane).GetValue<PrCaneEquip>();
                    if (efficiency == 0f)
                        cane.mp_use_ratio = 12f;
                    else if (efficiency <= 65f)
                        cane.mp_use_ratio = Mathf.Sqrt(65f / efficiency);
                    else if (efficiency <= 169f)
                        cane.mp_use_ratio = (float)((169.0 - efficiency) / 104.0);
                    else
                        cane.mp_use_ratio = 0f;
                    skill.Field(EqCane).SetValue(cane);

                    HLog.Debug($"Cane efficiency set to: {efficiency} (mp_use_ratio: {cane.mp_use_ratio})");
                }
                catch (Exception ex)
                {
                    HLog.Error($"Unexpected error in {nameof(SetEfficiency)}.", ex);
                }
            }

            public static void SetRetention(float retention)
            {
                try
                {
                    if (retention < 0f)
                    {
                        HLog.Debug($"Ignored invalid cane retention: {retention}");
                        return;
                    }

                    var skill = GetM2PrSkill();
                    if (skill == null)
                    {
                        HLog.Notice("Failed to find PR skill for setting cane swing speed.");
                        return;
                    }

                    var cane = skill.Field(EqCane).GetValue<PrCaneEquip>();
                    cane.stability = retention / (55f * cane.mana_splash_ratio * (0.75f * (cane.castspeed_overhold - 1f) + 1f) * (0.5f * (cane.drain_after_lock - 1f) + 1f));
                    skill.Field(EqCane).SetValue(cane);

                    HLog.Debug($"Cane retention set to: {retention} (stability: {cane.stability})");
                }
                catch (Exception ex)
                {
                    HLog.Error($"Unexpected error in {nameof(SetRetention)}.", ex);
                }
            }

            public static void SetLockOn(float lockOn)
            {
                try
                {
                    if (lockOn < 0f)
                    {
                        HLog.Debug($"Ignored invalid cane lock-on: {lockOn}");
                        return;
                    }

                    var skill = GetM2PrSkill();
                    if (skill == null)
                    {
                        HLog.Notice("Failed to find PR skill for setting cane swing speed.");
                        return;
                    }

                    var cane = skill.Field(EqCane).GetValue<PrCaneEquip>();
                    cane.lockon_power = lockOn / 50f;
                    skill.Field(EqCane).SetValue(cane);

                    HLog.Debug($"Cane lock-on set to: {lockOn}");
                }
                catch (Exception ex)
                {
                    HLog.Error($"Unexpected error in {nameof(SetLockOn)}.", ex);
                }
            }

            public static void SetLongRange(float range)
            {
                try
                {
                    if (range < 0f)
                    {
                        HLog.Debug($"Ignored invalid cane long range: {range}");
                        return;
                    }

                    var skill = GetM2PrSkill();
                    if (skill == null)
                    {
                        HLog.Notice("Failed to find PR skill for setting cane swing speed.");
                        return;
                    }

                    var cane = skill.Field(EqCane).GetValue<PrCaneEquip>();
                    cane.far_power = range / 46f;
                    skill.Field(EqCane).SetValue(cane);

                    HLog.Debug($"Cane long range set to: {range}");
                }
                catch (Exception ex)
                {
                    HLog.Error($"Unexpected error in {nameof(SetLongRange)}.", ex);
                }
            }

            public static void SetShortRange(float range)
            {
                try
                {
                    if (range < 0f)
                    {
                        HLog.Debug($"Ignored invalid cane short range: {range}");
                        return;
                    }

                    var skill = GetM2PrSkill();
                    if (skill == null)
                    {
                        HLog.Notice("Failed to find PR skill for setting cane swing speed.");
                        return;
                    }

                    var cane = skill.Field(EqCane).GetValue<PrCaneEquip>();
                    cane.near_power = 4f * (range / (55f * cane.near_shotgun_power) - 1f) + 1f;
                    skill.Field(EqCane).SetValue(cane);

                    HLog.Debug($"Cane short range set to: {range} (near_power: {cane.near_power})");
                }
                catch (Exception ex)
                {
                    HLog.Error($"Unexpected error in {nameof(SetShortRange)}.", ex);
                }
            }

            public static void SetReach(float reach)
            {
                try
                {
                    if (reach < 0f)
                    {
                        HLog.Debug($"Ignored invalid cane reach: {reach}");
                        return;
                    }

                    var skill = GetM2PrSkill();
                    if (skill == null)
                    {
                        HLog.Notice("Failed to find PR skill for setting cane swing speed.");
                        return;
                    }

                    var cane = skill.Field(EqCane).GetValue<PrCaneEquip>();
                    cane.near_reach = reach / 50f;
                    skill.Field(EqCane).SetValue(cane);

                    HLog.Debug($"Cane reach set to: {reach}");
                }
                catch (Exception ex)
                {
                    HLog.Error($"Unexpected error in {nameof(SetReach)}.", ex);
                }
            }

            public static void SetNearPower(float nearPower)
            {
                try
                {
                    if (nearPower < 0f)
                    {
                        HLog.Debug($"Ignored invalid cane near power: {nearPower}");
                        return;
                    }

                    var skill = GetM2PrSkill();
                    if (skill == null)
                    {
                        HLog.Notice("Failed to find PR skill for setting cane swing speed.");
                        return;
                    }

                    var cane = skill.Field(EqCane).GetValue<PrCaneEquip>();
                    cane.near_power = nearPower;
                    skill.Field(EqCane).SetValue(cane);

                    HLog.Debug($"Cane near power set to: {nearPower}");
                }
                catch (Exception ex)
                {
                    HLog.Error($"Unexpected error in {nameof(SetNearPower)}.", ex);
                }
            }

            public static void SetNearShotgunPower(float nearShotgunPower)
            {
                try
                {
                    if (nearShotgunPower < 0f)
                    {
                        HLog.Debug($"Ignored invalid cane near shotgun power: {nearShotgunPower}");
                        return;
                    }

                    var skill = GetM2PrSkill();
                    if (skill == null)
                    {
                        HLog.Notice("Failed to find PR skill for setting cane swing speed.");
                        return;
                    }

                    var cane = skill.Field(EqCane).GetValue<PrCaneEquip>();
                    cane.near_shotgun_power = nearShotgunPower;
                    skill.Field(EqCane).SetValue(cane);

                    HLog.Debug($"Cane near shotgun power set to: {nearShotgunPower}");
                }
                catch (Exception ex)
                {
                    HLog.Error($"Unexpected error in {nameof(SetNearShotgunPower)}.", ex);
                }
            }

            public static void SetStability(float stability)
            {
                try
                {
                    if (stability < 0f)
                    {
                        HLog.Debug($"Ignored invalid cane stability: {stability}");
                        return;
                    }

                    var skill = GetM2PrSkill();
                    if (skill == null)
                    {
                        HLog.Notice("Failed to find PR skill for setting cane swing speed.");
                        return;
                    }

                    var cane = skill.Field(EqCane).GetValue<PrCaneEquip>();
                    cane.stability = stability;
                    skill.Field(EqCane).SetValue(cane);

                    HLog.Debug($"Cane stability set to: {stability}");
                }
                catch (Exception ex)
                {
                    HLog.Error($"Unexpected error in {nameof(SetStability)}.", ex);
                }
            }

            public static void SetManaSplashRatio(float manaSplashRatio)
            {
                try
                {
                    if (manaSplashRatio < 0f)
                    {
                        HLog.Debug($"Ignored invalid cane mana splash ratio: {manaSplashRatio}");
                        return;
                    }

                    var skill = GetM2PrSkill();
                    if (skill == null)
                    {
                        HLog.Notice("Failed to find PR skill for setting cane swing speed.");
                        return;
                    }

                    var cane = skill.Field(EqCane).GetValue<PrCaneEquip>();
                    cane.mana_splash_ratio = manaSplashRatio;
                    skill.Field(EqCane).SetValue(cane);

                    HLog.Debug($"Cane mana splash ratio set to: {manaSplashRatio}");
                }
                catch (Exception ex)
                {
                    HLog.Error($"Unexpected error in {nameof(SetManaSplashRatio)}.", ex);
                }
            }

            public static void SetCastspeedOverhold(float castspeedOverhold)
            {
                try
                {
                    if (castspeedOverhold < 0f)
                    {
                        HLog.Debug($"Ignored invalid cane castspeed overhold: {castspeedOverhold}");
                        return;
                    }

                    var skill = GetM2PrSkill();
                    if (skill == null)
                    {
                        HLog.Notice("Failed to find PR skill for setting cane swing speed.");
                        return;
                    }

                    var cane = skill.Field(EqCane).GetValue<PrCaneEquip>();
                    cane.castspeed_overhold = castspeedOverhold;
                    skill.Field(EqCane).SetValue(cane);

                    HLog.Debug($"Cane castspeed overhold set to: {castspeedOverhold}");
                }
                catch (Exception ex)
                {
                    HLog.Error($"Unexpected error in {nameof(SetCastspeedOverhold)}.", ex);
                }
            }

            public static void SetDrainAfterLock(float drainAfterLock)
            {
                try
                {
                    if (drainAfterLock < 0f)
                    {
                        HLog.Debug($"Ignored invalid cane drain after lock: {drainAfterLock}");
                        return;
                    }

                    var skill = GetM2PrSkill();
                    if (skill == null)
                    {
                        HLog.Notice("Failed to find PR skill for setting cane swing speed.");
                        return;
                    }

                    var cane = skill.Field(EqCane).GetValue<PrCaneEquip>();
                    cane.drain_after_lock = drainAfterLock;
                    skill.Field(EqCane).SetValue(cane);

                    HLog.Debug($"Cane drain after lock set to: {drainAfterLock}");
                }
                catch (Exception ex)
                {
                    HLog.Error($"Unexpected error in {nameof(SetDrainAfterLock)}.", ex);
                }
            }

            public static void SetCastspeed(float speed)
            {
                try
                {
                    if (speed < 0f)
                    {
                        HLog.Debug($"Ignored invalid cane cast speed: {speed}");
                        return;
                    }

                    var skill = GetM2PrSkill();
                    if (skill == null)
                    {
                        HLog.Notice("Failed to find PR skill for setting cane swing speed.");
                        return;
                    }

                    var cane = skill.Field(EqCane).GetValue<PrCaneEquip>();
                    cane.castspeed = speed;
                    skill.Field(EqCane).SetValue(cane);

                    HLog.Debug($"Cane cast speed set to: {speed}");
                }
                catch (Exception ex)
                {
                    HLog.Error($"Unexpected error in {nameof(SetCastspeed)}.", ex);
                }
            }

            public static void SetMagicPrepareSpeed(float magicPrepareSpeed)
            {
                try
                {
                    if (magicPrepareSpeed < 0f)
                    {
                        HLog.Debug($"Ignored invalid cane magic prepare speed: {magicPrepareSpeed}");
                        return;
                    }

                    var skill = GetM2PrSkill();
                    if (skill == null)
                    {
                        HLog.Notice("Failed to find PR skill for setting cane swing speed.");
                        return;
                    }

                    var cane = skill.Field(EqCane).GetValue<PrCaneEquip>();
                    cane.magic_prepare_speed = magicPrepareSpeed;
                    skill.Field(EqCane).SetValue(cane);

                    HLog.Debug($"Cane magic prepare speed set to: {magicPrepareSpeed}");
                }
                catch (Exception ex)
                {
                    HLog.Error($"Unexpected error in {nameof(SetMagicPrepareSpeed)}.", ex);
                }
            }

            public static Traverse GetM2PrSkill()
            {
                var pr = UnityEngine.Object.FindAnyObjectByType<PR>();
                if (pr == null)
                    return null;

                return Traverse.Create(pr.Skill);
            }
        }
    }
}
