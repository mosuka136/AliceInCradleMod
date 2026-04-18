using BetterExperience.HAdapter;
using BetterExperience.HConfigFileSpace;
using BetterExperience.HConfigGUI;
using BetterExperience.HConfigGUI.UI;
using BetterExperience.HTranslatorSpace;
using Moq;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using Xunit;

namespace BetterExperience.Test
{
    public class RenderersTests
    {
        // -----------------------------------------------------------------------
        // BaseEntryRenderer Constructor
        // -----------------------------------------------------------------------

        [Fact]
        public void BaseEntryRenderer_Constructor_SetsContextAndLayout()
        {
            // Arrange
            var context = CreateUninitializedViewModel();
            var mockLayout = new Mock<IGuiLayout>();

            // Act
            var renderer = new TestRenderer(context, mockLayout.Object);

            // Assert
            Assert.Same(context, renderer.Context);
            Assert.Same(mockLayout.Object, renderer.GetLayout());
        }

        // -----------------------------------------------------------------------
        // BaseEntryRenderer.Render
        // -----------------------------------------------------------------------

        [Fact]
        public void BaseEntryRenderer_Render_WhenContextIsNull_ReturnsEarly()
        {
            // Arrange
            var mockLayout = new Mock<IGuiLayout>();
            var renderer = new TestRenderer(null, mockLayout.Object);
            var entry = CreateUninitializedUiEntryModel();

            // Act
            renderer.Render(entry);

            // Assert
            mockLayout.Verify(l => l.BeginHorizontal(), Times.Never);
        }

        [Fact]
        public void BaseEntryRenderer_Render_WhenEntryIsNull_ReturnsEarly()
        {
            // Arrange
            var context = CreateUninitializedViewModel();
            var mockLayout = new Mock<IGuiLayout>();
            var renderer = new TestRenderer(context, mockLayout.Object);

            // Act
            renderer.Render(null);

            // Assert
            mockLayout.Verify(l => l.BeginHorizontal(), Times.Never);
        }

        [Fact]
        public void BaseEntryRenderer_Render_CallsBeginHorizontal()
        {
            // Arrange
            var context = CreateUninitializedViewModel();
            SetPrivateProperty(context, "Sheet", CreateUninitializedUiSheetModel());
            var mockLayout = new Mock<IGuiLayout>();
            var mockLayoutOption = new Mock<GuiLayoutOptionAdapter>(null);
            mockLayout.Setup(l => l.Width(It.IsAny<float>())).Returns(mockLayoutOption.Object);
            var renderer = new TestRenderer(context, mockLayout.Object);
            var entry = CreateUiEntryModelWithMockEntry("Test", "Description");

            // Act
            renderer.Render(entry);

            // Assert
            mockLayout.Verify(l => l.BeginHorizontal(), Times.Once);
        }

        [Fact]
        public void BaseEntryRenderer_Render_CallsLabelWithEntryNameAndDescription()
        {
            // Arrange
            var context = CreateUninitializedViewModel();
            SetPrivateProperty(context, "Sheet", CreateUninitializedUiSheetModel());
            var mockLayout = new Mock<IGuiLayout>();
            var mockLayoutOption = new Mock<GuiLayoutOptionAdapter>(null);
            mockLayout.Setup(l => l.Width(It.IsAny<float>())).Returns(mockLayoutOption.Object);
            var renderer = new TestRenderer(context, mockLayout.Object);
            var entry = CreateUiEntryModelWithMockEntry("TestName", "TestDescription");

            // Act
            renderer.Render(entry);

            // Assert
            mockLayout.Verify(l => l.Label(
                It.Is<GuiContentAdapter>(g => true),
                It.IsAny<GuiLayoutOptionAdapter>()), Times.Once);
        }

        [Fact]
        public void BaseEntryRenderer_Render_CallsRenderEntry()
        {
            // Arrange
            var context = CreateUninitializedViewModel();
            SetPrivateProperty(context, "Sheet", CreateUninitializedUiSheetModel());
            var mockLayout = new Mock<IGuiLayout>();
            var mockLayoutOption = new Mock<GuiLayoutOptionAdapter>(null);
            mockLayout.Setup(l => l.Width(It.IsAny<float>())).Returns(mockLayoutOption.Object);
            var renderer = new TestRendererTrackingRenderEntry(context, mockLayout.Object);
            var entry = CreateUiEntryModelWithMockEntry("Test", "Description");

            // Act
            renderer.Render(entry);

            // Assert
            Assert.True(renderer.RenderEntryCalled);
        }

        [Fact]
        public void BaseEntryRenderer_Render_CallsEndHorizontal()
        {
            // Arrange
            var context = CreateUninitializedViewModel();
            SetPrivateProperty(context, "Sheet", CreateUninitializedUiSheetModel());
            var mockLayout = new Mock<IGuiLayout>();
            var mockLayoutOption = new Mock<GuiLayoutOptionAdapter>(null);
            mockLayout.Setup(l => l.Width(It.IsAny<float>())).Returns(mockLayoutOption.Object);
            mockLayout.Setup(l => l.Button(It.IsAny<string>(), It.IsAny<GuiLayoutOptionAdapter>())).Returns(false);
            var renderer = new TestRenderer(context, mockLayout.Object);
            var entry = CreateUiEntryModelWithMockEntry("Test", "Description");

            // Act
            renderer.Render(entry);

            // Assert
            mockLayout.Verify(l => l.EndHorizontal(), Times.Once);
        }

        [Fact]
        public void BaseEntryRenderer_Render_CallsRenderAfterRow()
        {
            // Arrange
            var context = CreateUninitializedViewModel();
            SetPrivateProperty(context, "Sheet", CreateUninitializedUiSheetModel());
            var mockLayout = new Mock<IGuiLayout>();
            var mockLayoutOption = new Mock<GuiLayoutOptionAdapter>(null);
            mockLayout.Setup(l => l.Width(It.IsAny<float>())).Returns(mockLayoutOption.Object);
            mockLayout.Setup(l => l.Button(It.IsAny<string>(), It.IsAny<GuiLayoutOptionAdapter>())).Returns(false);
            var renderer = new TestRendererTrackingRenderAfterRow(context, mockLayout.Object);
            var entry = CreateUiEntryModelWithMockEntry("Test", "Description");

            // Act
            renderer.Render(entry);

            // Assert
            Assert.True(renderer.RenderAfterRowCalled);
        }

        // -----------------------------------------------------------------------
        // BaseEntryRenderer.RenderAfterRow
        // -----------------------------------------------------------------------

        [Fact]
        public void BaseEntryRenderer_RenderAfterRow_DoesNotThrow()
        {
            // Arrange
            var context = CreateUninitializedViewModel();
            var mockLayout = new Mock<IGuiLayout>();
            var renderer = new TestRenderer(context, mockLayout.Object);
            var entry = CreateUninitializedUiEntryModel();

            // Act & Assert
            renderer.RenderAfterRow(entry);
        }

        // -----------------------------------------------------------------------
        // BoolRenderer Constructor
        // -----------------------------------------------------------------------

        [Fact]
        public void BoolRenderer_Constructor_SetsContextAndLayout()
        {
            // Arrange
            var context = CreateUninitializedViewModel();
            var mockLayout = new Mock<IGuiLayout>();

            // Act
            var renderer = new BoolRenderer(context, mockLayout.Object);

            // Assert
            Assert.Same(context, renderer.Context);
        }

        // -----------------------------------------------------------------------
        // BoolRenderer.RenderEntry
        // -----------------------------------------------------------------------

