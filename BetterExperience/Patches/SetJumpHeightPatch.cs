using BetterExperience.BConfigManager;
using HarmonyLib;
using nel;

namespace BetterExperience.Patches
{
    [HarmonyPatch(typeof(PR), "jump_speed_ratio", MethodType.Getter)]
    internal static class SetJumpHeightPatch
    {
        [HarmonyPostfix]
        public static void Postfix(ref float __result)
        {
            if (ConfigManager.EnableBetterExperience?.Value != true)
                return;

            __result = ApplyMultiplier(__result, ConfigManager.PlayerJumpMultiplier?.Value ?? 1f);
        }

        // 保留 Ser.jumpSpeedRate 等原有修正；异常配置回退原结果，避免向物理系统传入非有限数值。
        internal static float ApplyMultiplier(float original, float multiplier)
        {
            if (!MovementGeometry.IsFinite(original) || !MovementGeometry.IsFinite(multiplier) ||
                multiplier < 0.1f || multiplier > 5f)
                return original;

            float result = original * multiplier;
            return MovementGeometry.IsFinite(result) ? result : original;
        }
    }
}
