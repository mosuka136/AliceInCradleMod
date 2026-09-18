using $safeprojectname$.BConfigManager;
using $safeprojectname$.BLogSpace;
using HarmonyLib;
using nel;
using System;

namespace $safeprojectname$.Patches
{
    public partial class HPatches
    {
        /// <summary>
        /// 示例补丁：关闭溺水伤害时重置缺氧和水中计时字段。
        /// 这里使用 Postfix 是为了让游戏原逻辑先更新周边状态，再覆盖会造成伤害的累计值。
        /// 新增补丁时复制本文件改为自己的目标类型/方法，并在 ConfigManager 中登记开关即可。
        /// </summary>
        [HarmonyPatch]
        public class DisableDrowningPatch
        {
            [HarmonyPostfix]
            [HarmonyPatch(typeof(M2PrMistApplier), "applyGasDamage")]
            public static void Postfix(M2PrMistApplier __instance)
            {
                try
                {
                    if (!ConfigManager.EnableNoDrowning.Value)
                        return;

                    Traverse.Create(__instance).Field("o2_point").SetValue(99.9f);
                    Traverse.Create(__instance).Field("t_water").SetValue(0f);
                    BLog.Debug($"{nameof(DisableDrowningPatch)} applied.");
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(DisableDrowningPatch)}", ex);
                }
            }
        }
    }
}
