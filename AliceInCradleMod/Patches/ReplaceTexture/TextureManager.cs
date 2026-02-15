using BepInEx;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace BetterExperience.Patches.ReplaceTexture
{
    internal class TextureManager
    {
        private TextureManager()
        {
            
        }

        static TextureManager()
        {
            Instance = new TextureManager();
            Instance.Initialize();
        }

        static public TextureManager Instance { get; private set; }

        private const string RelativeImagePath = "ReplaceTexture";
        public readonly string ImagePath = Path.Combine(Paths.PluginPath, nameof(BetterExperience), RelativeImagePath);
        public readonly string SenstiveImagePath = Path.Combine(Paths.PluginPath, nameof(BetterExperience), RelativeImagePath, "Sensitive");
        public readonly string[] SupportedExtensions = { ".png", ".btep"};

        private Dictionary<string, ImageInfo> _imageInfos = new Dictionary<string, ImageInfo>();

        public void Initialize()
        {
            try
            {
                if (!Directory.Exists(ImagePath))
                    Directory.CreateDirectory(ImagePath);
                if (!Directory.Exists(SenstiveImagePath))
                    Directory.CreateDirectory(SenstiveImagePath);

                var imageFiles = Directory.GetFiles(ImagePath, "*.*", SearchOption.AllDirectories)
                    .Where(f => SupportedExtensions.Any(s => f.EndsWith(s)));

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

                    var imageInfo = new ImageInfo();
                    imageInfo.Path = file;
                    imageInfo.ReplaceTexture = CreateTexture(file, fileWithoutExtension);
                    _imageInfos[fileWithoutExtension] = imageInfo;
                }

                HLog.Info($"Initialized ImageManager with {_imageInfos.Count} images.");
            }
            catch (Exception ex)
            {
                HLog.Error("Failed to initialize ImageManager.", ex);
            }
        }

        public Texture2D CreateTexture(string imageName, string assetName)
        {
            if (string.IsNullOrEmpty(imageName))
                return null;

            if(!CheckFileValid(imageName))
                return null;

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
                    HLog.Error($"Failed to load image as Texture: {imageName}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                HLog.Error($"Failed to create Texture from image: {imageName}", ex);
                return null;
            }
        }

        private bool CheckFileValid(string filePath)
        {
            if (!File.Exists(filePath))
                return false;

            try
            {
                var extension = Path.GetExtension(filePath);
                if (!SupportedExtensions.Contains(extension))
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

        public Texture2D GetReplaceTexture(string imageName)
        {
            if (string.IsNullOrEmpty(imageName))
                return null;

            if (!_imageInfos.TryGetValue(imageName, out var imageInfo))
                return null;

            return imageInfo.ReplaceTexture;
        }

        public void TryReplace(string name, Type type, ref UnityEngine.Object destination)
        {
            var replaceTexture = GetReplaceTexture(name);
            if (replaceTexture == null)
                return;

            if(type == typeof(Texture2D) || destination is Texture2D)
            {
                CopyTextureProperties(destination as Texture, replaceTexture);               
                destination = replaceTexture;
            }
            else if(type == typeof(Sprite) || destination is Sprite)
            {
                destination = Sprite.Create(replaceTexture, new Rect(0, 0, replaceTexture.width, replaceTexture.height), new Vector2(0.5f, 0.5f));
                destination.name = name;
            }
        }

        public void CopyTextureProperties(Texture source, Texture destination)
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

        public class ImageInfo
        {
            public string Path { get; set; }
            public Texture2D ReplaceTexture { get; set; }
        }
    }
}
