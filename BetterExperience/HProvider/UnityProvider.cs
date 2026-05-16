using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace BetterExperience.HProvider
{
    /// <summary>
    /// Unity API 访问适配器。
    /// 该类型集中封装时间、输入、场景和少量数学函数，便于非 MonoBehaviour 代码访问 Unity 状态，也让 GUI/热键逻辑更容易隔离。
    /// </summary>
    public class UnityProvider
    {
        public float DeltaTime => Time.deltaTime;
        public float UnscaledDeltaTime => Time.unscaledDeltaTime;
        public float RealtimeSinceStartup => Time.realtimeSinceStartup;
        public int FrameCount => Time.frameCount;

        public Keyboard KeyboardCurrent => Keyboard.current;
        public Gamepad GamepadCurrent => Gamepad.current;

        /// <summary>
        /// 当前 IMGUI 事件中的鼠标位置。只能在 OnGUI 调用链内可靠使用。
        /// </summary>
        public Vector2 CurrentMousePosition => Event.current.mousePosition;

        public event Action UnityQuitting
        {
            add => Application.quitting += value;
            remove => Application.quitting -= value;
        }

        public Scene ActiveScene => SceneManager.GetActiveScene();

        public void DebugLog(object message)
        {
            Debug.Log(message);
        }

        public float Clamp(float value, float min, float max)
        {
            return Mathf.Clamp(value, min, max);
        }

        public bool Approximately(float a, float b)
        {
            return Mathf.Approximately(a, b);
        }

        public float Round(float value)
        {
            return Mathf.Round(value);
        }

        public float Min(float a, float b)
        {
            return a < b ? a : b;
        }

        public float Max(float a, float b)
        {
            return a > b ? a : b;
        }
    }
}
