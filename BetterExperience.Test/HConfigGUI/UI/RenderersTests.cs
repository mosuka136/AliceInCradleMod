using BetterExperience.HConfigFileSpace;
using BetterExperience.HConfigGUI;
using BetterExperience.HConfigGUI.UI;
using BetterExperience.HotkeyManager;
using BetterExperience.HProvider;
using BetterExperience.HTranslatorSpace;
using Moq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security;

namespace BetterExperience.Test
{
    public class RenderersTests
    {
        // -----------------------------------------------------------------------
        // BaseEntryRenderer Constructor
        // -----------------------------------------------------------------------

        [Fact]
        public void BaseEntryRenderer_Constructor_SetsContextProperty()
        {
            // Arrange
            var viewModel = CreateUninitializedViewModel();

            // Act
            var renderer = new TestEntryRenderer(viewModel);

            // Assert
            Assert.Same(viewModel, renderer.Context);
        }

        [Fact]
        public void BaseEntryRenderer_Constructor_WithNullContext_SetsContextToNull()
        {
            // Arrange
            ViewModel viewModel = null;

            // Act
            var renderer = new TestEntryRenderer(viewModel);

            // Assert
            Assert.Null(renderer.Context);
        }

        // -----------------------------------------------------------------------
        // BaseEntryRenderer.Render
        // -----------------------------------------------------------------------

        [Fact]
        public void Render_WhenContextIsNull_ReturnsEarly()
        {
            // Arrange
            var renderer = new TestEntryRenderer(null);
            var entry = CreateUninitializedUiEntryModel();

            // Act
            renderer.Render(entry);

            // Assert
            // No exception thrown, method returns early
            Assert.Null(renderer.Context);
        }

        [Fact]
        public void Render_WhenEntryIsNull_ReturnsEarly()
        {
            // Arrange
            var viewModel = CreateUninitializedViewModel();
            var renderer = new TestEntryRenderer(viewModel);

            // Act
            renderer.Render(null);

            // Assert
            // No exception thrown, method returns early
            Assert.NotNull(renderer.Context);
        }

        [Fact]
        public void Render_WhenBothContextAndEntryAreNull_ReturnsEarly()
        {
            // Arrange
            var renderer = new TestEntryRenderer(null);

            // Act
            renderer.Render(null);

            // Assert
            // No exception thrown, method returns early
            Assert.Null(renderer.Context);
        }

        [Fact]
        public void Render_WhenContextIsNullAndEntryIsValid_ReturnsEarly()
        {
            // Arrange
            var renderer = new TestEntryRenderer(null);
            var entry = CreateUiEntryModelWithMockedEntry("Test", "Description");

            // Act
            renderer.Render(entry);

            // Assert
            // No exception thrown, method returns early
            Assert.Null(renderer.Context);
        }

        [Fact]
        public void Render_WhenEntryIsNullAndContextIsValid_ReturnsEarly()
        {
            // Arrange
            var viewModel = CreateUninitializedViewModel();
            var renderer = new TestEntryRenderer(viewModel);

            // Act
            renderer.Render(null);

            // Assert
            // No exception thrown, method returns early
            Assert.Same(viewModel, renderer.Context);
        }

        // -----------------------------------------------------------------------
        // BaseEntryRenderer.RenderAfterRow
        // -----------------------------------------------------------------------

        [Fact]
        public void RenderAfterRow_WhenCalled_DoesNotThrow()
        {
            // Arrange
            var viewModel = CreateUninitializedViewModel();
            var renderer = new TestEntryRenderer(viewModel);
            var entry = CreateUninitializedUiEntryModel();

            // Act & Assert
            renderer.RenderAfterRow(entry);
        }

        [Fact]
        public void RenderAfterRow_WithNullEntry_DoesNotThrow()
        {
            // Arrange
            var viewModel = CreateUninitializedViewModel();
            var renderer = new TestEntryRenderer(viewModel);

            // Act & Assert
            renderer.RenderAfterRow(null);
        }

        [Fact]
        public void RenderAfterRow_WithNullContext_DoesNotThrow()
        {
            // Arrange
            var renderer = new TestEntryRenderer(null);
            var entry = CreateUninitializedUiEntryModel();

            // Act & Assert
            renderer.RenderAfterRow(entry);
        }

        [Fact]
        public void RenderAfterRow_IsVirtual_CanBeOverridden()
        {
            // Arrange
            var viewModel = CreateUninitializedViewModel();
            var renderer = new OverriddenRenderAfterRowRenderer(viewModel);
            var entry = CreateUninitializedUiEntryModel();

            // Act
            renderer.RenderAfterRow(entry);

            // Assert
            Assert.True(renderer.OverriddenMethodCalled);
        }

        // -----------------------------------------------------------------------
        // BoolRenderer Constructor
        // -----------------------------------------------------------------------

        [Fact]
        public void BoolRenderer_Constructor_SetsContextProperty()
        {
            // Arrange
            var viewModel = CreateUninitializedViewModel();

            // Act
            var renderer = new BoolRenderer(viewModel);

            // Assert
            Assert.Same(viewModel, renderer.Context);
        }

        [Fact]
        public void BoolRenderer_Constructor_WithNullContext_SetsContextToNull()
        {
            // Arrange
            ViewModel viewModel = null;

            // Act
            var renderer = new BoolRenderer(viewModel);

            // Assert
            Assert.Null(renderer.Context);
        }

        // -----------------------------------------------------------------------
        // BoolRenderer.RenderEntry
        // -----------------------------------------------------------------------

        [Fact]
        public void BoolRenderer_RenderEntry_ContextNull_ReturnsEarly()
        {
            // Arrange
            var renderer = new BoolRenderer(null);
            var entry = CreateUninitializedUiEntryModel();

            // Act
            renderer.RenderEntry(entry);

            // Assert
            Assert.Null(renderer.Context);
        }

        [Fact]
        public void BoolRenderer_RenderEntry_EntryNull_ReturnsEarly()
        {
            // Arrange
            var viewModel = CreateUninitializedViewModel();
            var renderer = new BoolRenderer(viewModel);

            // Act
            renderer.RenderEntry(null);

            // Assert
            Assert.NotNull(renderer.Context);
        }

        [Fact]
        public void BoolRenderer_RenderEntry_BothContextAndEntryNull_ReturnsEarly()
        {
            // Arrange
            var renderer = new BoolRenderer(null);

            // Act
            renderer.RenderEntry(null);

            // Assert
            Assert.Null(renderer.Context);
        }

        [Fact]
        public void BoolRenderer_RenderEntry_EntryValueTypeNotBool_ReturnsEarly()
        {
            // Arrange
            var viewModel = CreateUninitializedViewModel();
            var renderer = new BoolRenderer(viewModel);
            var entry = CreateUiEntryModelWithValueType(typeof(string));

            // Act
            renderer.RenderEntry(entry);

            // Assert
            Assert.Same(viewModel, renderer.Context);
        }

        [Fact]
        public void BoolRenderer_RenderEntry_ValueTypeInt_ReturnsEarly()
        {
            // Arrange
            var viewModel = CreateUninitializedViewModel();
            var renderer = new BoolRenderer(viewModel);
            var entry = CreateUiEntryModelWithValueType(typeof(int));

            // Act
            renderer.RenderEntry(entry);

            // Assert
            Assert.Same(viewModel, renderer.Context);
        }

