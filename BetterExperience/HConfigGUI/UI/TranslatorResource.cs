using BetterExperience.HTranslatorSpace;

namespace BetterExperience.HConfigGUI.UI
{
    public static class TranslatorResource
    {
        public static readonly Translator Title = new Translator("更好的体验模组配置", "BetterExperience Mod Configurations");
        public static readonly Translator On = new Translator("开启", "On");
        public static readonly Translator Off = new Translator("关闭", "Off");
        public static readonly Translator Reset = new Translator("重置", "Reset");
        public static readonly Translator Changed = new Translator("已修改: ", "Changed: ");
        public static readonly Translator ResetDone = new Translator("已重置: ", "Reset: ");
        public static readonly Translator Record = new Translator("录制", "Record");
        public static readonly Translator Apply = new Translator("应用", "Apply");
        public static readonly Translator Add = new Translator("增加", "Add");
        public static readonly Translator Remove = new Translator("移除", "Remove");
        public static readonly Translator InvalidSliderMetadata = new Translator("无效的滑动条元数据", "Invalid slider metadata");
        public static readonly Translator CanNotHideBeforeEndRecord = new Translator("无法在录制结束前隐藏界面", "Can't hide before end record");
    }
}
