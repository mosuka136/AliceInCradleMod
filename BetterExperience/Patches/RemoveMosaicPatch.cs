using BetterExperience.BConfigManager;
using HarmonyLib;
using nel;
using System;

namespace BetterExperience.Patches
{
    public partial class HPatches
    {
        /// <summary>
        /// 关闭马赛克绘制。
        /// 配置关闭时在绘制函数后覆盖返回值，避免原绘制结果继续生效。
        /// </summary>
        [HarmonyPatch(typeof(MosaicShower), "FnDrawMosaic")]
        public class RemoveMosaicPatch
        {
            private static bool _hasLoggedActivation = false;

            public static void Postfix(ref bool __result)
            {
                try
                {
                    if (ConfigManager.EnableMosaic.Value)
                        return;

                    __result = false;

                    if (!_hasLoggedActivation)
                    {
                        HLog.Debug($"{nameof(RemoveMosaicPatch)} applied.");
                        _hasLoggedActivation = true;
                    }
                }
                catch (Exception ex)
                {
                    HLog.Error($"Unexpected error in {nameof(RemoveMosaicPatch)}.", ex);
                }
            }
        }
    }
}
