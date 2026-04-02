using BetterExperience.BConfigManager;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;

namespace BetterExperience.Patches
{
    public partial class HPatches
    {
        [HarmonyPatch]
        public class NoHpDamagePatch
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
