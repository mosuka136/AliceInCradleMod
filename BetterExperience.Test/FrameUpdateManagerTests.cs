using BetterExperience.HLogSpace;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace BetterExperience.Test
{
    public class FrameUpdateManagerTests
    {
        [Fact]
        public void UpdaterUpdate_NoHandlersSubscribed_DoesNotThrow()
        {
            // Arrange
            using var scope = FrameUpdateManagerStateScope.Create();

            // Act
            var exception = Record.Exception(InvokeUpdaterUpdate);

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public void UpdaterUpdate_HandlerSubscribed_InvokesHandler()
        {
            // Arrange
            var invocationCount = 0;
            using var scope = FrameUpdateManagerStateScope.Create();
            FrameUpdateManager.OnFrameUpdate += () => invocationCount++;

            // Act
            var exception = Record.Exception(InvokeUpdaterUpdate);

            // Assert
            Assert.Null(exception);
            Assert.Equal(1, invocationCount);
        }

        [Fact]
        public void UpdaterUpdate_HandlerThrowsException_InvokesRemainingHandlersAndKeepsThrowingHandler()
        {
            // Arrange
            var invocationCount = 0;
            Action throwingHandler = () => throw new InvalidOperationException("handler failure");
            Action countingHandler = () => invocationCount++;
            using var scope = FrameUpdateManagerStateScope.Create();
            FrameUpdateManager.OnFrameUpdate += throwingHandler;
            FrameUpdateManager.OnFrameUpdate += countingHandler;

            // Act
            var exception = Record.Exception(InvokeUpdaterUpdate);

            // Assert
            Assert.Null(exception);
            Assert.Equal(1, invocationCount);
            Assert.Contains(throwingHandler, scope.GetHandlers());
            Assert.Contains(countingHandler, scope.GetHandlers());
        }

        [Fact]
        public void UpdaterUpdate_HandlerThrowsMissingMethodException_RemovesThatHandlerAndInvokesRemainingHandlers()
        {
            // Arrange
            var invocationCount = 0;
            Action missingHandler = () => throw new MissingMethodException("missing");
            Action countingHandler = () => invocationCount++;
            using var scope = FrameUpdateManagerStateScope.Create();
            FrameUpdateManager.OnFrameUpdate += missingHandler;
            FrameUpdateManager.OnFrameUpdate += countingHandler;

            // Act
            var exception = Record.Exception(InvokeUpdaterUpdate);

            // Assert
            Assert.Null(exception);
            Assert.Equal(1, invocationCount);
            Assert.DoesNotContain(missingHandler, scope.GetHandlers());
            Assert.Contains(countingHandler, scope.GetHandlers());
        }

        private static void InvokeUpdaterUpdate()
        {
            var updater = (FrameUpdateManager.Updater)RuntimeHelpers.GetUninitializedObject(typeof(FrameUpdateManager.Updater));
            var update = typeof(FrameUpdateManager.Updater).GetMethod("Update", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(update);
            update.Invoke(updater, null);
        }

        private sealed class FrameUpdateManagerStateScope : IDisposable
        {
            private static readonly FieldInfo OnFrameUpdateField = GetRequiredField(nameof(FrameUpdateManager.OnFrameUpdate));

            private readonly bool _originalEnableLog;
            private readonly Action _originalOnFrameUpdate;

            private FrameUpdateManagerStateScope(bool originalEnableLog, Action originalOnFrameUpdate)
            {
                _originalEnableLog = originalEnableLog;
                _originalOnFrameUpdate = originalOnFrameUpdate;
            }

            public static FrameUpdateManagerStateScope Create()
            {
                var scope = new FrameUpdateManagerStateScope(
                    HLog.EnableLog,
                    (Action)OnFrameUpdateField.GetValue(null));

                HLog.EnableLog = false;
                OnFrameUpdateField.SetValue(null, null);

                return scope;
            }

            public IReadOnlyList<Action> GetHandlers()
            {
                var handler = (Action)OnFrameUpdateField.GetValue(null);
                return (handler?.GetInvocationList() ?? Array.Empty<Delegate>())
                    .Cast<Action>()
                    .ToArray();
            }

            public void Dispose()
            {
                OnFrameUpdateField.SetValue(null, _originalOnFrameUpdate);
                HLog.EnableLog = _originalEnableLog;
            }

            private static FieldInfo GetRequiredField(string fieldName)
            {
                var field = typeof(FrameUpdateManager).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Static);
                if (field == null)
                {
                    throw new InvalidOperationException($"Field '{fieldName}' was not found on {typeof(FrameUpdateManager).FullName}.");
                }

                return field;
            }
        }
    }
}
