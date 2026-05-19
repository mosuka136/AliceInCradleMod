using BetterExperience.BConfigManager;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace BetterExperience.Patches.ReplaceTexture
{
    /// <summary>
    /// 管理外部替换贴图的加载、缓存与替换。
    /// 该类型只处理磁盘图片到 Unity <see cref="Texture2D"/>/<see cref="Sprite"/> 的运行时替换，不负责具体 Harmony 拦截点。
    /// </summary>
    public static class TextureManager
    {
        private static string _imagePath;
        private static string _sensitiveImagePath;
        private static string[] _supportExtensions;

        // 键使用不带扩展名的文件名，因此不同目录下同名图片会冲突并被跳过。
        private static readonly Dictionary<string, Texture2D> _imageInfos = new Dictionary<string, Texture2D>();

        /// <summary>
        /// 初始化贴图缓存。
        /// </summary>
        /// <param name="imagePath">普通替换贴图根目录。</param>
        /// <param name="sensitiveImagePath">敏感贴图子目录，关闭敏感内容时会被排除。</param>
        /// <param name="supportedExtensions">允许扫描的扩展名列表。</param>
        public static void Initialize(string imagePath, string sensitiveImagePath, string[] supportedExtensions)
        {
            try
            {
                _imagePath = imagePath;
                _sensitiveImagePath = sensitiveImagePath;
                _supportExtensions = supportedExtensions;

                if (!ConfigManager.EnableReplaceTexture.Value)
                    return;

                if (!Directory.Exists(imagePath))
                    Directory.CreateDirectory(imagePath);
                if (!Directory.Exists(sensitiveImagePath))
                    Directory.CreateDirectory(sensitiveImagePath);

                var imageFiles = Directory.GetFiles(imagePath, "*.*", SearchOption.AllDirectories)
                    .Where(f => supportedExtensions.Any(s => f.EndsWith(s, StringComparison.OrdinalIgnoreCase)));

                if (!ConfigManager.EnableSensitivities.Value)
                {
                    // 敏感内容开关关闭时，只排除敏感目录下的文件；普通目录中的同名文件仍可作为替换资源。
                    imageFiles = imageFiles.Where(f => !f.StartsWith(sensitiveImagePath, StringComparison.OrdinalIgnoreCase));
                }

                foreach (var file in imageFiles)
                {
                    var fileWithoutExtension = Path.GetFileNameWithoutExtension(file);

                    if (string.IsNullOrEmpty(fileWithoutExtension))
                        continue;

                    if (_imageInfos.ContainsKey(fileWithoutExtension))
                    {
                        HLog.Warn($"Duplicate image name found: {fileWithoutExtension}. Skipping {file}.");
                        continue;
                    }

                    var image = CreateTexture(file);
                    if (image == null)
                        continue;
                    _imageInfos[fileWithoutExtension] = image;
                    HLog.Info($"Loaded image: {fileWithoutExtension} from {file}");
                }
                
                HLog.Info($"Initialized ImageManager with {_imageInfos.Count} images.");
            }
            catch (Exception ex)
            {
                HLog.Error("Failed to initialize ImageManager.", ex);
            }
        }

        /// <summary>
        /// 释放已缓存贴图并按上次初始化参数重新扫描磁盘。
        /// </summary>
        public static void Reload()
        {
            DestroyAllTextures();

            if (!string.IsNullOrEmpty(_imagePath) && !string.IsNullOrEmpty(_sensitiveImagePath) && _supportExtensions != null)
            {
                Initialize(_imagePath, _sensitiveImagePath, _supportExtensions);
            }
        }

        private static void DestroyAllTextures()
        {
            foreach (var texture in _imageInfos.Values)
            {
                if (texture != null)
                    UnityEngine.Object.Destroy(texture);
            }
            _imageInfos.Clear();
        }

        public static Texture2D CreateTexture(string imageName)
        {
            if (string.IsNullOrEmpty(imageName))
                return null;

            if (!CheckFileValid(imageName))
            {
                HLog.Warn($"Invalid image file: {imageName}.");
                return null;
            }

            try
            {
                var bytes = File.ReadAllBytes(imageName);

                var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                if (texture.LoadImage(bytes))
                {
                    return texture;
                }
                else
                {
                    HLog.Warn($"Failed to load image as Texture: {imageName}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                HLog.Error($"Failed to create Texture from image: {imageName}", ex);
                return null;
            }
        }

        private static bool CheckFileValid(string filePath)
        {
            if (!File.Exists(filePath))
                return false;

            try
            {
                var extension = Path.GetExtension(filePath);
                if (_supportExtensions == null || !_supportExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
                    return false;

                using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    // PNG 文件签名为 8 字节：89 50 4E 47 0D 0A 1A 0A
                    var required = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
                    if (fs.Length < required.Length)
                        return false;

                    var buffer = new byte[required.Length];
                    var read = fs.Read(buffer, 0, buffer.Length);
                    if (read != buffer.Length)
                        return false;

                    for (int i = 0; i < required.Length; i++)
                    {
                        if (buffer[i] != required[i])
                            return false;
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                HLog.Error($"Failed to validate image file '{filePath}'", ex);
                return false;
            }
        }

        public static Texture2D GetReplaceTexture(string textureName)
        {
            if (string.IsNullOrEmpty(textureName))
                return null;

            if (!_imageInfos.TryGetValue(textureName, out var texture))
                return null;

            return texture;
        }

        /// <summary>
        /// 尝试把目标 Unity 对象替换为外部贴图或由外部贴图创建的 Sprite。
        /// </summary>
        /// <param name="name">替换资源名，对应缓存中的无扩展名文件名。</param>
        /// <param name="type">原始资源声明类型。</param>
        /// <param name="destination">待替换对象；成功时会被改写为新对象。</param>
        /// <returns>是否完成替换。</returns>
        public static bool TryReplace(string name, Type type, ref UnityEngine.Object destination)
        {
            if (string.IsNullOrEmpty(name) || type == null || destination == null)
                return false;

            var replaceTexture = GetReplaceTexture(name);
            if (replaceTexture == null)
                return false;

            if (type == typeof(Texture2D) || destination is Texture2D)
            {
                if (replaceTexture == destination)
                    return false;

                CopyTextureProperties(destination as Texture, replaceTexture);
                destination = replaceTexture;

                return true;
            }
            else if (type == typeof(Sprite) || destination is Sprite)
            {
                if (replaceTexture == destination)
                    return false;

                destination = Sprite.Create(replaceTexture, new Rect(0, 0, replaceTexture.width, replaceTexture.height), new Vector2(0.5f, 0.5f));
                destination.name = name;

                return true;
            }

            return false;
        }

        /// <summary>
        /// 将原贴图的采样和隐藏标志复制到替换贴图。
        /// 这样可以让外部贴图尽量保持原资源在 Unity 中的显示行为。
        /// </summary>
        public static void CopyTextureProperties(Texture source, Texture destination)
        {
            if (source == null || destination == null)
                return;

            destination.name = source.name;
            destination.filterMode = source.filterMode;
            destination.wrapMode = source.wrapMode;
            destination.wrapModeV = source.wrapModeV;
            destination.wrapModeW = source.wrapModeW;
            destination.wrapModeU = source.wrapModeU;
            destination.anisoLevel = source.anisoLevel;
            destination.mipMapBias = source.mipMapBias;
            destination.hideFlags = source.hideFlags;
        }
    }
}
