using HarmonyLib;
using nel;
using System;
using XX;

namespace BetterExperience.Patches
{
    internal partial class HPatches
    {
        [HarmonyPatch]
        private class RemoveLimitInPuppetNpcDefeatedPatch
        {
            private const string PUP_KILL = "PUP_KILL";
            private static uint _pup_kill;
            private static bool _isInitialized = false;
            private static bool _isChanging = false;

            [HarmonyPrefix]
            [HarmonyPatch(typeof(SCN), "isPuppetWNpcDefeated")]
            private static bool IsPuppetWNpcDefeatedPrefix(ref bool __result)
            {
                if (!ConfigManager.EnableRemoveLimitInPuppetNpcDefeated.Value)
                    return true;

                var n = GF.getC(PUP_KILL);
                if (n % 2 == 1)
                {
                    _pup_kill = n;

                    if (!_isInitialized)
                    {
                        OnSiteProtectionManager.Instance.OnSiteProtectionActivated += RecoverPupKill;
                        _isInitialized = true;
                    }

                    _isChanging = true;
                    GF.setC(PUP_KILL, X.Mn(14U, (uint)((int)n * 2 + 2)));
                    _isChanging = false;
                }
                __result = false;

                return false;
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(GF), "setC", new Type[] { typeof(string), typeof(uint) })]
            private static void SetCPrefix(string key)
            {
                if (_isInitialized && key == PUP_KILL && !_isChanging)
                    _pup_kill = GF.getC(PUP_KILL);
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(SCN), "isWNpcEnable")]
            private static void IsWNpcEnablePostfix(WanderingManager.TYPE type, ref bool __result)
            {
                if (!ConfigManager.EnableRemoveLimitInPuppetNpcDefeated.Value)
                    return;

                if (type == WanderingManager.TYPE.PUP)
                    __result = true;
            }

            private static void RecoverPupKill()
            {
                GF.setC(PUP_KILL, _pup_kill);
            }
        }
    }
}
