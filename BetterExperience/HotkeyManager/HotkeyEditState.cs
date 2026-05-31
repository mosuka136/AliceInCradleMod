namespace BetterExperience.HotkeyManager
{
    public enum HotkeyEditState
    {
        Idle,
        Expanded,
        WaitingPress,
        Recording,
        WaitingConfirm,
    }

    public class HotkeyInputSnapshot
    {
        public bool HasAnyPressed { get; set; }
        public KeyboardChord KeyboardChord { get; set; }
        public GamepadChord GamepadChord { get; set; }
    }
}
