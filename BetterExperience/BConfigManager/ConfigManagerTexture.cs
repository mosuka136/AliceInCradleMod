using BetterExperience.HConfigSpace;
using BetterExperience.HLogSpace;
using BetterExperience.HTranslatorSpace;
using System;

namespace BetterExperience.BConfigManager
{
    public partial class ConfigManager
    {
        // 贴图配置影响资源加载阶段；替换贴图开关关闭时不会扫描外部图片目录。
        public static ConfigEntry<bool> EnableMosaic { get; private set; }
        public static ConfigEntry<bool> EnableReplaceTexture { get; private set; }
        public static ConfigEntry<bool> EnableSensitivities { get; private set; }

        private const string SectionTexture = "Texture";

        /// <summary>
        /// 初始化马赛克和外部贴图替换相关配置。
        /// </summary>
        public static void InitializeTexture()
        {
            try
            {
                Config.CreateTable(SectionTexture, new Translator(chinese: "贴图", english: "Texture"));

                EnableMosaic = Config.Bind(
                    SectionTexture,
                    nameof(EnableMosaic),
                    false,
                    new Translator(chinese: "启用马赛克效果", english: "Enable Mosaic"),
                    new Translator(
                        chinese: "启用马赛克效果。",
                        english: "Enable mosaic effect."
                    )
                    );
                EnableReplaceTexture = Config.Bind(
                    SectionTexture,
                    nameof(EnableReplaceTexture),
                    false,
                    new Translator(chinese: "启用替换贴图", english: "Enable Replace Texture"),
                    new Translator(
                        chinese: "启用替换贴图。将使用 BetterExperience\\ReplaceTexture 文件夹中的贴图替换原始贴图。\n" +
                        "请确保需要替换的文件和被替换的文件名相同。支持的文件格式为png文件，后缀可为.png或.btep。",
                        english: "Enable replace texture. " +
                        "It will use the texture from the BetterExperience\\ReplaceTexture folder to replace the original texture.\n" +
                        "Please ensure that the file to be replaced has the same name as the original file.\n" +
                        "Supported file formats are PNG files, with extensions .png or .btep."
                    )
                    );
                EnableSensitivities = Config.Bind(
                    SectionTexture,
                    nameof(EnableSensitivities),
                    true,
                    new Translator(chinese: "启用敏感内容贴图", english: "Enable Sensitivities"),
                    new Translator(
                        chinese: "启用敏感内容贴图。若关闭，将不会加载 BetterExperience\\ReplaceTexture\\Sensitive 文件夹中的贴图来替换原始贴图。",
                        english: "Enable sensitivities. If disabled, textures in the BetterExperience\\ReplaceTexture\\Sensitive folder will not be loaded to replace the original textures."
                    )
                    );
            }
            catch (Exception ex)
            {
                HLog.Error("Failed to initialize config manager. for texture", ex);
            }
        }
    }
}
