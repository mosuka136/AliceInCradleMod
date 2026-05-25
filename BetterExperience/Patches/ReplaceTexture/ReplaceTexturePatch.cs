using BetterExperience.BConfigManager;
using BetterExperience.HClassAttribute;
using BetterExperience.HLogSpace;
using BetterExperience.Patches.ReplaceTexture;
using HarmonyLib;
using nel;
using System;
using System.Collections.Generic;
using UnityEngine;
using XX;

namespace BetterExperience.Patches
{
    public partial class HPatches
    {
        /// <summary>
        /// 在游戏贴图加载/清理路径上替换外部图片资源。
        /// 该补丁分别处理普通 MTI 图片和 Spine 渲染贴图，并缓存原始贴图以支持热键刷新时恢复。
        /// </summary>
        [HarmonyPatch]
        public class ReplaceTexturePatch
        {
            private static bool _initialized = false;

            // Spine 贴图需要在 cleanExecute 后重新贴图；普通图片需要保留 MTIOneImage 到原始 Texture 的映射。
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
                HLog.Debug($"{nameof(ReplaceTexturePatch)} initialized.");
            }

            public static void Update()
            {
                if (ConfigManager.EnableReplaceTexture?.Value != true)
                    return;

                if (ConfigManager.FlushTextureHotkey?.Value?.WasPressedThisFrame() == true)
                {
                    try
                    {
                        // 刷新时先恢复原资源，再重新加载外部图片，避免把已替换贴图当作下一轮的“原图”。
                        RestoreOriginalTexture();

                        TextureManager.Reload();

                        foreach (var texture in new List<BetobetoManager.SvTexture>(_spineTexture))
                        {
                            texture?.cleanExecute();
                        }

                        foreach (var texture in _pictureTexture)
                        {
                            TryReplace(texture.Value, texture.Key);
                        }

                        HLog.Info("Textures flushed.");
                    }
                    catch (Exception ex)
                    {
                        HLog.Error($"Unexpected error while flushing textures.", ex);
                    }
                }
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(BetobetoManager.SvTexture), nameof(BetobetoManager.SvTexture.cleanExecute))]
            public static void CleanExecutePostfix(BetobetoManager.SvTexture __instance)
            {
                try
                {
                    if (!ConfigManager.EnableReplaceTexture.Value)
                        return;

                    if (__instance.MtiImage0 == null || __instance.MtiImage0.Image == null)
                    {
                        HLog.Debug("SvTexture has no image.");
                        return;
                    }

                    if (!_spineTexture.Contains(__instance))
                        _spineTexture.Add(__instance);

                    var imageName = __instance.MtiImage0.Image.name;
                    var image = TextureManager.GetReplaceTexture(imageName);
                    if (image == null)
                    {
                        HLog.Debug("No replacement texture found for " + imageName);
                        return;
                    }

                    if (!_originalSpineTexture.ContainsKey(__instance))
                        _originalSpineTexture[__instance] = __instance.MtiImage0.Image;

                    TryReplace(__instance, image);
                    HLog.Info($"SvTexture {imageName} replaced.");
                }
                catch (Exception ex)
                {
                    HLog.Error($"Unexpected error in {nameof(ReplaceTexturePatch)}", ex);
                }
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(MTI), nameof(MTI.LoadContainerOneImage))]
            public static void LoadContainerOneImagePostfix(MTIOneImage __result, string asset_key, string load_key, string image_key)
            {
                try
                {
                    if (!ConfigManager.EnableReplaceTexture.Value)
                        return;

                    _pictureTexture[asset_key] = __result;

                    HLog.Debug($"Try replace texture for {asset_key}.");
                    TryReplace(__result, asset_key);
                }
                catch (Exception ex)
                {
                    HLog.Error($"Unexpected error in {nameof(ReplaceTexturePatch)}", ex);
                }
            }

            public static void TryReplace(MTIOneImage mti, string asset_key)
            {
                if (mti == null)
                    return;

                var split = asset_key.Split('/');
                if (split.Length < 2)
                    return;
                // 游戏资源键通常形如 "目录/图片名"，替换文件只使用图片名匹配。
                var name = split[1];

                var texture = TextureManager.GetReplaceTexture(name);
                if (texture == null)
                    return;

                var image = Traverse.Create(mti).Field<MImage>("LImage_").Value;
                if (image == null)
                    return;

                if (image.Tx == null || image.Tx == texture)
                    return;

                if (!_originalPictureTexture.ContainsKey(mti))
                    _originalPictureTexture[mti] = image.Tx;

                TextureManager.CopyTextureProperties(image.Tx, texture);
                image.Tx = texture;

                HLog.Info($"ReplaceTexture: {name}");
            }

            public static void TryReplace(BetobetoManager.SvTexture svTexture, Texture image)
            {
                if (svTexture == null || svTexture.MtiImage0 == null || svTexture.MtiImage0.Image == null)
                    return;

                if (image == null)
                    return;

                TextureManager.CopyTextureProperties(svTexture.MtiImage0.Image, image);

                var Base = svTexture.getRendered();
                BLIT.PasteTo(Base, image, Base.width * 0.5f, Base.height * 0.5f, 1f);
            }

            public static void RestoreOriginalTexture()
            {
                foreach (var texture in _originalSpineTexture)
                {
                    try
                    {
                        TryReplace(texture.Key, texture.Value);
                    }
                    catch (Exception ex)
                    {
                        HLog.Error($"Error restoring original spine texture for {texture.Key.MtiImage0.Image.name}", ex);
                    }
                }

                foreach (var texture in _originalPictureTexture)
                {
                    var image = Traverse.Create(texture.Key).Field<MImage>("LImage_").Value;
                    if (image == null)
                        continue;

                    image.Tx = texture.Value;
                }
            }
        }
    }
}
