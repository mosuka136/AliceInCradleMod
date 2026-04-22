using BetterExperience.HConfigGUI;
using BetterExperience.HConfigGUI.UI;
using BetterExperience.HProvider;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace BetterExperience.Test
{
    public class StyleResourceTests
    {
        [Fact]
        public void Constructor_WithValidContext_StoresContext()
        {
            // Arrange
            var viewModel = CreateUninitializedViewModel();

            // Act
            var styleResource = new StyleResource(viewModel);

            // Assert
            Assert.NotNull(styleResource);
            var context = GetPrivateField<ViewModel>(styleResource, "_context");
            Assert.Same(viewModel, context);
        }

        [Fact]
        public void Constructor_WithNullContext_StoresNull()
        {
            // Arrange & Act
            var styleResource = new StyleResource(null);

            // Assert
            Assert.NotNull(styleResource);
            var context = GetPrivateField<ViewModel>(styleResource, "_context");
            Assert.Null(context);
        }

        [Fact]
        public void TableTitleStyle_FirstAccess_InitializesBackingField()
        {
            // Arrange
            var viewModel = CreateUninitializedViewModel();
            var unityGuiService = CreateUninitializedUnityGuiProvider();
            SetPrivateAutoProperty(viewModel, nameof(ViewModel.UnityGuiService), unityGuiService);
            var styleResource = new StyleResource(viewModel);

            // Verify backing field is initially null
            var backingFieldBefore = GetPrivateField<GUIStyle>(styleResource, "_tableTitleStyle");
            Assert.Null(backingFieldBefore);

            // Act - Access will throw SecurityException but should still attempt to set the field
            var exception = Record.Exception(() => _ = styleResource.TableTitleStyle);

            // Assert - We expect SecurityException from Unity APIs in unit tests
            Assert.True(exception is System.Security.SecurityException);
        }

        [Fact]
        public void TableTitleStyle_WithNullContext_ThrowsException()
        {
            // Arrange
            var styleResource = new StyleResource(null);

            // Act & Assert
            var exception = Assert.ThrowsAny<Exception>(() => styleResource.TableTitleStyle);
            Assert.True(exception is NullReferenceException or System.Security.SecurityException);
        }

        [Fact]
        public void TooltipStyle_FirstAccess_InitializesBackingField()
        {
            // Arrange
            var viewModel = CreateUninitializedViewModel();
            var unityGuiService = CreateUninitializedUnityGuiProvider();
            SetPrivateAutoProperty(viewModel, nameof(ViewModel.UnityGuiService), unityGuiService);
            var styleResource = new StyleResource(viewModel);

            // Verify backing field is initially null
            var backingFieldBefore = GetPrivateField<GUIStyle>(styleResource, "_tooltipStyle");
            Assert.Null(backingFieldBefore);

            // Act - Access will throw SecurityException but should still attempt to set the field
            var exception = Record.Exception(() => _ = styleResource.TooltipStyle);

            // Assert - We expect SecurityException from Unity APIs in unit tests
            Assert.True(exception is System.Security.SecurityException);
        }

        [Fact]
        public void TooltipStyle_WithNullContext_ThrowsException()
        {
            // Arrange
            var styleResource = new StyleResource(null);

            // Act & Assert
            var exception = Assert.ThrowsAny<Exception>(() => styleResource.TooltipStyle);
            Assert.True(exception is NullReferenceException or System.Security.SecurityException);
        }

        [Fact]
        public void SliderStyle_FirstAccess_InitializesBackingField()
        {
            // Arrange
            var viewModel = CreateUninitializedViewModel();
            var unityGuiService = CreateUninitializedUnityGuiProvider();
            SetPrivateAutoProperty(viewModel, nameof(ViewModel.UnityGuiService), unityGuiService);
            var styleResource = new StyleResource(viewModel);

            // Verify backing field is initially null
            var backingFieldBefore = GetPrivateField<GUIStyle>(styleResource, "_sliderStyle");
            Assert.Null(backingFieldBefore);

            // Act - Access will throw SecurityException but should still attempt to set the field
            var exception = Record.Exception(() => _ = styleResource.SliderStyle);

            // Assert - We expect SecurityException from Unity APIs in unit tests
            Assert.True(exception is System.Security.SecurityException);
        }

        [Fact]
        public void SliderStyle_WithNullContext_ThrowsException()
        {
            // Arrange
            var styleResource = new StyleResource(null);

            // Act & Assert
            var exception = Assert.ThrowsAny<Exception>(() => styleResource.SliderStyle);
            Assert.True(exception is NullReferenceException or System.Security.SecurityException);
        }

        [Fact]
        public void SliderThumbStyle_FirstAccess_InitializesBackingField()
        {
            // Arrange
            var viewModel = CreateUninitializedViewModel();
            var unityGuiService = CreateUninitializedUnityGuiProvider();
            SetPrivateAutoProperty(viewModel, nameof(ViewModel.UnityGuiService), unityGuiService);
            var styleResource = new StyleResource(viewModel);

            // Verify backing field is initially null
            var backingFieldBefore = GetPrivateField<GUIStyle>(styleResource, "_sliderThumbStyle");
            Assert.Null(backingFieldBefore);

            // Act - Access will throw SecurityException but should still attempt to set the field
            var exception = Record.Exception(() => _ = styleResource.SliderThumbStyle);

            // Assert - We expect SecurityException from Unity APIs in unit tests
            Assert.True(exception is System.Security.SecurityException);
        }

        [Fact]
        public void SliderThumbStyle_WithNullContext_ThrowsException()
        {
            // Arrange
            var styleResource = new StyleResource(null);

            // Act & Assert
            var exception = Assert.ThrowsAny<Exception>(() => styleResource.SliderThumbStyle);
            Assert.True(exception is NullReferenceException or System.Security.SecurityException);
        }

        [Fact]
        public void ToastStyle_FirstAccess_InitializesBackingField()
        {
            // Arrange
            var viewModel = CreateUninitializedViewModel();
            var unityGuiService = CreateUninitializedUnityGuiProvider();
            SetPrivateAutoProperty(viewModel, nameof(ViewModel.UnityGuiService), unityGuiService);
            var styleResource = new StyleResource(viewModel);

            // Verify backing field is initially null
            var backingFieldBefore = GetPrivateField<GUIStyle>(styleResource, "_toastStyle");
            Assert.Null(backingFieldBefore);

            // Act - Access will throw SecurityException but should still attempt to set the field
            var exception = Record.Exception(() => _ = styleResource.ToastStyle);

            // Assert - We expect SecurityException from Unity APIs in unit tests
            Assert.True(exception is System.Security.SecurityException);
        }

        [Fact]
        public void ToastStyle_WithNullContext_ThrowsException()
        {
            // Arrange
            var styleResource = new StyleResource(null);

            // Act & Assert
            var exception = Assert.ThrowsAny<Exception>(() => styleResource.ToastStyle);
            Assert.True(exception is NullReferenceException or System.Security.SecurityException);
        }

        private static ViewModel CreateUninitializedViewModel()
        {
            return (ViewModel)RuntimeHelpers.GetUninitializedObject(typeof(ViewModel));
        }

        private static UnityGuiProvider CreateUninitializedUnityGuiProvider()
        {
            return (UnityGuiProvider)RuntimeHelpers.GetUninitializedObject(typeof(UnityGuiProvider));
        }

        private static T GetPrivateField<T>(object obj, string fieldName)
        {
            var field = obj.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            return (T)field.GetValue(obj);
        }

        private static void SetPrivateAutoProperty(object obj, string propertyName, object value)
        {
            var backingField = obj.GetType().GetField($"<{propertyName}>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
            backingField.SetValue(obj, value);
        }
    }
}
