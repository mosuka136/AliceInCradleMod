using BetterExperience.HTranslatorSpace;
using TranslatorResource = BetterExperience.HConfigGUI.Resource.TranslatorResource;

namespace BetterExperience.Test.HConfigGUI.Resource
{
    public class TranslatorResourceTests
    {
        public static IEnumerable<object[]> ResourceTexts => new[]
        {
            new object[] { TranslatorResource.Title, "更好的体验", "BetterExperience" },
            new object[] { TranslatorResource.On, "开启", "On" },
            new object[] { TranslatorResource.Off, "关闭", "Off" },
            new object[] { TranslatorResource.Reset, "重置", "Reset" },
            new object[] { TranslatorResource.Changed, "已修改：", "Changed: " },
            new object[] { TranslatorResource.ResetDone, "已重置：", "Reset: " },
            new object[] { TranslatorResource.RecordHotkeyPopupTitle, "录制热键中", "Recording Hotkey" },
            new object[] { TranslatorResource.Record, "录制", "Record" },
            new object[] { TranslatorResource.Apply, "应用", "Apply" },
            new object[] { TranslatorResource.Cancel, "取消", "Cancel" },
            new object[] { TranslatorResource.Add, "增加", "Add" },
            new object[] { TranslatorResource.Remove, "移除", "Remove" },
            new object[] { TranslatorResource.Close, "关闭", "Close" },
            new object[] { TranslatorResource.InvalidSliderMetadata, "无效的滑动条元数据", "Invalid slider metadata" },
        };

        [Theory]
        [MemberData(nameof(ResourceTexts))]
        public void StaticTranslatorResource_HasExpectedChineseAndEnglishText(
            Translator translator,
            string expectedChinese,
            string expectedEnglish)
        {
            // Assert
            Assert.Equal(expectedChinese, translator.Chinese);
            Assert.Equal(expectedEnglish, translator.English);
        }
    }
}
