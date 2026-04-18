using UnityEngine;

namespace BetterExperience.HAdapter
{
    public class GuiAdapter
    {
        public static string Tooltip => GUI.tooltip;

        public static Color Color
        {
            get
            {
                var c = GUI.color;
                return new Color(c.r, c.g, c.b, c.a);
            }
            set => GUI.color = new UnityEngine.Color(value.r, value.g, value.b, value.a);
        }

        public static void Label(Rect position, GuiContentAdapter content, GuiStyleAdapter style)
        {
            GUI.Label(position, content.Content, style.Style);
        }
    }

    public struct Color
    {
        public float r;
        public float g;
        public float b;
        public float a;

        public Color(float r, float g, float b, float a)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }
    }

    public struct Rect
    {
        private UnityEngine.Rect _rect;

        public float x;
        public float y;
        public float width;
        public float height;
        public Vector2 position;

        public Rect(float x, float y, float width, float height)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;

            _rect = new UnityEngine.Rect(x, y, width, height);
            position = _rect.position;
        }

        public static implicit operator UnityEngine.Rect(Rect rect)
        {
            return rect._rect;
        }

        public static implicit operator Rect(UnityEngine.Rect rect)
        {
            return new Rect(rect.x, rect.y, rect.width, rect.height);
        }

        public bool Contains(Vector2 point)
        {
            return _rect.Contains(point);
        }
    }
}
