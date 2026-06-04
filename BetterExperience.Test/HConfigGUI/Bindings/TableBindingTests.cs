using BetterExperience.HConfigGUI.Bindings;
using BetterExperience.HTranslatorSpace;
using Moq;
using System.Collections.Generic;

namespace BetterExperience.Test.HConfigGUI.Bindings
{
    public class TableBindingTests
    {
        [Fact]
        public void Constructor_WithValues_AssignsProvidedReferences()
        {
            // Arrange
            var entryBindingMock = new Mock<IEntryBinding>();
            IEnumerable<IEntryBinding> table = new List<IEntryBinding> { entryBindingMock.Object };
            var name = new Translator("名称", "Name");
            var description = new Translator("描述", "Description");

            // Act
            var result = new TableBinding(table, name, description);

            // Assert
            Assert.Same(table, result.Table);
            Assert.Same(name, result.Name);
            Assert.Same(description, result.Description);
        }

        [Fact]
        public void Constructor_WithNullValues_AssignsNullProperties()
        {
            // Arrange
            IEnumerable<IEntryBinding> table = null;
            Translator name = null;
            Translator description = null;

            // Act
            var result = new TableBinding(table, name, description);

            // Assert
            Assert.Null(result.Table);
            Assert.Null(result.Name);
            Assert.Null(result.Description);
        }

    }
}
