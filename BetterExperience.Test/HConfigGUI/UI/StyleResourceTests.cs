using BetterExperience.HConfigGUI.UI;

namespace BetterExperience.Test.HConfigGUI.UI
{
    /// <summary>
    /// StyleResource cannot be unit tested in a standard .NET environment.
    /// All properties create Unity GUI objects (GUIStyle, Texture2D) that require
    /// Unity's native runtime. Calling these properties throws SecurityException
    /// ("ECall methods must be packaged into a system module") outside Unity.
    /// Testing requires Unity Test Framework or redesigning the code to be testable.
    /// </summary>
    public class StyleResourceTests
    {
    }
}

