using BepInEx.Configuration;

namespace BetterExperience.BepConfigManager
{
    internal sealed partial class ConfigManager
    {
        public static ConfigEntry<bool> EnableSensitivities { get; private set; }
        public static ConfigEntry<bool> EnableTextureImmediateReload { get; private set; }

        private const string SectionTexture = "Texture";

        public static void InitializeTexture()
        {
            var Config = BetterExperience.Instance.Config;

            EnableSensitivities = Config.Bind(
                SectionTexture,
                nameof(EnableSensitivities),
                true,
                "Enable sensitivities. If disabled, textures in the BetterExperience\\ReplaceTexture\\Sensitive folder will not be loaded to replace the original textures.\n" +
                "启用敏感内容贴图。若关闭，将不会加载 BetterExperience\\ReplaceTexture\\Sensitive 文件夹中的贴图来替换原始贴图。"
                );
            EnableTextureImmediateReload = Config.Bind(
                SectionTexture,
                nameof(EnableTextureImmediateReload),
                false,
                "Enable immediate reload. If enabled, the texture will be reloaded immediately after the texture is changed.\n" +
                "启用立即重载。开启后，贴图发生变化时会立刻重新加载。"
                );
        }
    }
}
