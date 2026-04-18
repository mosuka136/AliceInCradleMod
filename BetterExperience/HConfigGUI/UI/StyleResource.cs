using BetterExperience.HAdapter;
using UnityEngine;
using Color = UnityEngine.Color;

namespace BetterExperience.HConfigGUI.UI
{
    public class StyleResource
    {
        private StyleResource()
        {
            
        }
        public static StyleResource Instance { get; } = new StyleResource();

        private GuiStyleAdapter _tableTitleStyle;
        private GuiStyleAdapter _tooltipStyle;
        private GuiStyleAdapter _sliderStyle;
        private GuiStyleAdapter _sliderThumbStyle;
        private GuiStyleAdapter _toastStyle;

        public GuiStyleAdapter TableTitleStyle => _tableTitleStyle ?? (_tableTitleStyle = CreateTableTitleStyle());
        public GuiStyleAdapter TooltipStyle => _tooltipStyle ?? (_tooltipStyle = CreateTooltipStyle());
        public GuiStyleAdapter SliderStyle => _sliderStyle ?? (_sliderStyle = CreateSliderStyle());
        public GuiStyleAdapter SliderThumbStyle => _sliderThumbStyle ?? (_sliderThumbStyle = CreateSliderThumbStyle());
        public GuiStyleAdapter ToastStyle => _toastStyle ?? (_toastStyle = CreateToastStyle());

        private GuiStyleAdapter CreateTableTitleStyle()
        {
            return new GUIStyle(GuiStyleAdapter.LabelStyle.Style)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                fontSize = 14
            };
        }

        private GuiStyleAdapter CreateTooltipStyle()
        {
            var style = new GUIStyle(GuiStyleAdapter.BoxStyle.Style)
            {
                wordWrap = true,
                alignment = TextAnchor.MiddleCenter,
                padding = new RectOffset(6, 6, 4, 4)
            };
            style.normal.background = CreateSolidTexture(new Color(0.15f, 0.15f, 0.15f, 0.95f));
            style.normal.textColor = Color.white;
            return style;
        }

        private GuiStyleAdapter CreateSliderStyle()
        {
            var baseStyle = GuiStyleAdapter.HorizontalSlider.Style;
            return new GUIStyle(baseStyle)
            {
                margin = new RectOffset(baseStyle.margin.left, baseStyle.margin.right, 6, 6),
                fixedHeight = 18f
            };
        }

        private GuiStyleAdapter CreateSliderThumbStyle()
        {
            return new GUIStyle(GuiStyleAdapter.HorizontalSliderThumb.Style)
            {
                margin = new RectOffset(0, 0, 2, 0),
                fixedWidth = 17f,
                fixedHeight = 13f
            };
        }

        private GuiStyleAdapter CreateToastStyle()
        {
            var style = new GUIStyle(GuiStyleAdapter.BoxStyle.Style)
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
