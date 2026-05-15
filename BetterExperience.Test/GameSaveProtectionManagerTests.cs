namespace BetterExperience.Test
{
    public class GameSaveProtectionManagerTests
    {
        [Fact]
        public void SaveGamePrefix_HandlerSubscribed_InvokesHandler()
        {
            // Arrange
            var invocationCount = 0;
            Action handler = () => invocationCount++;
            GameSaveProtectionManager.OnSavingActivated += handler;

            try
            {
                // Act
                var exception = Record.Exception(() => GameSaveProtectionManager.GameSaveProtectionPatch.SaveGamePrefix());

                // Assert
                Assert.Null(exception);
                Assert.Equal(1, invocationCount);
            }
            finally
            {
                GameSaveProtectionManager.OnSavingActivated -= handler;
            }
        }

        [Fact]
        public void SaveGamePrefix_NoHandlerSubscribed_DoesNotThrow()
        {
            // Act
            var exception = Record.Exception(() => GameSaveProtectionManager.GameSaveProtectionPatch.SaveGamePrefix());

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public void SaveGamePrefix_HandlerThrows_SwallowsException()
        {
            // Arrange
            Action handler = () => throw new InvalidOperationException("handler failure");
            GameSaveProtectionManager.OnSavingActivated += handler;

            try
            {
                // Act
                var exception = Record.Exception(() => GameSaveProtectionManager.GameSaveProtectionPatch.SaveGamePrefix());

                // Assert
                Assert.Null(exception);
            }
            finally
            {
                GameSaveProtectionManager.OnSavingActivated -= handler;
            }
        }

        [Fact]
        public void SaveGamePostfix_HandlerSubscribed_InvokesHandler()
        {
            // Arrange
            var invocationCount = 0;
            Action handler = () => invocationCount++;
            GameSaveProtectionManager.OnSavingCompleted += handler;

            try
            {
                // Act
                var exception = Record.Exception(() => GameSaveProtectionManager.GameSaveProtectionPatch.SaveGamePostfix());

                // Assert
                Assert.Null(exception);
                Assert.Equal(1, invocationCount);
            }
            finally
            {
                GameSaveProtectionManager.OnSavingCompleted -= handler;
            }
        }

        [Fact]
        public void SaveGamePostfix_NoHandlerSubscribed_DoesNotThrow()
        {
            // Act
            var exception = Record.Exception(() => GameSaveProtectionManager.GameSaveProtectionPatch.SaveGamePostfix());

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public void SaveGamePostfix_HandlerThrows_SwallowsException()
        {
            // Arrange
            Action handler = () => throw new InvalidOperationException("handler failure");
            GameSaveProtectionManager.OnSavingCompleted += handler;

            try
            {
                // Act
                var exception = Record.Exception(() => GameSaveProtectionManager.GameSaveProtectionPatch.SaveGamePostfix());

                // Assert
                Assert.Null(exception);
            }
            finally
            {
                GameSaveProtectionManager.OnSavingCompleted -= handler;
            }
        }
    }
}
