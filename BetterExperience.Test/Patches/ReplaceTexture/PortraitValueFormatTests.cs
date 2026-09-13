using Moq;
using UnityModBase.HEntrySpace;
using UnityModBase.HGuiSpace.Bindings;
using UnityModBase.HGuiSpace.Editor;
using UnityModBase.HGuiSpace.Editor.ValueEditor;
using UnityModBase.HGuiSpace.Resource;
using UnityModBase.HConfigSpace;
using UnityModBase.HProvider;

namespace BetterExperience.Test.Patches.ReplaceTexture
{
    /// <summary>
    /// 探索配置框架对复合元素集合（(string, bool) / EntryValue&lt;string, bool&gt;）的编码与 GUI 编辑支持。
    /// </summary>
    public class PortraitValueFormatTests
    {
        private static ConfigFileResult<string> Encode<T>(T value) where T : class =>
            ConfigFileEntry.EncodeValue(value);

        private static string EncodeOrThrow<T>(T value) where T : class
        {
            var result = Encode(value);
            Assert.True(result.Success, typeof(T).ToString() + " encode failed");
            return result.Value;
        }

        [Fact]
        public void ValueTupleList_RoundTrips()
        {
            string text = EncodeOrThrow(new List<(string, bool)> { ("one", true), ("two", false) });
            Assert.Equal("[(\"one\",True),(\"two\",False)]", text);
            var decoded = ConfigFileEntry.DecodeValue<List<(string, bool)>>(text);
            Assert.True(decoded.Success, "decode failed");
            Assert.Equal(new List<(string, bool)> { ("one", true), ("two", false) }, decoded.Value);
        }

        [Fact]
        public void EntryValueList_EncodingLosesElementBoundaries()
        {
            // EntryValue 平铺为无定界符逗号文本，嵌套进集合后元素边界无法逆解（框架会拒绝绑定这种元素类型）。
            string text = EncodeOrThrow(new List<EntryValue<string, bool>> { new("one", true), new("two", false) });
            Assert.Equal("[\"one\",True,\"two\",False]", text);
            Assert.False(ConfigFileEntry.DecodeValue<List<EntryValue<string, bool>>>(text).Success);
        }

        private static IValueEditor ResolveEditor(Type valueType)
        {
            var gui = new Mock<IUnityGuiProvider>();
            var style = new Mock<IEntryStyleResource>();
            var registry = ValueEditorRegistry.CreateDefault(
                new Mock<IUnityProvider>().Object, gui.Object, style.Object, null);
            var binding = new Mock<IEntryBinding>();
            binding.SetupGet(b => b.ValueType).Returns(valueType);
            binding.SetupGet(b => b.Key).Returns("probe");
            return registry.GetEditor(binding.Object);
        }

        [Fact]
        public void GuiEditor_SelectionForCandidateValueTypes()
        {
            var editors = new Dictionary<Type, string>
            {
                { typeof(List<string>), ResolveEditor(typeof(List<string>)).GetType().Name },
                { typeof(List<(string, bool)>), ResolveEditor(typeof(List<(string, bool)>)).GetType().Name },
                { typeof(List<EntryValue<string, bool>>), ResolveEditor(typeof(List<EntryValue<string, bool>>)).GetType().Name },
            };
            // 输出实际匹配结果供人工判断；断言宽松，仅保证解析不抛异常。
            foreach (var pair in editors) Console.WriteLine($"[probe] {pair.Key} -> {pair.Value}");
            Assert.Contains(typeof(List<string>), editors);
        }
    }
}
