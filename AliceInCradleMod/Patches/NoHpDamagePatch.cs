using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;

namespace BetterExperience.Patches
{
    internal partial class Patchs
    {
        [HarmonyPatch]
        private class NoHpDamagePatch
        {
            static IEnumerable<MethodBase> TargetMethods()
            {
                var m = AccessTools.Method(typeof(nel.PR), "applyHpDamage");
                if (m == null)
                {
                    HLog.Error("applyHpDamage not found on PR. Skip.");
                    yield break;
                }

                yield return m;
            }

            static bool Prefix()
            {
                if (!ConfigManager.EnableNoHpDamage.Value)
                    return true;

                return false;
            }
        }
    }
}
