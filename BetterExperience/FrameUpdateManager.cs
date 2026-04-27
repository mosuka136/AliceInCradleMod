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
            private void Update()
            {
                OnFrameUpdate?.Invoke();
            }
        }
    }
}
