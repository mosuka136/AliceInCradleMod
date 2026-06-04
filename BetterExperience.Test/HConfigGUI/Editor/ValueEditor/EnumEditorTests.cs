using BetterExperience.HConfigGUI;
using BetterExperience.HConfigGUI.Bindings;
using BetterExperience.HConfigGUI.Editor.ValueEditor;
using BetterExperience.HEnumHelper;
using BetterExperience.HProvider;
using Moq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using Xunit;

namespace BetterExperience.Test
{
    public class EnumEditorTests
    {
        [Fact]
        public void EnumEditor_WhenConstructed_StoresUnityGuiProvider()
        {
            // Arrange
            var unityGuiMock = new Mock<IUnityGuiProvider>(MockBehavior.Strict);

            // Act
            var editor = new EnumEditor(unityGuiMock.Object);

            // Assert
            Assert.Same(unityGuiMock.Object, editor.UnityGui);
        }

        [Theory]
        [InlineData(typeof(VisibleEnum), true)]
        [InlineData(typeof(string), false)]
        public void CanEdit_WhenEntryHasDifferentValueTypes_ReturnsExpectedResult(Type valueType, bool expected)
        {
            // Arrange
            var editor = CreateEditor();
            var entryMock = new Mock<IEntryBinding>(MockBehavior.Strict);
            entryMock.SetupGet(x => x.ValueType).Returns(valueType);

            // Act
            var result = editor.CanEdit(entryMock.Object);

            // Assert
            Assert.Equal(expected, result);
            entryMock.VerifyGet(x => x.ValueType, Times.Once);
        }

        [Fact]
        public void DrawValue_WhenEntryTypeIsNotEnum_DoesNothing()
        {
            // Arrange
            var unityGuiMock = new Mock<IUnityGuiProvider>(MockBehavior.Strict);
            var editor = new EnumEditor(unityGuiMock.Object);
            var entryMock = CreateEntry("NotEnumEntry", typeof(string), "text");
            var state = new GuiStateStore();
            var changeSink = new EntryChangeSink();

            // Act
            editor.DrawValue(entryMock.Object, state, changeSink);

            // Assert
            Assert.Equal("text", entryMock.Object.Value);
            unityGuiMock.VerifyNoOtherCalls();
        }

        [Fact]
        public void DrawValue_WhenEntryValueIsNull_DoesNothing()
        {
            // Arrange
            var unityGuiMock = new Mock<IUnityGuiProvider>(MockBehavior.Strict);
            var editor = new EnumEditor(unityGuiMock.Object);
            var entryMock = CreateEntry("NullEnumEntry", typeof(VisibleEnum), null);
            var state = new GuiStateStore();
            var changeSink = new EntryChangeSink();

            // Act
            editor.DrawValue(entryMock.Object, state, changeSink);

            // Assert
            Assert.Null(entryMock.Object.Value);
            unityGuiMock.VerifyNoOtherCalls();
        }

        [Fact]
        public void DrawValue_WhenButtonIsNotClicked_DoesNotChangeExpandedState()
        {
            // Arrange
            GUILayoutOption expandWidth = null;
            var unityGuiMock = new Mock<IUnityGuiProvider>(MockBehavior.Strict);
            unityGuiMock.Setup(x => x.ExpandWidth(true)).Returns(expandWidth);
            unityGuiMock
                .Setup(x => x.Button("Visible Option", It.Is<GUILayoutOption[]>(options => options.Length == 1 && ReferenceEquals(options[0], expandWidth))))
                .Returns(false);
            var editor = new EnumEditor(unityGuiMock.Object);
            var entryMock = CreateEntry("EnumEntry", typeof(VisibleEnum), VisibleEnum.Visible);
            var state = new GuiStateStore();
            var key = state.GetKey(entryMock.Object, "_enumExpanded");

            // Act
            editor.DrawValue(entryMock.Object, state, new EntryChangeSink());

            // Assert
            Assert.False(state.GetBool(key, true));
            Assert.Equal(string.Empty, state.GetText("expandedEntryKey", string.Empty));
            unityGuiMock.Verify(x => x.ExpandWidth(true), Times.Once);
            unityGuiMock.Verify(x => x.Button("Visible Option", It.Is<GUILayoutOption[]>(options => options.Length == 1 && ReferenceEquals(options[0], expandWidth))), Times.Once);
        }

