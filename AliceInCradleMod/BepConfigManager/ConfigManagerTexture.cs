using BepInEx.Configuration;

namespace BetterExperience.BepConfigManager
{
    internal sealed partial class ConfigManager
    {
        public static ConfigEntry<bool> EnableMosaic { get; private set; }
        public static ConfigEntry<bool> EnableReplaceTexture { get; private set; }
        public static ConfigEntry<bool> EnableSensitivities { get; private set; }

        private const string SectionTexture = "Texture";

        public static void InitializeTexture()
        {
            var Config = BetterExperience.Instance.Config;

            EnableMosaic = Config.Bind(
                SectionTexture,
                nameof(EnableMosaic),
                false,
                "Enable mosaic effect.\n启用马赛克效果。"
                );
            EnableReplaceTexture = Config.Bind(
                SectionTexture,
                nameof(EnableReplaceTexture),
                false,
                "Enable replace texture. " +
                "It will use the texture from the BetterExperience\\ReplaceTexture folder to replace the original texture.\n" +
                "Please ensure that the file to be replaced has the same name as the original file.\n" +
                "Supported file formats are PNG files, with extensions .png or .btep.\n" +
                "启用替换贴图。将使用 BetterExperience\\ReplaceTexture 文件夹中的贴图替换原始贴图。\n" +
                "请确保需要替换的文件和被替换的文件名相同。支持的文件格式为png文件，后缀可为.png或.btep。"
                );
            EnableSensitivities = Config.Bind(
                SectionTexture,
                nameof(EnableSensitivities),
                true,
                "Enable sensitivities. If disabled, textures in the BetterExperience\\ReplaceTexture\\Sensitive folder will not be loaded to replace the original textures.\n" +
                "启用敏感内容贴图。若关闭，将不会加载 BetterExperience\\ReplaceTexture\\Sensitive 文件夹中的贴图来替换原始贴图。"
                );
        }
    }
}
