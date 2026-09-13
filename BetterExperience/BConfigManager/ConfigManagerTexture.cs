using BetterExperience.BLogSpace;
using System;
using System.Collections.Generic;
using UnityModBase.HConfigSpace;
using UnityModBase.HTranslatorSpace;

namespace BetterExperience.BConfigManager
{
    public static partial class ConfigManager
    {
        // 贴图配置影响资源加载阶段；替换贴图开关关闭时不会扫描外部图片目录。
        public static ConfigEntry<bool> EnableMosaic { get; private set; }
        public static ConfigEntry<bool> EnableReplaceTexture { get; private set; }
        public static ConfigEntry<bool> EnableSensitivities { get; private set; }
        public static ConfigEntry<bool> EnableReplacePortrait { get; private set; }

        // 立绘包启用列表：每行 (包 id, 是否启用)，配置界面展开后逐行显示 id 文本框和启用开关；扫描会把新发现的包自动追加成行。
        public static ConfigEntry<List<(string Id, bool Enabled)>> EnabledPortraitPacks { get; private set; }

        internal const string SectionTexture = "Texture";

        /// <summary>
        /// 初始化马赛克和外部贴图替换相关配置。
        /// </summary>
        public static void InitializeTexture()
        {
            try
            {
                Config.CreateTable(SectionTexture, new Translator(chinese: "贴图", english: "Texture"));

                EnableReplacePortrait = Config.Bind(
                    SectionTexture, nameof(EnableReplacePortrait), false,
                    new Translator(chinese: "启用立绘附件替换", english: "Enable Portrait Attachments"),
                    new Translator(chinese: "自动扫描 ReplaceTexture 中的 .portrait.json 清单，保留原骨骼和动画。修改在下一次立绘切换时生效，使用刷新贴图热键可立即生效。",
                        english: "Auto-scan .portrait.json manifests from ReplaceTexture, preserving original bones and animations. Changes apply at the next portrait switch, or immediately via the texture refresh hotkey."));
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
                EnabledPortraitPacks = Config.Bind(
                    SectionTexture, nameof(EnabledPortraitPacks), new List<(string, bool)>(),
                    new Translator(chinese: "立绘包列表", english: "Portrait Packs"),
                    new Translator(chinese: "每行一个立绘包：id 旁边的开关决定是否启用。插件自动扫描 ReplaceTexture，" +
                                             "扫描到的新包会自动追加到列表末尾（默认关闭）。同一目标只能启用一个包：" +
                                             "同时启用多个同目标包会冲突，相关包全部不生效并记录错误。文件修改后使用刷新贴图热键。",
                        english: "One portrait pack per row; the toggle next to the id decides whether it is enabled. " +
                                 "The plugin auto-scans ReplaceTexture and appends newly found packs at the end of the list (disabled by default). " +
                                 "Only one pack per target may be enabled: enabling several packs with the same target conflicts, none of them applies, and errors are logged. " +
                                 "Use the texture refresh hotkey after changing pack files."));
            }
            catch (Exception ex)
            {
                BLog.Error("Failed to initialize config manager. for texture", ex);
            }
        }
    }
}
