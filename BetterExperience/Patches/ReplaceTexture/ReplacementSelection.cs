using System;
using System.Collections.Generic;
using System.Linq;

namespace BetterExperience.Patches.ReplaceTexture
{
    /// <summary>一次已应用的开关快照；选择变化只比较目录中的目标，不读取磁盘。</summary>
    internal sealed class ReplacementSelection
    {
        private readonly List<ReplacementTarget> targets = new List<ReplacementTarget>();
        private readonly Dictionary<string, List<ReplacementTarget>> byIdentity = new Dictionary<string, List<ReplacementTarget>>(StringComparer.Ordinal);
        private readonly HashSet<string> invalid = new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<string> unidentified = new HashSet<string>(StringComparer.Ordinal);
        private static readonly ReplacementTarget[] Empty = new ReplacementTarget[0];
        internal readonly bool Enabled;
        internal readonly bool AllowSensitive;
        internal readonly string[] EnabledIds;

        internal ReplacementSelection(ReplacementCatalog catalog, IEnumerable<string> ids, bool enabled, bool allowSensitive)
        {
            Enabled = enabled;
            AllowSensitive = allowSensitive;
            EnabledIds = (ids ?? Enumerable.Empty<string>()).Distinct(StringComparer.Ordinal).ToArray();
            if (!enabled) return;
            var packages = catalog.Packages.ToDictionary(package => package.Id, StringComparer.Ordinal);
            foreach (string id in EnabledIds)
            {
                if (!packages.TryGetValue(id, out var package) || (package.Sensitive && !allowSensitive)) continue;
                foreach (string identity in package.InvalidTargetIdentities) invalid.Add(identity);
                if (package.HasUnidentifiedTargetErrors) unidentified.Add(id);
                foreach (var target in package.Targets)
                {
                    targets.Add(target);
                    if (!byIdentity.TryGetValue(target.Identity, out var layers))
                        byIdentity.Add(target.Identity, layers = new List<ReplacementTarget>());
                    layers.Add(target);
                }
            }
        }

        internal IReadOnlyList<ReplacementTarget> Layers(string identity) =>
            byIdentity.TryGetValue(identity, out var layers) ? (IReadOnlyList<ReplacementTarget>)layers : Empty;

        internal ReplacementTarget Texture(string loader, string key, string imageKey, string objectType) =>
            targets.LastOrDefault(target => target.Type == "texture" && target.Loader == loader
                && (loader == "mti" ? target.AssetKey == key && (target.ImageKey == null || target.ImageKey == imageKey)
                    : target.ResourcePath == key && target.ObjectType == objectType));

        internal bool Invalid(string identity) => invalid.Contains(identity);
        internal List<ReplacementTarget> NewlyEnabledPortraits(ReplacementSelection previous) => targets
            .Where(target => target.Type == "spine" && !Invalid(target.Identity)
                && !previous.Authorizes(new[] { target.Owner })).ToList();
        internal bool Authorizes(IEnumerable<ReplacementPackage> sources) => Enabled
            && sources.Where(source => source != null).All(source => EnabledIds.Contains(source.Id)
                && (!source.Sensitive || AllowSensitive));
        internal bool InvalidTexture(string loader, string key, string imageKey, string objectType) =>
            invalid.Contains(TextureIdentity(loader, key, imageKey, objectType))
            || (loader == "mti" && invalid.Contains(TextureIdentity(loader, key, null, objectType)));

        internal bool SameSpine(ReplacementSelection other, string identity) =>
            Layers(identity).SequenceEqual(other.Layers(identity)) && Invalid(identity) == other.Invalid(identity)
            && unidentified.SetEquals(other.unidentified);

        internal bool SameTexture(ReplacementSelection other, string loader, string key, string imageKey, string objectType) =>
            ReferenceEquals(Texture(loader, key, imageKey, objectType), other.Texture(loader, key, imageKey, objectType))
            && InvalidTexture(loader, key, imageKey, objectType) == other.InvalidTexture(loader, key, imageKey, objectType)
            && unidentified.SetEquals(other.unidentified);

        private static string TextureIdentity(string loader, string key, string imageKey, string objectType) =>
            loader == "mti" ? "texture\nmti\n" + key + "\n" + (imageKey ?? "")
                : "texture\nresources\n" + key + "\n" + objectType;
    }

    internal sealed class ReplacementSelectionDelay
    {
        internal const float Delay = 0.15f;
        private string current;
        private string pending;
        private float due;
        internal bool Waiting => pending != null;

        internal void Reset(string value) { current = value; pending = null; }

        internal bool Ready(string value, float now, bool immediate)
        {
            if (value == current) { pending = null; return false; }
            if (value != pending) { pending = value; due = now + Delay; }
            return immediate || now >= due;
        }
    }
}