        [Fact]
        public void DrawValue_WhenButtonClickedAndExpanded_CollapsesEntryAndClearsExpandedKey()
        {
            // Arrange
            GUILayoutOption expandWidth = null;
            var unityGuiMock = new Mock<IUnityGuiProvider>(MockBehavior.Strict);
            unityGuiMock.Setup(x => x.ExpandWidth(true)).Returns(expandWidth);
            unityGuiMock
                .Setup(x => x.Button("Visible Option", It.Is<GUILayoutOption[]>(options => options.Length == 1 && ReferenceEquals(options[0], expandWidth))))
                .Returns(true);
            var editor = new EnumEditor(unityGuiMock.Object);
            var entryMock = CreateEntry("ExpandedEnumEntry", typeof(VisibleEnum), VisibleEnum.Visible);
            var state = new GuiStateStore();
            var key = state.GetKey(entryMock.Object, "_enumExpanded");
            state.SetBool(key, true);
            state.SetText("expandedEntryKey", key);

            // Act
            editor.DrawValue(entryMock.Object, state, new EntryChangeSink());

            // Assert
            Assert.False(state.GetBool(key, true));
            Assert.Equal(string.Empty, state.GetText("expandedEntryKey", "fallback"));
            unityGuiMock.Verify(x => x.ExpandWidth(true), Times.Once);
            unityGuiMock.Verify(x => x.Button("Visible Option", It.Is<GUILayoutOption[]>(options => options.Length == 1 && ReferenceEquals(options[0], expandWidth))), Times.Once);
        }

        [Fact]
        public void DrawValue_WhenButtonClickedAndAnotherEntryExpanded_CollapsesPreviousEntryAndExpandsCurrentEntry()
        {
            // Arrange
            GUILayoutOption expandWidth = null;
            var unityGuiMock = new Mock<IUnityGuiProvider>(MockBehavior.Strict);
            unityGuiMock.Setup(x => x.ExpandWidth(true)).Returns(expandWidth);
            unityGuiMock
                .Setup(x => x.Button("Visible Option", It.Is<GUILayoutOption[]>(options => options.Length == 1 && ReferenceEquals(options[0], expandWidth))))
                .Returns(true);
            var editor = new EnumEditor(unityGuiMock.Object);
            var entryMock = CreateEntry("CurrentEnumEntry", typeof(VisibleEnum), VisibleEnum.Visible);
            var state = new GuiStateStore();
            var previousKey = "PreviousEntry+_enumExpanded";
            var currentKey = state.GetKey(entryMock.Object, "_enumExpanded");
            state.SetText("expandedEntryKey", previousKey);
            state.SetBool(previousKey, true);

            // Act
            editor.DrawValue(entryMock.Object, state, new EntryChangeSink());

            // Assert
            Assert.False(state.GetBool(previousKey, true));
            Assert.True(state.GetBool(currentKey, false));
            Assert.Equal(currentKey, state.GetText("expandedEntryKey", string.Empty));
            unityGuiMock.Verify(x => x.ExpandWidth(true), Times.Once);
            unityGuiMock.Verify(x => x.Button("Visible Option", It.Is<GUILayoutOption[]>(options => options.Length == 1 && ReferenceEquals(options[0], expandWidth))), Times.Once);
        }

