using HarmonyLib;
using nel;
using System;

namespace BetterExperience.Patches
{
    internal partial class Patchs
    {
        [HarmonyPatch]
        private class BetterSaveSitePatch
        {
            [HarmonyPrefix]
            [HarmonyPatch(typeof(NelM2DBase), "canSaveInCurMap")]
            private static bool CanSaveInCurMapPrefix(NelM2DBase __instance, ref bool __result)
            {
                if (!ConfigManager.EnableBetterSaveSite.Value)
                    return true;

                if (__instance == null)
                    return true;

                if (__instance.curMap == null)
                    return true;

                __result = true;
                return false;
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(PR), "canSave", new Type[] { typeof(bool) })]
            private static bool CanSavePrefix(ref bool __result)
            {
                if (!ConfigManager.EnableBetterSaveSite.Value)
                    return true;

                __result = true;
                return false;
            }
        }
    }
}
