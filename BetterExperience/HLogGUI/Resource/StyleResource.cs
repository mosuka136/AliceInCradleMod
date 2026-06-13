using BetterExperience.HGuiSpace;
using BetterExperience.HProvider;
using UnityEngine;

namespace BetterExperience.HLogGUI.Resource
{
    public class StyleResource : IStyleResource
    {
        public IUnityGuiProvider UnityGui { get; }

        public StyleResource(IUnityGuiProvider unityGui)
        {
            UnityGui = unityGui;
        }

        private GUIStyle _columnVisibilityToggleStyle;
        private GUIStyle _columnLogLevelToggleStyle;
        private GUIStyle _idButtonStyle;
        private GUIStyle _timestampButtonStyle;
        private GUIStyle _threadIdButtonStyle;
        private GUIStyle _frameButtonStyle;
        private GUIStyle _sceneButtonStyle;
        private GUIStyle _levelButtonStyle;
        private GUIStyle _messageButtonStyle;
        private GUIStyle _fileButtonStyle;
        private GUIStyle _lineButtonStyle;
        private GUIStyle _memberButtonStyle;
        private GUIStyle _exceptionButtonStyle;
        private GUIStyle _lastRepeatTimeButtonStyle;
        private GUIStyle _repeatCountButtonStyle;
        private GUIStyle _miscButtonStyle;
        private GUIStyle _toastStyle;
        private GUIStyle _tooltipStyle;

        public GUIStyle ColumnVisibilityToggleStyle => _columnVisibilityToggleStyle ?? (_columnVisibilityToggleStyle = CreateColumnVisibilityToggleStyle());
        public GUIStyle ColumnLogLevelToggleStyle => _columnLogLevelToggleStyle ?? (_columnLogLevelToggleStyle = CreateColumnLogLevelToggleStyle());
        public GUIStyle IdButtonStyle => _idButtonStyle ?? (_idButtonStyle = CreateIdButtonStyle());
        public GUIStyle TimestampButtonStyle => _timestampButtonStyle ?? (_timestampButtonStyle = CreateTimestampButtonStyle());
        public GUIStyle ThreadIdButtonStyle => _threadIdButtonStyle ?? (_threadIdButtonStyle = CreateThreadIdButtonStyle());
        public GUIStyle FrameButtonStyle => _frameButtonStyle ?? (_frameButtonStyle = CreateFrameButtonStyle());
        public GUIStyle SceneButtonStyle => _sceneButtonStyle ?? (_sceneButtonStyle = CreateSceneButtonStyle());
        public GUIStyle LevelButtonStyle => _levelButtonStyle ?? (_levelButtonStyle = CreateLevelButtonStyle());
        public GUIStyle MessageButtonStyle => _messageButtonStyle ?? (_messageButtonStyle = CreateMessageButtonStyle());
        public GUIStyle FileButtonStyle => _fileButtonStyle ?? (_fileButtonStyle = CreateFileButtonStyle());
        public GUIStyle LineButtonStyle => _lineButtonStyle ?? (_lineButtonStyle = CreateLineButtonStyle());
        public GUIStyle MemberButtonStyle => _memberButtonStyle ?? (_memberButtonStyle = CreateMemberButtonStyle());
        public GUIStyle ExceptionButtonStyle => _exceptionButtonStyle ?? (_exceptionButtonStyle = CreateExceptionButtonStyle());
        public GUIStyle LastRepeatTimeButtonStyle => _lastRepeatTimeButtonStyle ?? (_lastRepeatTimeButtonStyle = CreateLastRepeatTimeButtonStyle());
        public GUIStyle RepeatCountButtonStyle => _repeatCountButtonStyle ?? (_repeatCountButtonStyle = CreateRepeatCountButtonStyle());
        public GUIStyle MiscButtonStyle => _miscButtonStyle ?? (_miscButtonStyle = CreateMiscButtonStyle());
        public GUIStyle ToastStyle => _toastStyle ?? (_toastStyle = CreateToastStyle());
        public GUIStyle TooltipStyle => _tooltipStyle ?? (_tooltipStyle = CreateTooltipStyle());

