using BetterExperience.BLogSpace;
using System.Reflection;

namespace BetterExperience.Test
{
    public class GameQuitManagerTests
    {
        [Fact]
        public void Dispose_NoHandlerSubscribed_DoesNotThrow()
        {
            // Arrange
            using var scope = GameQuitManagerStateScope.Create();

            // Act
            var exception = Record.Exception(GameQuitManager.Dispose);

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public void Dispose_HandlerSubscribed_InvokesHandler()
        {
            // Arrange
            var invocationCount = 0;
            using var scope = GameQuitManagerStateScope.Create();
            GameQuitManager.OnGameQuit += () => invocationCount++;

            // Act
            var exception = Record.Exception(GameQuitManager.Dispose);

            // Assert
            Assert.Null(exception);
            Assert.Equal(1, invocationCount);
        }

        [Fact]
        public void Dispose_HandlerThrows_InvokesRemainingHandlersAndSwallowsException()
        {
            // Arrange
            var invocationCount = 0;
            using var scope = GameQuitManagerStateScope.Create();
            GameQuitManager.OnGameQuit += () => throw new InvalidOperationException("handler failure");
            GameQuitManager.OnGameQuit += () => invocationCount++;

            // Act
            var exception = Record.Exception(GameQuitManager.Dispose);

            // Assert
            Assert.Null(exception);
            Assert.Equal(1, invocationCount);
        }

        private sealed class GameQuitManagerStateScope : IDisposable
        {
            private static readonly FieldInfo InitializedField = GetRequiredField("_initialized");
            private static readonly FieldInfo OnGameQuitField = GetRequiredField("OnGameQuit");

            private readonly bool _originalEnableLog;
            private readonly bool _originalInitialized;
            private readonly Action _originalOnGameQuit;

            private GameQuitManagerStateScope(
                bool originalEnableLog,
                bool originalInitialized,
                Action originalOnGameQuit)
            {
                _originalEnableLog = originalEnableLog;
                _originalInitialized = originalInitialized;
                _originalOnGameQuit = originalOnGameQuit;
            }

            public static GameQuitManagerStateScope Create()
            {
                var scope = new GameQuitManagerStateScope(
                    BLog.EnableLog,
                    (bool)InitializedField.GetValue(null),
                    (Action)OnGameQuitField.GetValue(null));

                BLog.EnableLog = false;
                InitializedField.SetValue(null, false);
                OnGameQuitField.SetValue(null, null);

                return scope;
            }

            public void Dispose()
            {
                OnGameQuitField.SetValue(null, _originalOnGameQuit);
                InitializedField.SetValue(null, _originalInitialized);
                BLog.EnableLog = _originalEnableLog;
            }

            private static FieldInfo GetRequiredField(string fieldName)
            {
                var field = typeof(GameQuitManager).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Static);
                if (field == null)
                {
                    throw new InvalidOperationException($"Field '{fieldName}' was not found on {typeof(GameQuitManager).FullName}.");
                }

                return field;
            }
        }
    }
}
