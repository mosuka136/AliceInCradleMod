using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace BetterExperience.HProvider
{
    public interface IUnityProvider
    {
        float DeltaTime { get; }
        float UnscaledDeltaTime { get; }
        float RealtimeSinceStartup { get; }
        int FrameCount { get; }

        Keyboard KeyboardCurrent { get; }
        Gamepad GamepadCurrent { get; }

        Vector2 CurrentMousePosition { get; }

        event Action UnityQuitting;

        Scene ActiveScene { get; }

        void DebugLog(object message);
        float Clamp(float value, float min, float max);
        bool Approximately(float a, float b);
        float Round(float value);
        float Min(float a, float b);
        float Max(float a, float b);
        void ClipboardCopy(string text);
        string ClipboardPaste();
    }
}