        public GUIStyle CreateColumnVisibilityToggleStyle()
        {
            return new GUIStyle(UnityGui.ToggleStyle)
            {
                fontSize = 14,
            };
        }

        public GUIStyle CreateColumnLogLevelToggleStyle()
        {
            return new GUIStyle(UnityGui.ToggleStyle)
            {
                fontSize = 14,
                alignment = TextAnchor.MiddleCenter,
                stretchWidth = false
            };
        }

        public GUIStyle CreateIdButtonStyle()
        {
            return CreateColumnButtonStyle(TextAnchor.MiddleCenter, new Color(0.82f, 0.82f, 0.78f));
        }

        private GUIStyle CreateTimestampButtonStyle()
        {
            return CreateColumnButtonStyle(TextAnchor.MiddleCenter, new Color(0.38f, 0.76f, 0.94f));
        }

        private GUIStyle CreateThreadIdButtonStyle()
        {
            return CreateColumnButtonStyle(TextAnchor.MiddleCenter, new Color(0.58f, 0.68f, 1f));
        }

        private GUIStyle CreateFrameButtonStyle()
        {
            return CreateColumnButtonStyle(TextAnchor.MiddleCenter, new Color(0.42f, 0.86f, 0.48f));
        }

        private GUIStyle CreateSceneButtonStyle()
        {
            return CreateColumnButtonStyle(TextAnchor.MiddleLeft, new Color(0.98f, 0.72f, 0.34f));
        }

        private GUIStyle CreateLevelButtonStyle()
        {
            return CreateColumnButtonStyle(TextAnchor.MiddleCenter, new Color(1f, 0.46f, 0.42f));
        }

        private GUIStyle CreateMessageButtonStyle()
        {
            return CreateColumnButtonStyle(TextAnchor.MiddleLeft, new Color(0.9f, 0.88f, 0.78f));
        }

        private GUIStyle CreateFileButtonStyle()
        {
            return CreateColumnButtonStyle(TextAnchor.MiddleLeft, new Color(0.36f, 0.86f, 0.8f));
        }

        private GUIStyle CreateLineButtonStyle()
        {
            return CreateColumnButtonStyle(TextAnchor.MiddleCenter, new Color(0.74f, 0.88f, 0.36f));
        }

        private GUIStyle CreateMemberButtonStyle()
        {
            return CreateColumnButtonStyle(TextAnchor.MiddleLeft, new Color(0.96f, 0.56f, 0.9f));
        }

        private GUIStyle CreateExceptionButtonStyle()
        {
            return CreateColumnButtonStyle(TextAnchor.MiddleLeft, new Color(1f, 0.36f, 0.58f));
        }

        private GUIStyle CreateLastRepeatTimeButtonStyle()
        {
            return CreateColumnButtonStyle(TextAnchor.MiddleCenter, new Color(0.68f, 0.58f, 1f));
        }

        private GUIStyle CreateRepeatCountButtonStyle()
        {
            return CreateColumnButtonStyle(TextAnchor.MiddleCenter, new Color(1f, 0.86f, 0.3f));
        }

        private GUIStyle CreateMiscButtonStyle()
        {
            return CreateColumnButtonStyle(TextAnchor.MiddleCenter, new Color(0.7f, 0.84f, 0.92f));
        }

        private GUIStyle CreateColumnButtonStyle(TextAnchor alignment, Color textColor)
        {
            var style = new GUIStyle(UnityGui.ButtonStyle)
            {
                fontStyle = FontStyle.Normal,
                fontSize = 14,
                alignment = alignment,
                clipping = TextClipping.Clip,
                wordWrap = false,
                stretchWidth = false
            };

            style.normal.textColor = textColor;
            style.hover.textColor = textColor;
            style.active.textColor = textColor;
            style.focused.textColor = textColor;
            return style;
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
