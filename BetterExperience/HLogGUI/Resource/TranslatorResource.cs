using BetterExperience.HTranslatorSpace;

namespace BetterExperience.HLogGUI.Resource
{
    public static class TranslatorResource
    {
        public static readonly Translator Title = new Translator("日志", "Log");
        public static readonly Translator IdTopBar = new Translator("ID", "ID");
        public static readonly Translator TimestampTopBar = new Translator("时间戳", "Timestamp");
        public static readonly Translator ThreadIdTopBar = new Translator("线程ID", "Thread ID");
        public static readonly Translator FrameTopBar = new Translator("帧数", "Frame");
        public static readonly Translator SceneTopBar = new Translator("场景", "Scene");
        public static readonly Translator LevelTopBar = new Translator("等级", "Level");
        public static readonly Translator MessageTopBar = new Translator("消息", "Message");
        public static readonly Translator FileTopBar = new Translator("文件", "File");
        public static readonly Translator LineTopBar = new Translator("行号", "Line");
        public static readonly Translator MemberTopBar = new Translator("成员", "Member");
        public static readonly Translator ExceptionTopBar = new Translator("异常", "Exception");
        public static readonly Translator LastRepeatTimeTopBar = new Translator("最后重复时间", "Last Repeat Time");
        public static readonly Translator RepeatCountTopBar = new Translator("重复次数", "Repeat Count");
        public static readonly Translator MiscMenu = new Translator("杂项", "Misc");
        public static readonly Translator CopyLog = new Translator("复制日志", "Copy Log");
    }
}
