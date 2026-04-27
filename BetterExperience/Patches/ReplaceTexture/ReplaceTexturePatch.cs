using BetterExperience.BConfigManager;
using BetterExperience.HClassAttribute;
using BetterExperience.Patches.ReplaceTexture;
using HarmonyLib;
using nel;
using System.Collections.Generic;
using UnityEngine;
using XX;

namespace BetterExperience.Patches
{
    public partial class HPatches
    {
        [HarmonyPatch]
        public class ReplaceTexturePatch
        {
            private static bool _initialized = false;

            private static readonly List<BetobetoManager.SvTexture> _spineTexture = new List<BetobetoManager.SvTexture>();
            private static readonly Dictionary<string, MTIOneImage> _pictureTexture = new Dictionary<string, MTIOneImage>();
            private static readonly Dictionary<MTIOneImage, Texture> _originalPictureTexture = new Dictionary<MTIOneImage, Texture>();
            private static readonly Dictionary<BetobetoManager.SvTexture, Texture> _originalSpineTexture = new Dictionary<BetobetoManager.SvTexture, Texture>();

            [InitializeOnGameBoot]
            public static void Initialize()
            {
                if (_initialized)
                    return;

                FrameUpdateManager.OnFrameUpdate += Update;

                _initialized = true;
            }

            public static void Update()
            {
                if (!ConfigManager.EnableReplaceTexture.Value)
                    return;

                if (ConfigManager.FlushTextureHotkey.Value.WasPressedThisFrame())
                {
                    RestoreOriginalTexture();

                    TextureManager.Instance.Reload();

                    foreach (var texture in new List<BetobetoManager.SvTexture>(_spineTexture))
                    {
                        texture?.cleanExecute();
                    }

                    foreach (var texture in _pictureTexture)
                    {
                        TryReplace(texture.Value, texture.Key);
                    }
                }
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(BetobetoManager.SvTexture), nameof(BetobetoManager.SvTexture.cleanExecute))]
            public static void CleanExecutePostfix(BetobetoManager.SvTexture __instance)
            {
                if (!ConfigManager.EnableReplaceTexture.Value)
                    return;

                if (__instance.MtiImage0 == null || __instance.MtiImage0.Image == null)
                    return;

                if (!_spineTexture.Contains(__instance))
                    _spineTexture.Add(__instance);

                var image = TextureManager.Instance.GetReplaceTexture(__instance.MtiImage0.Image.name);
                if (image == null)
                    return;

                if (!_originalSpineTexture.ContainsKey(__instance))
                    _originalSpineTexture[__instance] = __instance.MtiImage0.Image;

                TryReplace(__instance, image);
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(MTI), nameof(MTI.LoadContainerOneImage))]
            public static void LoadContainerOneImagePostfix(MTIOneImage __result, string asset_key, string load_key, string image_key)
            {
                if (!ConfigManager.EnableReplaceTexture.Value)
                    return;

                if (!_pictureTexture.ContainsKey(asset_key))
                    _pictureTexture[asset_key] = __result;

                TryReplace(__result, asset_key);
            }

            public static void TryReplace(MTIOneImage mti, string asset_key)
            {
                if (mti == null)
                    return;

                var split = asset_key.Split('/');
                if (split.Length < 2)
                    return;
                var name = split[1];

                var texture = TextureManager.Instance.GetReplaceTexture(name);
                if (texture == null)
                    return;

                var image = Traverse.Create(mti).Field<MImage>("LImage_").Value;
                if (image == null)
                    return;

                if (image.Tx == null || image.Tx == texture)
                    return;

                if (!_originalPictureTexture.ContainsKey(mti))
                    _originalPictureTexture[mti] = image.Tx;

                TextureManager.Instance.CopyTextureProperties(image.Tx, texture);
                image.Tx = texture;

                HLog.Info($"ReplaceTexture: {name}");
            }

            public static void TryReplace(BetobetoManager.SvTexture svTexture, Texture image)
            {
                if (svTexture == null || svTexture.MtiImage0 == null || svTexture.MtiImage0.Image == null)
                    return;

                if (image == null)
                    return;

                TextureManager.Instance.CopyTextureProperties(svTexture.MtiImage0.Image, image);

                var Base = svTexture.getRendered();
                BLIT.PasteTo(Base, image, Base.width * 0.5f, Base.height * 0.5f, 1f);

                HLog.Info($"ReplaceTexture: {svTexture.MtiImage0.Image.name}");
            }

            public static void RestoreOriginalTexture()
            {
                foreach (var texture in _originalSpineTexture)
                {
                    TryReplace(texture.Key, texture.Value);
                }

                foreach (var texture in _originalPictureTexture)
                {
                    var image = Traverse.Create(texture.Key).Field<MImage>("LImage_").Value;
                    if (image == null)
                        return;

                    image.Tx = texture.Value;
                }
            }
        }
    }
}
