using Spine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace BetterExperience.Patches.ReplaceTexture
{
    internal sealed class PreparedSpine
    {
        internal byte[] ImageBytes;
        internal string AtlasText;
        internal Atlas Atlas;
        internal SpineCompositionResult Composition;
    }

    /// <summary>只操作文件和独立的托管数据；不得访问 Unity 对象或实时配置。</summary>
    internal static class ReplacementPreparation
    {
        internal static PreparedSpine Spine(string originalJson, string originalAtlas, float originalScale,
            IReadOnlyList<ReplacementTarget> layers, string root, string sensitive, bool allowSensitive, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            Validate(layers, root, sensitive, allowSensitive);
            token.ThrowIfCancellationRequested();
            string atlasPath = layers.Select(layer => layer.AtlasPath).LastOrDefault(path => path != null);
            string imagePath = layers.Select(layer => layer.ImagePath).LastOrDefault(path => path != null);
            string atlasText = atlasPath == null ? originalAtlas : ReplacementResourceIO.ReadText(atlasPath);
            var atlas = PortraitCatalog.ReadAtlas(atlasText);
            if (atlas.Pages.Count != 1) throw new InvalidDataException("Only single-page Spine atlases are supported.");
            token.ThrowIfCancellationRequested();
            byte[] image = imagePath == null ? null : ReplacementResourceIO.ReadBytes(imagePath);
            if (image != null) PortraitCatalog.ValidateImage(image, atlas);
            token.ThrowIfCancellationRequested();
            var composition = SpineComposer.Compose(originalJson, layers, atlas, originalScale, token);
            return new PreparedSpine { ImageBytes = image, AtlasText = atlasText, Atlas = atlas, Composition = composition };
        }

        internal static byte[] Texture(ReplacementTarget layer, string root, string sensitive, bool allowSensitive, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            Validate(new[] { layer }, root, sensitive, allowSensitive);
            var bytes = ReplacementResourceIO.ReadBytes(layer.ImagePath);
            token.ThrowIfCancellationRequested();
            return bytes;
        }

        internal static void Validate(IEnumerable<ReplacementTarget> layers, string root, string sensitive, bool allowSensitive)
        {
            foreach (var target in layers)
            {
                if (target.Owner == null) throw new InvalidDataException("Replacement package is missing.");
                if (target.Owner.Sensitive && !allowSensitive) throw new InvalidDataException("Sensitive content is disabled.");
                foreach (string path in new[] { target.Owner.ManifestPath, target.ImagePath, target.AtlasPath, target.JsonPath }.Where(path => path != null))
                {
                    if (!File.Exists(path)) throw new FileNotFoundException("Replacement dependency was removed.", path);
                    PortraitCatalog.Resolve(root, Path.GetDirectoryName(path), Path.GetFileName(path));
                    if (PortraitCatalog.Within(sensitive, path) != target.Owner.Sensitive)
                        throw new InvalidDataException("Replacement dependencies crossed the Sensitive boundary.");
                }
            }
        }
    }
}
