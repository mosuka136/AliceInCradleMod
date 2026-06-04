using BetterExperience.HConfigGUI;
using BetterExperience.HConfigGUI.Bindings;
using BetterExperience.HConfigGUI.Editor.ValueEditor;
using BetterExperience.HProvider;
using Moq;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BetterExperience.Test
{
    public class NumberEditorTests
    {
        [Fact]
        public void NumberEditor_WhenConstructed_StoresUnityGuiProvider()
        {
            // Arrange
            var unityGuiMock = new Mock<IUnityGuiProvider>(MockBehavior.Strict);

            // Act
            var editor = new NumberEditor(unityGuiMock.Object);

            // Assert
            Assert.Same(unityGuiMock.Object, editor.UnityGui);
            Assert.Equal(0.5f, editor.DelayApplyDuration);
        }

        [Theory]
        [InlineData(typeof(int), true)]
        [InlineData(typeof(bool), false)]
        [InlineData(typeof(char), false)]
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

        [Theory]
        [InlineData(typeof(string), "value")]
        [InlineData(typeof(bool), true)]
        [InlineData(typeof(char), 'A')]
        public void DrawValue_WhenEntryTypeIsNotEditable_ReturnsWithoutDrawing(Type valueType, object value)
        {
            // Arrange
            var unityGuiMock = new Mock<IUnityGuiProvider>(MockBehavior.Strict);
            var editor = new NumberEditor(unityGuiMock.Object);
            var state = new GuiStateStore();
            var changeSink = new EntryChangeSink();
            var entryMock = CreateEntry("NonEditableEntry", valueType, value);
            var originalValue = entryMock.Object.Value;

            // Act
            editor.DrawValue(entryMock.Object, state, changeSink);
            changeSink.FlushValue(1.0f);

            // Assert
            Assert.Equal(originalValue, entryMock.Object.Value);
            unityGuiMock.VerifyNoOtherCalls();
        }

        [Fact]
        public void DrawValue_WhenTextFieldReturnsSameValue_DoesNotQueueChange()
        {
            // Arrange
            const int currentValue = 123;
            const string currentValueText = "123";
            var unityGuiMock = new Mock<IUnityGuiProvider>(MockBehavior.Strict);
            unityGuiMock.Setup(x => x.ExpandWidth(true)).Returns((GUILayoutOption)null);
            unityGuiMock
                .Setup(x => x.TextField(currentValueText, It.Is<GUILayoutOption[]>(options => options.Length == 1 && options[0] == null)))
                .Returns(currentValueText);
            var editor = new NumberEditor(unityGuiMock.Object);
            var state = new GuiStateStore();
            var changeSink = new EntryChangeSink();
            var entryMock = CreateEntry("SameNumberEntry", typeof(int), currentValue);
            var key = state.GetKey(entryMock.Object, "_number");

            // Act
            editor.DrawValue(entryMock.Object, state, changeSink);
            changeSink.FlushValue(1.0f);

            // Assert
            Assert.Equal(currentValueText, state.GetText(key, "fallback"));
            Assert.Equal(currentValue, entryMock.Object.Value);
            unityGuiMock.Verify(x => x.ExpandWidth(true), Times.Once);
            unityGuiMock.Verify(x => x.TextField(currentValueText, It.Is<GUILayoutOption[]>(options => options.Length == 1 && options[0] == null)), Times.Once);
        }

        [Fact]
        public void DrawValue_WhenTextFieldReturnsDifferentValue_UpdatesStateAndQueuesDelayedChange()
        {
            // Arrange
            const int currentValue = 123;
            const int newValue = 456;
            const string currentValueText = "123";
            const string newValueText = "456";
            var unityGuiMock = new Mock<IUnityGuiProvider>(MockBehavior.Strict);
            unityGuiMock.Setup(x => x.ExpandWidth(true)).Returns((GUILayoutOption)null);
            unityGuiMock
                .Setup(x => x.TextField(currentValueText, It.Is<GUILayoutOption[]>(options => options.Length == 1 && options[0] == null)))
                .Returns(newValueText);
            var editor = new NumberEditor(unityGuiMock.Object)
            {
                DelayApplyDuration = 0.5f,
            };
            var state = new GuiStateStore();
            var changeSink = new EntryChangeSink();
            var entryMock = CreateEntry("ChangedNumberEntry", typeof(int), currentValue);
            var key = state.GetKey(entryMock.Object, "_number");

            // Act
            editor.DrawValue(entryMock.Object, state, changeSink);
            changeSink.FlushValue(0.4f);
            var valueBeforeDelayExpires = entryMock.Object.Value;
            changeSink.FlushValue(0.1f);

            // Assert
            Assert.Equal(newValueText, state.GetText(key, "fallback"));
            Assert.Equal(currentValue, valueBeforeDelayExpires);
            Assert.Equal(newValue, entryMock.Object.Value);
            unityGuiMock.Verify(x => x.ExpandWidth(true), Times.Once);
            unityGuiMock.Verify(x => x.TextField(currentValueText, It.Is<GUILayoutOption[]>(options => options.Length == 1 && options[0] == null)), Times.Once);
        }

        [Theory]
        [MemberData(nameof(GetSupportedNumberTypes))]
        public void SetValue_WhenInputIsValidForSupportedType_QueuesDelayedParsedValue(Type valueType, object initialValue, string newValueText, object expectedValue)
        {
            // Arrange
            var editor = CreateEditor();
            var changeSink = new EntryChangeSink();
            var entryMock = CreateEntry("ValidSetValueEntry", valueType, initialValue);

            // Act
            editor.SetValue(entryMock.Object, newValueText, changeSink);
            changeSink.FlushValue(0.4f);
            var valueBeforeDelayExpires = entryMock.Object.Value;
            changeSink.FlushValue(0.1f);

            // Assert
            Assert.Equal(initialValue, valueBeforeDelayExpires);
            Assert.IsType(valueType, entryMock.Object.Value);
            Assert.Equal(expectedValue, entryMock.Object.Value);
        }

        [Theory]
        [MemberData(nameof(GetInvalidNumberInputs))]
        public void SetValue_WhenInputIsInvalidForSupportedType_DoesNotChangeEntryValue(Type valueType, object initialValue, string invalidValueText)
        {
            // Arrange
            var editor = CreateEditor();
            var changeSink = new EntryChangeSink();
            var entryMock = CreateEntry("InvalidSetValueEntry", valueType, initialValue);

            // Act
            editor.SetValue(entryMock.Object, invalidValueText, changeSink);
            changeSink.FlushValue(1.0f);

            // Assert
            Assert.Equal(initialValue, entryMock.Object.Value);
            Assert.IsType(valueType, entryMock.Object.Value);
        }

        [Fact]
        public void SetValue_WhenTypeIsUnsupported_DoesNotChangeEntryValue()
        {
            // Arrange
            var editor = CreateEditor();
            var changeSink = new EntryChangeSink();
            var entryMock = CreateEntry("UnsupportedSetValueEntry", typeof(decimal), 1.25m);

            // Act
            editor.SetValue(entryMock.Object, "2.50", changeSink);
            changeSink.FlushValue(1.0f);

            // Assert
            Assert.Equal(1.25m, entryMock.Object.Value);
            Assert.IsType<decimal>(entryMock.Object.Value);
        }

        [Fact]
        public void DrawExtra_WhenCalled_DoesNothing()
        {
            // Arrange
            var editor = CreateEditor();
            var entryMock = new Mock<IEntryBinding>(MockBehavior.Strict);

            // Act
            var exception = Record.Exception(() => editor.DrawExtra(entryMock.Object, new GuiStateStore(), new EntryChangeSink()));

            // Assert
            Assert.Null(exception);
        }

        public static IEnumerable<object[]> GetSupportedNumberTypes()
        {
            yield return new object[] { typeof(byte), (byte)1, "200", (byte)200 };
            yield return new object[] { typeof(sbyte), (sbyte)1, "-100", (sbyte)-100 };
            yield return new object[] { typeof(short), (short)1, "-32000", (short)-32000 };
            yield return new object[] { typeof(ushort), (ushort)1, "65000", (ushort)65000 };
            yield return new object[] { typeof(int), 1, "123456", 123456 };
            yield return new object[] { typeof(uint), (uint)1, "123456", (uint)123456 };
            yield return new object[] { typeof(long), (long)1, "1234567890123", (long)1234567890123 };
            yield return new object[] { typeof(ulong), (ulong)1, "1234567890123", (ulong)1234567890123 };
            yield return new object[] { typeof(float), 1.0f, "12.5", 12.5f };
            yield return new object[] { typeof(double), 1.0d, "12.5", 12.5d };
        }

        public static IEnumerable<object[]> GetInvalidNumberInputs()
        {
            yield return new object[] { typeof(byte), (byte)1, "invalid" };
            yield return new object[] { typeof(sbyte), (sbyte)1, "invalid" };
            yield return new object[] { typeof(short), (short)1, "invalid" };
            yield return new object[] { typeof(ushort), (ushort)1, "invalid" };
            yield return new object[] { typeof(int), 1, "invalid" };
            yield return new object[] { typeof(uint), (uint)1, "invalid" };
            yield return new object[] { typeof(long), (long)1, "invalid" };
            yield return new object[] { typeof(ulong), (ulong)1, "invalid" };
            yield return new object[] { typeof(float), 1.0f, "invalid" };
            yield return new object[] { typeof(double), 1.0d, "invalid" };
        }

        private static NumberEditor CreateEditor()
        {
            return new NumberEditor(new Mock<IUnityGuiProvider>(MockBehavior.Strict).Object);
        }

        private static Mock<IEntryBinding> CreateEntry(string key, Type valueType, object value)
        {
            var entryMock = new Mock<IEntryBinding>(MockBehavior.Strict);
            entryMock.SetupGet(x => x.Key).Returns(key);
            entryMock.SetupGet(x => x.ValueType).Returns(valueType);
            entryMock.SetupProperty(x => x.Value, value);
            return entryMock;
        }
    }
}
