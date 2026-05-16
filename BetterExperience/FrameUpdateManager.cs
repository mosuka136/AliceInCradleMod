using BetterExperience.HClassAttribute;
using System;
using UnityEngine;

namespace BetterExperience
{
    /// <summary>
    /// 提供一个全局帧更新分发点。
    /// 该管理器在游戏启动后创建隐藏的 Unity 组件，把 MonoBehaviour.Update 转换为普通事件，供不适合作为组件的补丁模块订阅。
    /// </summary>
    public static class FrameUpdateManager
    {
        /// <summary>
        /// 每帧触发一次。订阅者应自行处理异常和开关判断，避免阻塞其他订阅者。
        /// </summary>
        public static event Action OnFrameUpdate;

        /// <summary>
        /// 由 <see cref="GameBootManager"/> 创建的实际 Unity 更新组件。
        /// </summary>
        [RegisterOnGameBoot]
        public class Updater : MonoBehaviour
        {
            private void Awake()
            {
                HLog.Info("Frame update dispatcher created.");
            }

            private void Update()
            {
                try
                {
                    OnFrameUpdate?.Invoke();
                }
                catch (Exception ex)
                {
                    HLog.Error("An error occurred while invoking OnFrameUpdate event.", ex);
                }
            }
        }
    }
}
