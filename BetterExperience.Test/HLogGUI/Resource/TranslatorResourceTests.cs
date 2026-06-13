using BetterExperience.HTranslatorSpace;
using TranslatorResource = BetterExperience.HLogGUI.Resource.TranslatorResource;

namespace BetterExperience.Test.HLogGUI.Resource
{
    public class TranslatorResourceTests
    {
        public static IEnumerable<object[]> ResourceTexts => new[]
        {
            new object[] { TranslatorResource.Title, "日志", "Log" },
            new object[] { TranslatorResource.IdTopBar, "ID", "ID" },
            new object[] { TranslatorResource.TimestampTopBar, "时间戳", "Timestamp" },
            new object[] { TranslatorResource.ThreadIdTopBar, "线程ID", "Thread ID" },
            new object[] { TranslatorResource.FrameTopBar, "帧数", "Frame" },
            new object[] { TranslatorResource.SceneTopBar, "场景", "Scene" },
            new object[] { TranslatorResource.LevelTopBar, "等级", "Level" },
            new object[] { TranslatorResource.MessageTopBar, "消息", "Message" },
            new object[] { TranslatorResource.FileTopBar, "文件", "File" },
            new object[] { TranslatorResource.LineTopBar, "行号", "Line" },
            new object[] { TranslatorResource.MemberTopBar, "成员", "Member" },
            new object[] { TranslatorResource.ExceptionTopBar, "异常", "Exception" },
            new object[] { TranslatorResource.LastRepeatTimeTopBar, "最后重复时间", "Last Repeat Time" },
            new object[] { TranslatorResource.RepeatCountTopBar, "重复次数", "Repeat Count" },
            new object[] { TranslatorResource.MiscMenu, "杂项", "Misc" },
            new object[] { TranslatorResource.CopyLog, "复制日志", "Copy Log" },
            new object[] { TranslatorResource.Copied, "已复制：", "Copied:" },
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
