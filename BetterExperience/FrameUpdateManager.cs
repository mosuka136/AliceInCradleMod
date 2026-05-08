using BetterExperience.HClassAttribute;
using System;
using UnityEngine;

namespace BetterExperience
{
    public static class FrameUpdateManager
    {
        public static event Action OnFrameUpdate;

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
