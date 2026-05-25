using BetterExperience.HClassAttribute;
using BetterExperience.HLogSpace;
using BetterExperience.HProvider;
using System;
using System.Linq;

namespace BetterExperience
{
    public static class GameQuitManager
    {
        private static bool _initialized = false;

        public static event Action OnGameQuit;

        [InitializeOnGameBoot]
        public static void Initialize()
        {
            if (_initialized)
                return;

            var unityProvider = new UnityProvider();
            unityProvider.UnityQuitting += Dispose;
            HLog.Info("GameQuitManager initialized.");

            _initialized = true;
        }

        public static void Dispose()
        {
            foreach (var handler in (OnGameQuit?.GetInvocationList() ?? Array.Empty<Delegate>()).Cast<Action>())
            {
                try
                {
                    handler?.Invoke();
                }
                catch
                {
                }
            }
        }
    }
}
