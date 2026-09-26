using HarmonyLib;
using nel;
using System.Collections.Generic;
using System.Reflection;

namespace BetterExperience.Patches
{
    [HarmonyPatch]
    internal static class PortraitFadePatch
    {
        private static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(UIPicture), nameof(UIPicture.setFade));
            yield return AccessTools.Method(typeof(UIPictureBase), nameof(UIPictureBase.setFade));
        }

        private static bool Prefix(UIPictureBase __instance, ref bool __result)
        {
            if (!PortraitControlRuntime.Blocks(__instance)) return true;
            __result = false;
            return false;
        }
    }

    [HarmonyPatch(typeof(UIPicture), nameof(UIPicture.changeEmotDefault))]
    internal static class PortraitDefaultPatch
    {
        private static bool Prefix(UIPicture __instance, ref bool __result)
        {
            if (!PortraitControlRuntime.Blocks(__instance)) return true;
            __result = true;
            return false;
        }
    }

    [HarmonyPatch]
    internal static class PortraitChangePatch
    {
        private static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(UIPictureBase), nameof(UIPictureBase.readFader));
            yield return AccessTools.Method(typeof(UIPictureBase), nameof(UIPictureBase.changeEmotIn));
        }

        private static bool Prefix(UIPictureBase __instance, ref UIPictureFader.UIP_RES __result)
        {
            if (!PortraitControlRuntime.Blocks(__instance)) return true;
            __result = 0;
            return false;
        }
    }

    [HarmonyPatch]
    internal static class PortraitFaderRunPatch
    {
        private static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(UIPictureFader), nameof(UIPictureFader.run));
            yield return AccessTools.Method(typeof(UIPictureFader), nameof(UIPictureFader.runGroundLevel));
            yield return AccessTools.Method(typeof(UIPictureFader), nameof(UIPictureFader.Explode));
        }

        private static bool Prefix(UIPictureBase PCon, ref UIPictureFader.UIP_RES __result)
        {
            if (!PortraitControlRuntime.Blocks(PCon)) return true;
            __result = 0;
            return false;
        }
    }

    [HarmonyPatch(typeof(UIPicture), nameof(UIPicture.getAdditionalState))]
    internal static class PortraitAdditionalPatch
    {
        private static bool Prefix(UIPicture __instance, ref UIPictureBase.EMSTATE_ADD __result,
            ref UIPictureBase.EMSTATE_ADD ___pre_sta)
        {
            if (!PortraitControlRuntime.TryAdditional(__instance, out var state)) return true;
            ___pre_sta = __result = state;
            return false;
        }
    }

    [HarmonyPatch(typeof(UIPicture), "get_TS_animation")]
    internal static class PortraitAnimationPatch
    {
        private static bool Prefix(UIPicture __instance, ref float __result)
        {
            if (!PortraitControlRuntime.LocksAnimation(__instance)) return true;
            __result = 1f;
            return false;
        }
    }

    [HarmonyPatch]
    internal static class PortraitSessionResetPatch
    {
        private static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(COOK), "readBinaryContent");
            yield return AccessTools.Method(typeof(COOK), nameof(COOK.newGame));
            yield return AccessTools.Method(typeof(NelM2DBase), nameof(NelM2DBase.destruct));
        }

        private static void Prefix() => PortraitControlRuntime.Stop(false);
    }

    [HarmonyPatch(typeof(UIPicture), nameof(UIPicture.destruct))]
    internal static class PortraitDestructPatch
    {
        private static void Prefix(UIPicture __instance) => PortraitControlRuntime.OnDestroyed(__instance);
    }

    [HarmonyPatch(typeof(UIPictureBase), nameof(UIPictureBase.reloadUiScript))]
    internal static class PortraitReloadPatch
    {
        private static void Postfix(UIPictureBase __instance) => PortraitControlRuntime.OnScriptReload(__instance);
    }
}