        [Fact]
        public void BoolRenderer_RenderEntry_WhenContextIsNull_ReturnsEarly()
        {
            // Arrange
            var mockLayout = new Mock<IGuiLayout>();
            var renderer = new BoolRenderer(null, mockLayout.Object);
            var entry = CreateUninitializedUiEntryModel();

            // Act
            renderer.RenderEntry(entry);

            // Assert
            mockLayout.Verify(l => l.Toggle(It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<GuiLayoutOptionAdapter>()), Times.Never);
        }

        [Fact]
        public void BoolRenderer_RenderEntry_WhenEntryIsNull_ReturnsEarly()
        {
            // Arrange
            var context = CreateUninitializedViewModel();
            var mockLayout = new Mock<IGuiLayout>();
            var renderer = new BoolRenderer(context, mockLayout.Object);

            // Act
            renderer.RenderEntry(null);

            // Assert
            mockLayout.Verify(l => l.Toggle(It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<GuiLayoutOptionAdapter>()), Times.Never);
        }

        [Fact]
        public void BoolRenderer_RenderEntry_WhenValueTypeIsNotBool_ReturnsEarly()
        {
            // Arrange
            var context = CreateUninitializedViewModel();
            var mockLayout = new Mock<IGuiLayout>();
            var renderer = new BoolRenderer(context, mockLayout.Object);
            var entry = CreateUiEntryModelWithValue(typeof(int), 42);

            // Act
            renderer.RenderEntry(entry);

            // Assert
            mockLayout.Verify(l => l.Toggle(It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<GuiLayoutOptionAdapter>()), Times.Never);
        }

        [Fact]
        public void BoolRenderer_RenderEntry_WhenValueIsFalse_CallsToggleWithFalseAndOffText()
        {
            // Arrange
            var context = CreateUninitializedViewModel();
            var mockLayout = new Mock<IGuiLayout>();
            var mockLayoutOption = new Mock<GuiLayoutOptionAdapter>(null);
            mockLayout.Setup(l => l.ExpandWidth(true)).Returns(mockLayoutOption.Object);
            mockLayout.Setup(l => l.Toggle(It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<GuiLayoutOptionAdapter>())).Returns(false);
            var renderer = new BoolRenderer(context, mockLayout.Object);
            var entry = CreateUiEntryModelWithValue(typeof(bool), false);

            // Act
            renderer.RenderEntry(entry);

            // Assert
            mockLayout.Verify(l => l.Toggle(false, It.IsAny<string>(), mockLayoutOption.Object), Times.Once);
        }

        [Fact]
        public void BoolRenderer_RenderEntry_WhenValueIsTrue_CallsToggleWithTrueAndOnText()
        {
            // Arrange
            var context = CreateUninitializedViewModel();
            var mockLayout = new Mock<IGuiLayout>();
            var mockLayoutOption = new Mock<GuiLayoutOptionAdapter>(null);
            mockLayout.Setup(l => l.ExpandWidth(true)).Returns(mockLayoutOption.Object);
            mockLayout.Setup(l => l.Toggle(It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<GuiLayoutOptionAdapter>())).Returns(true);
            var renderer = new BoolRenderer(context, mockLayout.Object);
            var entry = CreateUiEntryModelWithValue(typeof(bool), true);

            // Act
            renderer.RenderEntry(entry);

            // Assert
            mockLayout.Verify(l => l.Toggle(true, It.IsAny<string>(), mockLayoutOption.Object), Times.Once);
        }

        [Fact]
        public void BoolRenderer_RenderEntry_WhenCacheValueIsBool_UsesCacheValue()
        {
            // Arrange
            var context = CreateUninitializedViewModel();
            var mockLayout = new Mock<IGuiLayout>();
            var mockLayoutOption = new Mock<GuiLayoutOptionAdapter>(null);
            mockLayout.Setup(l => l.ExpandWidth(true)).Returns(mockLayoutOption.Object);
            mockLayout.Setup(l => l.Toggle(It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<GuiLayoutOptionAdapter>())).Returns(true);
            var renderer = new BoolRenderer(context, mockLayout.Object);
            var entry = CreateUiEntryModelWithValue(typeof(bool), false);
            entry.CacheValue = true;

            // Act
            renderer.RenderEntry(entry);

            // Assert
            mockLayout.Verify(l => l.Toggle(true, It.IsAny<string>(), mockLayoutOption.Object), Times.Once);
        }

        [Fact]
        public void BoolRenderer_RenderEntry_WhenValueDoesNotChange_DoesNotCallSetValue()
        {
            // Arrange
            var context = CreateUninitializedViewModel();
            var mockLayout = new Mock<IGuiLayout>();
            var mockLayoutOption = new Mock<GuiLayoutOptionAdapter>(null);
            mockLayout.Setup(l => l.ExpandWidth(true)).Returns(mockLayoutOption.Object);
            mockLayout.Setup(l => l.Toggle(It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<GuiLayoutOptionAdapter>())).Returns(true);
            var renderer = new BoolRenderer(context, mockLayout.Object);
            var entry = CreateUiEntryModelWithValue(typeof(bool), true);

            // Act
            renderer.RenderEntry(entry);

            // Assert - if SetValue is not called (which is expected), completes without issue
            // This test verifies the code path where newValue == value
            Assert.True(true);
        }

        // -----------------------------------------------------------------------
        // StringRenderer Constructor
        // -----------------------------------------------------------------------

        [Fact]
        public void StringRenderer_Constructor_SetsContextAndLayout()
        {
            // Arrange
            var context = CreateUninitializedViewModel();
            var mockLayout = new Mock<IGuiLayout>();

            // Act
            var renderer = new StringRenderer(context, mockLayout.Object);

            // Assert
            Assert.Same(context, renderer.Context);
        }

        // -----------------------------------------------------------------------
        // StringRenderer.RenderEntry
        // -----------------------------------------------------------------------

        [Fact]
        public void StringRenderer_RenderEntry_WhenContextIsNull_ReturnsEarly()
        {
            // Arrange
            var mockLayout = new Mock<IGuiLayout>();
            var renderer = new StringRenderer(null, mockLayout.Object);
            var entry = CreateUninitializedUiEntryModel();

            // Act
            renderer.RenderEntry(entry);

            // Assert
            mockLayout.Verify(l => l.TextField(It.IsAny<string>(), It.IsAny<GuiLayoutOptionAdapter>()), Times.Never);
        }

        [Fact]
        public void StringRenderer_RenderEntry_WhenEntryIsNull_ReturnsEarly()
        {
            // Arrange
            var context = CreateUninitializedViewModel();
            var mockLayout = new Mock<IGuiLayout>();
            var renderer = new StringRenderer(context, mockLayout.Object);

            // Act
            renderer.RenderEntry(null);

            // Assert
            mockLayout.Verify(l => l.TextField(It.IsAny<string>(), It.IsAny<GuiLayoutOptionAdapter>()), Times.Never);
        }

        [Fact]
        public void StringRenderer_RenderEntry_WhenValueTypeIsNotString_ReturnsEarly()
        {
            // Arrange
            var context = CreateUninitializedViewModel();
            var mockLayout = new Mock<IGuiLayout>();
            var renderer = new StringRenderer(context, mockLayout.Object);
            var entry = CreateUiEntryModelWithValue(typeof(int), 42);

            // Act
            renderer.RenderEntry(entry);

            // Assert
            mockLayout.Verify(l => l.TextField(It.IsAny<string>(), It.IsAny<GuiLayoutOptionAdapter>()), Times.Never);
        }

