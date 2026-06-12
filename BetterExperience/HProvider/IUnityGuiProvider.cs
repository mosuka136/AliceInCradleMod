using UnityEngine;

namespace BetterExperience.HProvider
{
    public interface IUnityGuiProvider
    {
        string Tooltip { get; }
        GUIStyle LabelStyle { get; }
        GUIStyle ToggleStyle { get; }
        GUIStyle ButtonStyle { get; }
        GUIStyle TextFieldStyle { get; }
        GUIStyle HorizontalSliderStyle { get; }
        GUIStyle HorizontalSliderThumbStyle { get; }
        GUIStyle BoxStyle { get; }
        Color Color { get; set; }
        float ScreenWidth { get; }
        float ScreenHeight { get; }

        Rect ModalWindow(int id, Rect clientRect, GUI.WindowFunction func, string title, GUIStyle style);
        void BeginArea(Rect screenRect);
        void EndArea();
        void BeginHorizontal(params GUILayoutOption[] options);
        void BeginHorizontal(GUIStyle style, params GUILayoutOption[] options);
        void EndHorizontal();
        Vector2 BeginScrollView(Vector2 scrollPosition, params GUILayoutOption[] options);
        void EndScrollView();
        void BeginVertical();
        void BeginVertical(GUIStyle style, params GUILayoutOption[] options);
        void EndVertical();
        void Space(float pixels);
        void FlexibleSpace();
        void Label(string text, params GUILayoutOption[] options);
        void Label(GUIContent content, params GUILayoutOption[] options);
        void Label(string text, GUIStyle style, params GUILayoutOption[] options);
        void Label(GUIContent content, GUIStyle style, params GUILayoutOption[] options);
        void Label(Rect position, GUIContent content, GUIStyle style);
        bool Toggle(bool value, string text, params GUILayoutOption[] options);
        bool Toggle(bool value, string text, GUIStyle style, params GUILayoutOption[] options);
        string TextField(string text, params GUILayoutOption[] options);
        bool Button(string text, params GUILayoutOption[] options);
        bool Button(string text, GUIStyle style, params GUILayoutOption[] options);
        float HorizontalSlider(float value, float leftValue, float rightValue, GUIStyle slider, GUIStyle thumb, params GUILayoutOption[] options);
        int SelectionGrid(int selected, string[] texts, int xCount, params GUILayoutOption[] options);
        void Box(Rect position, string text);
        GUILayoutOption Width(float width);
        GUILayoutOption MinWidth(float width);
        GUILayoutOption ExpandWidth(bool expand);
        Rect GetRect(float x, float y, float width, float height);
        Color GetColor(float r, float g, float b, float a);
        GUIContent GetContent(string content);
        GUIContent GetContent(string content, string tooltip);
    }
}