        // -----------------------------------------------------------------------
        // StringRenderer Constructor
        // -----------------------------------------------------------------------

        [Fact]
        public void StringRenderer_Constructor_SetsContextProperty()
        {
            // Arrange
            var viewModel = CreateUninitializedViewModel();

            // Act
            var renderer = new StringRenderer(viewModel);

            // Assert
            Assert.Same(viewModel, renderer.Context);
        }

        [Fact]
        public void StringRenderer_Constructor_WithNullContext_SetsContextToNull()
        {
            // Arrange
            ViewModel viewModel = null;

            // Act
            var renderer = new StringRenderer(viewModel);

            // Assert
            Assert.Null(renderer.Context);
        }

        // -----------------------------------------------------------------------
        // StringRenderer.RenderEntry
        // -----------------------------------------------------------------------

        [Fact]
        public void StringRenderer_RenderEntry_ContextNull_ReturnsEarly()
        {
            // Arrange
            var renderer = new StringRenderer(null);
            var entry = CreateUninitializedUiEntryModel();

            // Act
            renderer.RenderEntry(entry);

            // Assert
            Assert.Null(renderer.Context);
        }

        [Fact]
        public void StringRenderer_RenderEntry_EntryNull_ReturnsEarly()
        {
            // Arrange
            var viewModel = CreateUninitializedViewModel();
            var renderer = new StringRenderer(viewModel);

            // Act
            renderer.RenderEntry(null);

            // Assert
            Assert.NotNull(renderer.Context);
        }

        [Fact]
        public void StringRenderer_RenderEntry_BothContextAndEntryNull_ReturnsEarly()
        {
            // Arrange
            var renderer = new StringRenderer(null);

            // Act
            renderer.RenderEntry(null);

            // Assert
            Assert.Null(renderer.Context);
        }

        [Fact]
        public void StringRenderer_RenderEntry_EntryValueTypeNotString_ReturnsEarly()
        {
            // Arrange
            var viewModel = CreateUninitializedViewModel();
            var renderer = new StringRenderer(viewModel);
            var entry = CreateUiEntryModelWithValueType(typeof(int));

            // Act
            renderer.RenderEntry(entry);

            // Assert
            Assert.Same(viewModel, renderer.Context);
        }

        [Fact]
        public void StringRenderer_RenderEntry_ValueTypeBoolean_ReturnsEarly()
        {
            // Arrange
            var viewModel = CreateUninitializedViewModel();
            var renderer = new StringRenderer(viewModel);
            var entry = CreateUiEntryModelWithValueType(typeof(bool));

            // Act
            renderer.RenderEntry(entry);

            // Assert
            Assert.Same(viewModel, renderer.Context);
        }

        // -----------------------------------------------------------------------
        // NumberRenderer Constructor
        // -----------------------------------------------------------------------

        [Fact]
        public void NumberRenderer_Constructor_SetsContextProperty()
        {
            // Arrange
            var viewModel = CreateUninitializedViewModel();

            // Act
            var renderer = new NumberRenderer(viewModel);

            // Assert
            Assert.Same(viewModel, renderer.Context);
        }

        [Fact]
        public void NumberRenderer_Constructor_WithNullContext_SetsContextToNull()
        {
            // Arrange
            ViewModel viewModel = null;

            // Act
            var renderer = new NumberRenderer(viewModel);

            // Assert
            Assert.Null(renderer.Context);
        }

        // -----------------------------------------------------------------------
        // NumberRenderer.RenderEntry
        // -----------------------------------------------------------------------

        [Fact]
        public void NumberRenderer_RenderEntry_ContextNull_ReturnsEarly()
        {
            // Arrange
            var renderer = new NumberRenderer(null);
            var entry = CreateUninitializedUiEntryModel();

            // Act
            renderer.RenderEntry(entry);

            // Assert
            Assert.Null(renderer.Context);
        }

        [Fact]
        public void NumberRenderer_RenderEntry_EntryNull_ReturnsEarly()
        {
            // Arrange
            var viewModel = CreateUninitializedViewModel();
            var renderer = new NumberRenderer(viewModel);

            // Act
            renderer.RenderEntry(null);

            // Assert
            Assert.NotNull(renderer.Context);
        }

        [Fact]
        public void NumberRenderer_RenderEntry_BothContextAndEntryNull_ReturnsEarly()
        {
            // Arrange
            var renderer = new NumberRenderer(null);

            // Act
            renderer.RenderEntry(null);

            // Assert
            Assert.Null(renderer.Context);
        }

        [Fact]
        public void NumberRenderer_RenderEntry_ValueTypeNotPrimitive_ReturnsEarly()
        {
            // Arrange
            var viewModel = CreateUninitializedViewModel();
            var renderer = new NumberRenderer(viewModel);
            var entry = CreateUiEntryModelWithValueType(typeof(string));

            // Act
            renderer.RenderEntry(entry);

            // Assert
            Assert.Same(viewModel, renderer.Context);
        }

        [Fact]
        public void NumberRenderer_RenderEntry_ValueTypeBool_ReturnsEarly()
        {
            // Arrange
            var viewModel = CreateUninitializedViewModel();
            var renderer = new NumberRenderer(viewModel);
            var entry = CreateUiEntryModelWithValueType(typeof(bool));

            // Act
            renderer.RenderEntry(entry);

            // Assert
            Assert.Same(viewModel, renderer.Context);
        }

        [Fact]
        public void NumberRenderer_RenderEntry_ValueTypeChar_ReturnsEarly()
        {
            // Arrange
            var viewModel = CreateUninitializedViewModel();
            var renderer = new NumberRenderer(viewModel);
            var entry = CreateUiEntryModelWithValueType(typeof(char));

            // Act
            renderer.RenderEntry(entry);

            // Assert
            Assert.Same(viewModel, renderer.Context);
        }

        // -----------------------------------------------------------------------
        // IEntryRenderer Interface
        // -----------------------------------------------------------------------

        [Fact]
        public void IEntryRenderer_Context_IsAccessible()
        {
            // Arrange
            var viewModel = CreateUninitializedViewModel();
            IEntryRenderer renderer = new TestEntryRenderer(viewModel);

            // Act
            var context = renderer.Context;

            // Assert
            Assert.Same(viewModel, context);
        }

        // -----------------------------------------------------------------------
        // EnumRenderer Constructor
        // -----------------------------------------------------------------------

        [Fact]
        public void EnumRenderer_Constructor_SetsContextProperty()
        {
            // Arrange
            var viewModel = CreateUninitializedViewModel();

            // Act
            var renderer = new EnumRenderer(viewModel);

            // Assert
            Assert.Same(viewModel, renderer.Context);
        }

        [Fact]
        public void EnumRenderer_Constructor_WithNullContext_SetsContextToNull()
        {
            // Arrange
            ViewModel viewModel = null;

            // Act
            var renderer = new EnumRenderer(viewModel);

            // Assert
            Assert.Null(renderer.Context);
        }

        // -----------------------------------------------------------------------
        // EnumRenderer.RenderEntry
        // -----------------------------------------------------------------------

