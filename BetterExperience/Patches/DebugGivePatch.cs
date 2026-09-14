using BetterExperience.BLogSpace;
using BetterExperience.BPatchGUI;
using nel;
using System;
using System.Collections.Generic;
using UnityModBase.HTranslatorSpace;

namespace BetterExperience.Patches
{
    public partial class HPatches
    {
        /// <summary>
        /// 把原版调试器的给物品、解锁技能、揭示配方接到控制页。
        /// 用本地化名称列表单选，不要求填写内部键。
        /// </summary>
        public static class DebugGive
        {
            internal static readonly object NoticeOwner = new object();

            private static GiveCatalogKind _kind;
            private static string _filter = "";
            private static string _selectedKey = "";
            private static int _itemGrade;
            private static int _itemCount = 1;
            private static List<(string Key, string Display)> _catalog = new List<(string Key, string Display)>();
            private static List<(string Display, bool Selected)> _choices = new List<(string Display, bool Selected)>();
            private static GiveCatalogKind _catalogKind;
            private static string _catalogFilter = "";

            internal static GiveCatalogKind GetKind() => _kind;

            internal static void SetKind(GiveCatalogKind value)
            {
                if (_kind == value)
                    return;
                _kind = value;
                _selectedKey = "";
                RebuildCatalog(force: true);
            }

            internal static string GetFilter() => _filter;

            internal static void SetFilter(string value)
            {
                string next = DebugGiveLogic.NormalizeKey(value);
                if (_filter == next)
                    return;
                _filter = next;
                RebuildCatalog(force: true);
            }

            internal static List<(string Display, bool Selected)> GetChoices()
            {
                RebuildCatalog(force: false);
                return _choices;
            }

            internal static void SetChoices(List<(string Display, bool Selected)> value)
            {
                RebuildCatalog(force: false);
                _selectedKey = DebugGiveLogic.SelectedKeyFromChoices(value, _catalog, _selectedKey);
                _choices = DebugGiveLogic.ToChoiceList(_catalog, _selectedKey);
            }

            internal static int GetGiveItemGrade() => _itemGrade;

            internal static void SetGiveItemGrade(int value)
            {
                _itemGrade = DebugGiveLogic.ClampGrade(value);
            }

            internal static int GetGiveItemCount() => _itemCount;

            internal static void SetGiveItemCount(int value)
            {
                _itemCount = DebugGiveLogic.ClampCount(value);
            }

            internal static bool GetGive() => false;

            internal static void SetGive(bool value)
            {
                if (!value)
                    return;

                switch (_kind)
                {
                    case GiveCatalogKind.Skill:
                        UnlockSkill();
                        break;
                    case GiveCatalogKind.Recipe:
                        RevealRecipe();
                        break;
                    default:
                        GiveItem();
                        break;
                }
            }

            internal static void GiveItem()
            {
                try
                {
                    if (!TryGetWorld(out var m2d, out var imng))
                        return;

                    string key = DebugGiveLogic.NormalizeKey(_selectedKey);
                    if (key.Length == 0)
                    {
                        Notice(new Translator(chinese: "请先在列表中选择物品。", english: "Select an item in the list first."));
                        return;
                    }

                    var item = NelItem.GetById(key, no_error: true);
                    if (item == null)
                    {
                        Notice(new Translator(chinese: "未知物品。", english: "Unknown item."));
                        return;
                    }

                    if (item.is_skillbook)
                    {
                        imng.getItem(item);
                        SkillManager.Get(item)?.Obtain();
                        Notice(new Translator(chinese: "已解锁技能书：" + DisplayOf(key), english: "Unlocked skill book: " + DisplayOf(key)));
                        return;
                    }

                    if (m2d.curMap == null)
                    {
                        NoticeUnavailable();
                        return;
                    }

                    float x;
                    float y;
                    if ((UnityEngine.Object)m2d.curMap.Pr == (UnityEngine.Object)null)
                    {
                        x = m2d.Cam.x;
                        y = m2d.Cam.y;
                    }
                    else
                    {
                        x = m2d.curMap.Pr.x;
                        y = m2d.curMap.Pr.y - 1f;
                    }

                    int grade = DebugGiveLogic.ResolveItemGrade(item.individual_grade, _itemGrade);
                    int count = DebugGiveLogic.ClampCount(_itemCount);
                    var drop = imng.dropManual(item, count, grade, x, y, vy: -0.05f, check_auto_absorb: true);
                    if (drop != null)
                        drop.type |= NelItemManager.TYPE.ABSORB;

                    Notice(new Translator(chinese: "已给予：" + DisplayOf(key), english: "Gave: " + DisplayOf(key)));
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(GiveItem)}", ex);
                }
            }