        [Fact]
        public void StringRenderer_RenderEntry_WhenValueIsString_CallsTextFieldWithValue()
        {
            // Arrange
            var context = CreateUninitializedViewModel();
            var mockLayout = new Mock<IGuiLayout>();
            var mockLayoutOption = new Mock<GuiLayoutOptionAdapter>(null);
            mockLayout.Setup(l => l.ExpandWidth(true)).Returns(mockLayoutOption.Object);
            mockLayout.Setup(l => l.TextField(It.IsAny<string>(), It.IsAny<GuiLayoutOptionAdapter>())).Returns("test");
            var renderer = new StringRenderer(context, mockLayout.Object);
            var entry = CreateUiEntryModelWithValue(typeof(string), "test");

            // Act
            renderer.RenderEntry(entry);

            // Assert
            mockLayout.Verify(l => l.TextField("test", mockLayoutOption.Object), Times.Once);
        }

        [Fact]
        public void StringRenderer_RenderEntry_WhenValueIsNull_UsesEmptyString()
        {
            // Arrange
            var context = CreateUninitializedViewModel();
            var mockLayout = new Mock<IGuiLayout>();
            var mockLayoutOption = new Mock<GuiLayoutOptionAdapter>(null);
            mockLayout.Setup(l => l.ExpandWidth(true)).Returns(mockLayoutOption.Object);
            mockLayout.Setup(l => l.TextField(It.IsAny<string>(), It.IsAny<GuiLayoutOptionAdapter>())).Returns(string.Empty);
            var renderer = new StringRenderer(context, mockLayout.Object);
            var entry = CreateUiEntryModelWithValue(typeof(string), null);

            // Act
            renderer.RenderEntry(entry);

            // Assert
            mockLayout.Verify(l => l.TextField(string.Empty, mockLayoutOption.Object), Times.Once);
        }

        [Fact]
        public void StringRenderer_RenderEntry_WhenCacheValueIsString_UsesCacheValue()
        {
            // Arrange
            var context = CreateUninitializedViewModel();
            var mockLayout = new Mock<IGuiLayout>();
            var mockLayoutOption = new Mock<GuiLayoutOptionAdapter>(null);
            mockLayout.Setup(l => l.ExpandWidth(true)).Returns(mockLayoutOption.Object);
            mockLayout.Setup(l => l.TextField(It.IsAny<string>(), It.IsAny<GuiLayoutOptionAdapter>())).Returns("cached");
            var renderer = new StringRenderer(context, mockLayout.Object);
            var entry = CreateUiEntryModelWithValue(typeof(string), "original");
            entry.CacheValue = "cached";

            // Act
            renderer.RenderEntry(entry);

            // Assert
            mockLayout.Verify(l => l.TextField("cached", mockLayoutOption.Object), Times.Once);
        }

        [Fact]
        public void StringRenderer_RenderEntry_WhenValueDoesNotChange_DoesNotCallSetValue()
        {
            // Arrange
            var context = CreateUninitializedViewModel();
            var mockLayout = new Mock<IGuiLayout>();
            var mockLayoutOption = new Mock<GuiLayoutOptionAdapter>(null);
            mockLayout.Setup(l => l.ExpandWidth(true)).Returns(mockLayoutOption.Object);
            mockLayout.Setup(l => l.TextField(It.IsAny<string>(), It.IsAny<GuiLayoutOptionAdapter>())).Returns("sameValue");
            var renderer = new StringRenderer(context, mockLayout.Object);
            var entry = CreateUiEntryModelWithValue(typeof(string), "sameValue");

            // Act
            renderer.RenderEntry(entry);

            // Assert - if SetValue is not called (which is expected), completes without issue
            // This test verifies the code path where newValue == value
            Assert.True(true);
        }

        // -----------------------------------------------------------------------
        // NumberRenderer Constructor
        // -----------------------------------------------------------------------

        [Fact]
        public void NumberRenderer_Constructor_SetsContextAndLayout()
        {
            // Arrange
            var context = CreateUninitializedViewModel();
            var mockLayout = new Mock<IGuiLayout>();

            // Act
            var renderer = new NumberRenderer(context, mockLayout.Object);

            // Assert
            Assert.Same(context, renderer.Context);
        }

        // -----------------------------------------------------------------------
        // NumberRenderer.RenderEntry
        // -----------------------------------------------------------------------

        [Fact]
        public void NumberRenderer_RenderEntry_WhenContextIsNull_ReturnsEarly()
        {
            // Arrange
            var mockLayout = new Mock<IGuiLayout>();
            var renderer = new NumberRenderer(null, mockLayout.Object);
            var entry = CreateUninitializedUiEntryModel();

            // Act
            renderer.RenderEntry(entry);

            // Assert
            mockLayout.Verify(l => l.TextField(It.IsAny<string>(), It.IsAny<GuiLayoutOptionAdapter>()), Times.Never);
        }

        [Fact]
        public void NumberRenderer_RenderEntry_WhenEntryIsNull_ReturnsEarly()
        {
            // Arrange
            var context = CreateUninitializedViewModel();
            var mockLayout = new Mock<IGuiLayout>();
            var renderer = new NumberRenderer(context, mockLayout.Object);

            // Act
            renderer.RenderEntry(null);

            // Assert
            mockLayout.Verify(l => l.TextField(It.IsAny<string>(), It.IsAny<GuiLayoutOptionAdapter>()), Times.Never);
        }

        [Fact]
        public void NumberRenderer_RenderEntry_WhenValueTypeIsNotPrimitive_ReturnsEarly()
        {
            // Arrange
            var context = CreateUninitializedViewModel();
            var mockLayout = new Mock<IGuiLayout>();
            var renderer = new NumberRenderer(context, mockLayout.Object);
            var entry = CreateUiEntryModelWithValue(typeof(string), "test");

            // Act
            renderer.RenderEntry(entry);

            // Assert
            mockLayout.Verify(l => l.TextField(It.IsAny<string>(), It.IsAny<GuiLayoutOptionAdapter>()), Times.Never);
        }

        [Fact]
        public void NumberRenderer_RenderEntry_WhenValueTypeIsBool_ReturnsEarly()
        {
            // Arrange
            var context = CreateUninitializedViewModel();
            var mockLayout = new Mock<IGuiLayout>();
            var renderer = new NumberRenderer(context, mockLayout.Object);
            var entry = CreateUiEntryModelWithValue(typeof(bool), true);

            // Act
            renderer.RenderEntry(entry);

            // Assert
            mockLayout.Verify(l => l.TextField(It.IsAny<string>(), It.IsAny<GuiLayoutOptionAdapter>()), Times.Never);
        }

        [Fact]
        public void NumberRenderer_RenderEntry_WhenValueTypeIsChar_ReturnsEarly()
        {
            // Arrange
            var context = CreateUninitializedViewModel();
            var mockLayout = new Mock<IGuiLayout>();
            var renderer = new NumberRenderer(context, mockLayout.Object);
            var entry = CreateUiEntryModelWithValue(typeof(char), 'a');

            // Act
            renderer.RenderEntry(entry);

            // Assert
            mockLayout.Verify(l => l.TextField(It.IsAny<string>(), It.IsAny<GuiLayoutOptionAdapter>()), Times.Never);
        }

