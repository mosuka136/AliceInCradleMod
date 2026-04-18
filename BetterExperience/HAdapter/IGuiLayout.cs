using System.Linq;
using UnityEngine;

namespace BetterExperience.HAdapter
{
    public interface IGuiLayout
    {
        void BeginArea(Rect screenRect);
        void EndArea();
        void BeginHorizontal(params GuiLayoutOptionAdapter[] options);
        void EndHorizontal();
        Vector2 BeginScrollView(Vector2 scrollPosition, params GuiLayoutOptionAdapter[] options);
        void EndScrollView();
        void BeginVertical(GuiStyleAdapter style, params GuiLayoutOptionAdapter[] options);
        void EndVertical();
        void Space(float pixels);
        void Label(GuiContentAdapter content, params GuiLayoutOptionAdapter[] options);
        void Label(GuiContentAdapter content, GuiStyleAdapter style, params GuiLayoutOptionAdapter[] options);
        bool Toggle(bool value, string text, params GuiLayoutOptionAdapter[] options);
        string TextField(string text, params GuiLayoutOptionAdapter[] options);
        bool Button(string text, params GuiLayoutOptionAdapter[] options);
        float HorizontalSlider(float value, float leftValue, float rightValue, GuiStyleAdapter slider, GuiStyleAdapter thumb, params GuiLayoutOptionAdapter[] options);
        int SelectionGrid(int selected, string[] texts, int xCount, params GuiLayoutOptionAdapter[] options);
        GuiLayoutOptionAdapter Width(float width);
        GuiLayoutOptionAdapter MinWidth(float width);
        GuiLayoutOptionAdapter ExpandWidth(bool expand);
    }

    public class GuiLayoutAdapter : IGuiLayout
    {
        public void BeginArea(Rect screenRect)
        {
            GUILayout.BeginArea(screenRect);
        }

        public void EndArea()
        {
            GUILayout.EndArea();
        }

        public void BeginHorizontal(params GuiLayoutOptionAdapter[] options)
        {
            GUILayout.BeginHorizontal(options.Select(o => o.Option).ToArray());
        }

        public void EndHorizontal()
        {
            GUILayout.EndHorizontal();
        }

        public Vector2 BeginScrollView(Vector2 scrollPosition, params GuiLayoutOptionAdapter[] options)
        {
            return GUILayout.BeginScrollView(scrollPosition, options.Select(o => o.Option).ToArray());
        }

        public void EndScrollView()
        {
            GUILayout.EndScrollView();
        }

        public void BeginVertical(GuiStyleAdapter style, params GuiLayoutOptionAdapter[] options)
        {
            GUILayout.BeginVertical(style != null ? style.Style : GUIStyle.none, options.Select(o => o.Option).ToArray());
        }

        public void EndVertical()
        {
            GUILayout.EndVertical();
        }

        public void Space(float pixels)
        {
            GUILayout.Space(pixels);
        }

        public void Label(GuiContentAdapter content, params GuiLayoutOptionAdapter[] options)
        {
            GUILayout.Label(content.Content, options.Select(o => o.Option).ToArray());
        }

        public void Label(GuiContentAdapter content, GuiStyleAdapter style, params GuiLayoutOptionAdapter[] options)
        {
            GUILayout.Label(content.Content, style != null ? style.Style : GUIStyle.none, options.Select(o => o.Option).ToArray());
        }

        public bool Toggle(bool value, string text, params GuiLayoutOptionAdapter[] options)
        {
            return GUILayout.Toggle(value, text, options.Select(o => o.Option).ToArray());
        }

        public string TextField(string text, params GuiLayoutOptionAdapter[] options)
        {
            return GUILayout.TextField(text, options.Select(o => o.Option).ToArray());
        }

        public bool Button(string text, params GuiLayoutOptionAdapter[] options)
        {
            return GUILayout.Button(text, options.Select(o => o.Option).ToArray());
        }

        public float HorizontalSlider(float value, float leftValue, float rightValue, GuiStyleAdapter slider, GuiStyleAdapter thumb, params GuiLayoutOptionAdapter[] options)
        {
            return GUILayout.HorizontalSlider(value, leftValue, rightValue, slider != null ? slider.Style : GUIStyle.none, thumb != null ? thumb.Style : GUIStyle.none, options.Select(o => o.Option).ToArray());
        }

        public int SelectionGrid(int selected, string[] texts, int xCount, params GuiLayoutOptionAdapter[] options)
        {
            return GUILayout.SelectionGrid(selected, texts, xCount, options.Select(o => o.Option).ToArray());
        }

        public GuiLayoutOptionAdapter Width(float width)
        {
            return new GuiLayoutOptionAdapter(GUILayout.Width(width));
        }

        public GuiLayoutOptionAdapter MinWidth(float width)
        {
            return new GuiLayoutOptionAdapter(GUILayout.MinWidth(width));
        }

        public GuiLayoutOptionAdapter ExpandWidth(bool expand)
        {
            return new GuiLayoutOptionAdapter(GUILayout.ExpandWidth(expand));
        }
    }

    public class GuiLayoutOptionAdapter
    {
        public GUILayoutOption Option { get; }

        public GuiLayoutOptionAdapter(GUILayoutOption option)
        {
            Option = option;
        }
    }
}
