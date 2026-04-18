using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BetterExperience.HAdapter
{
    public static class UnityEngineAdapter
    {
        public static event Action UnityQuitting
        {
            add => Application.quitting += value;
            remove => Application.quitting -= value;
        }

        public static Scene ActiveScene => SceneManager.GetActiveScene();

        public static void DebugLog(object message)
        {
            Debug.Log(message);
        }
    }
}
