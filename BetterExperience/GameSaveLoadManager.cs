using evt;
using HarmonyLib;
using System;

namespace BetterExperience
{
    /// <summary>
    /// 监听游戏存档读取完成信号。
    /// 该管理器把游戏事件脚本中的初始化标记转换为插件事件，供预加载类补丁在存档数据可用后一次性应用配置。
    /// </summary>
    public static class GameSaveLoadManager
    {
        /// <summary>
        /// 存档或新游戏初始化完成后触发。
        /// </summary>
        public static event Action OnGameSaveLoadCompleted;

        /// <summary>
        /// 通过 <c>EV.stack("__INITNEWGAME")</c> 识别游戏完成初始化的时机。
        /// </summary>
        [HarmonyPatch]
        public class GameSaveLoadPatch
        {
            [HarmonyPostfix]
            [HarmonyPatch(typeof(EV), nameof(EV.stack))]
            public static void LoadCompletedPostfix(string _name)
            {
                if (_name != "__INITNEWGAME")
                    return;

                HLog.Info("Detected game save/load completion event.");

                try
                {
                    OnGameSaveLoadCompleted?.Invoke();
                }
                catch (Exception ex)
                {
                    HLog.Error("An error occurred while invoking OnGameSaveLoadCompleted event.", ex);
                }
            }
        }
    }
}