            internal static void UnlockSkill()
            {
                try
                {
                    if (GetM2D() == null)
                    {
                        NoticeUnavailable();
                        return;
                    }

                    string key = DebugGiveLogic.SkillKeyFromInput(_selectedKey);
                    if (key.Length == 0)
                    {
                        Notice(new Translator(chinese: "请先在列表中选择技能。", english: "Select a skill in the list first."));
                        return;
                    }

                    var skill = SkillManager.Get(key);
                    if (skill == null)
                    {
                        Notice(new Translator(chinese: "未知技能。", english: "Unknown skill."));
                        return;
                    }

                    skill.Obtain();
                    Notice(new Translator(chinese: "已解锁技能：" + DisplayOf(_selectedKey), english: "Unlocked skill: " + DisplayOf(_selectedKey)));
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(UnlockSkill)}", ex);
                }
            }

            internal static void RevealRecipe()
            {
                try
                {
                    var imng = GetIMNG();
                    if (imng == null)
                    {
                        NoticeUnavailable();
                        return;
                    }

                    string recipeKey = DebugGiveLogic.RecipeKeyFromInput(_selectedKey);
                    if (recipeKey.Length == 0)
                    {
                        Notice(new Translator(chinese: "请先在列表中选择配方。", english: "Select a recipe in the list first."));
                        return;
                    }

                    var recipe = RCP.Get(recipeKey);
                    var recipeItem = recipe?.RecipeItem ?? NelItem.GetById(DebugGiveLogic.RecipePrefix + recipeKey, no_error: true);
                    if (recipeItem == null || !recipeItem.is_recipe)
                    {
                        Notice(new Translator(chinese: "未知配方。", english: "Unknown recipe."));
                        return;
                    }

                    recipeItem.touchObtainCount();
                    RCP.Get(recipeItem)?.touchObtainCountAllIngredients();
                    imng.getItem(recipeItem);
                    Notice(new Translator(chinese: "已揭示配方：" + DisplayOf(_selectedKey), english: "Revealed recipe: " + DisplayOf(_selectedKey)));
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(RevealRecipe)}", ex);
                }
            }

            private static void RebuildCatalog(bool force)
            {
                if (!force && _catalogKind == _kind && _catalogFilter == _filter && _catalog.Count > 0)
                    return;

                _catalogKind = _kind;
                _catalogFilter = _filter;
                _catalog = DebugGiveLogic.FilterCatalog(BuildSourceCatalog(_kind), _filter);
                if (!string.IsNullOrEmpty(_selectedKey))
                {
                    bool stillThere = false;
                    for (int i = 0; i < _catalog.Count; i++)
                    {
                        if (_catalog[i].Key == _selectedKey)
                        {
                            stillThere = true;
                            break;
                        }
                    }

                    if (!stillThere)
                        _selectedKey = "";
                }

                _choices = DebugGiveLogic.ToChoiceList(_catalog, _selectedKey);
            }

            private static List<(string Key, string Display)> BuildSourceCatalog(GiveCatalogKind kind)
            {
                switch (kind)
                {
                    case GiveCatalogKind.Skill:
                        return BuildSkillCatalog();
                    case GiveCatalogKind.Recipe:
                        return BuildRecipeCatalog();
                    default:
                        return BuildItemCatalog();
                }
            }

            private static List<(string Key, string Display)> BuildItemCatalog()
            {
                var result = new List<(string Key, string Display)>();
                var used = new HashSet<string>(StringComparer.Ordinal);
                var data = NelItem.getWholeDictionary();
                if (data == null)
                    return result;

                foreach (var pair in (Dictionary<string, NelItem>)data)
                {
                    var item = pair.Value;
                    if (item == null || item.is_cache_item || item.is_recipe)
                        continue;
                    if (pair.Key == NelItem.money_key || pair.Key.StartsWith("spconfig_", StringComparison.Ordinal))
                        continue;

                    string display = SafeItemName(item);
                    result.Add((item.key, DebugGiveLogic.UniqueDisplay(display, item.key, used)));
                }

                result.Sort((a, b) => string.Compare(a.Display, b.Display, StringComparison.CurrentCultureIgnoreCase));
                return result;
            }

            private static List<(string Key, string Display)> BuildSkillCatalog()
            {
                var result = new List<(string Key, string Display)>();
                var used = new HashSet<string>(StringComparer.Ordinal);
                var data = SkillManager.getSkillDictionary();
                if (data == null)
                    return result;

                foreach (var pair in (Dictionary<string, PrSkill>)data)
                {
                    var skill = pair.Value;
                    if (skill == null || string.IsNullOrEmpty(skill.key))
                        continue;

                    result.Add((skill.key, DebugGiveLogic.UniqueDisplay(SafeSkillTitle(skill), skill.key, used)));
                }

                result.Sort((a, b) => string.Compare(a.Display, b.Display, StringComparison.CurrentCultureIgnoreCase));
                return result;
            }

            private static List<(string Key, string Display)> BuildRecipeCatalog()
            {
                var result = new List<(string Key, string Display)>();
                var used = new HashSet<string>(StringComparer.Ordinal);
                var data = NelItem.getWholeDictionary();
                if (data == null)
                    return result;

                foreach (var pair in (Dictionary<string, NelItem>)data)
                {
                    var item = pair.Value;
                    if (item == null || !item.is_recipe)
                        continue;

                    string recipeKey = DebugGiveLogic.RecipeKeyFromInput(item.key);
                    string display = SafeItemName(item);
                    result.Add((recipeKey, DebugGiveLogic.UniqueDisplay(display, recipeKey, used)));
                }

                result.Sort((a, b) => string.Compare(a.Display, b.Display, StringComparison.CurrentCultureIgnoreCase));
                return result;
            }

            private static string SafeItemName(NelItem item)
            {
                try
                {
                    string name = item.getLocalizedName(0);
                    return string.IsNullOrWhiteSpace(name) ? item.key : name;
                }
                catch
                {
                    return item.key;
                }
            }

            private static string SafeSkillTitle(PrSkill skill)
            {
                try
                {
                    string title = skill.title;
                    return string.IsNullOrWhiteSpace(title) ? skill.key : title;
                }
                catch
                {
                    return skill.key;
                }
            }

            private static string DisplayOf(string key)
            {
                for (int i = 0; i < _catalog.Count; i++)
                {
                    if (_catalog[i].Key == key)
                        return _catalog[i].Display;
                }

                return key;
            }

            private static bool TryGetWorld(out NelM2DBase m2d, out NelItemManager imng)
            {
                m2d = GetM2D();
                imng = m2d?.IMNG;
                if (m2d == null || imng == null)
                {
                    NoticeUnavailable();
                    return false;
                }

                return true;
            }

            private static void NoticeUnavailable()
            {
                Notice(new Translator(chinese: "未进入游戏或尚未读档。", english: "Not in game or no save loaded."));
            }

            private static void Notice(Translator message)
            {
                NoticeGUI.Show(message, owner: NoticeOwner);
            }
        }
    }
}
