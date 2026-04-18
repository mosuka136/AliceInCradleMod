using UnityEngine;

namespace BetterExperience.HAdapter
{
    public class GuiContentAdapter
    {
        public GUIContent Content { get; }

        public GuiContentAdapter(GUIContent content)
        {
            Content = content;
        }

        public GuiContentAdapter(string text)
        {
            Content = new GUIContent(text);
        }

        public GuiContentAdapter(string text, string tooltip)
        {
            Content = new GUIContent(text, tooltip);
        }
    }
}
