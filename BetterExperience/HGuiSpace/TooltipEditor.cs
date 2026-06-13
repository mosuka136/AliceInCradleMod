using BetterExperience.HProvider;
using UnityEngine;

namespace BetterExperience.HGuiSpace
{
    public class TooltipEditor
    {
        public IUnityProvider UnityService { get; }
        public IUnityGuiProvider UnityGui { get; }
        public IStyleResource StyleProvider { get; }

        public TooltipEditor(IUnityProvider unityService, IUnityGuiProvider unityGui, IStyleResource styleProvider)
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
            float tooltipWidth = UnityService.Min(StyleProvider.TooltipStyle.CalcSize(tooltipContent).x + 30f, maxTooltipWidth);
            float tooltipHeight = StyleProvider.TooltipStyle.CalcHeight(tooltipContent, tooltipWidth);

            var mousePosition = UnityService.CurrentMousePosition;
            float x = UnityService.Clamp(mousePosition.x + 15f, 0f, windowRect.width - tooltipWidth);
            float y = UnityService.Clamp(mousePosition.y + 15f, 0f, windowRect.height - tooltipHeight);

            UnityGui.Label(UnityGui.GetRect(x, y, tooltipWidth, tooltipHeight), tooltipContent, StyleProvider.TooltipStyle);
        }
    }
}
