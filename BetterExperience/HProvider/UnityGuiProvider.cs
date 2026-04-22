using UnityEngine;

namespace BetterExperience.HProvider
{
    public class UnityGuiProvider
    {
        public string Tooltip => GUI.tooltip;
        public GUIStyle LabelStyle => GUI.skin.label;
        public GUIStyle ButtonStyle => GUI.skin.button;
        public GUIStyle TextFieldStyle => GUI.skin.textField;
        public GUIStyle HorizontalSliderStyle => GUI.skin.horizontalSlider;
        public GUIStyle HorizontalSliderThumbStyle => GUI.skin.horizontalSliderThumb;
        public GUIStyle BoxStyle => GUI.skin.box;
        public Color Color
        {
            get => GUI.color;
            set => GUI.color = value;
        }

        public bool Contains(Vector2 point)
        {
            return false;
        }

        public GUIStyle Style { get; }

        public Vector2 CalcSize(GUIContent content)
        {
            return Style.CalcSize(content);
        }

        public float CalcHeight(GUIContent content, float width)
        {
            return Style.CalcHeight(content, width);
        }

        public void BeginArea(Rect screenRect)
        {
            GUILayout.BeginArea(screenRect);
        }

        public void EndArea()
        {
            GUILayout.EndArea();
        }

        public void BeginHorizontal(params GUILayoutOption[] options)
        {
            GUILayout.BeginHorizontal(options);
        }

        public void EndHorizontal()
        {
            GUILayout.EndHorizontal();
        }

        public Vector2 BeginScrollView(Vector2 scrollPosition, params GUILayoutOption[] options)
        {
            return GUILayout.BeginScrollView(scrollPosition, options);
        }

        public void EndScrollView()
        {
            GUILayout.EndScrollView();
        }

        public void BeginVertical(GUIStyle style, params GUILayoutOption[] options)
        {
            GUILayout.BeginVertical(style != null ? style : GUIStyle.none, options);
        }

        public void EndVertical()
        {
            GUILayout.EndVertical();
        }

        public void Space(float pixels)
        {
            GUILayout.Space(pixels);
        }

        public void Label(GUIContent content, params GUILayoutOption[] options)
        {
            GUILayout.Label(content, options);
        }

        public void Label(GUIContent content, GUIStyle style, params GUILayoutOption[] options)
        {
            GUILayout.Label(content, style != null ? style : GUIStyle.none, options);
        }

        public void Label(Rect position, GUIContent content, GUIStyle style)
        {
            GUI.Label(position, content, style);
        }

        public bool Toggle(bool value, string text, params GUILayoutOption[] options)
        {
            return GUILayout.Toggle(value, text, options);
        }

        public string TextField(string text, params GUILayoutOption[] options)
        {
            return GUILayout.TextField(text, options);
        }

        public bool Button(string text, params GUILayoutOption[] options)
        {
            return GUILayout.Button(text, options);
        }

        public float HorizontalSlider(float value, float leftValue, float rightValue, GUIStyle slider, GUIStyle thumb, params GUILayoutOption[] options)
        {
            return GUILayout.HorizontalSlider(value, leftValue, rightValue, slider != null ? slider : GUIStyle.none, thumb != null ? thumb : GUIStyle.none, options);
        }

        public int SelectionGrid(int selected, string[] texts, int xCount, params GUILayoutOption[] options)
        {
            return GUILayout.SelectionGrid(selected, texts, xCount, options);
        }

        public GUILayoutOption Width(float width)
        {
            return GUILayout.Width(width);
        }

        public GUILayoutOption MinWidth(float width)
        {
            return GUILayout.MinWidth(width);
        }

        public GUILayoutOption ExpandWidth(bool expand)
        {
            return GUILayout.ExpandWidth(expand);
        }

        public Rect GetRect(float x, float y, float width, float height)
        {
            return new Rect(x, y, width, height);
        }

        public Color GetColor(float r, float g, float b, float a)
        {
            return new Color(r, g, b, a);
        }

        public GUIContent GetContent(string content)
        {
            return new GUIContent(content);
        }

        public GUIContent GetContent(string content, string tooltip)
        {
            return new GUIContent(content, tooltip);
        }
    }
}
