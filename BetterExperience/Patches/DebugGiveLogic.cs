using System;
using System.Collections.Generic;
using UnityModBase.HClassAttribute;

namespace BetterExperience.Patches
{
    internal enum GiveCatalogKind
    {
        [EnumGuiDescription("物品", "Item")]
        Item = 0,
        [EnumGuiDescription("技能", "Skill")]
        Skill = 1,
        [EnumGuiDescription("配方", "Recipe")]
        Recipe = 2
    }

    /// <summary>
    /// 调试给予的键名、筛选与单选列表规则，不访问游戏对象。
    /// </summary>
    internal static class DebugGiveLogic
    {
        internal const int GradeMin = 0;
        internal const int GradeMax = 4;
        internal const int CountMin = 1;
        internal const int CountMax = 99;
        internal const string SkillbookPrefix = "skillbook_";
        internal const string RecipePrefix = "Recipe_";

        internal static string NormalizeKey(string raw)
        {
            return string.IsNullOrWhiteSpace(raw) ? "" : raw.Trim();
        }

        internal static int ClampGrade(int grade)
        {
            if (grade < GradeMin)
                return GradeMin;
            if (grade > GradeMax)
                return GradeMax;
            return grade;
        }

        internal static int ClampCount(int count)
        {
            if (count < CountMin)
                return CountMin;
            if (count > CountMax)
                return CountMax;
            return count;
        }

        internal static string SkillKeyFromInput(string input)
        {
            string key = NormalizeKey(input);
            return key.StartsWith(SkillbookPrefix, StringComparison.Ordinal)
                ? key.Substring(SkillbookPrefix.Length)
                : key;
        }

        internal static string RecipeKeyFromInput(string input)
        {
            string key = NormalizeKey(input);
            return key.StartsWith(RecipePrefix, StringComparison.Ordinal)
                ? key.Substring(RecipePrefix.Length)
                : key;
        }

        internal static int ResolveItemGrade(bool individualGrade, int requested)
        {
            return individualGrade ? 0 : ClampGrade(requested);
        }

        internal static string UniqueDisplay(string name, string key, ISet<string> used)
        {
            string display = string.IsNullOrWhiteSpace(name) ? key : name.Trim();
            if (used != null && !used.Add(display))
            {
                display = display + " [" + key + "]";
                used.Add(display);
            }

            return display;
        }

        internal static bool MatchesFilter(string display, string key, string filter)
        {
            if (string.IsNullOrEmpty(filter))
                return true;
            return (!string.IsNullOrEmpty(display) &&
                    display.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0) ||
                   (!string.IsNullOrEmpty(key) &&
                    key.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        internal static List<(string Key, string Display)> FilterCatalog(
            IReadOnlyList<(string Key, string Display)> catalog,
            string filter)
        {
            var result = new List<(string Key, string Display)>();
            if (catalog == null)
                return result;

            string normalized = NormalizeKey(filter);
            for (int i = 0; i < catalog.Count; i++)
            {
                var entry = catalog[i];
                if (MatchesFilter(entry.Display, entry.Key, normalized))
                    result.Add(entry);
            }

            return result;
        }

        internal static List<(string Display, bool Selected)> ToChoiceList(
            IReadOnlyList<(string Key, string Display)> catalog,
            string selectedKey)
        {
            var result = new List<(string Display, bool Selected)>();
            if (catalog == null)
                return result;

            for (int i = 0; i < catalog.Count; i++)
            {
                var entry = catalog[i];
                result.Add((entry.Display, entry.Key == selectedKey));
            }

            return result;
        }

        /// <summary>
        /// 把多选控件提交的整份列表折叠回单选键。
        /// 勾选 previousKey 之外的条目视为切换选择；全不勾视为取消选择。
        /// </summary>
        internal static string SelectedKeyFromChoices(
            IReadOnlyList<(string Display, bool Selected)> choices,
            IReadOnlyList<(string Key, string Display)> catalog,
            string previousKey)
        {
            if (choices == null || catalog == null || choices.Count != catalog.Count)
                return previousKey ?? "";

            previousKey = previousKey ?? "";

            string found = null;
            bool previousStillTicked = false;
            for (int i = 0; i < choices.Count; i++)
            {
                if (!choices[i].Selected)
                    continue;
                if (catalog[i].Key == previousKey)
                {
                    previousStillTicked = true;
                    continue;
                }

                if (found == null)
                    found = catalog[i].Key;
            }

            if (found != null)
                return found;
            return previousStillTicked ? previousKey : "";
        }
    }
}
