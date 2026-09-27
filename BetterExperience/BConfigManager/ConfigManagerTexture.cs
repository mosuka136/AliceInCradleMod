using BetterExperience.BLogSpace;
using System;
using System.Collections.Generic;
using UnityModBase.HConfigSpace;
using UnityModBase.HTranslatorSpace;

namespace BetterExperience.BConfigManager
{
    public static partial class ConfigManager
    {
        // 资源替换配置影响资源加载阶段；总开关关闭时仅保留清单选项，不激活外部资源。
        public static ConfigEntry<bool> EnableMosaic { get; private set; }
        public static ConfigEntry<bool> EnableResourceReplacement { get; private set; }
        public static ConfigEntry<bool> EnableSensitivities { get; private set; }
        public static ConfigEntry<List<(string Id, bool Enabled)>> EnabledReplacementPacks { get; private set; }

        internal const string SectionTexture = "Texture";

        /// <summary>
        /// 初始化马赛克和外部贴图替换相关配置。
        /// </summary>
        public static void InitializeTexture()
        {
            try
            {
                Config.CreateTable(SectionTexture, new Translator(chinese: "贴图", english: "Texture"));

                EnableResourceReplacement = Config.Bind(
                    SectionTexture, nameof(EnableResourceReplacement), false,
                    new Translator(chinese: "启用资源替换", english: "Enable Resource Replacement"),
                    new Translator(
                        chinese: "扫描 ReplaceTexture 中的 v2 .replacement.json 资源包。资源包可按列表顺序分层替换普通图片、Spine 图集、骨骼、插槽、皮肤、附件、约束、事件和动画；靠后的包优先。",
                        english: "Scan v2 .replacement.json packs in ReplaceTexture. Packs layer in list order and can replace regular images, Spine atlases, bones, slots, skins, attachments, constraints, events, and animations; later packs have higher priority."));
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
                EnableSensitivities = Config.Bind(
                    SectionTexture,
                    nameof(EnableSensitivities),
                    true,
                    new Translator(chinese: "启用敏感内容资源", english: "Enable Sensitivities"),
                    new Translator(
                        chinese: "启用 Sensitive 目录中的资源包。若关闭，其中的清单及全部依赖会立即失去替换授权。",
                        english: "Enable replacement packs under Sensitive. If disabled, their manifests and all dependencies immediately lose replacement authorization."
                        )
                    );
                EnabledReplacementPacks = Config.Bind(
                    SectionTexture, nameof(EnabledReplacementPacks), new List<(string, bool)>(),
                    new Translator(chinese: "资源替换包列表", english: "Replacement Packs"),
                    new Translator(
                        chinese: "每行一个资源包。启用包按当前行序组合，越靠后优先级越高；同一目标的多个包不会自动互斥。新启用立绘包加载完成后，会先预览对应姿态约 2 秒，再恢复。修改资源文件后使用刷新贴图热键。",
                        english: "One replacement pack per row. Enabled packs compose in row order, with later rows taking priority; packs targeting the same resource remain enabled together. Once loaded, newly enabled portrait packs preview a matching pose for about 2 seconds, then restore the previous pose, lock and normal ordering. Use the texture refresh hotkey after editing resource files."));
            }
            catch (Exception ex)
            {
                BLog.Error("Failed to initialize config manager. for texture", ex);
            }
        }
    }
}
