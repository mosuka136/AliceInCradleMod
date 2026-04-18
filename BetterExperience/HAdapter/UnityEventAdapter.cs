using UnityEngine;

namespace BetterExperience.HAdapter
{
    public class UnityEventAdapter
    {
        public static UnityEventAdapter Current { get; } = new UnityEventAdapter();

        public Vector2 MousePosition => Event.current.mousePosition;
    }
}