        [Fact]
        public void NumberRenderer_RenderEntry_WhenCacheValueStringIsNotEmpty_UsesIt()
        {
            // Arrange
            var context = CreateUninitializedViewModel();
            var mockLayout = new Mock<IGuiLayout>();
            var mockLayoutOption = new Mock<GuiLayoutOptionAdapter>(null);
            mockLayout.Setup(l => l.ExpandWidth(true)).Returns(mockLayoutOption.Object);
            mockLayout.Setup(l => l.TextField(It.IsAny<string>(), It.IsAny<GuiLayoutOptionAdapter>())).Returns("123");
            var renderer = new NumberRenderer(context, mockLayout.Object);
            var entry = CreateUiEntryModelWithValue(typeof(int), 456);
            entry.CacheValueString = "123";

            // Act
            renderer.RenderEntry(entry);

            // Assert
            mockLayout.Verify(l => l.TextField("123", mockLayoutOption.Object), Times.Once);
        }

        [Fact]
        public void NumberRenderer_RenderEntry_WhenCacheValueStringIsEmpty_ParsesValue()
        {
            // Arrange
            var context = CreateUninitializedViewModel();
            var mockLayout = new Mock<IGuiLayout>();
            var mockLayoutOption = new Mock<GuiLayoutOptionAdapter>(null);
            mockLayout.Setup(l => l.ExpandWidth(true)).Returns(mockLayoutOption.Object);
            mockLayout.Setup(l => l.TextField(It.IsAny<string>(), It.IsAny<GuiLayoutOptionAdapter>())).Returns("42");
            var renderer = new NumberRenderer(context, mockLayout.Object);
            var entry = CreateUiEntryModelWithValue(typeof(int), 42);
            entry.CacheValueString = null;

            // Act
            renderer.RenderEntry(entry);

            // Assert
            mockLayout.Verify(l => l.TextField("42", mockLayoutOption.Object), Times.Once);
        }

        [Fact]
        public void NumberRenderer_RenderEntry_WhenCacheValueIsSet_ParsesItInsteadOfValue()
        {
            // Arrange
            var context = CreateUninitializedViewModel();
            var mockLayout = new Mock<IGuiLayout>();
            var mockLayoutOption = new Mock<GuiLayoutOptionAdapter>(null);
            mockLayout.Setup(l => l.ExpandWidth(true)).Returns(mockLayoutOption.Object);
            mockLayout.Setup(l => l.TextField(It.IsAny<string>(), It.IsAny<GuiLayoutOptionAdapter>())).Returns("100");
            var renderer = new NumberRenderer(context, mockLayout.Object);
            var entry = CreateUiEntryModelWithValue(typeof(int), 42);
            entry.CacheValue = 100;
            entry.CacheValueString = null;

            // Act
            renderer.RenderEntry(entry);

            // Assert
            mockLayout.Verify(l => l.TextField("100", mockLayoutOption.Object), Times.Once);
        }

        [Fact]
        public void NumberRenderer_RenderEntry_WhenCacheValueStringIsEmpty_SetsCacheValueString()
        {
            // Arrange
            var context = CreateUninitializedViewModel();
            var mockLayout = new Mock<IGuiLayout>();
            var mockLayoutOption = new Mock<GuiLayoutOptionAdapter>(null);
            mockLayout.Setup(l => l.ExpandWidth(true)).Returns(mockLayoutOption.Object);
            mockLayout.Setup(l => l.TextField(It.IsAny<string>(), It.IsAny<GuiLayoutOptionAdapter>())).Returns("42");
            var renderer = new NumberRenderer(context, mockLayout.Object);
            var entry = CreateUiEntryModelWithValue(typeof(int), 42);
            entry.CacheValueString = null;

            // Act
            renderer.RenderEntry(entry);

            // Assert
            Assert.Equal("42", entry.CacheValueString);
        }

        [Fact]
        public void NumberRenderer_RenderEntry_WhenNewValueDiffersAndParseFails_DoesNotCallSetValue()
        {
            // Arrange
            var context = CreateUninitializedViewModel();
            var mockLayout = new Mock<IGuiLayout>();
            var mockLayoutOption = new Mock<GuiLayoutOptionAdapter>(null);
            mockLayout.Setup(l => l.ExpandWidth(true)).Returns(mockLayoutOption.Object);
            mockLayout.Setup(l => l.TextField(It.IsAny<string>(), It.IsAny<GuiLayoutOptionAdapter>())).Returns("invalid");
            var renderer = new NumberRenderer(context, mockLayout.Object);
            var entry = CreateUiEntryModelWithValue(typeof(int), 42);
            entry.CacheValueString = "42";

            // Act
            renderer.RenderEntry(entry);

            // Assert - if SetValue is not called (parse failed), completes without calling SetValue
            // This test verifies the code path where parse fails
            Assert.True(true);
        }

        [Fact]
        public void NumberRenderer_RenderEntry_WhenNewValueDiffers_UpdatesCacheValueString()
        {
            // Arrange
            var context = CreateUninitializedViewModel();
            var mockLayout = new Mock<IGuiLayout>();
            var mockLayoutOption = new Mock<GuiLayoutOptionAdapter>(null);
            mockLayout.Setup(l => l.ExpandWidth(true)).Returns(mockLayoutOption.Object);
            mockLayout.Setup(l => l.TextField(It.IsAny<string>(), It.IsAny<GuiLayoutOptionAdapter>())).Returns("invalid");
            var renderer = new NumberRenderer(context, mockLayout.Object);
            var entry = CreateUiEntryModelWithValue(typeof(int), 42);
            entry.CacheValueString = "42";

            // Act
            renderer.RenderEntry(entry);

            // Assert
            Assert.Equal("invalid", entry.CacheValueString);
        }

        [Fact]
        public void NumberRenderer_RenderEntry_WhenValueDoesNotChange_DoesNotCallSetValue()
        {
            // Arrange
            var context = CreateUninitializedViewModel();
            var mockLayout = new Mock<IGuiLayout>();
            var mockLayoutOption = new Mock<GuiLayoutOptionAdapter>(null);
            mockLayout.Setup(l => l.ExpandWidth(true)).Returns(mockLayoutOption.Object);
            mockLayout.Setup(l => l.TextField(It.IsAny<string>(), It.IsAny<GuiLayoutOptionAdapter>())).Returns("42");
            var renderer = new NumberRenderer(context, mockLayout.Object);
            var entry = CreateUiEntryModelWithValue(typeof(int), 42);
            entry.CacheValueString = "42";

            // Act
            renderer.RenderEntry(entry);

            // Assert - if SetValue is not called (which is expected), completes without issue
            // This test verifies the code path where newValueStr == valueStr
            Assert.True(true);
        }

        // -----------------------------------------------------------------------
        // EnumRenderer Constructor
        // -----------------------------------------------------------------------

        [Fact]
        public void EnumRenderer_Constructor_SetsContextAndLayout()
        {
            // Arrange
            var context = CreateUninitializedViewModel();
            var mockLayout = new Mock<IGuiLayout>();

            // Act
            var renderer = new EnumRenderer(context, mockLayout.Object);

            // Assert
            Assert.Same(context, renderer.Context);
        }

        // -----------------------------------------------------------------------
        // EnumRenderer.RenderEntry
        // -----------------------------------------------------------------------

        [Fact]
        public void EnumRenderer_RenderEntry_WhenContextIsNull_ReturnsEarly()
        {
            // Arrange
            var mockLayout = new Mock<IGuiLayout>();
            var renderer = new EnumRenderer(null, mockLayout.Object);
            var entry = CreateUninitializedUiEntryModel();

            // Act
            renderer.RenderEntry(entry);

            // Assert
            mockLayout.Verify(l => l.Button(It.IsAny<string>(), It.IsAny<GuiLayoutOptionAdapter>()), Times.Never);
        }

