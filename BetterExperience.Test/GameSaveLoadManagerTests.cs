namespace BetterExperience.Test
{
    public class GameSaveLoadManagerTests
    {
        [Fact]
        public void LoadCompletedPostfix_NameIsNotInitNewGame_DoesNotInvokeEvent()
        {
            // Arrange
            var invocationCount = 0;
            Action handler = () => invocationCount++;
            GameSaveLoadManager.OnGameSaveLoadCompleted += handler;

            try
            {
                // Act
                var exception = Record.Exception(() => GameSaveLoadManager.GameSaveLoadPatch.LoadCompletedPostfix("OTHER_SAVE"));

                // Assert
                Assert.Null(exception);
                Assert.Equal(0, invocationCount);
            }
            finally
            {
                GameSaveLoadManager.OnGameSaveLoadCompleted -= handler;
            }
        }

        [Fact]
        public void LoadCompletedPostfix_NameIsInitNewGame_InvokesEvent()
        {
            // Arrange
            var invocationCount = 0;
            Action handler = () => invocationCount++;
            GameSaveLoadManager.OnGameSaveLoadCompleted += handler;

            try
            {
                // Act
                var exception = Record.Exception(() => GameSaveLoadManager.GameSaveLoadPatch.LoadCompletedPostfix("__INITNEWGAME"));

                // Assert
                Assert.Null(exception);
                Assert.Equal(1, invocationCount);
            }
            finally
            {
                GameSaveLoadManager.OnGameSaveLoadCompleted -= handler;
            }
        }

        [Fact]
        public void LoadCompletedPostfix_EventHandlerThrows_SwallowsException()
        {
            // Arrange
            Action handler = () => throw new InvalidOperationException("handler failure");
            GameSaveLoadManager.OnGameSaveLoadCompleted += handler;

            try
            {
                // Act
                var exception = Record.Exception(() => GameSaveLoadManager.GameSaveLoadPatch.LoadCompletedPostfix("__INITNEWGAME"));

                // Assert
                Assert.Null(exception);
            }
            finally
            {
                GameSaveLoadManager.OnGameSaveLoadCompleted -= handler;
            }
        }
    }
}