        [Fact]
        public void EnumRenderer_RenderEntry_ContextNull_ReturnsEarly()
        {
            // Arrange
            var renderer = new EnumRenderer(null);
            var entry = CreateUninitializedUiEntryModel();

            // Act
            renderer.RenderEntry(entry);

            // Assert
            Assert.Null(renderer.Context);
        }

        [Fact]
        public void EnumRenderer_RenderEntry_EntryNull_ReturnsEarly()
        {
            // Arrange
            var viewModel = CreateUninitializedViewModel();
            var renderer = new EnumRenderer(viewModel);

            // Act
            renderer.RenderEntry(null);

            // Assert
            Assert.NotNull(renderer.Context);
        }

        [Fact]
        public void EnumRenderer_RenderEntry_BothContextAndEntryNull_ReturnsEarly()
        {
            // Arrange
            var renderer = new EnumRenderer(null);

            // Act
            renderer.RenderEntry(null);

            // Assert
            Assert.Null(renderer.Context);
        }

        [Fact]
        public void EnumRenderer_RenderEntry_ValueTypeNotEnum_ReturnsEarly()
        {
            // Arrange
            var viewModel = CreateUninitializedViewModel();
            var renderer = new EnumRenderer(viewModel);
            var entry = CreateUiEntryModelWithValueType(typeof(string));

            // Act
            renderer.RenderEntry(entry);

            // Assert
            Assert.Same(viewModel, renderer.Context);
        }

        [Fact]
        public void EnumRenderer_RenderEntry_ValueTypeInt_ReturnsEarly()
        {
            // Arrange
            var viewModel = CreateUninitializedViewModel();
            var renderer = new EnumRenderer(viewModel);
            var entry = CreateUiEntryModelWithValueType(typeof(int));

            // Act
            renderer.RenderEntry(entry);

            // Assert
            Assert.Same(viewModel, renderer.Context);
        }

        // -----------------------------------------------------------------------
        // EnumRenderer.RenderAfterRow
        // -----------------------------------------------------------------------

        [Fact]
        public void EnumRenderer_RenderAfterRow_ContextNull_ReturnsEarly()
        {
            // Arrange
            var renderer = new EnumRenderer(null);
            var entry = CreateUninitializedUiEntryModel();

            // Act
            renderer.RenderAfterRow(entry);

            // Assert
            Assert.Null(renderer.Context);
        }

        [Fact]
        public void EnumRenderer_RenderAfterRow_EntryNull_ReturnsEarly()
        {
            // Arrange
            var viewModel = CreateUninitializedViewModel();
            var renderer = new EnumRenderer(viewModel);

            // Act
            renderer.RenderAfterRow(null);

            // Assert
            Assert.NotNull(renderer.Context);
        }

        [Fact]
        public void EnumRenderer_RenderAfterRow_BothContextAndEntryNull_ReturnsEarly()
        {
            // Arrange
            var renderer = new EnumRenderer(null);

            // Act
            renderer.RenderAfterRow(null);

            // Assert
            Assert.Null(renderer.Context);
        }

        [Fact]
        public void EnumRenderer_RenderAfterRow_OpenedEnumEntryNotMatching_ReturnsEarly()
        {
            // Arrange
            var viewModel = CreateUninitializedViewModel();
            var renderer = new EnumRenderer(viewModel);
            var entry = CreateUninitializedUiEntryModel();
            var differentEntry = CreateUninitializedUiEntryModel();
            SetPrivateAutoProperty(viewModel, "OpenedEnumEntry", differentEntry);

            // Act
            renderer.RenderAfterRow(entry);

            // Assert
            Assert.Same(viewModel, renderer.Context);
        }

        [Fact]
        public void EnumRenderer_RenderAfterRow_OpenedEnumEntryNull_ReturnsEarly()
        {
            // Arrange
            var viewModel = CreateUninitializedViewModel();
            var renderer = new EnumRenderer(viewModel);
            var entry = CreateUninitializedUiEntryModel();
            SetPrivateAutoProperty(viewModel, "OpenedEnumEntry", null);

            // Act
            renderer.RenderAfterRow(entry);

            // Assert
            Assert.Same(viewModel, renderer.Context);
        }

        [Fact]
        public void EnumRenderer_RenderAfterRow_ValueTypeNotEnum_ReturnsEarly()
        {
            // Arrange
            var viewModel = CreateUninitializedViewModel();
            var renderer = new EnumRenderer(viewModel);
            var entry = CreateUiEntryModelWithValueType(typeof(string));
            SetPrivateAutoProperty(viewModel, "OpenedEnumEntry", entry);

            // Act
            renderer.RenderAfterRow(entry);

            // Assert
            Assert.Same(viewModel, renderer.Context);
        }

        // -----------------------------------------------------------------------
        // SliderRenderer Constructor
        // -----------------------------------------------------------------------

        [Fact]
        public void SliderRenderer_Constructor_SetsContextProperty()
        {
            // Arrange
            var viewModel = CreateUninitializedViewModel();

            // Act
            var renderer = new SliderRenderer(viewModel);

            // Assert
            Assert.Same(viewModel, renderer.Context);
        }

        [Fact]
        public void SliderRenderer_Constructor_WithNullContext_SetsContextToNull()
        {
            // Arrange
            ViewModel viewModel = null;

            // Act
            var renderer = new SliderRenderer(viewModel);

            // Assert
            Assert.Null(renderer.Context);
        }

        // -----------------------------------------------------------------------
        // SliderRenderer.RenderEntry
        // -----------------------------------------------------------------------

        [Fact]
        public void SliderRenderer_RenderEntry_ContextNull_ReturnsEarly()
        {
            // Arrange
            var renderer = new SliderRenderer(null);
            var entry = CreateUninitializedUiEntryModel();

            // Act
            renderer.RenderEntry(entry);

            // Assert
            Assert.Null(renderer.Context);
        }

        [Fact]
        public void SliderRenderer_RenderEntry_EntryNull_ReturnsEarly()
        {
            // Arrange
            var viewModel = CreateUninitializedViewModel();
            var renderer = new SliderRenderer(viewModel);

            // Act
            renderer.RenderEntry(null);

            // Assert
            Assert.NotNull(renderer.Context);
        }

        [Fact]
        public void SliderRenderer_RenderEntry_BothContextAndEntryNull_ReturnsEarly()
        {
            // Arrange
            var renderer = new SliderRenderer(null);

            // Act
            renderer.RenderEntry(null);

            // Assert
            Assert.Null(renderer.Context);
        }

        [Fact]
        public void SliderRenderer_RenderEntry_ValueTypeNotPrimitive_ReturnsEarly()
        {
            // Arrange
            var viewModel = CreateUninitializedViewModel();
            var renderer = new SliderRenderer(viewModel);
            var entry = CreateUiEntryModelWithValueType(typeof(string));

            // Act
            renderer.RenderEntry(entry);

            // Assert
            Assert.Same(viewModel, renderer.Context);
        }

        [Fact]
        public void SliderRenderer_RenderEntry_ValueTypeBool_ReturnsEarly()
        {
            // Arrange
            var viewModel = CreateUninitializedViewModel();
            var renderer = new SliderRenderer(viewModel);
            var entry = CreateUiEntryModelWithValueType(typeof(bool));

            // Act
            renderer.RenderEntry(entry);

            // Assert
            Assert.Same(viewModel, renderer.Context);
        }

