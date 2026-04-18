using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using BetterExperience.HConfigGUI;
using Xunit;

namespace BetterExperience.Test
{
    public class ConfigGUITests
    {
        public ConfigGUITests()
        {
            // Ensure static initialization is complete and set flag before each test
            RuntimeHelpers.RunClassConstructor(typeof(ConfigGUI).TypeHandle);
            SetInitializedFlag(true);
            Thread.MemoryBarrier();
        }

        private static void SetInitializedFlag(bool value)
        {
            var field = typeof(ConfigGUI).GetField("_initialized", BindingFlags.NonPublic | BindingFlags.Static);
            field.SetValue(null, value);
        }

        private static bool GetInitializedFlag()
        {
            var field = typeof(ConfigGUI).GetField("_initialized", BindingFlags.NonPublic | BindingFlags.Static);
            return (bool)field.GetValue(null);
        }
    }
}