        [Fact]
        public void DrawValue_WhenButtonClickedAndNoEntryExpanded_ExpandsCurrentEntry()
        {
            // Arrange
            GUILayoutOption expandWidth = null;
            var unityGuiMock = new Mock<IUnityGuiProvider>(MockBehavior.Strict);
            unityGuiMock.Setup(x => x.ExpandWidth(true)).Returns(expandWidth);
            unityGuiMock
                .Setup(x => x.Button("Visible Option", It.Is<GUILayoutOption[]>(options => options.Length == 1 && ReferenceEquals(options[0], expandWidth))))
                .Returns(true);
            var editor = new EnumEditor(unityGuiMock.Object);
            var entryMock = CreateEntry("CollapsedEnumEntry", typeof(VisibleEnum), VisibleEnum.Visible);
            var state = new GuiStateStore();
            var key = state.GetKey(entryMock.Object, "_enumExpanded");

            // Act
            editor.DrawValue(entryMock.Object, state, new EntryChangeSink());

            // Assert
            Assert.True(state.GetBool(key, false));
            Assert.Equal(key, state.GetText("expandedEntryKey", string.Empty));
            unityGuiMock.Verify(x => x.ExpandWidth(true), Times.Once);
            unityGuiMock.Verify(x => x.Button("Visible Option", It.Is<GUILayoutOption[]>(options => options.Length == 1 && ReferenceEquals(options[0], expandWidth))), Times.Once);
        }

        [Fact]
        public void DrawExtra_WhenEntryTypeIsNotEnum_DoesNothing()
        {
            // Arrange
            var unityGuiMock = new Mock<IUnityGuiProvider>(MockBehavior.Strict);
            var editor = new EnumEditor(unityGuiMock.Object);
            var entryMock = CreateEntry("NotEnumEntry", typeof(string), "text");

            // Act
            editor.DrawExtra(entryMock.Object, new GuiStateStore(), new EntryChangeSink());

            // Assert
            Assert.Equal("text", entryMock.Object.Value);
            unityGuiMock.VerifyNoOtherCalls();
        }

        [Fact]
        public void DrawExtra_WhenEntryValueIsNull_DoesNothing()
        {
            // Arrange
            var unityGuiMock = new Mock<IUnityGuiProvider>(MockBehavior.Strict);
            var editor = new EnumEditor(unityGuiMock.Object);
            var entryMock = CreateEntry("NullEnumEntry", typeof(VisibleEnum), null);

            // Act
            editor.DrawExtra(entryMock.Object, new GuiStateStore(), new EntryChangeSink());

            // Assert
            Assert.Null(entryMock.Object.Value);
            unityGuiMock.VerifyNoOtherCalls();
        }

        [Fact]
        public void DrawExtra_WhenEntryIsNotExpanded_DoesNothing()
        {
            // Arrange
            var unityGuiMock = new Mock<IUnityGuiProvider>(MockBehavior.Strict);
            var editor = new EnumEditor(unityGuiMock.Object);
            var entryMock = CreateEntry("CollapsedEnumEntry", typeof(VisibleEnum), VisibleEnum.Visible);
            var state = new GuiStateStore();
            var key = state.GetKey(entryMock.Object, "_enumExpanded");
            state.SetBool(key, false);

            // Act
            editor.DrawExtra(entryMock.Object, state, new EntryChangeSink());

            // Assert
            Assert.Equal(VisibleEnum.Visible, entryMock.Object.Value);
            unityGuiMock.VerifyNoOtherCalls();
        }

