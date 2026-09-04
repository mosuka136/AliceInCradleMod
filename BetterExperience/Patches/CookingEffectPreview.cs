using Better;
using nel;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BetterExperience.Patches
{
    /// <summary>
    /// 保存一次选材的原始效果及已展示的抽取结果。只复制和结算效果，不接触库存或物品注册表。
    /// </summary>
    internal sealed class CookingEffectPreview
    {
        private IngredientSnapshot _ingredients;
        private BDic<RCP.RPI_EFFECT, Vector2> _originalEffects;

        internal RCP.RecipeDish Result { get; private set; }
        internal bool Displayed { get; set; }

        internal static bool IsEligible(bool enabled, bool readOnly, RCP.RecipeDish dish)
        {
            return enabled && !readOnly && dish?.Rcp != null
                && dish.Rcp.categ == RCP.RP_CATEG.COOK && dish.Rcp.Completion == null
                && dish.OEffect != null
                && dish.OEffect.TryGetValue(RCP.RPI_EFFECT.RANDOM, out var random)
                && random.y >= 1f;
        }

        internal bool Matches(RCP.RecipeDish dish, List<List<UiCraftBase.IngEntryRow>> ingredients)
        {
            return Result != null && dish != null && ReferenceEquals(Result.Rcp, dish.Rcp)
                && EffectsEqual(_originalEffects, dish.OEffect) && _ingredients.Matches(ingredients);
        }

        /// <returns>本次是否生成了新的预览。</returns>
        internal bool Update(RCP.RecipeDish dish, List<List<UiCraftBase.IngEntryRow>> ingredients, bool reroll = false)
        {
            if (!IsEligible(true, false, dish))
            {
                Clear();
                return false;
            }
            if (!reroll && Matches(dish, ingredients))
                return false;

            // RecipeDish 的复制构造函数共享 OEffect；必须先断开引用，再交给原生结算。
            var original = CopyEffects(dish.OEffect);
            var result = new RCP.RecipeDish(dish) { OEffect = CopyEffects(original) };
            result.finalizeDishEffect();
            var snapshot = new IngredientSnapshot(ingredients);

            _originalEffects = original;
            _ingredients = snapshot;
            Result = result;
            Displayed = false;
            return true;
        }

        internal RCP.RecipeDish GetEffects(bool randomOnly)
        {
            var effects = new BDic<RCP.RPI_EFFECT, Vector2>();
            foreach (var pair in Result.OEffect)
            {
                // 原生抽取跳过所有已有类型，所以新增键恰好是本次随机产生的效果。
                if ((!_originalEffects.ContainsKey(pair.Key)) == randomOnly)
                    effects.Add(pair.Key, pair.Value);
            }
            return new RCP.RecipeDish(Result) { OEffect = effects };
        }

        internal CookingEffectCommit BeginCommit(RCP.RecipeDish dish, List<List<UiCraftBase.IngEntryRow>> ingredients)
        {
            if (!Displayed || !Matches(dish, ingredients))
                return null;
            return new CookingEffectCommit(dish, Result.OEffect);
        }

        internal void Clear()
        {
            _ingredients = null;
            _originalEffects = null;
            Result = null;
            Displayed = false;
        }

        internal static BDic<RCP.RPI_EFFECT, Vector2> CopyEffects(IDictionary<RCP.RPI_EFFECT, Vector2> source)
        {
            return new BDic<RCP.RPI_EFFECT, Vector2>(source);
        }

        private static bool EffectsEqual(IDictionary<RCP.RPI_EFFECT, Vector2> left, IDictionary<RCP.RPI_EFFECT, Vector2> right)
        {
            if (right == null || left.Count != right.Count)
                return false;
            foreach (var pair in left)
            {
                if (!right.TryGetValue(pair.Key, out var value) || !pair.Value.Equals(value))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// 按槽位和嵌套结构快照选材；不使用可变行对象的引用或有碰撞可能的哈希作为缓存依据。
        /// </summary>
        private sealed class IngredientSnapshot
        {
            private readonly EntrySnapshot[][] _rows;

            internal IngredientSnapshot(List<List<UiCraftBase.IngEntryRow>> rows)
            {
                _rows = rows?.Select(row => row?
                    .Select(entry => entry == null ? null : new EntrySnapshot(entry)).ToArray()).ToArray();
            }

            internal bool Matches(List<List<UiCraftBase.IngEntryRow>> rows)
            {
                if (_rows == null || rows == null)
                    return _rows == null && rows == null;
                if (_rows.Length != rows.Count)
                    return false;
                for (int i = 0; i < rows.Count; i++)
                {
                    var before = _rows[i];
                    var after = rows[i];
                    if (before == null || after == null)
                    {
                        if (before != null || after != null)
                            return false;
                        continue;
                    }
                    if (before.Length != after.Count)
                        return false;
                    for (int j = 0; j < after.Count; j++)
                    {
                        if (before[j] == null ? after[j] != null : !before[j].Matches(after[j]))
                            return false;
                    }
                }
                return true;
            }
        }

        private sealed class EntrySnapshot
        {
            private readonly NelItem _item;
            private readonly int _grade;
            private readonly RCP.RecipeIngredient _source;
            private readonly IngredientSnapshot _children;

            internal EntrySnapshot(UiCraftBase.IngEntryRow entry)
            {
                _item = entry.Itm;
                _grade = entry.grade;
                _source = entry.Source;
                _children = new IngredientSnapshot(entry.getInnerIngredient());
            }

            internal bool Matches(UiCraftBase.IngEntryRow entry)
            {
                return entry != null && ReferenceEquals(_item, entry.Itm) && _grade == entry.grade
                    && ReferenceEquals(_source, entry.Source) && _children.Matches(entry.getInnerIngredient());
            }
        }
    }

    /// <summary>
    /// 制作期间只替换当前料理的效果字典。开始入库后字典可能被成品共享，异常时不得再回写它。
    /// </summary>
    internal sealed class CookingEffectCommit
    {
        private readonly BDic<RCP.RPI_EFFECT, Vector2> _original;
        private readonly BDic<RCP.RPI_EFFECT, Vector2> _installed;
        private readonly RCP.RecipeDish _dish;
        internal bool StorageStarted { get; set; }

        internal CookingEffectCommit(RCP.RecipeDish dish, IDictionary<RCP.RPI_EFFECT, Vector2> effects)
        {
            _dish = dish;
            _original = dish.OEffect;
            _installed = CookingEffectPreview.CopyEffects(effects);
            dish.OEffect = _installed;
        }

        internal void Finish(bool succeeded)
        {
            if (!succeeded && !StorageStarted && ReferenceEquals(_dish.OEffect, _installed))
                _dish.OEffect = _original;
        }
    }
}
