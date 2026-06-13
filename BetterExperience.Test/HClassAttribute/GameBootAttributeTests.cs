using BetterExperience.HClassAttribute;

namespace BetterExperience.Test.HClassAttribute
{
    public class GameBootAttributeTests
    {
        [Fact]
        public void InitializeOnGameBootAttribute_UsageTargetsMethodsOnly()
        {
            // Act
            var usage = Attribute.GetCustomAttribute(
                typeof(InitializeOnGameBootAttribute),
                typeof(AttributeUsageAttribute)) as AttributeUsageAttribute;

            // Assert
            Assert.NotNull(usage);
            Assert.Equal(AttributeTargets.Method, usage.ValidOn);
            Assert.False(usage.Inherited);
            Assert.False(usage.AllowMultiple);
        }

        [Fact]
        public void RegisterOnGameBootAttribute_UsageTargetsClassesOnly()
        {
            // Act
            var usage = Attribute.GetCustomAttribute(
                typeof(RegisterOnGameBootAttribute),
                typeof(AttributeUsageAttribute)) as AttributeUsageAttribute;

            // Assert
            Assert.NotNull(usage);
            Assert.Equal(AttributeTargets.Class, usage.ValidOn);
            Assert.False(usage.Inherited);
            Assert.False(usage.AllowMultiple);
        }
    }
}
