using UnityEngine;

namespace BetterExperience.HAdapter
{
    public class GuiStyleAdapter
    {
        public static GuiStyleAdapter LabelStyle => GUI.skin.label;
        public static GuiStyleAdapter ButtonStyle => GUI.skin.button;
        public static GuiStyleAdapter TextFieldStyle => GUI.skin.textField;
        public static GuiStyleAdapter HorizontalSlider => GUI.skin.horizontalSlider;
        public static GuiStyleAdapter HorizontalSliderThumb => GUI.skin.horizontalSliderThumb;
        public static GuiStyleAdapter BoxStyle => GUI.skin.box;

        public GUIStyle Style { get; }

        public GuiStyleAdapter(GUIStyle style)
        {
            Style = style;
        }

        public static implicit operator GuiStyleAdapter(GUIStyle style)
        {
            return new GuiStyleAdapter(style);
        }

        public Vector2 CalcSize(GuiContentAdapter content)
        {
            return Style.CalcSize(content.Content);
        }

        public float CalcHeight(GuiContentAdapter content, float width)
        {
            return Style.CalcHeight(content.Content, width);
        }
    }
}
