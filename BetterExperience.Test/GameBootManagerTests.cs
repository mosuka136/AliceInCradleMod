using System.Reflection;
using System.Runtime.Loader;
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

            using (var proxy = IsolatedGameBootManagerProxy.Create())
            {
                proxy.AddOnGameBootHandler(() => invocationCount++);
                proxy.RegisterComponentOnGameBoot(typeof(string));

                // Act
                var exception = Record.Exception(() => proxy.Postfix());

                // Assert
                Assert.Null(exception);
                Assert.Equal(1, invocationCount);
            }
        }

        [Fact]
        public void RegisterComponentOnGameBoot_AbstractComponentType_PostfixSwallowsComponentCreationFailure()
        {
            // Arrange
            var invocationCount = 0;

            using (var proxy = IsolatedGameBootManagerProxy.Create())
            {
                proxy.AddOnGameBootHandler(() => invocationCount++);
                proxy.RegisterComponentOnGameBoot(typeof(AbstractTestComponent));

                // Act
                var exception = Record.Exception(() => proxy.Postfix());

                // Assert
                Assert.Null(exception);
                Assert.Equal(1, invocationCount);
            }
        }

        [Fact]
        public void LoadScenePatch_PrefixAndPostfix_CalledTwiceInvokesCustomHandlerOnlyOnce()
        {
            // Arrange
            var invocationCount = 0;

            using (var proxy = IsolatedGameBootManagerProxy.Create())
            {
                proxy.AddOnGameBootHandler(() => invocationCount++);

                // Act
                var exception = Record.Exception(() =>
                {
                    proxy.Prefix();
                    proxy.Postfix();
                    proxy.Prefix();
                    proxy.Postfix();
                });

                // Assert
                Assert.Null(exception);
                Assert.Equal(1, invocationCount);
            }
        }

        private abstract class AbstractTestComponent : MonoBehaviour
        {
        }

        private sealed class IsolatedGameBootManagerProxy : IDisposable
        {
            private readonly IsolatedAssemblyContext _context;
            private readonly EventInfo _onGameBootEvent;
            private readonly MethodInfo _registerComponentOnGameBootMethod;
            private readonly MethodInfo _prefixMethod;
            private readonly MethodInfo _postfixMethod;

            private IsolatedGameBootManagerProxy(
                IsolatedAssemblyContext context,
                EventInfo onGameBootEvent,
                MethodInfo registerComponentOnGameBootMethod,
                MethodInfo prefixMethod,
                MethodInfo postfixMethod)
            {
                _context = context;
                _onGameBootEvent = onGameBootEvent;
                _registerComponentOnGameBootMethod = registerComponentOnGameBootMethod;
                _prefixMethod = prefixMethod;
                _postfixMethod = postfixMethod;
            }

            public static IsolatedGameBootManagerProxy Create()
            {
                var assemblyPath = typeof(GameBootManager).Assembly.Location;
                var context = new IsolatedAssemblyContext(assemblyPath);
                var assembly = context.LoadFromAssemblyPath(assemblyPath);
                var gameBootManagerType = assembly.GetType("BetterExperience.GameBootManager", throwOnError: true);
                var loadScenePatchType = gameBootManagerType.GetNestedType("LoadScenePatch", BindingFlags.Public);
                var onGameBootEvent = gameBootManagerType.GetEvent("OnGameBoot", BindingFlags.Public | BindingFlags.Static);
                var registerComponentOnGameBootMethod = gameBootManagerType.GetMethod("RegisterComponentOnGameBoot", BindingFlags.Public | BindingFlags.Static);
                var prefixMethod = loadScenePatchType.GetMethod("Prefix", BindingFlags.Public | BindingFlags.Static);
                var postfixMethod = loadScenePatchType.GetMethod("Postfix", BindingFlags.Public | BindingFlags.Static);

                return new IsolatedGameBootManagerProxy(
                    context,
                    onGameBootEvent,
                    registerComponentOnGameBootMethod,
                    prefixMethod,
                    postfixMethod);
            }

            public void AddOnGameBootHandler(Action handler)
            {
                _onGameBootEvent.AddEventHandler(null, handler);
            }

            public void RegisterComponentOnGameBoot(Type type)
            {
                _registerComponentOnGameBootMethod.Invoke(null, new object[] { type });
            }

            public void Prefix()
            {
                _prefixMethod.Invoke(null, null);
            }

            public void Postfix()
            {
                _postfixMethod.Invoke(null, null);
            }

            public void Dispose()
            {
                _context.Unload();
            }
        }

        private sealed class IsolatedAssemblyContext : AssemblyLoadContext
        {
            private readonly string _assemblyDirectory;
            private readonly string _targetAssemblyName;

            public IsolatedAssemblyContext(string assemblyPath)
                : base(true)
            {
                _assemblyDirectory = Path.GetDirectoryName(assemblyPath);
                _targetAssemblyName = Path.GetFileNameWithoutExtension(assemblyPath);
            }

            protected override Assembly Load(AssemblyName assemblyName)
            {
                if (!string.Equals(assemblyName.Name, _targetAssemblyName, StringComparison.Ordinal))
                {
                    var sharedAssembly = AssemblyLoadContext.Default.Assemblies.FirstOrDefault(
                        a => string.Equals(a.GetName().Name, assemblyName.Name, StringComparison.Ordinal));
                    if (sharedAssembly != null)
                    {
                        return sharedAssembly;
                    }
                }

                var candidatePath = Path.Combine(_assemblyDirectory, assemblyName.Name + ".dll");
                if (File.Exists(candidatePath))
                {
                    return LoadFromAssemblyPath(candidatePath);
                }

                return null;
            }
        }
    }
}
