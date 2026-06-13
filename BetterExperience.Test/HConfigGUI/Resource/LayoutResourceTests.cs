using BetterExperience.HConfigGUI.Bindings;
using BetterExperience.HConfigGUI.Resource;
using BetterExperience.HConfigSpace;
using BetterExperience.HProvider;
using BetterExperience.HTranslatorSpace;
using Moq;
using System.Linq;
using System.Runtime.Serialization;
using UnityEngine;

namespace BetterExperience.Test.HConfigGUI.Resource
{
    public class LayoutResourceTests
    {
        [Fact]
        public void LayoutResource_WhenConstructedWithUnityGui_StoresUnityGui()
        {
            // Arrange
            var unityGuiMock = new Mock<IUnityGuiProvider>(MockBehavior.Strict);

            // Act
            var resource = new LayoutResource(unityGuiMock.Object);

            // Assert
            Assert.Same(unityGuiMock.Object, resource.UnityGui);
        }

        [Fact]
        public void LayoutResource_WhenConstructedWithNull_StoresNullUnityGui()
        {
            // Arrange

            // Act
            var resource = new LayoutResource(null);

            // Assert
            Assert.Null(resource.UnityGui);
        }

        [Fact]
        public void GetEntryLabelWidth_WhenSheetIsNull_ReturnsZero()
        {
            // Arrange
            var unityGuiMock = new Mock<IUnityGuiProvider>(MockBehavior.Strict);
            var resource = new LayoutResource(unityGuiMock.Object);

            // Act
            var result = resource.GetEntryLabelWidth(null);

            // Assert
            Assert.Equal(0f, result);
            unityGuiMock.VerifyNoOtherCalls();
        }

        [Fact]
        public void GetTableButtonWidth_WhenSheetIsNull_ReturnsZero()
        {
            // Arrange
            var unityGuiMock = new Mock<IUnityGuiProvider>(MockBehavior.Strict);
            var resource = new LayoutResource(unityGuiMock.Object);

            // Act
            var result = resource.GetTableButtonWidth(null);

            // Assert
            Assert.Equal(0f, result);
            unityGuiMock.VerifyNoOtherCalls();
        }

        private static SheetBinding CreateSheetBinding(params (Translator TableName, Translator EntryName)[] definitions)
        {
            var sheet = new ConfigSheet();

            foreach (var group in definitions.GroupBy(x => x.TableName.English))
            {
                var first = group.First();
                var table = new ConfigTable(
                    group.Key,
                    new ConfigFileTable(group.Key, new Translator()),
                    first.TableName,
                    new Translator());

                foreach (var definition in group)
                {
                    table.Add(CreateEntryMock(definition.EntryName).Object);
                }

                sheet.Add(group.Key, table);
            }

            return SheetBinding.CreateSheet(sheet);
        }

        private static Mock<IConfigEntry> CreateEntryMock(Translator name)
        {
            var entryMock = new Mock<IConfigEntry>(MockBehavior.Strict);
            entryMock.SetupGet(x => x.Key).Returns(name.English);
            entryMock.SetupGet(x => x.Name).Returns(name);
            entryMock.SetupGet(x => x.Description).Returns(new Translator());
            entryMock.SetupGet(x => x.ValueType).Returns(typeof(string));
            return entryMock;
        }

        private static Translator CreateTranslator(string value)
        {
            return new Translator(value, value);
        }
    }
}