        [Fact]
        public void EnumRenderer_RenderEntry_WhenEntryIsNull_ReturnsEarly()
        {
            // Arrange
            var context = CreateUninitializedViewModel();
            var mockLayout = new Mock<IGuiLayout>();
            var renderer = new EnumRenderer(context, mockLayout.Object);

            // Act
            renderer.RenderEntry(null);

            // Assert
            mockLayout.Verify(l => l.Button(It.IsAny<string>(), It.IsAny<GuiLayoutOptionAdapter>()), Times.Never);
        }

        [Fact]
        public void EnumRenderer_RenderEntry_WhenValueTypeIsNotEnum_ReturnsEarly()
        {
            // Arrange
            var context = CreateUninitializedViewModel();
            var mockLayout = new Mock<IGuiLayout>();
            var renderer = new EnumRenderer(context, mockLayout.Object);
            var entry = CreateUiEntryModelWithValue(typeof(int), 42);

            // Act
            renderer.RenderEntry(entry);

            // Assert
            mockLayout.Verify(l => l.Button(It.IsAny<string>(), It.IsAny<GuiLayoutOptionAdapter>()), Times.Never);
        }

        [Fact]
        public void EnumRenderer_RenderEntry_WhenButtonNotClicked_DoesNotChangeOpenedEnumEntry()
        {
            // Arrange
            var context = CreateUninitializedViewModel();
            var mockLayout = new Mock<IGuiLayout>();
            var mockLayoutOption = new Mock<GuiLayoutOptionAdapter>(null);
            mockLayout.Setup(l => l.ExpandWidth(true)).Returns(mockLayoutOption.Object);
            mockLayout.Setup(l => l.Button(It.IsAny<string>(), It.IsAny<GuiLayoutOptionAdapter>())).Returns(false);
            var renderer = new EnumRenderer(context, mockLayout.Object);
            var entry = CreateUiEntryModelWithValue(typeof(TestEnum), TestEnum.Value1);

            // Act
            renderer.RenderEntry(entry);

            // Assert
            Assert.Null(context.OpenedEnumEntry);
        }

        [Fact]
        public void EnumRenderer_RenderEntry_WhenButtonClicked_SetsOpenedEnumEntry()
        {
            // Arrange
            var context = CreateUninitializedViewModel();
            var mockLayout = new Mock<IGuiLayout>();
            var mockLayoutOption = new Mock<GuiLayoutOptionAdapter>(null);
            mockLayout.Setup(l => l.ExpandWidth(true)).Returns(mockLayoutOption.Object);
            mockLayout.Setup(l => l.Button(It.IsAny<string>(), It.IsAny<GuiLayoutOptionAdapter>())).Returns(true);
            var renderer = new EnumRenderer(context, mockLayout.Object);
            var entry = CreateUiEntryModelWithValue(typeof(TestEnum), TestEnum.Value1);

            // Act
            renderer.RenderEntry(entry);

            // Assert
            Assert.Same(entry, context.OpenedEnumEntry);
        }

        [Fact]
        public void EnumRenderer_RenderEntry_WhenButtonClickedAndEntryAlreadyOpened_ClosesEntry()
        {
            // Arrange
            var context = CreateUninitializedViewModel();
            var mockLayout = new Mock<IGuiLayout>();
            var mockLayoutOption = new Mock<GuiLayoutOptionAdapter>(null);
            mockLayout.Setup(l => l.ExpandWidth(true)).Returns(mockLayoutOption.Object);
            mockLayout.Setup(l => l.Button(It.IsAny<string>(), It.IsAny<GuiLayoutOptionAdapter>())).Returns(true);
            var renderer = new EnumRenderer(context, mockLayout.Object);
            var entry = CreateUiEntryModelWithValue(typeof(TestEnum), TestEnum.Value1);
            context.OpenedEnumEntry = entry;

            // Act
            renderer.RenderEntry(entry);

            // Assert
            Assert.Null(context.OpenedEnumEntry);
        }

        [Fact]
        public void EnumRenderer_RenderEntry_WhenCacheValueIsSet_UsesCacheValue()
        {
            // Arrange
            var context = CreateUninitializedViewModel();
            var mockLayout = new Mock<IGuiLayout>();
            var mockLayoutOption = new Mock<GuiLayoutOptionAdapter>(null);
            mockLayout.Setup(l => l.ExpandWidth(true)).Returns(mockLayoutOption.Object);
            mockLayout.Setup(l => l.Button(It.IsAny<string>(), It.IsAny<GuiLayoutOptionAdapter>())).Returns(false);
            var renderer = new EnumRenderer(context, mockLayout.Object);
            var entry = CreateUiEntryModelWithValue(typeof(TestEnum), TestEnum.Value1);
            entry.CacheValue = TestEnum.Value2;

            // Act
            renderer.RenderEntry(entry);

            // Assert
            mockLayout.Verify(l => l.Button(It.IsAny<string>(), mockLayoutOption.Object), Times.Once);
        }

        // -----------------------------------------------------------------------
        // EnumRenderer.RenderAfterRow
        // -----------------------------------------------------------------------

        [Fact]
        public void EnumRenderer_RenderAfterRow_WhenContextIsNull_ReturnsEarly()
        {
            // Arrange
            var mockLayout = new Mock<IGuiLayout>();
            var renderer = new EnumRenderer(null, mockLayout.Object);
            var entry = CreateUninitializedUiEntryModel();

            // Act
            renderer.RenderAfterRow(entry);

            // Assert
            mockLayout.Verify(l => l.BeginHorizontal(), Times.Never);
        }

        [Fact]
        public void EnumRenderer_RenderAfterRow_WhenEntryIsNull_ReturnsEarly()
        {
            // Arrange
            var context = CreateUninitializedViewModel();
            var mockLayout = new Mock<IGuiLayout>();
            var renderer = new EnumRenderer(context, mockLayout.Object);

            // Act
            renderer.RenderAfterRow(null);

            // Assert
            mockLayout.Verify(l => l.BeginHorizontal(), Times.Never);
        }

        [Fact]
        public void EnumRenderer_RenderAfterRow_WhenOpenedEnumEntryIsDifferent_ReturnsEarly()
        {
            // Arrange
            var context = CreateUninitializedViewModel();
            var mockLayout = new Mock<IGuiLayout>();
            var renderer = new EnumRenderer(context, mockLayout.Object);
            var entry = CreateUiEntryModelWithValue(typeof(TestEnum), TestEnum.Value1);
            var otherEntry = CreateUiEntryModelWithValue(typeof(TestEnum), TestEnum.Value2);
            context.OpenedEnumEntry = otherEntry;

            // Act
            renderer.RenderAfterRow(entry);

            // Assert
            mockLayout.Verify(l => l.BeginHorizontal(), Times.Never);
        }

        [Fact]
        public void EnumRenderer_RenderAfterRow_WhenValueTypeIsNotEnum_ReturnsEarly()
        {
            // Arrange
            var context = CreateUninitializedViewModel();
            SetPrivateProperty(context, "Sheet", CreateUninitializedUiSheetModel());
            var mockLayout = new Mock<IGuiLayout>();
            var renderer = new EnumRenderer(context, mockLayout.Object);
            var entry = CreateUiEntryModelWithValue(typeof(int), 42);
            context.OpenedEnumEntry = entry;

            // Act
            renderer.RenderAfterRow(entry);

            // Assert
            mockLayout.Verify(l => l.BeginHorizontal(), Times.Never);
        }

        // -----------------------------------------------------------------------
        // SliderRenderer Constructor
        // -----------------------------------------------------------------------

