using BetterExperience.HGuiSpace;
using BetterExperience.HProvider;
using UnityEngine;

namespace BetterExperience.HConfigGUI.Resource
{
    /// <summary>
    /// 配置界面的 IMGUI 样式工厂。
    /// 样式按需创建并缓存，避免在每帧 OnGUI 中重复分配 GUIStyle 和纹理对象。
    /// </summary>
    public class StyleResource : IStyleResource
    {
        public IUnityGuiProvider UnityGui { get; }

        public StyleResource(IUnityGuiProvider unityGui)
        {
            UnityGui = unityGui;
        }

        private GUIStyle _popupTitleStyle;
        private GUIStyle _sidebarEntryStyle;
        private GUIStyle _sidebarSelectedEntryStyle;
        private GUIStyle _tableTitleStyle;
        private GUIStyle _recordingHotkeyLabelStyle;
        private GUIStyle _sliderStyle;
        private GUIStyle _sliderThumbStyle;
        private GUIStyle _toastStyle;
        private GUIStyle _tooltipStyle;

        public GUIStyle PopupTitleStyle => _popupTitleStyle ?? (_popupTitleStyle = CreatePopupTitleStyle());
        public GUIStyle SidebarEntryStyle => _sidebarEntryStyle ?? (_sidebarEntryStyle = CreateSidebarEntryStyle());
        public GUIStyle SidebarSelectedEntryStyle => _sidebarSelectedEntryStyle ?? (_sidebarSelectedEntryStyle = CreateSidebarSelectedEntryStyle());
        public GUIStyle TableTitleStyle => _tableTitleStyle ?? (_tableTitleStyle = CreateTableTitleStyle());
        public GUIStyle RecordingHotkeyLabelStyle => _recordingHotkeyLabelStyle ?? (_recordingHotkeyLabelStyle = CreateRecordingHotkeyLabelStyle());
        public GUIStyle SliderStyle => _sliderStyle ?? (_sliderStyle = CreateSliderStyle());
        public GUIStyle SliderThumbStyle => _sliderThumbStyle ?? (_sliderThumbStyle = CreateSliderThumbStyle());
        public GUIStyle ToastStyle => _toastStyle ?? (_toastStyle = CreateToastStyle());
        public GUIStyle TooltipStyle => _tooltipStyle ?? (_tooltipStyle = CreateTooltipStyle());

        private GUIStyle CreatePopupTitleStyle()
        {
            return new GUIStyle(UnityGui.LabelStyle)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                fontSize = 14
            };
        }

        private GUIStyle CreateSidebarEntryStyle()
        {
            return new GUIStyle(UnityGui.ButtonStyle)
            {
                fontStyle = FontStyle.Normal,
                fontSize = 14,
                alignment = TextAnchor.MiddleCenter,
                fixedHeight = 40f,
            };
        }

        private GUIStyle CreateSidebarSelectedEntryStyle()
        {
            return new GUIStyle(UnityGui.ButtonStyle)
            {
                fontStyle = FontStyle.Bold,
                fontSize = 15,
                alignment = TextAnchor.MiddleCenter,
                fixedHeight = 40f,
            };
        }

        private GUIStyle CreateTableTitleStyle()
        {
            return new GUIStyle(UnityGui.LabelStyle)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                fontSize = 14
            };
        }

        private GUIStyle CreateRecordingHotkeyLabelStyle()
        {
            return new GUIStyle(UnityGui.LabelStyle)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.BoldAndItalic,
                fontSize = 22,
            };
        }

        private GUIStyle CreateSliderStyle()
        {
            var baseStyle = UnityGui.HorizontalSliderStyle;
            return new GUIStyle(baseStyle)
            {
                margin = new RectOffset(baseStyle.margin.left, baseStyle.margin.right, 6, 6),
                fixedHeight = 18f
            };
        }

        private GUIStyle CreateSliderThumbStyle()
        {
            return new GUIStyle(UnityGui.HorizontalSliderThumbStyle)
            {
                margin = new RectOffset(0, 0, 2, 0),
                fixedWidth = 17f,
                fixedHeight = 13f
            };
        }

        private Texture2D CreateSolidTexture(Color color)
        {
            var texture = new Texture2D(1, 1);
            // 样式背景纹理只服务于运行时 GUI，不应随场景保存或显示在层级中。
            texture.hideFlags = HideFlags.HideAndDontSave;
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return texture;
        }

        private GUIStyle CreateToastStyle()
        {
            var style = new GUIStyle(UnityGui.BoxStyle)
            {
                alignment = TextAnchor.MiddleCenter,
                padding = new RectOffset(10, 10, 6, 6)
            };
            style.normal.background = CreateSolidTexture(new Color(0f, 0.3f, 0.25f, 1f));
            style.normal.textColor = Color.white;
            return style;
        }

        private GUIStyle CreateTooltipStyle()
        {
            var style = new GUIStyle(UnityGui.BoxStyle)
            {
                wordWrap = true,
                alignment = TextAnchor.MiddleCenter,
                padding = new RectOffset(6, 6, 4, 4)
            };
            style.normal.background = CreateSolidTexture(new Color(0.15f, 0.15f, 0.15f, 0.95f));
            style.normal.textColor = Color.white;
            return style;
        }
    }
}