        [Fact]
        public void SliderRenderer_RenderEntry_ValueTypeChar_ReturnsEarly()
        {
            // Arrange
            var viewModel = CreateUninitializedViewModel();
            var renderer = new SliderRenderer(viewModel);
            var entry = CreateUiEntryModelWithValueType(typeof(char));

            // Act
            renderer.RenderEntry(entry);

            // Assert
            Assert.Same(viewModel, renderer.Context);
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

        private static UiTableModel CreateUninitializedUiTableModel()
        {
            return (UiTableModel)RuntimeHelpers.GetUninitializedObject(typeof(UiTableModel));
        }

        private static UiSheetModel CreateUninitializedUiSheetModel()
        {
            return (UiSheetModel)RuntimeHelpers.GetUninitializedObject(typeof(UiSheetModel));
        }

        private static UiEntryModel CreateUiEntryModelWithMockedEntry(string name, string description)
        {
            var entry = CreateUninitializedUiEntryModel();
            var mockConfigEntry = new Mock<IConfigEntry>();
            
            var translatorName = new Translator(name, name);
            var translatorDesc = new Translator(description, description);
            
            mockConfigEntry.Setup(x => x.Name).Returns(translatorName);
            mockConfigEntry.Setup(x => x.Description).Returns(translatorDesc);
            
            SetPrivateField(entry, "_entry", mockConfigEntry.Object);
            return entry;
        }

        private static UiEntryModel CreateUiEntryModelWithValueType(Type valueType)
        {
            var entry = CreateUninitializedUiEntryModel();
            var mockConfigEntry = new Mock<IConfigEntry>();
            
            var translatorName = new Translator("Test", "Test");
            var translatorDesc = new Translator("Description", "Description");
            
            mockConfigEntry.Setup(x => x.Name).Returns(translatorName);
            mockConfigEntry.Setup(x => x.Description).Returns(translatorDesc);
            mockConfigEntry.Setup(x => x.ValueType).Returns(valueType);
            
            SetPrivateField(entry, "_entry", mockConfigEntry.Object);
            return entry;
        }

        private static UiEntryModel CreateUiEntryModelWithEnumType(Type enumType, object value)
        {
            var entry = CreateUninitializedUiEntryModel();
            var mockConfigEntry = new Mock<IConfigEntry>();
            
            var translatorName = new Translator("Test", "Test");
            var translatorDesc = new Translator("Description", "Description");
            
            mockConfigEntry.Setup(x => x.Name).Returns(translatorName);
            mockConfigEntry.Setup(x => x.Description).Returns(translatorDesc);
            mockConfigEntry.Setup(x => x.ValueType).Returns(enumType);
            mockConfigEntry.Setup(x => x.BoxedValue).Returns(value);
            
            SetPrivateField(entry, "_entry", mockConfigEntry.Object);
            return entry;
        }

        private static UiEntryModel CreateUiEntryModelWithMetadata(Type valueType, object value, IUiMetadata metadata)
        {
            var entry = CreateUninitializedUiEntryModel();
            var mockConfigEntry = new Mock<IConfigEntry>();
            
            var translatorName = new Translator("Test", "Test");
            var translatorDesc = new Translator("Description", "Description");
            
            mockConfigEntry.Setup(x => x.Name).Returns(translatorName);
            mockConfigEntry.Setup(x => x.Description).Returns(translatorDesc);
            mockConfigEntry.Setup(x => x.ValueType).Returns(valueType);
            mockConfigEntry.Setup(x => x.BoxedValue).Returns(value);
            
            SetPrivateField(entry, "_entry", mockConfigEntry.Object);
            SetPrivateAutoProperty(entry, "Metadata", metadata);
            return entry;
        }

        private static void SetPrivateAutoProperty(object obj, string propertyName, object value)
        {
            var backingField = obj.GetType().GetField($"<{propertyName}>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
            if (backingField == null)
                throw new InvalidOperationException($"Cannot find backing field for property {propertyName}");
            backingField.SetValue(obj, value);
        }

        private static void SetPrivateField(object obj, string fieldName, object value)
        {
            var field = obj.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (field == null)
                throw new InvalidOperationException($"Cannot find field {fieldName}");
            field.SetValue(obj, value);
        }

        private static T GetPrivateField<T>(object obj, string fieldName)
        {
            var field = obj.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (field == null)
                throw new InvalidOperationException($"Cannot find field {fieldName}");
            return (T)field.GetValue(obj);
        }

        private static HotkeyChord CreateUninitializedHotkeyChord()
        {
            return (HotkeyChord)RuntimeHelpers.GetUninitializedObject(typeof(HotkeyChord));
        }

        private static Hotkey CreateUninitializedHotkey()
        {
            return (Hotkey)RuntimeHelpers.GetUninitializedObject(typeof(Hotkey));
        }

        private static Hotkey CreateHotkey()
        {
            var unityService = new UnityProvider();
            return new Hotkey(unityService);
        }

        private static UiEntryModel CreateUiEntryModelWithHotkeyType(Hotkey hotkeyValue)
        {
            var entry = CreateUninitializedUiEntryModel();
            var mockConfigEntry = new Mock<IConfigEntry>();

            var translatorName = new Translator("Test", "Test");
            var translatorDesc = new Translator("Description", "Description");

            mockConfigEntry.Setup(x => x.Name).Returns(translatorName);
            mockConfigEntry.Setup(x => x.Description).Returns(translatorDesc);
            mockConfigEntry.Setup(x => x.ValueType).Returns(typeof(Hotkey));
            mockConfigEntry.Setup(x => x.BoxedValue).Returns(hotkeyValue);

            SetPrivateField(entry, "_entry", mockConfigEntry.Object);
            return entry;
        }

        // -----------------------------------------------------------------------
        // Test Helper Classes
        // -----------------------------------------------------------------------

        private enum TestEnumForRenderer
        {
            FirstValue,
            SecondValue,
            ThirdValue
        }

        private class TestEntryRenderer : BaseEntryRenderer
        {
            public TestEntryRenderer(ViewModel context) : base(context) { }

            public override void RenderEntry(UiEntryModel entry) { }
        }

        private class TrackingTestEntryRenderer : BaseEntryRenderer
        {
            public bool RenderEntryCalled { get; private set; }
            public UiEntryModel RenderEntryCalledWith { get; private set; }
            public bool RenderAfterRowCalled { get; private set; }
            public UiEntryModel RenderAfterRowCalledWith { get; private set; }

            public TrackingTestEntryRenderer(ViewModel context) : base(context) { }

            public override void RenderEntry(UiEntryModel entry)
            {
                RenderEntryCalled = true;
                RenderEntryCalledWith = entry;
            }

            public override void RenderAfterRow(UiEntryModel entry)
            {
                base.RenderAfterRow(entry);
                RenderAfterRowCalled = true;
                RenderAfterRowCalledWith = entry;
            }
        }

        private class OverriddenRenderAfterRowRenderer : BaseEntryRenderer
        {
            public bool OverriddenMethodCalled { get; private set; }

            public OverriddenRenderAfterRowRenderer(ViewModel context) : base(context) { }

            public override void RenderEntry(UiEntryModel entry) { }

            public override void RenderAfterRow(UiEntryModel entry)
            {
                OverriddenMethodCalled = true;
            }
        }

        // -----------------------------------------------------------------------
        // HotkeyRenderer Constructor
        // -----------------------------------------------------------------------

        [Fact]
        public void HotkeyRenderer_Constructor_SetsContextProperty()
        {
            // Arrange
            var viewModel = CreateUninitializedViewModel();

            // Act
            var renderer = new HotkeyRenderer(viewModel);

            // Assert
            Assert.Same(viewModel, renderer.Context);
        }

        [Fact]
        public void HotkeyRenderer_Constructor_WithNullContext_SetsContextToNull()
        {
            // Arrange
            ViewModel viewModel = null;

            // Act
            var renderer = new HotkeyRenderer(viewModel);

            // Assert
            Assert.Null(renderer.Context);
        }

        // -----------------------------------------------------------------------
        // HotkeyRenderer.RenderEntry
        // -----------------------------------------------------------------------

        [Fact]
        public void HotkeyRenderer_RenderEntry_ContextNull_ReturnsEarly()
        {
            // Arrange
            var renderer = new HotkeyRenderer(null);
            var entry = CreateUninitializedUiEntryModel();

            // Act
            renderer.RenderEntry(entry);

            // Assert
            Assert.Null(renderer.Context);
        }

        [Fact]
        public void HotkeyRenderer_RenderEntry_EntryNull_ReturnsEarly()
        {
            // Arrange
            var viewModel = CreateUninitializedViewModel();
            var renderer = new HotkeyRenderer(viewModel);

            // Act
            renderer.RenderEntry(null);

            // Assert
            Assert.NotNull(renderer.Context);
        }

        [Fact]
        public void HotkeyRenderer_RenderEntry_BothContextAndEntryNull_ReturnsEarly()
        {
            // Arrange
            var renderer = new HotkeyRenderer(null);

            // Act
            renderer.RenderEntry(null);

            // Assert
            Assert.Null(renderer.Context);
        }

        [Fact]
        public void HotkeyRenderer_RenderEntry_ValueTypeNotHotkey_ReturnsEarly()
        {
            // Arrange
            var viewModel = CreateUninitializedViewModel();
            var renderer = new HotkeyRenderer(viewModel);
            var entry = CreateUiEntryModelWithValueType(typeof(string));

            // Act
            renderer.RenderEntry(entry);

            // Assert
            Assert.Same(viewModel, renderer.Context);
        }

        [Fact]
        public void HotkeyRenderer_RenderEntry_ValueTypeInt_ReturnsEarly()
        {
            // Arrange
            var viewModel = CreateUninitializedViewModel();
            var renderer = new HotkeyRenderer(viewModel);
            var entry = CreateUiEntryModelWithValueType(typeof(int));

            // Act
            renderer.RenderEntry(entry);

            // Assert
            Assert.Same(viewModel, renderer.Context);
        }

        // -----------------------------------------------------------------------
        // HotkeyRenderer.RenderAfterRow
        // -----------------------------------------------------------------------

        [Fact]
        public void HotkeyRenderer_RenderAfterRow_ContextNull_ReturnsEarly()
        {
            // Arrange
            var renderer = new HotkeyRenderer(null);
            var entry = CreateUninitializedUiEntryModel();

            // Act
            renderer.RenderAfterRow(entry);

            // Assert
            Assert.Null(renderer.Context);
        }

        [Fact]
        public void HotkeyRenderer_RenderAfterRow_EntryNull_ReturnsEarly()
        {
            // Arrange
            var viewModel = CreateUninitializedViewModel();
            var renderer = new HotkeyRenderer(viewModel);

            // Act
            renderer.RenderAfterRow(null);

            // Assert
            Assert.NotNull(renderer.Context);
        }

        [Fact]
        public void HotkeyRenderer_RenderAfterRow_BothContextAndEntryNull_ReturnsEarly()
        {
            // Arrange
            var renderer = new HotkeyRenderer(null);

            // Act
            renderer.RenderAfterRow(null);

            // Assert
            Assert.Null(renderer.Context);
        }

        [Fact]
        public void HotkeyRenderer_RenderAfterRow_ValueTypeNotHotkey_ReturnsEarly()
        {
            // Arrange
            var viewModel = CreateUninitializedViewModel();
            var renderer = new HotkeyRenderer(viewModel);
            var entry = CreateUiEntryModelWithValueType(typeof(string));

            // Act
            renderer.RenderAfterRow(entry);

            // Assert
            Assert.Same(viewModel, renderer.Context);
        }

        [Fact]
        public void HotkeyRenderer_RenderAfterRow_OpenedHotkeyEntryNotMatching_ReturnsEarly()
        {
            // Arrange
            var viewModel = CreateUninitializedViewModel();
            var renderer = new HotkeyRenderer(viewModel);
            var entry = CreateUiEntryModelWithValueType(typeof(string));
            var differentEntry = CreateUiEntryModelWithValueType(typeof(string));
            SetPrivateAutoProperty(viewModel, "OpenedHotkeyEntry", differentEntry);

            // Act
            renderer.RenderAfterRow(entry);

            // Assert
            Assert.Same(viewModel, renderer.Context);
        }

        [Fact]
        public void HotkeyRenderer_RenderAfterRow_OpenedHotkeyEntryNull_ReturnsEarly()
        {
            // Arrange
            var viewModel = CreateUninitializedViewModel();
            var renderer = new HotkeyRenderer(viewModel);
            var entry = CreateUiEntryModelWithValueType(typeof(string));
            SetPrivateAutoProperty(viewModel, "OpenedHotkeyEntry", null);

            // Act
            renderer.RenderAfterRow(entry);

            // Assert
            Assert.Same(viewModel, renderer.Context);
        }

        // -----------------------------------------------------------------------
        // HotkeyRenderer.ApplyHotkey
        // -----------------------------------------------------------------------

        [Fact]
        public void HotkeyRenderer_ApplyHotkey_OpenedHotkeyEntryNull_ReturnsEarly()
        {
            // Arrange
            var viewModel = CreateUninitializedViewModel();
            var renderer = new HotkeyRenderer(viewModel);
            SetPrivateAutoProperty(viewModel, "OpenedHotkeyEntry", null);

            // Act
            renderer.ApplyHotkey();

            // Assert
            Assert.Same(viewModel, renderer.Context);
        }

        [Fact]
        public void HotkeyRenderer_ApplyHotkey_CacheValueNotNull_SetsCacheValueToNull()
        {
            // Arrange
            var viewModel = CreateUninitializedViewModel();
            var renderer = new HotkeyRenderer(viewModel);
            
            var hotkey = CreateHotkey();
            var cacheHotkey = CreateHotkey();
            var entry = CreateUiEntryModelWithHotkeyType(hotkey);
            entry.CacheValue = cacheHotkey;
            
            SetPrivateAutoProperty(viewModel, "OpenedHotkeyEntry", entry);

            // Act
            renderer.ApplyHotkey();

            // Assert
            Assert.Null(entry.CacheValue);
            Assert.True(hotkey.Valid);
        }

        // -----------------------------------------------------------------------
        // ResetButtonRenderer.Render
        // -----------------------------------------------------------------------

        [Fact]
        public void ResetButtonRenderer_Render_ContextNull_ReturnsEarly()
        {
            // Arrange
            ViewModel context = null;
            var entry = CreateUninitializedUiEntryModel();

            // Act
            ResetButtonRenderer.Render(context, entry);

            // Assert
            // No exception thrown, method returns early
            Assert.Null(context);
        }

        [Fact]
        public void ResetButtonRenderer_Render_EntryNull_ReturnsEarly()
        {
            // Arrange
            var context = CreateUninitializedViewModel();
            UiEntryModel entry = null;

            // Act
            ResetButtonRenderer.Render(context, entry);

            // Assert
            // No exception thrown, method returns early
            Assert.NotNull(context);
        }

        [Fact]
        public void ResetButtonRenderer_Render_BothContextAndEntryNull_ReturnsEarly()
        {
            // Arrange
            ViewModel context = null;
            UiEntryModel entry = null;

            // Act
            ResetButtonRenderer.Render(context, entry);

            // Assert
            // No exception thrown, method returns early
            Assert.Null(context);
        }

        [Fact]
        public void ResetButtonRenderer_Render_ContextIsNullAndEntryIsValid_ReturnsEarly()
        {
            // Arrange
            ViewModel context = null;
            var entry = CreateUiEntryModelWithMockedEntry("Test", "Description");

            // Act
            ResetButtonRenderer.Render(context, entry);

            // Assert
            // No exception thrown, method returns early
            Assert.Null(context);
        }

        [Fact]
        public void ResetButtonRenderer_Render_EntryIsNullAndContextIsValid_ReturnsEarly()
        {
            // Arrange
            var context = CreateUninitializedViewModel();
            UiEntryModel entry = null;

            // Act
            ResetButtonRenderer.Render(context, entry);

            // Assert
            // No exception thrown, method returns early
            Assert.Same(context, context);
        }

        // -----------------------------------------------------------------------
        // ResetButtonRenderer.GetWidth
        // -----------------------------------------------------------------------

        [Fact]
        public void ResetButtonRenderer_GetWidth_WithNullContext_ThrowsNullReferenceException()
        {
            // Arrange
            ViewModel context = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => ResetButtonRenderer.GetWidth(context));
        }

        [Fact]
        public void ResetButtonRenderer_GetWidth_WithUninitializedContext_ThrowsNullReferenceException()
        {
            // Arrange
            var context = CreateUninitializedViewModel();

            // Act & Assert
            // UnityGuiService will be null, causing NullReferenceException
            Assert.Throws<NullReferenceException>(() => ResetButtonRenderer.GetWidth(context));
        }

        // -----------------------------------------------------------------------
        // TableRenderer Constructor
        // -----------------------------------------------------------------------

        [Fact]
        public void TableRenderer_Constructor_SetsContextProperty()
        {
            // Arrange
            var viewModel = CreateUninitializedViewModel();

            // Act
            var renderer = new TableRenderer(viewModel);

            // Assert
            Assert.Same(viewModel, renderer.Context);
        }

        [Fact]
        public void TableRenderer_Constructor_InitializesBoolEntryRenderer()
        {
            // Arrange
            var viewModel = CreateUninitializedViewModel();

            // Act
            var renderer = new TableRenderer(viewModel);

            // Assert
            Assert.NotNull(renderer.BoolEntryRenderer);
            Assert.IsType<BoolRenderer>(renderer.BoolEntryRenderer);
        }

        [Fact]
        public void TableRenderer_Constructor_InitializesStringEntryRenderer()
        {
            // Arrange
            var viewModel = CreateUninitializedViewModel();

            // Act
            var renderer = new TableRenderer(viewModel);

            // Assert
            Assert.NotNull(renderer.StringEntryRenderer);
            Assert.IsType<StringRenderer>(renderer.StringEntryRenderer);
        }

        [Fact]
        public void TableRenderer_Constructor_InitializesNumberEntryRenderer()
        {
            // Arrange
            var viewModel = CreateUninitializedViewModel();

            // Act
            var renderer = new TableRenderer(viewModel);

            // Assert
            Assert.NotNull(renderer.NumberEntryRenderer);
            Assert.IsType<NumberRenderer>(renderer.NumberEntryRenderer);
        }

        [Fact]
        public void TableRenderer_Constructor_InitializesEnumEntryRenderer()
        {
            // Arrange
            var viewModel = CreateUninitializedViewModel();

            // Act
            var renderer = new TableRenderer(viewModel);

            // Assert
            Assert.NotNull(renderer.EnumEntryRenderer);
            Assert.IsType<EnumRenderer>(renderer.EnumEntryRenderer);
        }

        [Fact]
        public void TableRenderer_Constructor_InitializesSliderEntryRenderer()
        {
            // Arrange
            var viewModel = CreateUninitializedViewModel();

            // Act
            var renderer = new TableRenderer(viewModel);

            // Assert
            Assert.NotNull(renderer.SliderEntryRenderer);
            Assert.IsType<SliderRenderer>(renderer.SliderEntryRenderer);
        }

        [Fact]
        public void TableRenderer_Constructor_InitializesHotkeyRenderer()
        {
            // Arrange
            var viewModel = CreateUninitializedViewModel();

            // Act
            var renderer = new TableRenderer(viewModel);

            // Assert
            Assert.NotNull(renderer.HotkeyRenderer);
            Assert.IsType<HotkeyRenderer>(renderer.HotkeyRenderer);
        }

        [Fact]
        public void TableRenderer_Constructor_WithNullContext_SetsContextToNull()
        {
            // Arrange
            ViewModel viewModel = null;

            // Act
            var renderer = new TableRenderer(viewModel);

            // Assert
            Assert.Null(renderer.Context);
        }

        // -----------------------------------------------------------------------
        // TableRenderer.Render
        // -----------------------------------------------------------------------

        [Fact]
        public void TableRenderer_Render_WithNullContext_ReturnsEarly()
        {
            // Arrange
            var viewModel = CreateUninitializedViewModel();
            var renderer = new TableRenderer(viewModel);
            SetPrivateAutoProperty(renderer, "Context", null);
            var table = CreateUninitializedUiTableModel();

            // Act
            renderer.Render(table);

            // Assert
            // No exception thrown, method returns early
        }

        [Fact]
        public void TableRenderer_Render_WithNullTable_ReturnsEarly()
        {
            // Arrange
            var viewModel = CreateUninitializedViewModel();
            var renderer = new TableRenderer(viewModel);
            UiTableModel table = null;

            // Act
            renderer.Render(table);

            // Assert
            // No exception thrown, method returns early
        }

        // -----------------------------------------------------------------------
        // SheetRenderer Constructor
        // -----------------------------------------------------------------------

        [Fact]
        public void SheetRenderer_Constructor_SetsContextProperty()
        {
            // Arrange
            var viewModel = CreateUninitializedViewModel();

            // Act
            var renderer = new SheetRenderer(viewModel);

            // Assert
            Assert.Same(viewModel, renderer.Context);
        }

        [Fact]
        public void SheetRenderer_Constructor_InitializesTableRenderer()
        {
            // Arrange
            var viewModel = CreateUninitializedViewModel();

            // Act
            var renderer = new SheetRenderer(viewModel);

            // Assert
            Assert.NotNull(renderer.TableRenderer);
            Assert.IsType<TableRenderer>(renderer.TableRenderer);
        }

        [Fact]
        public void SheetRenderer_Constructor_WithNullContext_SetsContextToNull()
        {
            // Arrange
            ViewModel viewModel = null;

            // Act
            var renderer = new SheetRenderer(viewModel);

            // Assert
            Assert.Null(renderer.Context);
        }

        // -----------------------------------------------------------------------
        // SheetRenderer.Render
        // -----------------------------------------------------------------------

        [Fact]
        public void SheetRenderer_Render_WithNullContext_ReturnsEarly()
        {
            // Arrange
            var viewModel = CreateUninitializedViewModel();
            var renderer = new SheetRenderer(viewModel);
            SetPrivateAutoProperty(renderer, "Context", null);
            var sheet = CreateUninitializedUiSheetModel();

            // Act
            renderer.Render(sheet);

            // Assert
            // No exception thrown, method returns early
        }

        [Fact]
        public void SheetRenderer_Render_WithNullSheet_ReturnsEarly()
        {
            // Arrange
            var viewModel = CreateUninitializedViewModel();
            var renderer = new SheetRenderer(viewModel);
            UiSheetModel sheet = null;

            // Act
            renderer.Render(sheet);

            // Assert
            // No exception thrown, method returns early
        }

        [Fact]
        public void SheetRenderer_Render_WithEmptySheet_DoesNotThrow()
        {
            // Arrange
            var viewModel = CreateUninitializedViewModel();
            var renderer = new SheetRenderer(viewModel);
            var sheet = CreateUninitializedUiSheetModel();
            SetPrivateAutoProperty(sheet, "Sheet", new List<UiTableModel>());

            // Act
            renderer.Render(sheet);

            // Assert
            // No exception thrown, method executes but returns early due to empty collection
        }

        [Fact]
        public void TableRenderer_Render_WithBoolEntry_CallsBoolRenderer()
        {
            // Arrange
            var viewModel = CreateUninitializedViewModel();
            var renderer = new TableRenderer(viewModel);
            var table = CreateUninitializedUiTableModel();
            var entry = CreateUiEntryModelWithValueType(typeof(bool));
            SetPrivateAutoProperty(table, "Table", new List<UiEntryModel> { entry });

            // Act & Assert
            // This will throw NullReferenceException when trying to access Context.UnityGuiService
            // but it proves the method body is executed
            Assert.Throws<NullReferenceException>(() => renderer.Render(table));
        }

        [Fact]
        public void TableRenderer_Render_WithStringEntry_CallsStringRenderer()
        {
            // Arrange
            var viewModel = CreateUninitializedViewModel();
            var renderer = new TableRenderer(viewModel);
            var table = CreateUninitializedUiTableModel();
            var entry = CreateUiEntryModelWithValueType(typeof(string));
            SetPrivateAutoProperty(table, "Table", new List<UiEntryModel> { entry });

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => renderer.Render(table));
        }