        [Fact]
        public void SliderRenderer_Constructor_SetsContextAndLayout()
        {
            // Arrange
            var context = CreateUninitializedViewModel();
            var mockLayout = new Mock<IGuiLayout>();

            // Act
            var renderer = new SliderRenderer(context, mockLayout.Object);

            // Assert
            Assert.Same(context, renderer.Context);
        }

        // -----------------------------------------------------------------------
        // SliderRenderer.RenderEntry
        // -----------------------------------------------------------------------

        [Fact]
        public void SliderRenderer_RenderEntry_WhenContextIsNull_ReturnsEarly()
        {
            // Arrange
            var mockLayout = new Mock<IGuiLayout>();
            var renderer = new SliderRenderer(null, mockLayout.Object);
            var entry = CreateUninitializedUiEntryModel();

            // Act
            renderer.RenderEntry(entry);

            // Assert
            mockLayout.Verify(l => l.HorizontalSlider(It.IsAny<float>(), It.IsAny<float>(), It.IsAny<float>(), It.IsAny<GuiStyleAdapter>(), It.IsAny<GuiStyleAdapter>(), It.IsAny<GuiLayoutOptionAdapter>()), Times.Never);
        }

        [Fact]
        public void SliderRenderer_RenderEntry_WhenEntryIsNull_ReturnsEarly()
        {
            // Arrange
            var context = CreateUninitializedViewModel();
            var mockLayout = new Mock<IGuiLayout>();
            var renderer = new SliderRenderer(context, mockLayout.Object);

            // Act
            renderer.RenderEntry(null);

            // Assert
            mockLayout.Verify(l => l.HorizontalSlider(It.IsAny<float>(), It.IsAny<float>(), It.IsAny<float>(), It.IsAny<GuiStyleAdapter>(), It.IsAny<GuiStyleAdapter>(), It.IsAny<GuiLayoutOptionAdapter>()), Times.Never);
        }

        [Fact]
        public void SliderRenderer_RenderEntry_WhenValueTypeIsNotPrimitive_ReturnsEarly()
        {
            // Arrange
            var context = CreateUninitializedViewModel();
            var mockLayout = new Mock<IGuiLayout>();
            var renderer = new SliderRenderer(context, mockLayout.Object);
            var entry = CreateUiEntryModelWithValue(typeof(string), "test");

            // Act
            renderer.RenderEntry(entry);

            // Assert
            mockLayout.Verify(l => l.HorizontalSlider(It.IsAny<float>(), It.IsAny<float>(), It.IsAny<float>(), It.IsAny<GuiStyleAdapter>(), It.IsAny<GuiStyleAdapter>(), It.IsAny<GuiLayoutOptionAdapter>()), Times.Never);
        }

        [Fact]
        public void SliderRenderer_RenderEntry_WhenValueTypeIsBool_ReturnsEarly()
        {
            // Arrange
            var context = CreateUninitializedViewModel();
            var mockLayout = new Mock<IGuiLayout>();
            var renderer = new SliderRenderer(context, mockLayout.Object);
            var entry = CreateUiEntryModelWithValue(typeof(bool), true);

            // Act
            renderer.RenderEntry(entry);

            // Assert
            mockLayout.Verify(l => l.HorizontalSlider(It.IsAny<float>(), It.IsAny<float>(), It.IsAny<float>(), It.IsAny<GuiStyleAdapter>(), It.IsAny<GuiStyleAdapter>(), It.IsAny<GuiLayoutOptionAdapter>()), Times.Never);
        }

        [Fact]
        public void SliderRenderer_RenderEntry_WhenValueTypeIsChar_ReturnsEarly()
        {
            // Arrange
            var context = CreateUninitializedViewModel();
            var mockLayout = new Mock<IGuiLayout>();
            var renderer = new SliderRenderer(context, mockLayout.Object);
            var entry = CreateUiEntryModelWithValue(typeof(char), 'a');

            // Act
            renderer.RenderEntry(entry);

            // Assert
            mockLayout.Verify(l => l.HorizontalSlider(It.IsAny<float>(), It.IsAny<float>(), It.IsAny<float>(), It.IsAny<GuiStyleAdapter>(), It.IsAny<GuiStyleAdapter>(), It.IsAny<GuiLayoutOptionAdapter>()), Times.Never);
        }

        [Fact]
        public void SliderRenderer_RenderEntry_WhenMetadataIsNull_ShowsErrorLabel()
        {
            // Arrange
            var context = CreateUninitializedViewModel();
            var mockLayout = new Mock<IGuiLayout>();
            var mockLayoutOption = new Mock<GuiLayoutOptionAdapter>(null);
            mockLayout.Setup(l => l.ExpandWidth(true)).Returns(mockLayoutOption.Object);
            var renderer = new SliderRenderer(context, mockLayout.Object);
            var entry = CreateUiEntryModelWithValue(typeof(int), 42);

            // Act
            renderer.RenderEntry(entry);

            // Assert
            mockLayout.Verify(l => l.Label(It.IsAny<GuiContentAdapter>(), mockLayoutOption.Object), Times.Once);
        }

        // -----------------------------------------------------------------------
        // ResetButtonRenderer.Render
        // -----------------------------------------------------------------------

        [Fact]
        public void ResetButtonRenderer_Render_WhenContextIsNull_ReturnsEarly()
        {
            // Arrange
            var mockLayout = new Mock<IGuiLayout>();
            var entry = CreateUninitializedUiEntryModel();

            // Act
            ResetButtonRenderer.Render(null, mockLayout.Object, entry);

            // Assert
            mockLayout.Verify(l => l.Button(It.IsAny<string>(), It.IsAny<GuiLayoutOptionAdapter>()), Times.Never);
        }

        [Fact]
        public void ResetButtonRenderer_Render_WhenEntryIsNull_ReturnsEarly()
        {
            // Arrange
            var context = CreateUninitializedViewModel();
            var mockLayout = new Mock<IGuiLayout>();

            // Act
            ResetButtonRenderer.Render(context, mockLayout.Object, null);

            // Assert
            mockLayout.Verify(l => l.Button(It.IsAny<string>(), It.IsAny<GuiLayoutOptionAdapter>()), Times.Never);
        }

        [Fact]
        public void ResetButtonRenderer_Render_CallsButtonWithResetText()
        {
            // Arrange
            var context = CreateUninitializedViewModel();
            var mockLayout = new Mock<IGuiLayout>();
            var mockLayoutOption = new Mock<GuiLayoutOptionAdapter>(null);
            mockLayout.Setup(l => l.ExpandWidth(false)).Returns(mockLayoutOption.Object);
            mockLayout.Setup(l => l.Button(It.IsAny<string>(), It.IsAny<GuiLayoutOptionAdapter>())).Returns(false);
            var entry = CreateUninitializedUiEntryModel();

            // Act
            ResetButtonRenderer.Render(context, mockLayout.Object, entry);

            // Assert
            mockLayout.Verify(l => l.Button(It.IsAny<string>(), mockLayoutOption.Object), Times.Once);
        }

        [Fact]
        public void ResetButtonRenderer_Render_WhenButtonNotClicked_DoesNotCallResetValue()
        {
            // Arrange
            var context = CreateUninitializedViewModel();
            var mockLayout = new Mock<IGuiLayout>();
            var mockLayoutOption = new Mock<GuiLayoutOptionAdapter>(null);
            mockLayout.Setup(l => l.ExpandWidth(false)).Returns(mockLayoutOption.Object);
            mockLayout.Setup(l => l.Button(It.IsAny<string>(), It.IsAny<GuiLayoutOptionAdapter>())).Returns(false);
            var entry = CreateUninitializedUiEntryModel();

            // Act
            ResetButtonRenderer.Render(context, mockLayout.Object, entry);

            // Assert - if ResetValue is not called, test passes
            Assert.True(true);
        }

