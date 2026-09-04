using HarmonyLib;
using m2d;
using nel;
using System;
using UnityEngine;

namespace BetterExperience.Patches
{
    [HarmonyPatch(typeof(PR), nameof(PR.runPhysics))]
    internal static class NoclipPatch
    {
        [HarmonyPrefix]
        public static void Prefix(PR __instance, out Vector2? __state)
        {
            __state = PlayerMovementController.BeforePhysics(__instance);
        }

        [HarmonyPostfix]
        public static void Postfix(PR __instance, float fcnt, Vector2? __state)
        {
            PlayerMovementController.AfterPhysics(__instance, fcnt, __state);
        }

        [HarmonyFinalizer]
        public static Exception Finalizer(Exception __exception)
        {
            if (__exception != null)
                PlayerMovementController.Stop();
            return __exception;
        }
    }

    [HarmonyPatch]
    internal static class NoclipMapLifecyclePatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(PR), nameof(PR.appear))]
        public static void BeforeAppear()
        {
            PlayerMovementController.CancelForMapChange();
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(M2MoverPr), nameof(M2MoverPr.deactivateFromMap))]
        public static void BeforeDeactivate()
        {
            PlayerMovementController.CancelForMapChange();
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(M2MoverPr), nameof(M2MoverPr.newGame))]
        public static void BeforeNewGame()
        {
            PlayerMovementController.CancelForMapChange();
        }
    }
}
