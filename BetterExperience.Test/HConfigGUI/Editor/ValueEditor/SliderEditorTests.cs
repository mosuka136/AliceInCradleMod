using BetterExperience.HConfigGUI;
using BetterExperience.HConfigGUI.Bindings;
using BetterExperience.HConfigGUI.Editor.ValueEditor;
using BetterExperience.HConfigGUI.Resource;
using BetterExperience.HProvider;
using Moq;
using System;
using UnityEngine;

namespace BetterExperience.Test.HConfigGUI.Editor.ValueEditor
{
    public class SliderEditorTests
    {
        [Fact]
        public void SliderEditor_WhenConstructed_StoresDependencies()
        {
            // Arrange
            var unityGuiMock = new Mock<IUnityGuiProvider>(MockBehavior.Strict);
            var unityProviderMock = new Mock<IUnityProvider>(MockBehavior.Strict);
            var styleResource = new StyleResource(null);

            // Act
            var editor = new SliderEditor(unityGuiMock.Object, unityProviderMock.Object, styleResource);

            // Assert
            Assert.Same(unityGuiMock.Object, editor.UnityGui);
            Assert.Same(unityProviderMock.Object, editor.UnityService);
            Assert.Same(styleResource, editor.StyleProvider);
            Assert.Equal(0.5f, editor.DelayApplyDuration);
        }

        [Fact]
        public void CanEdit_WhenEntryIsPrimitiveAndHasSliderMetadata_ReturnsTrue()
        {
            // Arrange
            var editor = CreateEditor();
            var entryMock = CreateEntry(typeof(int), 5, new UiSliderMetadata(0f, 10f, 1f));

            // Act
            var result = editor.CanEdit(entryMock.Object);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void CanEdit_WhenEntryIsPrimitiveAndMetadataIsNotSlider_ReturnsFalse()
        {
            // Arrange
            var editor = CreateEditor();
            var metadataMock = new Mock<IUiMetadata>(MockBehavior.Strict);
            var entryMock = CreateEntry(typeof(int), 5, metadataMock.Object);

            // Act
            var result = editor.CanEdit(entryMock.Object);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void CanEdit_WhenEntryTypeIsNotEditable_ReturnsFalse()
        {
            // Arrange
            var editor = CreateEditor();
            var entryMock = CreateEntry(typeof(bool), true, new UiSliderMetadata(0f, 1f, 1f));

            // Act
            var result = editor.CanEdit(entryMock.Object);

            // Assert
            Assert.False(result);
        }

        [Theory]
        [InlineData(typeof(string), "value")]
        [InlineData(typeof(bool), true)]
        [InlineData(typeof(char), 'A')]
        public void DrawValue_WhenEntryTypeIsNotSupported_ReturnsWithoutDrawing(Type valueType, object value)
        {
            // Arrange
            var unityGuiMock = new Mock<IUnityGuiProvider>(MockBehavior.Strict);
            var editor = new SliderEditor(unityGuiMock.Object, new Mock<IUnityProvider>(MockBehavior.Strict).Object, new StyleResource(null));
            var entryMock = CreateEntry(valueType, value, new UiSliderMetadata(0f, 10f, 1f));
            var state = new GuiStateStore();
            var changeSink = new EntryChangeSink();

            // Act
            editor.DrawValue(entryMock.Object, state, changeSink);
            changeSink.FlushValue(1.0f);

            // Assert
            Assert.Equal(value, entryMock.Object.Value);
            unityGuiMock.VerifyNoOtherCalls();
        }

        [Fact]
        public void DrawValue_WhenMetadataIsMissing_DrawsInvalidMetadataLabelAndReturns()
        {
            // Arrange
            var invalidMetadataText = TranslatorResource.InvalidSliderMetadata.ToString();
            var content = new GUIContent(invalidMetadataText);
            var unityGuiMock = new Mock<IUnityGuiProvider>(MockBehavior.Strict);
            unityGuiMock.Setup(x => x.GetContent(invalidMetadataText)).Returns(content);
            unityGuiMock.Setup(x => x.ExpandWidth(true)).Returns((GUILayoutOption)null);
            unityGuiMock
                .Setup(x => x.Label(content, It.Is<GUILayoutOption[]>(options => options.Length == 1 && options[0] == null)));
            var editor = new SliderEditor(unityGuiMock.Object, new Mock<IUnityProvider>(MockBehavior.Strict).Object, new StyleResource(null));
            var entryMock = CreateEntry(typeof(int), 5, null);
            var state = new GuiStateStore();

            // Act
            editor.DrawValue(entryMock.Object, state, new EntryChangeSink());

            // Assert
            unityGuiMock.Verify(x => x.GetContent(invalidMetadataText), Times.Once);
            unityGuiMock.Verify(x => x.ExpandWidth(true), Times.Once);
            unityGuiMock.Verify(x => x.Label(content, It.Is<GUILayoutOption[]>(options => options.Length == 1 && options[0] == null)), Times.Once);
        }


        private static SliderEditor CreateEditor()
        {
            return new SliderEditor(
                new Mock<IUnityGuiProvider>(MockBehavior.Strict).Object,
                new Mock<IUnityProvider>(MockBehavior.Strict).Object,
                new StyleResource(null));
        }

        private static Mock<IEntryBinding> CreateEntry(Type valueType, object value, IUiMetadata metadata, string key = "SliderEntry")
        {
            var entryMock = new Mock<IEntryBinding>(MockBehavior.Strict);
            entryMock.SetupGet(x => x.Key).Returns(key);
            entryMock.SetupGet(x => x.ValueType).Returns(valueType);
            entryMock.SetupProperty(x => x.Value, value);
            entryMock.SetupGet(x => x.Metadata).Returns(metadata);
            return entryMock;
        }
    }
}
