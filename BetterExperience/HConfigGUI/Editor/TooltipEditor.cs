using BetterExperience.HConfigGUI.Resource;
using BetterExperience.HProvider;
using UnityEngine;

namespace BetterExperience.HConfigGUI.Editor
{
    public class TooltipEditor
    {
        public IUnityProvider UnityService { get; }
        public IUnityGuiProvider UnityGui { get; }
        public StyleResource StyleProvider { get; }

        public TooltipEditor(IUnityProvider unityService, IUnityGuiProvider unityGui, StyleResource styleProvider)
        {
            UnityService = unityService;
            UnityGui = unityGui;
            StyleProvider = styleProvider;
        }

        public void DrawTooltip(Rect windowRect)
        {
            if (string.IsNullOrEmpty(UnityGui.Tooltip))
                return;

            var tooltipContent = UnityGui.GetContent(UnityGui.Tooltip);
            float maxTooltipWidth = windowRect.width * 0.6f;
            float tooltipHeight = StyleProvider.TooltipStyle.CalcHeight(tooltipContent, maxTooltipWidth);

            var mousePosition = UnityService.CurrentMousePosition;
            float x = UnityService.Clamp(mousePosition.x + 15f, 0f, windowRect.width - maxTooltipWidth);
            float y = UnityService.Clamp(mousePosition.y + 15f, 0f, windowRect.height - tooltipHeight);

            UnityGui.Label(UnityGui.GetRect(x, y, maxTooltipWidth, tooltipHeight), tooltipContent, StyleProvider.TooltipStyle);
        }
    }
}