        [Fact]
        public void DrawExtra_WhenSelectionDoesNotChange_KeepsExpandedStateAndEntryValue()
        {
            // Arrange
            GUIStyle boxStyle = null;
            GUILayoutOption expandWidth = null;
            var unityGuiMock = new Mock<IUnityGuiProvider>(MockBehavior.Strict);
            unityGuiMock.SetupGet(x => x.BoxStyle).Returns(boxStyle);
            unityGuiMock.Setup(x => x.BeginHorizontal());
            unityGuiMock.Setup(x => x.Space(12.5f));
            unityGuiMock.Setup(x => x.BeginVertical(boxStyle, It.Is<GUILayoutOption[]>(options => options.Length == 0)));
            unityGuiMock.Setup(x => x.ExpandWidth(true)).Returns(expandWidth);
            unityGuiMock
                .Setup(x => x.SelectionGrid(0, It.Is<string[]>(names => names.Length == 2 && names[0] == "Visible Option" && names[1] == "Another Visible Option"), 1, It.Is<GUILayoutOption[]>(options => options.Length == 1 && ReferenceEquals(options[0], expandWidth))))
                .Returns(0);
            unityGuiMock.Setup(x => x.EndVertical());
            unityGuiMock.Setup(x => x.Space(8.5f));
            unityGuiMock.Setup(x => x.EndHorizontal());
            var editor = new EnumEditor(unityGuiMock.Object);
            var entryMock = CreateEntry("VisibleEnumEntry", typeof(VisibleEnum), VisibleEnum.Visible);
            var state = new GuiStateStore();
            state.SetFloat(GuiStateStore.LeadingBlankWidthKey, 12.5f);
            state.SetFloat(GuiStateStore.RearBlankWidthKey, 8.5f);
            var key = state.GetKey(entryMock.Object, "_enumExpanded");
            state.SetBool(key, true);

            // Act
            editor.DrawExtra(entryMock.Object, state, new EntryChangeSink());

            // Assert
            Assert.True(state.GetBool(key, false));
            Assert.Equal(VisibleEnum.Visible, entryMock.Object.Value);
            unityGuiMock.VerifyGet(x => x.BoxStyle, Times.Once);
            unityGuiMock.Verify(x => x.BeginHorizontal(), Times.Once);
            unityGuiMock.Verify(x => x.Space(12.5f), Times.Once);
            unityGuiMock.Verify(x => x.BeginVertical(boxStyle, It.Is<GUILayoutOption[]>(options => options.Length == 0)), Times.Once);
            unityGuiMock.Verify(x => x.ExpandWidth(true), Times.Once);
            unityGuiMock.Verify(x => x.SelectionGrid(0, It.Is<string[]>(names => names.Length == 2 && names[0] == "Visible Option" && names[1] == "Another Visible Option"), 1, It.Is<GUILayoutOption[]>(options => options.Length == 1 && ReferenceEquals(options[0], expandWidth))), Times.Once);
            unityGuiMock.Verify(x => x.EndVertical(), Times.Once);
            unityGuiMock.Verify(x => x.Space(8.5f), Times.Once);
            unityGuiMock.Verify(x => x.EndHorizontal(), Times.Once);
        }

