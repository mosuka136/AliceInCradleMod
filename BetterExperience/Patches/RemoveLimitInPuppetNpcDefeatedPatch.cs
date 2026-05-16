using BetterExperience.BConfigManager;
using HarmonyLib;
using nel;
using System;
using XX;

namespace BetterExperience.Patches
{
    public partial class HPatches
    {
        /// <summary>
        /// 允许木偶商人在相关击败标记未满足时仍然启用。
        /// 补丁会临时改写全局剧情计数以通过游戏检查，并在保存前恢复原值，避免把临时状态写入存档。
        /// </summary>
        [HarmonyPatch]
        public class RemoveLimitInPuppetNpcDefeatedPatch
        {
            private const string PUP_KILL = "PUP_KILL";
            // 保存用户真实的 PUP_KILL 值；_isChanging 防止本补丁自己的临时写入覆盖缓存。
            private static uint _pup_kill;
            private static bool _isInitialized = false;
            private static bool _isChanging = false;

            [HarmonyPrefix]
            [HarmonyPatch(typeof(SCN), "isPuppetWNpcDefeated")]
            public static bool IsPuppetWNpcDefeatedPrefix(ref bool __result)
            {
                try
                {
                    if (!ConfigManager.EnableRemoveLimitInPuppetNpcDefeated.Value)
                        return true;

                    var n = GF.getC(PUP_KILL);
                    if (n % 2 == 1)
                    {
                        _pup_kill = n;

                        if (!_isInitialized)
                        {
                            GameSaveProtectionManager.OnSavingActivated += RecoverPupKill;
                            _isInitialized = true;
                            HLog.Debug($"{nameof(RemoveLimitInPuppetNpcDefeatedPatch)} applied.");
                        }

                        _isChanging = true;
                        GF.setC(PUP_KILL, X.Mn(14U, (uint)((int)n * 2 + 2)));
                        _isChanging = false;
                    }
                    __result = false;

                    return false;
                }
                catch (Exception ex)
                {
                    HLog.Error($"Unexpected error in {nameof(RemoveLimitInPuppetNpcDefeatedPatch)}", ex);
                    return true;
                }
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(GF), "setC", new Type[] { typeof(string), typeof(uint) })]
            public static void SetCPrefix(string key)
            {
                try
                {
                    if (_isInitialized && key == PUP_KILL && !_isChanging)
                        _pup_kill = GF.getC(PUP_KILL);
                }
                catch (Exception ex)
                {
                    HLog.Error($"Unexpected error in {nameof(RemoveLimitInPuppetNpcDefeatedPatch)}", ex);
                }
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(SCN), "isWNpcEnable")]
            public static void IsWNpcEnablePostfix(WanderingManager.TYPE type, ref bool __result)
            {
                try
                {
                    if (!ConfigManager.EnableRemoveLimitInPuppetNpcDefeated.Value)
                        return;

                    if (type == WanderingManager.TYPE.PUP)
                        __result = true;
                    HLog.Debug($"{nameof(IsWNpcEnablePostfix)} applied.");
                }
                catch (Exception ex)
                {
                    HLog.Error($"Unexpected error in {nameof(RemoveLimitInPuppetNpcDefeatedPatch)}", ex);
                }
            }

            public static void RecoverPupKill()
            {
                GF.setC(PUP_KILL, _pup_kill);
                HLog.Debug($"Recovered PUP_KILL before save: {_pup_kill}");
            }
        }
    }
}
