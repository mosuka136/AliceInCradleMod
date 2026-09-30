using BetterExperience.Patches;
using HarmonyLib;
using nel;
using System.Reflection;

namespace BetterExperience.Test.Patches
{
    public class NoMpDamagePatchTests
    {
        [Fact]
        public void Prefix_TargetsCurrentGameMpDamageOverload()
        {
            var prefix = typeof(HPatches.NoMpDamagePatch).GetMethod(nameof(HPatches.NoMpDamagePatch.Prefix));
            var target = prefix.GetCustomAttribute<HarmonyPatch>().info;
            var method = target.declaringType.GetMethod(target.methodName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly,
                null, target.argumentTypes, null);

            Assert.NotNull(method);
            Assert.Equal(typeof(PR), method.DeclaringType);
            Assert.Equal(typeof(int), method.ReturnType);
            var parameters = method.GetParameters();
            Assert.True(parameters[0].IsOut);
            Assert.Equal("gauge_break", parameters[0].Name);
            Assert.Equal("use_cusion", parameters[^1].Name);
            Assert.Equal(typeof(bool), parameters[^1].ParameterType);
        }
    }
}
