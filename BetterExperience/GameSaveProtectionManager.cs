using HarmonyLib;
using nel;
using System;

namespace BetterExperience
{
    /// <summary>
    /// 暴露游戏保存前后的事件。
    /// 某些补丁会临时改写运行时数据以改善体验，但保存文件应尽量记录游戏原本可推导的数据；这些事件用于保存前恢复、保存后再应用修改。
    /// </summary>
    public static class GameSaveProtectionManager
    {
        /// <summary>
        /// 游戏开始构造保存数据前触发。
        /// </summary>
        public static event Action OnSavingActivated;

        /// <summary>
        /// 游戏保存二进制数据完成后触发。
        /// </summary>
        public static event Action OnSavingCompleted;

        /// <summary>
        /// 挂接保存流程的前后两个关键点。
        /// </summary>
        [HarmonyPatch]
        public class GameSaveProtectionPatch
        {
            [HarmonyPrefix]
            [HarmonyPatch(typeof(COOK), nameof(COOK.createBinary))]
            public static void SaveGamePrefix()
            {
                HLog.Info("Detected game saving activation.");

                try
                {
                    OnSavingActivated?.Invoke();
                }
                catch (Exception ex)
                {
                    HLog.Error("An error occurred while invoking OnSavingActivated event.", ex);
                }
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(SVD), nameof(SVD.saveBinary))]
            public static void SaveGamePostfix()
            {
                HLog.Info("Detected game saving completion.");

                try
                {
                    OnSavingCompleted?.Invoke();
                }
                catch (Exception ex)
                {
                    HLog.Error("An error occurred while invoking OnSavingCompleted event.", ex);
                }
            }
        }
    }
}
