using BetterExperience.HConfigGUI;
using BetterExperience.HConfigGUI.Bindings;
using BetterExperience.HConfigGUI.Editor;
using BetterExperience.HConfigGUI.Editor.ValueEditor;
using BetterExperience.HConfigGUI.Resource;
using BetterExperience.HotkeyManager;
using BetterExperience.HProvider;
using Moq;
using Xunit;

namespace BetterExperience.Test
{
    public class TableEditorTests
    {
        [Fact]
        public void Constructor_ValidDependencies_InitializesPropertiesAndRegistersEditors()
        {
            // Arrange
            var unityProvider = new Mock<IUnityProvider>(MockBehavior.Strict);
            var unityGui = new Mock<IUnityGuiProvider>(MockBehavior.Strict);
            var guiStateStore = new GuiStateStore();
            var styleResource = new StyleResource(null);

            // Act
            var editor = new TableEditor(unityProvider.Object, unityGui.Object, guiStateStore, styleResource);

            // Assert
            Assert.Same(unityProvider.Object, editor.UnityService);
            Assert.Same(unityGui.Object, editor.UnityGui);
            Assert.Same(guiStateStore, editor.GuiStateStore);
            Assert.Same(styleResource, editor.StyleProvider);
            Assert.NotNull(editor.EditorRegistry);
            Assert.NotNull(editor.ChangeSink);
            Assert.NotNull(editor.EntryRenderer);
            Assert.Same(editor.EditorRegistry, editor.EntryRenderer.Registry);
            Assert.Same(guiStateStore, editor.EntryRenderer.State);
            Assert.Same(editor.ChangeSink, editor.EntryRenderer.ChangeSink);
            Assert.Same(unityGui.Object, editor.EntryRenderer.UnityGui);
            Assert.IsType<BooleanEditor>(editor.EditorRegistry.GetEditor(CreateEntryBindingMock(typeof(bool)).Object));
            Assert.IsType<StringEditor>(editor.EditorRegistry.GetEditor(CreateEntryBindingMock(typeof(string)).Object));
            Assert.IsType<SliderEditor>(editor.EditorRegistry.GetEditor(CreateEntryBindingMock(typeof(int), metadata: new UiSliderMetadata(0f, 10f, 1f)).Object));
            Assert.IsType<NumberEditor>(editor.EditorRegistry.GetEditor(CreateEntryBindingMock(typeof(float)).Object));
            Assert.IsType<EnumEditor>(editor.EditorRegistry.GetEditor(CreateEntryBindingMock(typeof(TestEnum)).Object));
            Assert.IsType<HotkeyEditor>(editor.EditorRegistry.GetEditor(CreateEntryBindingMock(typeof(Hotkey)).Object));
        }

        [Fact]
        public void Update_WhenPendingDelayedValueExists_FlushesValueToEntry()
        {
            // Arrange
            var unityProvider = new Mock<IUnityProvider>(MockBehavior.Strict);
            var unityGui = new Mock<IUnityGuiProvider>(MockBehavior.Strict);
            var guiStateStore = new GuiStateStore();
            var styleResource = new StyleResource(null);
            var entry = CreateEntryBindingMock(typeof(string), value: "old", key: "DelayedEntry");
            entry.SetupGet(x => x.Value).Returns("old");
            entry.SetupSet(x => x.Value = "new");
            var editor = new TableEditor(unityProvider.Object, unityGui.Object, guiStateStore, styleResource);
            editor.ChangeSink.SetValue(entry.Object, "new", 0.5f);

            // Act
            editor.Update(0.5f);

            // Assert
            entry.VerifySet(x => x.Value = "new", Times.Once);
        }

        private static Mock<IEntryBinding> CreateEntryBindingMock(System.Type valueType, object value = null, IUiMetadata metadata = null, string key = "Entry")
        {
            var mock = new Mock<IEntryBinding>(MockBehavior.Strict);
            mock.SetupGet(x => x.Key).Returns(key);
            mock.SetupGet(x => x.ValueType).Returns(valueType);
            mock.SetupGet(x => x.Value).Returns(value);
            mock.SetupGet(x => x.Metadata).Returns(metadata);
            return mock;
        }

        private enum TestEnum
        {
            One,
            Two,
        }
    }
}
