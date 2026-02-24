using HarmonyLib;
using nel;
using UnityEngine;

namespace BetterExperience.Patches
{
    internal partial class HPatches
    {
        [HarmonyPatch]
        internal class SetHpMpEpPatch
        {
            private static bool _initialized = false;

            [HarmonyPostfix]
            [HarmonyPatch(typeof(FrameUpdateBooster), nameof(FrameUpdateBooster.Awake))]
            private static void Initialize()
            {
                if (_initialized)
                    return;

                GameAttributePatchManager.Instance.OnGameSaveLoadCompleted += () =>
                {
                    if (ConfigManager.EnablePreloadPlayerHp.Value)
                        SetHp(ConfigManager.SetPlayerHp.Value);

                    if (ConfigManager.EnablePreloadPlayerMp.Value)
                        SetMp(ConfigManager.SetPlayerMp.Value);

                    if (ConfigManager.EnablePreloadPlayerEp.Value)
                        SetEp(ConfigManager.SetPlayerEp.Value);

                    if (ConfigManager.EnablePreloadPlayerMaxHp.Value)
                        SetMaxHp(ConfigManager.SetPlayerMaxHp.Value);

                    if (ConfigManager.EnablePreloadPlayerMaxMp.Value)
                        SetMaxMp(ConfigManager.SetPlayerMaxMp.Value);
                };

                ConfigManager.SetPlayerHp.SettingChanged += (s, e) =>
                {
                    SetHp(ConfigManager.SetPlayerHp.Value);
                };
                ConfigManager.SetPlayerMp.SettingChanged += (s, e) =>
                {
                    SetMp(ConfigManager.SetPlayerMp.Value);
                };
                ConfigManager.SetPlayerEp.SettingChanged += (s, e) =>
                {
                    SetEp(ConfigManager.SetPlayerEp.Value);
                };

                ConfigManager.SetPlayerMaxHp.SettingChanged += (s, e) =>
                {
                    SetMaxHp(ConfigManager.SetPlayerMaxHp.Value);
                };
                ConfigManager.SetPlayerMaxMp.SettingChanged += (s, e) =>
                {
                    SetMaxMp(ConfigManager.SetPlayerMaxMp.Value);
                };

                _initialized = true;
            }

            public static void SetHp(int hp)
            {
                if (hp < 0)
                    return;

                var pr = UnityEngine.Object.FindAnyObjectByType<PR>();
                if (pr == null)
                    return;

                var prTraverse = Traverse.Create(pr);
                if (prTraverse == null)
                    return;

                hp = Mathf.Min(hp, prTraverse.Field("maxhp").GetValue<int>());

                prTraverse.Field("hp").SetValue(hp);
                pr.cureHp(0);
            }

            public static void SetMp(int mp)
            {
                if (mp < 0)
                    return;

                var pr = UnityEngine.Object.FindAnyObjectByType<PR>();
                if (pr == null)
                    return;

                var prTraverse = Traverse.Create(pr);
                if (prTraverse == null)
                    return;

                mp = Mathf.Min(mp, prTraverse.Field("maxmp").GetValue<int>());

                prTraverse.Field("mp").SetValue(mp);
                pr.cureMp(0);
            }

            public static void SetEp(int ep)
            {
                if (ep < 0)
                    return;

                var pr = UnityEngine.Object.FindAnyObjectByType<PR>();
                if (pr == null)
                    return;

                var prTraverse = Traverse.Create(pr);
                if (prTraverse == null)
                    return;

                prTraverse.Field("ep").SetValue(ep);
                pr.EpCon.fineCounter(); // 刷新EP显示
            }

            public static void SetMaxHp(int maxHp)
            {
                if (maxHp <= 0)
                    return;

                var pr = UnityEngine.Object.FindAnyObjectByType<PR>();
                if (pr == null)
                    return;

                var prTraverse = Traverse.Create(pr);
                if (prTraverse == null)
                    return;

                prTraverse.Field("maxhp").SetValue(maxHp);
                pr.Ser.checkSer();
                pr.cureHp(0);
            }

            public static void SetMaxMp(int maxMp)
            {
                if (maxMp <= 0)
                    return;

                var pr = UnityEngine.Object.FindAnyObjectByType<PR>();
                if (pr == null)
                    return;

                var prTraverse = Traverse.Create(pr);
                if (prTraverse == null)
                    return;

                prTraverse.Field("maxmp").SetValue(maxMp);
                pr.cureMp(0);
            }
        }
    }
}
