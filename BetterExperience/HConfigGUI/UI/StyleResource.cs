using UnityEngine;

namespace BetterExperience.HConfigGUI.UI
{
    public class StyleResource
    {
        private StyleResource()
        {
            
        }
        public static StyleResource Instance { get; } = new StyleResource();

        private GUIStyle _tableTitleStyle;
        private GUIStyle _tooltipStyle;
        private GUIStyle _sliderStyle;
        private GUIStyle _sliderThumbStyle;
        private GUIStyle _toastStyle;

        public GUIStyle TableTitleStyle => _tableTitleStyle ?? (_tableTitleStyle = CreateTableTitleStyle());
        public GUIStyle TooltipStyle => _tooltipStyle ?? (_tooltipStyle = CreateTooltipStyle());
        public GUIStyle SliderStyle => _sliderStyle ?? (_sliderStyle = CreateSliderStyle());
        public GUIStyle SliderThumbStyle => _sliderThumbStyle ?? (_sliderThumbStyle = CreateSliderThumbStyle());
        public GUIStyle ToastStyle => _toastStyle ?? (_toastStyle = CreateToastStyle());

        private GUIStyle CreateTableTitleStyle()
        {
            return new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                fontSize = 14
            };
        }

        private GUIStyle CreateTooltipStyle()
        {
            var style = new GUIStyle(GUI.skin.box)
            {
                wordWrap = true,
                alignment = TextAnchor.MiddleCenter,
                padding = new RectOffset(6, 6, 4, 4)
            };
            style.normal.background = CreateSolidTexture(new Color(0.15f, 0.15f, 0.15f, 0.95f));
            style.normal.textColor = Color.white;
            return style;
        }

        private GUIStyle CreateSliderStyle()
        {
            var baseStyle = GUI.skin.horizontalSlider;
            return new GUIStyle(baseStyle)
            {
                margin = new RectOffset(baseStyle.margin.left, baseStyle.margin.right, 6, 6),
                fixedHeight = 18f
            };
        }

        private GUIStyle CreateSliderThumbStyle()
        {
            return new GUIStyle(GUI.skin.horizontalSliderThumb)
            {
                margin = new RectOffset(0, 0, 2, 0),
                fixedWidth = 17f,
                fixedHeight = 13f
            };
        }

        private GUIStyle CreateToastStyle()
        {
            var style = new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.MiddleCenter,
                padding = new RectOffset(10, 10, 6, 6)
            };
            style.normal.background = CreateSolidTexture(new Color(0f, 0.3f, 0.25f, 1f));
            style.normal.textColor = Color.white;
            return style;
        }

        private Texture2D CreateSolidTexture(Color color)
        {
            var texture = new Texture2D(1, 1);
            texture.hideFlags = HideFlags.HideAndDontSave;
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return texture;
        }
    }
}