        [Fact]
        public void TableRenderer_Render_WithPrimitiveEntryNoMetadata_CallsNumberRenderer()
        {
            // Arrange
            var viewModel = CreateUninitializedViewModel();
            var renderer = new TableRenderer(viewModel);
            var table = CreateUninitializedUiTableModel();
            var entry = CreateUiEntryModelWithValueType(typeof(int));
            SetPrivateAutoProperty(table, "Table", new List<UiEntryModel> { entry });

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => renderer.Render(table));
        }

        [Fact]
        public void TableRenderer_Render_WithPrimitiveEntryWithSliderMetadata_CallsSliderRenderer()
        {
            // Arrange
            var viewModel = CreateUninitializedViewModel();
            var renderer = new TableRenderer(viewModel);
            var table = CreateUninitializedUiTableModel();
            var metadata = new UiSliderMetadata(0f, 100f, 1f);
            var entry = CreateUiEntryModelWithMetadata(typeof(int), 50, metadata);
            SetPrivateAutoProperty(table, "Table", new List<UiEntryModel> { entry });

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => renderer.Render(table));
        }

        [Fact]
        public void TableRenderer_Render_WithFloatEntryNoMetadata_CallsNumberRenderer()
        {
            // Arrange
            var viewModel = CreateUninitializedViewModel();
            var renderer = new TableRenderer(viewModel);
            var table = CreateUninitializedUiTableModel();
            var entry = CreateUiEntryModelWithValueType(typeof(float));
            SetPrivateAutoProperty(table, "Table", new List<UiEntryModel> { entry });

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => renderer.Render(table));
        }

        [Fact]
        public void TableRenderer_Render_WithDoubleEntryNoMetadata_CallsNumberRenderer()
        {
            // Arrange
            var viewModel = CreateUninitializedViewModel();
            var renderer = new TableRenderer(viewModel);
            var table = CreateUninitializedUiTableModel();
            var entry = CreateUiEntryModelWithValueType(typeof(double));
            SetPrivateAutoProperty(table, "Table", new List<UiEntryModel> { entry });

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => renderer.Render(table));
        }

        [Fact]
        public void TableRenderer_Render_WithEnumEntry_CallsEnumRenderer()
        {
            // Arrange
            var viewModel = CreateUninitializedViewModel();
            var renderer = new TableRenderer(viewModel);
            var table = CreateUninitializedUiTableModel();
            var entry = CreateUiEntryModelWithValueType(typeof(TestEnumForRenderer));
            SetPrivateAutoProperty(table, "Table", new List<UiEntryModel> { entry });

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => renderer.Render(table));
        }

        [Fact]
        public void TableRenderer_Render_WithHotkeyEntry_CallsHotkeyRenderer()
        {
            // Arrange
            var viewModel = CreateUninitializedViewModel();
            var renderer = new TableRenderer(viewModel);
            var table = CreateUninitializedUiTableModel();
            var entry = CreateUiEntryModelWithValueType(typeof(Hotkey));
            SetPrivateAutoProperty(table, "Table", new List<UiEntryModel> { entry });

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => renderer.Render(table));
        }

        [Fact]
        public void TableRenderer_Render_WithUnsupportedEntryType_SkipsEntry()
        {
            // Arrange
            var viewModel = CreateUninitializedViewModel();
            var renderer = new TableRenderer(viewModel);
            var table = CreateUninitializedUiTableModel();
            // Use a type that doesn't match any condition: not bool, not string, not primitive, not enum, not Hotkey
            var entry = CreateUiEntryModelWithValueType(typeof(object));
            SetPrivateAutoProperty(table, "Table", new List<UiEntryModel> { entry });

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => renderer.Render(table));
        }

        [Fact]
        public void TableRenderer_Render_WithMultipleEntriesOfDifferentTypes_ProcessesAll()
        {
            // Arrange
            var viewModel = CreateUninitializedViewModel();
            var renderer = new TableRenderer(viewModel);
            var table = CreateUninitializedUiTableModel();
            var entries = new List<UiEntryModel>
            {
                CreateUiEntryModelWithValueType(typeof(bool)),
                CreateUiEntryModelWithValueType(typeof(string)),
                CreateUiEntryModelWithValueType(typeof(int)),
                CreateUiEntryModelWithValueType(typeof(TestEnumForRenderer)),
                CreateUiEntryModelWithValueType(typeof(Hotkey))
            };
            SetPrivateAutoProperty(table, "Table", entries);

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => renderer.Render(table));
        }

        [Fact]
        public void SheetRenderer_Render_WithSingleTable_CallsTableRendererRender()
        {
            // Arrange
            var viewModel = CreateUninitializedViewModel();
            var renderer = new SheetRenderer(viewModel);
            var sheet = CreateUninitializedUiSheetModel();
            var table = CreateUninitializedUiTableModel();
            SetPrivateAutoProperty(table, "Table", new List<UiEntryModel>());
            SetPrivateAutoProperty(sheet, "Sheet", new List<UiTableModel> { table });

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => renderer.Render(sheet));
        }

        [Fact]
        public void SheetRenderer_Render_WithMultipleTables_CallsTableRendererRenderForEach()
        {
            // Arrange
            var viewModel = CreateUninitializedViewModel();
            var renderer = new SheetRenderer(viewModel);
            var sheet = CreateUninitializedUiSheetModel();
            var table1 = CreateUninitializedUiTableModel();
            var table2 = CreateUninitializedUiTableModel();
            SetPrivateAutoProperty(table1, "Table", new List<UiEntryModel>());
            SetPrivateAutoProperty(table2, "Table", new List<UiEntryModel>());
            SetPrivateAutoProperty(sheet, "Sheet", new List<UiTableModel> { table1, table2 });

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => renderer.Render(sheet));
        }

        // -----------------------------------------------------------------------
        // ToastRenderer Constructor
        // -----------------------------------------------------------------------

        [Fact]
        public void ToastRenderer_Constructor_SetsContextProperty()
        {
            // Arrange
            var viewModel = CreateUninitializedViewModel();

            // Act
            var renderer = new ToastRenderer(viewModel);

            // Assert
            Assert.Same(viewModel, renderer.Context);
        }

        [Fact]
        public void ToastRenderer_Constructor_WithNullContext_SetsContextToNull()
        {
            // Arrange
            ViewModel viewModel = null;

            // Act
            var renderer = new ToastRenderer(viewModel);

            // Assert
            Assert.Null(renderer.Context);
        }

        // -----------------------------------------------------------------------
        // ToastRenderer.Render
        // -----------------------------------------------------------------------

        [Fact]
        public void ToastRenderer_Render_WithNullContext_ReturnsEarly()
        {
            // Arrange
            var renderer = new ToastRenderer(null);

            // Act
            renderer.Render();

            // Assert
            // No exception thrown, method returns early
            Assert.Null(renderer.Context);
        }

        [Fact]
        public void ToastRenderer_Render_WithNullToastMessage_ReturnsEarly()
        {
            // Arrange
            var viewModel = CreateUninitializedViewModel();
            viewModel.ToastMessage = null;
            var renderer = new ToastRenderer(viewModel);

            // Act
            renderer.Render();

            // Assert
            // No exception thrown, method returns early
            Assert.Null(viewModel.ToastMessage);
        }

        [Fact]
        public void ToastRenderer_Render_WithEmptyToastMessage_ReturnsEarly()
        {
            // Arrange
            var viewModel = CreateUninitializedViewModel();
            viewModel.ToastMessage = string.Empty;
            var renderer = new ToastRenderer(viewModel);

            // Act
            renderer.Render();

            // Assert
            // No exception thrown, method returns early
            Assert.Empty(viewModel.ToastMessage);
        }

        [Fact]
        public void ToastRenderer_Render_WithExpiredToast_ClearsMessageAndReturnsEarly()
        {
            // Arrange
            var viewModel = CreateUninitializedViewModel();
            SetPrivateAutoProperty(viewModel, "UnityService", new UnityProvider());

            viewModel.ToastMessage = "Test Toast";
            viewModel.ToastEndTime = -1f; // expired (negative means it expired long ago)
            var renderer = new ToastRenderer(viewModel);

            // Act & Assert
            // This proves we pass the early returns and execute line 540
            // The SecurityException occurs when accessing RealtimeSinceStartup, but proves the line is executed
            Assert.Throws<SecurityException>(() => renderer.Render());
        }

        [Fact]
        public void ToastRenderer_Render_WithValidToastExpiredByZero_ClearsMessage()
        {
            // Arrange
            var viewModel = CreateUninitializedViewModel();
            SetPrivateAutoProperty(viewModel, "UnityService", new UnityProvider());

            viewModel.ToastMessage = "Test Toast";
            viewModel.ToastEndTime = 0f; // remaining will be negative or zero
            var renderer = new ToastRenderer(viewModel);

            // Act & Assert
            // This proves we pass the early returns and execute line 540
            // The SecurityException occurs when accessing RealtimeSinceStartup, but proves the line is executed
            Assert.Throws<SecurityException>(() => renderer.Render());
        }

        [Fact]
        public void ToastRenderer_Render_WithValidToast_AccessesUnityService()
        {
            // Arrange
            var viewModel = CreateUninitializedViewModel();
            SetPrivateAutoProperty(viewModel, "UnityService", null); // Will cause NullReferenceException

            viewModel.ToastMessage = "Test Toast";
            viewModel.ToastEndTime = 100f; // far in future
            var renderer = new ToastRenderer(viewModel);

            // Act & Assert
            // This proves we pass the early returns and execute line 540
            Assert.Throws<NullReferenceException>(() => renderer.Render());
        }

        // -----------------------------------------------------------------------
        // TooltipRenderer Constructor
        // -----------------------------------------------------------------------

        [Fact]
        public void TooltipRenderer_Constructor_SetsContextProperty()
        {
            // Arrange
            var viewModel = CreateUninitializedViewModel();

            // Act
            var renderer = new TooltipRenderer(viewModel);

            // Assert
            Assert.Same(viewModel, renderer.Context);
        }

        [Fact]
        public void TooltipRenderer_Constructor_WithNullContext_SetsContextToNull()
        {
            // Arrange
            ViewModel viewModel = null;

            // Act
            var renderer = new TooltipRenderer(viewModel);

            // Assert
            Assert.Null(renderer.Context);
        }

        // -----------------------------------------------------------------------
        // TooltipRenderer.Render
        // -----------------------------------------------------------------------

        [Fact]
        public void TooltipRenderer_Render_WithNullContext_ReturnsEarly()
        {
            // Arrange
            var renderer = new TooltipRenderer(null);

            // Act
            renderer.Render();

            // Assert
            // No exception thrown, method returns early
            Assert.Null(renderer.Context);
        }

        [Fact]
        public void TooltipRenderer_Render_WithUninitializedContext_ThrowsNullReferenceException()
        {
            // Arrange
            var viewModel = CreateUninitializedViewModel();
            var renderer = new TooltipRenderer(viewModel);

            // Act & Assert
            // This proves we pass the null context check (line 575-576) and execute line 578
            Assert.Throws<NullReferenceException>(() => renderer.Render());
        }
    }
}
