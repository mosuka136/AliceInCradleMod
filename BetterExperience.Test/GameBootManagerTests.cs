using BetterExperience.HLogSpace;
using System.Reflection;
using UnityEngine;

namespace BetterExperience.Test
{
    public class GameBootManagerTests
    {
        [Fact]
        public void RegisterComponentOnGameBoot_NonComponentType_PostfixInvokesOtherHandlersNormally()
        {
            // Arrange
            var invocationCount = 0;
            using var scope = GameBootManagerStateScope.Create();
            Action handler = () => invocationCount++;

            GameBootManager.OnGameBoot += handler;
            GameBootManager.RegisterComponentOnGameBoot(typeof(string));

            // Act
            var exception = Record.Exception(GameBootManager.LoadScenePatch.Postfix);

            // Assert
            Assert.Null(exception);
            Assert.Equal(1, invocationCount);
        }

        [Fact]
        public void RegisterComponentOnGameBoot_AbstractComponentType_PostfixSwallowsComponentCreationFailure()
        {
            // Arrange
            var invocationCount = 0;
            using var scope = GameBootManagerStateScope.Create();
            Action handler = () => invocationCount++;

            GameBootManager.OnGameBoot += handler;
            GameBootManager.RegisterComponentOnGameBoot(typeof(AbstractTestComponent));

            // Act
            var exception = Record.Exception(GameBootManager.LoadScenePatch.Postfix);

            // Assert
            Assert.Null(exception);
            Assert.Equal(1, invocationCount);
        }

        [Fact]
        public void LoadScenePatch_Postfix_CalledTwiceInvokesCustomHandlerOnlyOnce()
        {
            // Arrange
            var invocationCount = 0;
            using var scope = GameBootManagerStateScope.Create();
            Action handler = () => invocationCount++;

            GameBootManager.OnGameBoot += handler;

            // Act
            var exception = Record.Exception(() =>
            {
                GameBootManager.LoadScenePatch.Postfix();
                GameBootManager.LoadScenePatch.Postfix();
            });

            // Assert
            Assert.Null(exception);
            Assert.Equal(1, invocationCount);
        }

        private abstract class AbstractTestComponent : MonoBehaviour
        {
        }

        private sealed class GameBootManagerStateScope : IDisposable
        {
            private static readonly FieldInfo InitializedField = GetRequiredField("_initialized");
            private static readonly FieldInfo OnGameBootField = GetRequiredField("OnGameBoot");

            private readonly bool _originalEnableLog;
            private readonly bool _originalInitialized;
            private readonly Action _originalOnGameBoot;

            private GameBootManagerStateScope(
                bool originalEnableLog,
                bool originalInitialized,
                Action originalOnGameBoot)
            {
                _originalEnableLog = originalEnableLog;
                _originalInitialized = originalInitialized;
                _originalOnGameBoot = originalOnGameBoot;
            }

            public static GameBootManagerStateScope Create()
            {
                var scope = new GameBootManagerStateScope(
                    HLog.EnableLog,
                    (bool)InitializedField.GetValue(null),
                    (Action)OnGameBootField.GetValue(null));

                HLog.EnableLog = false;
                InitializedField.SetValue(null, false);
                OnGameBootField.SetValue(null, null);

                return scope;
            }

            public void Dispose()
            {
                OnGameBootField.SetValue(null, _originalOnGameBoot);
                InitializedField.SetValue(null, _originalInitialized);
                HLog.EnableLog = _originalEnableLog;
            }

            private static FieldInfo GetRequiredField(string fieldName)
            {
                var field = typeof(GameBootManager).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Static);
                if (field == null)
                {
                    throw new InvalidOperationException($"Field '{fieldName}' was not found on {typeof(GameBootManager).FullName}.");
                }

                return field;
            }
        }
    }
}
