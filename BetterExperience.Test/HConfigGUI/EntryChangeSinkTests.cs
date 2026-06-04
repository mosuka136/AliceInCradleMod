using BetterExperience.HConfigGUI;
using BetterExperience.HConfigGUI.Bindings;
using Moq;
using Xunit;

namespace BetterExperience.Test
{
    public class EntryChangeSinkTests
    {
        [Fact]
        public void SetValue_WhenEntryIsNull_DoesNothing()
        {
            // Arrange
            var sink = new EntryChangeSink();

            // Act
            var exception = Record.Exception(() => sink.SetValue(null, new object()));

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public void SetValue_WhenDelayIsNotPositiveAndValueIsUnchanged_DoesNotSetValueOrRaiseEvent()
        {
            // Arrange
            var sink = new EntryChangeSink();
            var entryMock = new Mock<IEntryBinding>(MockBehavior.Strict);
            var value = new object();
            var eventCallCount = 0;
            Action<IEntryBinding> handler = _ => eventCallCount++;
            entryMock.SetupGet(x => x.Value).Returns(value);
            GuiPipe.OnEntryValueChanged += handler;

            try
            {
                // Act
                sink.SetValue(entryMock.Object, value, 0.0f);

                // Assert
                Assert.Equal(0, eventCallCount);
                entryMock.VerifyGet(x => x.Value, Times.Once);
                entryMock.VerifySet(x => x.Value = It.IsAny<object>(), Times.Never);
            }
            finally
            {
                GuiPipe.OnEntryValueChanged -= handler;
            }
        }

        [Fact]
        public void SetValue_WhenDelayIsNotPositiveAndValueChanges_SetsValueAndRaisesEvent()
        {
            // Arrange
            var sink = new EntryChangeSink();
            var entryMock = new Mock<IEntryBinding>(MockBehavior.Strict);
            var originalValue = new object();
            var newValue = new object();
            IEntryBinding changedEntry = null;
            var eventCallCount = 0;
            Action<IEntryBinding> handler = x =>
            {
                changedEntry = x;
                eventCallCount++;
            };
            entryMock.SetupGet(x => x.Value).Returns(originalValue);
            entryMock.SetupSet(x => x.Value = newValue);
            GuiPipe.OnEntryValueChanged += handler;

            try
            {
                // Act
                sink.SetValue(entryMock.Object, newValue, -1.0f);

                // Assert
                Assert.Equal(1, eventCallCount);
                Assert.Same(entryMock.Object, changedEntry);
                entryMock.VerifyGet(x => x.Value, Times.Once);
                entryMock.VerifySet(x => x.Value = newValue, Times.Once);
            }
            finally
            {
                GuiPipe.OnEntryValueChanged -= handler;
            }
        }

        [Fact]
        public void SetValue_WhenDelayIsPositive_StoresPendingValueUntilFlushCompletes()
        {
            // Arrange
            var sink = new EntryChangeSink();
            var entryMock = new Mock<IEntryBinding>(MockBehavior.Strict);
            var currentValue = new object();
            var delayedValue = new object();
            var storedValue = currentValue;
            IEntryBinding changedEntry = null;
            var eventCallCount = 0;
            Action<IEntryBinding> handler = x =>
            {
                changedEntry = x;
                eventCallCount++;
            };
            entryMock.SetupGet(x => x.Value).Returns(() => storedValue);
            entryMock.SetupSet(x => x.Value = delayedValue).Callback<object>(x => storedValue = x);
            GuiPipe.OnEntryValueChanged += handler;

            try
            {
                // Act
                sink.SetValue(entryMock.Object, delayedValue, 1.0f);
                sink.FlushValue(0.25f);

                // Assert
                Assert.Same(currentValue, storedValue);
                Assert.Equal(0, eventCallCount);
                entryMock.VerifySet(x => x.Value = It.IsAny<object>(), Times.Never);

                // Act
                sink.FlushValue(0.75f);

                // Assert
                Assert.Same(delayedValue, storedValue);
                Assert.Equal(1, eventCallCount);
                Assert.Same(entryMock.Object, changedEntry);
                entryMock.VerifySet(x => x.Value = delayedValue, Times.Once);
            }
            finally
            {
                GuiPipe.OnEntryValueChanged -= handler;
            }
        }

        [Fact]
        public void ResetValue_WhenEntryIsNull_DoesNothing()
        {
            // Arrange
            var sink = new EntryChangeSink();

            // Act
            var exception = Record.Exception(() => sink.ResetValue(null));

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public void ResetValue_WhenEntryHasPendingValue_RemovesPendingValueAndRaisesResetEvent()
        {
            // Arrange
            var sink = new EntryChangeSink();
            var entryMock = new Mock<IEntryBinding>(MockBehavior.Strict);
            var currentValue = new object();
            var delayedValue = new object();
            var storedValue = currentValue;
            IEntryBinding resetEntry = null;
            var resetEventCallCount = 0;
            Action<IEntryBinding> handler = x =>
            {
                resetEntry = x;
                resetEventCallCount++;
            };
            entryMock.SetupGet(x => x.Key).Returns("TestKey");
            entryMock.SetupGet(x => x.Value).Returns(() => storedValue);
            entryMock.Setup(x => x.ResetValue()).Callback(() => storedValue = currentValue);
            GuiPipe.OnEntryValueReset += handler;

            try
            {
                sink.SetValue(entryMock.Object, delayedValue, 1.0f);

                // Act
                sink.ResetValue(entryMock.Object);
                sink.FlushValue(1.0f);

                // Assert
                Assert.Equal(1, resetEventCallCount);
                Assert.Same(entryMock.Object, resetEntry);
                Assert.Same(currentValue, storedValue);
                entryMock.Verify(x => x.ResetValue(), Times.Once);
                entryMock.VerifySet(x => x.Value = It.IsAny<object>(), Times.Never);
            }
            finally
            {
                GuiPipe.OnEntryValueReset -= handler;
            }
        }

        [Fact]
        public void FlushValue_WhenPendingDelayExpiresAndValueMatchesCurrent_DoesNotRaiseEvent()
        {
            // Arrange
            var sink = new EntryChangeSink();
            var entryMock = new Mock<IEntryBinding>(MockBehavior.Strict);
            var value = new object();
            var storedValue = value;
            var eventCallCount = 0;
            Action<IEntryBinding> handler = _ => eventCallCount++;
            entryMock.SetupGet(x => x.Value).Returns(() => storedValue);
            GuiPipe.OnEntryValueChanged += handler;

            try
            {
                sink.SetValue(entryMock.Object, value, 0.5f);

                // Act
                sink.FlushValue(0.5f);
                storedValue = new object();
                sink.FlushValue(1.0f);

                // Assert
                Assert.Equal(0, eventCallCount);
                entryMock.VerifySet(x => x.Value = It.IsAny<object>(), Times.Never);
            }
            finally
            {
                GuiPipe.OnEntryValueChanged -= handler;
            }
        }
    }
}
