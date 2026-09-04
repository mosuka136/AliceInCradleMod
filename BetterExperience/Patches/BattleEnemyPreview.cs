using nel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BetterExperience.Patches
{
    internal enum BattleEnemySource { Main, Follower }

    internal sealed class BattleEnemyPreviewEntry
    {
        internal string EnemyKey;
        internal ENEMYID? EnemyId;
        internal bool Overdrive;
        internal ENATTR Attributes;
        internal ENATTR PossibleAttributes;
        internal BattleEnemySource Source;
        internal int Minimum;
        internal int? Maximum;
        internal bool FixedAttributes;
        internal bool Overdriveable;
        internal int AttributeThreshold;
        internal int OverdriveAttributeThreshold;
        internal ENATTR DeclinedAttributes;
        internal string UndeterminedReason;

        internal BattleEnemyPreviewEntry Copy() => (BattleEnemyPreviewEntry)MemberwiseClone();
    }

    internal sealed class BattleEnemyPreviewSnapshot
    {
        internal readonly List<BattleEnemyPreviewEntry> Entries = new List<BattleEnemyPreviewEntry>();
        internal int Minimum;
        internal int? Maximum;
        internal int RandomAddition;
        internal int RandomAttributes;
        internal int ThunderCapacity;
        internal bool Incomplete;
        internal bool DynamicReinforcements;
        internal string UndeterminedReason;
    }

    // 在主线程复制当前游戏数据；预览计算不创建或持有 SummonerPlayer。
    internal sealed class BattleEnemyPreviewContext
    {
        internal float DangerLevel;
        internal int AdditionalCount;
        internal int CountCapAddition;
        internal bool Night;
        internal int ThunderCapacity;
        internal int AttributeDanger;
        internal int AttributeBudget;
        internal int AttributeKindMaximum = 1;
        internal ENEMYID? QuestEnemy;
        internal int QuestMinimum;
        internal ENATTR QuestAttributes;
        internal bool QuestAttributeBudget;
        internal bool SpecialBattle;
        internal Func<string, NDAT.EnemyDescryption> GetEnemyDescription = NDAT.getTypeAndId;
        internal readonly Dictionary<string, string> Variables = new Dictionary<string, string>(StringComparer.Ordinal);
        internal readonly Dictionary<string, double> Values = new Dictionary<string, double>(StringComparer.Ordinal);
    }

    internal static class BattleEnemyPreview
    {
        internal sealed class Spawn
        {
            internal BattleEnemyPreviewEntry Entry;
            internal int Count;
            internal float Weight;
        }

        internal sealed class CountCap
        {
            internal string Key;
            internal int Maximum;
            internal int Used;

            internal bool Matches(BattleEnemyPreviewEntry entry)
            {
                var key = Key;
                if (key.StartsWith("OD_", StringComparison.Ordinal))
                {
                    if (!entry.Overdrive) return false;
                    key = key.Substring(3);
                }
                if (key.StartsWith("!", StringComparison.Ordinal)) return entry.EnemyKey == key.Substring(1);
                return entry.EnemyKey == key || entry.EnemyKey.StartsWith(key + "_", StringComparison.Ordinal);
            }
        }

        internal static BattleEnemyPreviewSnapshot Build(string script, BattleEnemyPreviewContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            var parser = new BattleEnemyPreviewScript(context);
            if (!parser.TryRead(script)) return parser.Candidates(script);
            // 原版在处理数量上限前，由普通体脚本条目设置 CI.odable_enemy_exist。
            int thunder = parser.Spawns.Any(row => !row.Entry.Overdrive && row.Entry.Overdriveable)
                ? Math.Max(0, context.ThunderCapacity) : 0;
            var result = new BattleEnemyPreviewSnapshot
            {
                Incomplete = parser.Incomplete || context.SpecialBattle,
                DynamicReinforcements = parser.DynamicReinforcements,
                RandomAttributes = context.AttributeBudget,
                ThunderCapacity = thunder,
                UndeterminedReason = context.SpecialBattle ? "Special battle replacement or rescue." : parser.UndeterminedReason
            };

            // 先倒序展开脚本条目，再分配共享的随机增补名额。
            foreach (var row in parser.Spawns.AsEnumerable().Reverse())
            {
                var caps = parser.Caps.Where(cap => cap.Matches(row.Entry)).ToArray();
                int available = caps.Aggregate(999, (value, cap) => Math.Min(value, cap.Maximum - cap.Used));
                if (available <= row.Count) row.Weight = 0;
                row.Count = Math.Max(0, Math.Min(row.Count, available));
                foreach (var cap in caps) cap.Used += row.Count;
                var entry = row.Entry.Copy();
                entry.Minimum = row.Count;
                entry.Maximum = row.Count;
                result.Entries.Add(entry);
            }

            int baseCount = result.Entries.Sum(entry => entry.Minimum);
            int pool = Math.Min(299, Math.Max(0, context.AdditionalCount));
            var additions = new List<BattleEnemyPreviewEntry>();
            foreach (var row in parser.Spawns.Where(row => row.Weight > 0))
            {
                var entry = row.Entry.Copy();
                entry.Overdrive = false; // 原版将 %EN_OD 产生的额外副本生成为普通体。
                int maximum = pool;
                foreach (var cap in parser.Caps.Where(cap => cap.Matches(entry)))
                {
                    // 原版增加副本后，只禁用本次选中的脚本条目。
                    // 同种魔物的其他可增补条目，即使已达上限，仍可能再增加一只。
                    maximum = Math.Min(maximum, Math.Max(1, cap.Maximum - cap.Used));
                }
                entry.Minimum = 0;
                entry.Maximum = maximum;
                if (maximum > 0) additions.Add(entry);
            }
            int extraMaximum = Math.Min(pool, additions.Sum(entry => entry.Maximum.Value));
            bool fullPool = additions.Count > 0 && parser.Caps.Count == 0;
            int extraMinimum = fullPool ? pool : 0;
            if (fullPool && additions.All(entry => SameGroup(entry, additions[0])))
                additions[0].Minimum = pool;
            Merge(additions);
            foreach (var addition in additions) addition.Maximum = Math.Min(pool, addition.Maximum.Value);
            result.RandomAddition = extraMaximum;
            result.Entries.AddRange(additions);
            Merge(result.Entries);
            int beforeMaximum = baseCount + extraMaximum;
            int beforeMinimum = baseCount + extraMinimum;

            // 天气选中的污染体不参与原版随机裁减。
            // 最多保留数量为 MAX_CNT + 2 × 天气污染体数量。
            int capMaximum = parser.TotalCap + 2 * thunder;
            int removedMaximum = Math.Max(0, beforeMaximum - parser.TotalCap);
            foreach (var entry in result.Entries)
            {
                entry.Minimum = Math.Max(0, entry.Minimum - removedMaximum);
                entry.Maximum = Math.Min(entry.Maximum.Value, Math.Min(beforeMaximum, capMaximum));
            }
            result.Minimum = Math.Min(beforeMinimum, parser.TotalCap);
            result.Maximum = Math.Min(beforeMaximum, capMaximum);
            ApplyQuestMinimum(result, context);
            ApplyThunder(result, thunder);
            foreach (var entry in result.Entries) ApplyAttributes(entry, context);
            Merge(result.Entries, true);
            result.Entries.RemoveAll(entry => entry.Maximum == 0);
            foreach (var entry in result.Entries)
            {
                if (entry.Source == BattleEnemySource.Follower) entry.Minimum = 0;
                if (CanSummon(entry.EnemyId)) result.DynamicReinforcements = true;
            }
            if (result.Entries.Any(entry => entry.Source == BattleEnemySource.Follower))
                result.Minimum = Math.Min(result.Minimum, result.Entries.Where(entry => entry.Source != BattleEnemySource.Follower).Sum(entry => entry.Minimum));
            if (result.Incomplete) result.Maximum = null;
            return result;
        }

        private static void ApplyQuestMinimum(BattleEnemyPreviewSnapshot result, BattleEnemyPreviewContext context)
        {
            if (!context.QuestEnemy.HasValue || context.QuestMinimum <= 0) return;
            var id = context.QuestEnemy.Value & ~ENEMYID._OVERDRIVE_FLAG;
            bool od = (context.QuestEnemy.Value & ENEMYID._OVERDRIVE_FLAG) != 0;
            var matching = result.Entries.Where(entry => entry.EnemyId == id && (!od || entry.Overdrive)).ToArray();
            int minimum = Math.Max(0, context.QuestMinimum - matching.Sum(entry => entry.Maximum ?? 0));
            int maximum = Math.Max(0, context.QuestMinimum - matching.Sum(entry => entry.Minimum));
            var added = BattleEnemyPreviewScript.CreateEntry(id.ToString(), od, ENATTR.NORMAL, BattleEnemySource.Main, context, false);
            added.Minimum = minimum;
            added.Maximum = maximum;
            result.Entries.Add(added);
            result.Minimum += minimum;
            result.Maximum += maximum;
        }

        private static void ApplyThunder(BattleEnemyPreviewSnapshot result, int capacity)
        {
            if (capacity <= 0) return;
            var eligible = result.Entries.Where(entry => !entry.Overdrive && entry.Overdriveable).ToArray();
            int minimumTotal = eligible.Sum(entry => entry.Minimum);
            int maximumTotal = eligible.Sum(entry => entry.Maximum.Value);
            foreach (var entry in eligible)
            {
                int maximum = Math.Min(capacity, entry.Maximum.Value);
                int minimum = Math.Max(0, Math.Min(capacity, minimumTotal) - (maximumTotal - entry.Maximum.Value));
                var converted = entry.Copy();
                converted.Overdrive = true;
                converted.Minimum = minimum;
                converted.Maximum = maximum;
                entry.Minimum = Math.Max(0, entry.Minimum - maximum);
                entry.Maximum -= minimum;
                result.Entries.Add(converted);
            }
        }

        private static void ApplyAttributes(BattleEnemyPreviewEntry entry, BattleEnemyPreviewContext context)
        {
            if (entry.FixedAttributes) return;
            if (context.QuestAttributeBudget)
            {
                // 任务属性可能替换脚本条目已有的同类属性。
                var replaced = (context.QuestAttributes & ENATTR._AATTR) != 0 ? ENATTR._AATTR : ENATTR.NORMAL;
                if ((context.QuestAttributes & ENATTR._MATTR) != 0) replaced |= ENATTR._MATTR;
                entry.PossibleAttributes |= (entry.Attributes | context.QuestAttributes) & ~entry.DeclinedAttributes;
                entry.Attributes &= ~replaced | context.QuestAttributes;
            }
            int threshold = entry.Overdrive ? entry.OverdriveAttributeThreshold : entry.AttributeThreshold;
            if (context.AttributeBudget > 0 && threshold != 255 && context.AttributeDanger >= threshold)
            {
                ENATTR possible = ENATTR._AATTR | ENATTR._MATTR;
                if (context.AttributeKindMaximum >= 2) possible |= ENATTR._KIND2;
                if ((entry.Attributes & ENATTR._MATTR) != 0) possible &= ~ENATTR._MATTR;
                entry.PossibleAttributes |= possible & ~entry.DeclinedAttributes;
            }
            entry.PossibleAttributes &= ~entry.Attributes;
        }

        private static bool SameGroup(BattleEnemyPreviewEntry a, BattleEnemyPreviewEntry b, bool displayOnly = false)
        {
            return a.EnemyKey == b.EnemyKey && a.Overdrive == b.Overdrive && a.Attributes == b.Attributes
                && a.Source == b.Source && (displayOnly || a.PossibleAttributes == b.PossibleAttributes
                && a.FixedAttributes == b.FixedAttributes && a.Overdriveable == b.Overdriveable);
        }

        internal static void Merge(List<BattleEnemyPreviewEntry> entries, bool displayOnly = false)
        {
            for (int i = 0; i < entries.Count; i++)
                for (int j = entries.Count - 1; j > i; j--)
                    if (SameGroup(entries[i], entries[j], displayOnly))
                    {
                        entries[i].Minimum += entries[j].Minimum;
                        entries[i].Maximum += entries[j].Maximum;
                        entries[i].PossibleAttributes |= entries[j].PossibleAttributes;
                        entries.RemoveAt(j);
                    }
        }

        internal static bool CanSummon(ENEMYID? id)
        {
            return id == ENEMYID.GOLEM_0 || id == ENEMYID.EHOME_0 || id == ENEMYID.HONEYCOMB_0
                || id == ENEMYID.BOSS_NUSI_0 || id == ENEMYID.BOSS_SPIDER_0;
        }
    }
}
