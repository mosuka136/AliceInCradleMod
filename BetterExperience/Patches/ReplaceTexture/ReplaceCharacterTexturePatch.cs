using BetterExperience.BConfigManager;
using BetterExperience.Patches.ReplaceTexture;
using HarmonyLib;
using nel;
using System.Collections.Generic;
using XX;

namespace BetterExperience.Patches
{
    public partial class HPatches
    {
        [HarmonyPatch]
        public class ReplaceCharacterTexturePatch
        {
            private static HotkeyInputSystem _flushTextureHotkey = null;
            private static readonly List<BetobetoManager.SvTexture> _texture = new List<BetobetoManager.SvTexture>();

            [HarmonyPostfix]
            [HarmonyPatch(typeof(FrameUpdateBooster), nameof(FrameUpdateBooster.Awake))]
            public static void Initialize()
            {
                FrameUpdateBooster.Instance.OnFrameUpdate += Update;

                ConfigManager.FlushTextureHotkey.OnValueChanged += (s, e) =>
                {
                    _flushTextureHotkey = null;
                };
            }

            public static void Update()
            {
                if (!ConfigManager.EnableReplaceTexture.Value)
                    return;

                if (_flushTextureHotkey == null)
                {
                    var h = ConfigManager.FlushTextureHotkey.Value;
                    if (!HotkeyInputSystem.TryParse(h, out _flushTextureHotkey))
                    {
                        HLog.Warn("Invalid Hotkey: " + h);

                        h = "Ctrl+T";
                        HotkeyInputSystem.TryParse(h, out _flushTextureHotkey);
                        HLog.Info("Flush texture hotkey set: " + h);
                    }
                }

                if (_flushTextureHotkey != null && _flushTextureHotkey.IsValid && _flushTextureHotkey.WasPressedThisFrame())
                {
                    TextureManager.Instance.Reload();

                    foreach (var texture in _texture)
                    {
                        texture.cleanExecute();
                    }
                }

            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(BetobetoManager.SvTexture), nameof(BetobetoManager.SvTexture.cleanExecute))]
            public static void CleanExecutePostfix(BetobetoManager.SvTexture __instance)
            {
                if (!ConfigManager.EnableReplaceTexture.Value)
                    return;

                if (!_texture.Contains(__instance))
                    _texture.Add(__instance);

                var image = TextureManager.Instance.GetReplaceTexture(__instance.MtiImage0.Image.name);
                if (image == null)
                    return;

                TextureManager.Instance.CopyTextureProperties(__instance.MtiImage0.Image, image);

                var Base = __instance.getRendered();
                BLIT.PasteTo(Base, image, Base.width * 0.5f, Base.height * 0.5f, 1f);
            }
        }
    }
}