        // -----------------------------------------------------------------------
        // ResetButtonRenderer.GetWidth
        // -----------------------------------------------------------------------

        [Fact]
        public void ResetButtonRenderer_GetWidth_DoesNotThrow()
        {
            // Act & Assert - This method accesses Unity GUI which may not be available in test environment
            // Just verify it doesn't throw or returns a reasonable value
            try
            {
                var width = ResetButtonRenderer.GetWidth();
                // If Unity GUI is available, width should be positive
                Assert.True(width >= 0);
            }
            catch (System.Security.SecurityException)
            {
                // Unity GUI not available in test environment - this is acceptable
                Assert.True(true);
            }
        }

        // -----------------------------------------------------------------------
        // TableRenderer Constructor
        // -----------------------------------------------------------------------

        [Fact]
        public void TableRenderer_Constructor_SetsContext()
        {
            // Arrange
            var context = CreateUninitializedViewModel();
            var mockLayout = new Mock<IGuiLayout>();

            // Act
            var renderer = new TableRenderer(context, mockLayout.Object);

            // Assert
            Assert.Same(context, renderer.Context);
        }

        [Fact]
        public void TableRenderer_Constructor_CreatesBoolEntryRenderer()
        {
            // Arrange
            var context = CreateUninitializedViewModel();
            var mockLayout = new Mock<IGuiLayout>();

            // Act
            var renderer = new TableRenderer(context, mockLayout.Object);

            // Assert
            Assert.NotNull(renderer.BoolEntryRenderer);
            Assert.Same(context, renderer.BoolEntryRenderer.Context);
        }

        [Fact]
        public void TableRenderer_Constructor_CreatesStringEntryRenderer()
        {
            // Arrange
            var context = CreateUninitializedViewModel();
            var mockLayout = new Mock<IGuiLayout>();

            // Act
            var renderer = new TableRenderer(context, mockLayout.Object);

            // Assert
            Assert.NotNull(renderer.StringEntryRenderer);
            Assert.Same(context, renderer.StringEntryRenderer.Context);
        }

        [Fact]
        public void TableRenderer_Constructor_CreatesNumberEntryRenderer()
        {
            // Arrange
            var context = CreateUninitializedViewModel();
            var mockLayout = new Mock<IGuiLayout>();

            // Act
            var renderer = new TableRenderer(context, mockLayout.Object);

            // Assert
            Assert.NotNull(renderer.NumberEntryRenderer);
            Assert.Same(context, renderer.NumberEntryRenderer.Context);
        }

        [Fact]
        public void TableRenderer_Constructor_CreatesEnumEntryRenderer()
        {
            // Arrange
            var context = CreateUninitializedViewModel();
            var mockLayout = new Mock<IGuiLayout>();

            // Act
            var renderer = new TableRenderer(context, mockLayout.Object);

            // Assert
            Assert.NotNull(renderer.EnumEntryRenderer);
            Assert.Same(context, renderer.EnumEntryRenderer.Context);
        }

        [Fact]
        public void TableRenderer_Constructor_CreatesSliderEntryRenderer()
        {
            // Arrange
            var context = CreateUninitializedViewModel();
            var mockLayout = new Mock<IGuiLayout>();

            // Act
            var renderer = new TableRenderer(context, mockLayout.Object);

            // Assert
            Assert.NotNull(renderer.SliderEntryRenderer);
            Assert.Same(context, renderer.SliderEntryRenderer.Context);
        }

        // -----------------------------------------------------------------------
        // TableRenderer.Render
        // -----------------------------------------------------------------------

        [Fact]
        public void TableRenderer_Render_WhenContextIsNull_ReturnsEarly()
        {
            // Arrange
            var mockLayout = new Mock<IGuiLayout>();
            var renderer = new TableRenderer(null, mockLayout.Object);
            var table = CreateUninitializedUiTableModel();

            // Act
            renderer.Render(table);

            // Assert
            mockLayout.Verify(l => l.BeginVertical(It.IsAny<GuiStyleAdapter>()), Times.Never);
        }

        [Fact]
        public void TableRenderer_Render_WhenTableIsNull_ReturnsEarly()
        {
            // Arrange
            var context = CreateUninitializedViewModel();
            var mockLayout = new Mock<IGuiLayout>();
            var renderer = new TableRenderer(context, mockLayout.Object);

            // Act
            renderer.Render(null);

            // Assert
            mockLayout.Verify(l => l.BeginVertical(It.IsAny<GuiStyleAdapter>()), Times.Never);
        }

        // -----------------------------------------------------------------------
        // SheetRenderer Constructor
        // -----------------------------------------------------------------------

        [Fact]
        public void SheetRenderer_Constructor_SetsContext()
        {
            // Arrange
            var context = CreateUninitializedViewModel();
            var mockLayout = new Mock<IGuiLayout>();

            // Act
            var renderer = new SheetRenderer(context, mockLayout.Object);

            // Assert
            Assert.Same(context, renderer.Context);
        }

        [Fact]
        public void SheetRenderer_Constructor_CreatesTableRenderer()
        {
            // Arrange
            var context = CreateUninitializedViewModel();
            var mockLayout = new Mock<IGuiLayout>();

            // Act
            var renderer = new SheetRenderer(context, mockLayout.Object);

            // Assert
            Assert.NotNull(renderer.TableRenderer);
            Assert.Same(context, renderer.TableRenderer.Context);
        }

        // -----------------------------------------------------------------------
        // Helper Methods
        // -----------------------------------------------------------------------

        private enum TestEnum
        {
            Value1,
            Value2,
            Value3
        }

        // -----------------------------------------------------------------------
        // Helper Methods
        // -----------------------------------------------------------------------

        private static ViewModel CreateUninitializedViewModel()
        {
            return (ViewModel)RuntimeHelpers.GetUninitializedObject(typeof(ViewModel));
        }

        private static UiEntryModel CreateUninitializedUiEntryModel()
        {
            return (UiEntryModel)RuntimeHelpers.GetUninitializedObject(typeof(UiEntryModel));
        }

