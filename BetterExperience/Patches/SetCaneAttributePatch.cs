using BetterExperience.BConfigManager;
using BetterExperience.HClassAttribute;
using HarmonyLib;
using nel;
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
            }

            public static void SetSwingSpeed(float speed)
            {
                if (speed < 0f)
                    return;

                var skill = GetM2PrSkill();
                if (skill == null)
                    return;

                var cane = skill.Field(EqCane).GetValue<PrCaneEquip>();
                cane.near_punch_speed = speed / 50f;
                skill.Field(EqCane).SetValue(cane);
            }

            public static void SetCastSpeed(float speed)
            {
                if (speed < 0f)
                    return;

                var skill = GetM2PrSkill();
                if (skill == null)
                    return;

                var cane = skill.Field(EqCane).GetValue<PrCaneEquip>();
                cane.castspeed = speed / (50f * (0.33f * (cane.magic_prepare_speed - 1f) + 1f) * (0.25f * (cane.castspeed_overhold - 1f) + 1f));
                skill.Field(EqCane).SetValue(cane);
            }

            public static void SetBalance(float balance)
            {
                if (balance < 0f)
                    return;

                var skill = GetM2PrSkill();
                if (skill == null)
                    return;

                var cane = skill.Field(EqCane).GetValue<PrCaneEquip>();
                cane.neutral = balance / 60f;
                skill.Field(EqCane).SetValue(cane);
            }

            public static void SetEfficiency(float efficiency)
            {
                if (efficiency < 0f)
                    return;

                var skill = GetM2PrSkill();
                if (skill == null)
                    return;

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
            }

            public static void SetRetention(float retention)
            {
                if (retention < 0f)
                    return;

                var skill = GetM2PrSkill();
                if (skill == null)
                    return;

                var cane = skill.Field(EqCane).GetValue<PrCaneEquip>();
                cane.stability = retention / (55f * cane.mana_splash_ratio * (0.75f * (cane.castspeed_overhold - 1f) + 1f) * (0.5f * (cane.drain_after_lock - 1f) + 1f));
                skill.Field(EqCane).SetValue(cane);
            }

            public static void SetLockOn(float lockOn)
            {
                if (lockOn < 0f)
                    return;

                var skill = GetM2PrSkill();
                if (skill == null)
                    return;

                var cane = skill.Field(EqCane).GetValue<PrCaneEquip>();
                cane.lockon_power = lockOn / 50f;
                skill.Field(EqCane).SetValue(cane);
            }

            public static void SetLongRange(float range)
            {
                if (range < 0f)
                    return;

                var skill = GetM2PrSkill();
                if (skill == null)
                    return;

                var cane = skill.Field(EqCane).GetValue<PrCaneEquip>();
                cane.far_power = range / 46f;
                skill.Field(EqCane).SetValue(cane);
            }

            public static void SetShortRange(float range)
            {
                if (range < 0f)
                    return;

                var skill = GetM2PrSkill();
                if (skill == null)
                    return;

                var cane = skill.Field(EqCane).GetValue<PrCaneEquip>();
                cane.near_power = 4f * (range / (55f * cane.near_shotgun_power) - 1f) + 1f;
                skill.Field(EqCane).SetValue(cane);
            }

            public static void SetReach(float reach)
            {
                if (reach < 0f)
                    return;

                var skill = GetM2PrSkill();
                if (skill == null)
                    return;

                var cane = skill.Field(EqCane).GetValue<PrCaneEquip>();
                cane.near_reach = reach / 50f;
                skill.Field(EqCane).SetValue(cane);
            }

            public static void SetNearPower(float nearPower)
            {
                if (nearPower < 0f)
                    return;

                var skill = GetM2PrSkill();
                if (skill == null)
                    return;

                var cane = skill.Field(EqCane).GetValue<PrCaneEquip>();
                cane.near_power = nearPower;
                skill.Field(EqCane).SetValue(cane);
            }

            public static void SetNearShotgunPower(float nearShotgunPower)
            {
                if (nearShotgunPower < 0f)
                    return;

                var skill = GetM2PrSkill();
                if (skill == null)
                    return;

                var cane = skill.Field(EqCane).GetValue<PrCaneEquip>();
                cane.near_shotgun_power = nearShotgunPower;
                skill.Field(EqCane).SetValue(cane);
            }

            public static void SetStability(float stability)
            {
                if (stability < 0f)
                    return;

                var skill = GetM2PrSkill();
                if (skill == null)
                    return;

                var cane = skill.Field(EqCane).GetValue<PrCaneEquip>();
                cane.stability = stability;
                skill.Field(EqCane).SetValue(cane);
            }

            public static void SetManaSplashRatio(float manaSplashRatio)
            {
                if (manaSplashRatio < 0f)
                    return;

                var skill = GetM2PrSkill();
                if (skill == null)
                    return;

                var cane = skill.Field(EqCane).GetValue<PrCaneEquip>();
                cane.mana_splash_ratio = manaSplashRatio;
                skill.Field(EqCane).SetValue(cane);
            }

            public static void SetCastspeedOverhold(float castspeedOverhold)
            {
                if (castspeedOverhold < 0f)
                    return;

                var skill = GetM2PrSkill();
                if (skill == null)
                    return;

                var cane = skill.Field(EqCane).GetValue<PrCaneEquip>();
                cane.castspeed_overhold = castspeedOverhold;
                skill.Field(EqCane).SetValue(cane);
            }

            public static void SetDrainAfterLock(float drainAfterLock)
            {
                if (drainAfterLock < 0f)
                    return;

                var skill = GetM2PrSkill();
                if (skill == null)
                    return;

                var cane = skill.Field(EqCane).GetValue<PrCaneEquip>();
                cane.drain_after_lock = drainAfterLock;
                skill.Field(EqCane).SetValue(cane);
            }

            public static void SetCastspeed(float speed)
            {
                if (speed < 0f)
                    return;

                var skill = GetM2PrSkill();
                if (skill == null)
                    return;

                var cane = skill.Field(EqCane).GetValue<PrCaneEquip>();
                cane.castspeed = speed;
                skill.Field(EqCane).SetValue(cane);
            }

            public static void SetMagicPrepareSpeed(float magicPrepareSpeed)
            {
                if (magicPrepareSpeed < 0f)
                    return;

                var skill = GetM2PrSkill();
                if (skill == null)
                    return;

                var cane = skill.Field(EqCane).GetValue<PrCaneEquip>();
                cane.magic_prepare_speed = magicPrepareSpeed;
                skill.Field(EqCane).SetValue(cane);
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