        [Fact]
        public void DrawExtra_WhenCurrentValueIsHidden_FallsBackToFirstVisibleOptionAndUpdatesEntry()
        {
            // Arrange
            GUIStyle boxStyle = null;
            GUILayoutOption expandWidth = null;
            var unityGuiMock = new Mock<IUnityGuiProvider>(MockBehavior.Strict);
            unityGuiMock.SetupGet(x => x.BoxStyle).Returns(boxStyle);
            unityGuiMock.Setup(x => x.BeginHorizontal());
            unityGuiMock.Setup(x => x.Space(0f));
            unityGuiMock.Setup(x => x.BeginVertical(boxStyle, It.Is<GUILayoutOption[]>(options => options.Length == 0)));
            unityGuiMock.Setup(x => x.ExpandWidth(true)).Returns(expandWidth);
            unityGuiMock
                .Setup(x => x.SelectionGrid(0, It.Is<string[]>(names => names.Length == 2 && names[0] == "Visible Option" && names[1] == "Another Visible Option"), 1, It.Is<GUILayoutOption[]>(options => options.Length == 1 && ReferenceEquals(options[0], expandWidth))))
                .Returns(1);
            unityGuiMock.Setup(x => x.EndVertical());
            unityGuiMock.Setup(x => x.Space(0f));
            unityGuiMock.Setup(x => x.EndHorizontal());
            var editor = new EnumEditor(unityGuiMock.Object);
            var entryMock = CreateEntry("HiddenEnumEntry", typeof(VisibleEnum), VisibleEnum.Hidden);
            var state = new GuiStateStore();
            var key = state.GetKey(entryMock.Object, "_enumExpanded");
            state.SetBool(key, true);
            var changeSink = new EntryChangeSink();

            // Act
            editor.DrawExtra(entryMock.Object, state, changeSink);

            // Assert
            Assert.False(state.GetBool(key, true));
            Assert.Equal(VisibleEnum.AnotherVisible, entryMock.Object.Value);
            unityGuiMock.VerifyGet(x => x.BoxStyle, Times.Once);
            unityGuiMock.Verify(x => x.BeginHorizontal(), Times.Once);
            unityGuiMock.Verify(x => x.Space(0f), Times.Exactly(2));
            unityGuiMock.Verify(x => x.BeginVertical(boxStyle, It.Is<GUILayoutOption[]>(options => options.Length == 0)), Times.Once);
            unityGuiMock.Verify(x => x.ExpandWidth(true), Times.Once);
            unityGuiMock.Verify(x => x.SelectionGrid(0, It.Is<string[]>(names => names.Length == 2 && names[0] == "Visible Option" && names[1] == "Another Visible Option"), 1, It.Is<GUILayoutOption[]>(options => options.Length == 1 && ReferenceEquals(options[0], expandWidth))), Times.Once);
            unityGuiMock.Verify(x => x.EndVertical(), Times.Once);
            unityGuiMock.Verify(x => x.EndHorizontal(), Times.Once);
        }

        [Fact]
        public void GetEnumInfo_WhenEntryTypeIsNotEnum_ReturnsNullValues()
        {
            // Arrange
            var editor = CreateEditor();
            var entryMock = CreateEntry("NotEnumEntry", typeof(string), "text");

            // Act
            var result = editor.GetEnumInfo(entryMock.Object);

            // Assert
            Assert.Null(result.values);
            Assert.Null(result.mapIndex);
            Assert.Null(result.names);
        }

        [Fact]
        public void GetEnumInfo_WhenEntryTypeIsEnum_ReturnsOnlyDisplayableValuesAndCachesResult()
        {
            // Arrange
            var editor = CreateEditor();
            var entryMock = CreateEntry("VisibleEnumEntry", typeof(VisibleEnum), VisibleEnum.Visible);

            // Act
            var firstResult = editor.GetEnumInfo(entryMock.Object);
            var secondResult = editor.GetEnumInfo(entryMock.Object);

            // Assert
            Assert.Equal(3, firstResult.values.Length);
            Assert.Equal(new List<int> { 0, 2 }, firstResult.mapIndex);
            Assert.Equal(new[] { "Visible Option", "Another Visible Option" }, firstResult.names);
            Assert.Same(firstResult.values, secondResult.values);
            Assert.Same(firstResult.mapIndex, secondResult.mapIndex);
            Assert.Same(firstResult.names, secondResult.names);
        }

        private static EnumEditor CreateEditor()
        {
            return new EnumEditor(new Mock<IUnityGuiProvider>(MockBehavior.Strict).Object);
        }

        private static Mock<IEntryBinding> CreateEntry(string key, Type valueType, object value)
        {
            var entryMock = new Mock<IEntryBinding>(MockBehavior.Strict);
            entryMock.SetupGet(x => x.Key).Returns(key);
            entryMock.SetupGet(x => x.ValueType).Returns(valueType);
            entryMock.SetupProperty(x => x.Value, value);
            return entryMock;
        }

        private enum VisibleEnum
        {
            [Description("Visible Option")]
            Visible,

            [Description("Hidden Option")]
            [DisplayEnum(false)]
            Hidden,

            [Description("Another Visible Option")]
            AnotherVisible,
        }
    }
}
