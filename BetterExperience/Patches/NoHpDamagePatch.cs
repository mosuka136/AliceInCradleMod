using BetterExperience.BConfigManager;
using HarmonyLib;
using System;
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
                    HLog.Error("applyHpDamage not found on PR.");
                    yield break;
                }

                yield return m;
            }

            static bool Prefix()
            {
                try
                {
                    if (!ConfigManager.EnableNoHpDamage.Value)
                        return true;

                    HLog.Debug($"{nameof(NoHpDamagePatch)} applied.");
                    return false;
                }
                catch (Exception ex)
                {
                    HLog.Error($"Unexpected error in {nameof(NoHpDamagePatch)}.", ex);
                    return true;
                }
            }
        }
    }
}