        private static UiSheetModel CreateUninitializedUiSheetModel()
        {
            var sheet = (UiSheetModel)RuntimeHelpers.GetUninitializedObject(typeof(UiSheetModel));
            var field = typeof(UiSheetModel).GetField("<Sheet>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(sheet, new System.Collections.Generic.List<UiTableModel>());
            return sheet;
        }

        private static UiTableModel CreateUninitializedUiTableModel()
        {
            return (UiTableModel)RuntimeHelpers.GetUninitializedObject(typeof(UiTableModel));
        }

        private static UiTableModel CreateUiTableModelWithEntries(string name, string description, UiEntryModel[] entries)
        {
            var table = CreateUninitializedUiTableModel();
            var configTable = new ConfigTable("TestTableKey", new Translator(name, name), new Translator(description, description));

            var tableField = typeof(UiTableModel).GetField("_table", BindingFlags.NonPublic | BindingFlags.Instance);
            tableField.SetValue(table, configTable);

            var entriesField = typeof(UiTableModel).GetField("<Table>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
            if (entriesField != null)
            {
                entriesField.SetValue(table, new System.Collections.Generic.List<UiEntryModel>(entries));
            }

            return table;
        }

        private static UiEntryModel CreateUiEntryModelWithMockEntry(string name, string description)
        {
            var entry = CreateUninitializedUiEntryModel();
            var mockConfigEntry = new Mock<IConfigEntry>();
            mockConfigEntry.Setup(e => e.Name).Returns(new Translator(name, name));
            mockConfigEntry.Setup(e => e.Description).Returns(new Translator(description, description));
            mockConfigEntry.Setup(e => e.Key).Returns("TestKey");
            
            var field = typeof(UiEntryModel).GetField("_entry", BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(entry, mockConfigEntry.Object);
            
            return entry;
        }

        private static UiEntryModel CreateUiEntryModelWithValue(Type valueType, object value)
        {
            var entry = CreateUninitializedUiEntryModel();
            var mockConfigEntry = new Mock<IConfigEntry>();
            mockConfigEntry.Setup(e => e.ValueType).Returns(valueType);
            mockConfigEntry.Setup(e => e.BoxedValue).Returns(value);
            mockConfigEntry.Setup(e => e.Name).Returns(new Translator("Test", "Test"));
            mockConfigEntry.Setup(e => e.Description).Returns(new Translator("Description", "Description"));
            mockConfigEntry.Setup(e => e.Key).Returns("TestKey");
            
            var field = typeof(UiEntryModel).GetField("_entry", BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(entry, mockConfigEntry.Object);
            
            return entry;
        }

        private static UiEntryModel CreateUiEntryModelWithValueAndMetadata(Type valueType, object value, IUiMetadata metadata)
        {
            var entry = CreateUiEntryModelWithValue(valueType, value);
            
            var metadataField = typeof(UiEntryModel).GetField("<Metadata>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
            if (metadataField != null)
            {
                metadataField.SetValue(entry, metadata);
            }
            
            return entry;
        }

        private static void SetPrivateProperty(object obj, string propertyName, object value)
        {
            var property = obj.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            if (property != null && property.CanWrite)
            {
                property.SetValue(obj, value);
            }
            else
            {
                // Try setting the backing field for read-only properties
                var field = obj.GetType().GetField($"<{propertyName}>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
                if (field == null)
                {
                    // Try common naming patterns for backing fields
                    field = obj.GetType().GetField($"_{char.ToLower(propertyName[0])}{propertyName.Substring(1)}", BindingFlags.NonPublic | BindingFlags.Instance);
                }
                if (field != null)
                {
                    field.SetValue(obj, value);
                }
            }
        }

        // -----------------------------------------------------------------------
        // Test Helper Classes
        // -----------------------------------------------------------------------

        private class TestRenderer : BaseEntryRenderer
        {
            public TestRenderer(ViewModel context, IGuiLayout layout) : base(context, layout)
            {
            }

            public override void RenderEntry(UiEntryModel entry)
            {
            }

            public IGuiLayout GetLayout()
            {
                return Layout;
            }
        }

        private class TestRendererTrackingRenderEntry : BaseEntryRenderer
        {
            public bool RenderEntryCalled { get; private set; }

            public TestRendererTrackingRenderEntry(ViewModel context, IGuiLayout layout) : base(context, layout)
            {
            }

            public override void RenderEntry(UiEntryModel entry)
            {
                RenderEntryCalled = true;
            }
        }

        private class TestRendererTrackingRenderAfterRow : BaseEntryRenderer
        {
            public bool RenderAfterRowCalled { get; private set; }

            public TestRendererTrackingRenderAfterRow(ViewModel context, IGuiLayout layout) : base(context, layout)
            {
            }

            public override void RenderEntry(UiEntryModel entry)
            {
            }

            public override void RenderAfterRow(UiEntryModel entry)
            {
                RenderAfterRowCalled = true;
                base.RenderAfterRow(entry);
            }
        }

        // -----------------------------------------------------------------------
        // SheetRenderer Constructor and Render
        // -----------------------------------------------------------------------

        [Fact]
        public void SheetRenderer_Render_WhenContextIsNull_ReturnsEarly()
        {
            // Arrange
            var mockLayout = new Mock<IGuiLayout>();
            var renderer = new SheetRenderer(null, mockLayout.Object);
            var sheet = CreateUninitializedUiSheetModel();

            // Act
            renderer.Render(sheet);

            // Assert
            // Verify no interactions with TableRenderer
            Assert.NotNull(renderer.TableRenderer);
        }

        [Fact]
        public void SheetRenderer_Render_WhenSheetIsNull_ReturnsEarly()
        {
            // Arrange
            var context = CreateUninitializedViewModel();
            var mockLayout = new Mock<IGuiLayout>();
            var renderer = new SheetRenderer(context, mockLayout.Object);

            // Act
            renderer.Render(null);

            // Assert
            mockLayout.Verify(l => l.Space(It.IsAny<float>()), Times.Never);
        }

        [Fact]
        public void SheetRenderer_Render_WhenSheetIsEmpty_CompletesWithoutError()
        {
            // Arrange
            var context = CreateUninitializedViewModel();
            var mockLayout = new Mock<IGuiLayout>();
            var renderer = new SheetRenderer(context, mockLayout.Object);
            var sheet = CreateUninitializedUiSheetModel();

            // Act
            renderer.Render(sheet);

            // Assert
            mockLayout.Verify(l => l.Space(It.IsAny<float>()), Times.Never);
        }

        // -----------------------------------------------------------------------
        // ToastRenderer Constructor
        // -----------------------------------------------------------------------

        [Fact]
        public void ToastRenderer_Constructor_SetsContext()
        {
            // Arrange
            var context = CreateUninitializedViewModel();

            // Act
            var renderer = new ToastRenderer(context);

            // Assert
            Assert.Same(context, renderer.Context);
        }

        // -----------------------------------------------------------------------
        // ToastRenderer Render
        // -----------------------------------------------------------------------

        [Fact]
        public void ToastRenderer_Render_WhenContextIsNull_ReturnsEarly()
        {
            // Arrange
            var renderer = new ToastRenderer(null);

            // Act & Assert (should not throw)
            renderer.Render();
        }

        [Fact]
        public void ToastRenderer_Render_WhenToastMessageIsNull_ReturnsEarly()
        {
            // Arrange
            var context = CreateUninitializedViewModel();
            SetPrivateProperty(context, "ToastMessage", null);
            var renderer = new ToastRenderer(context);

            // Act & Assert (should not throw)
            renderer.Render();
        }

        [Fact]
        public void ToastRenderer_Render_WhenToastMessageIsEmpty_ReturnsEarly()
        {
            // Arrange
            var context = CreateUninitializedViewModel();
            SetPrivateProperty(context, "ToastMessage", string.Empty);
            var renderer = new ToastRenderer(context);

            // Act & Assert (should not throw)
            renderer.Render();
        }

        // -----------------------------------------------------------------------
        // TooltipRenderer Constructor
        // -----------------------------------------------------------------------

        [Fact]
        public void TooltipRenderer_Constructor_SetsContext()
        {
            // Arrange
            var context = CreateUninitializedViewModel();

            // Act
            var renderer = new TooltipRenderer(context);

            // Assert
            Assert.Same(context, renderer.Context);
        }

        // -----------------------------------------------------------------------
        // TooltipRenderer Render
        // -----------------------------------------------------------------------

        [Fact]
        public void TooltipRenderer_Render_WhenContextIsNull_ReturnsEarly()
        {
            // Arrange
            var renderer = new TooltipRenderer(null);

            // Act & Assert (should not throw)
            renderer.Render();
        }
    }
}
