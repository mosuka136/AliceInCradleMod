using BetterExperience.HProvider;
using UnityEngine;

namespace BetterExperience.HGuiSpace
{
    public class ToastEditor
    {
        public IUnityProvider UnityService { get; }
        public IUnityGuiProvider UnityGui { get; }
        public IStyleResource StyleProvider { get; }

        public string Message { get; set; }
        public float Duration { get; set; } = 2f;
        public float FadeDuration { get; set; } = 0.5f;
        public float EndTime { get; set; }

        public ToastEditor(IUnityProvider unityService, IUnityGuiProvider unityGui, IStyleResource styleProvider)
        {
            UnityService = unityService;
            UnityGui = unityGui;
            StyleProvider = styleProvider;
        }

        public void SetToast(string message)
        {
            Message = message;
            EndTime = UnityService.RealtimeSinceStartup + Duration;
        }

        public void DrawToast(Rect windowRect)
        {
            if (string.IsNullOrEmpty(Message))
                return;

            float remaining = EndTime - UnityService.RealtimeSinceStartup;
            if (remaining <= 0f)
            {
                Message = null;
                return;
            }

            float alpha = remaining < FadeDuration ? remaining / FadeDuration : 1f;
            var previousColor = UnityGui.Color;
            UnityGui.Color = UnityGui.GetColor(1f, 1f, 1f, alpha);

            var content = UnityGui.GetContent(Message);
            var size = StyleProvider.ToastStyle.CalcSize(content);
            float toastWidth = UnityService.Min(size.x + 20f, windowRect.width - 20f);
            float toastHeight = size.y + 4f;
            float x = (windowRect.width - toastWidth) / 2f;
            float y = UnityService.Max(0f, windowRect.height - toastHeight - 30f);

            UnityGui.Label(UnityGui.GetRect(x, y, toastWidth, toastHeight), content, StyleProvider.ToastStyle);
            UnityGui.Color = previousColor;
        }
    }
}
