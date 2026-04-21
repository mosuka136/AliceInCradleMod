using UnityEngine.InputSystem;

namespace BetterExperience.HAdapter
{
    public class UnityInputAdapter
    {
        public static Keyboard KeyboardCurrent => Keyboard.current;
        public static Gamepad GamepadCurrent => Gamepad.current;
    }
}
