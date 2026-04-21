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
            
            // IMPORTANT: Must set to true BEFORE any test code runs to avoid Unity calls
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

        [Fact]
        public void SetAndGetInitializedFlag_Works()
        {
            // Arrange & Act
            SetInitializedFlag(false);
            var result1 = GetInitializedFlag();
            
            SetInitializedFlag(true);
            var result2 = GetInitializedFlag();

            // Assert
            Assert.False(result1);
            Assert.True(result2);
        }

    }
}
