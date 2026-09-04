using BetterExperience.Patches;
using nel;
using System.Reflection;
using UnityEngine;
using XX;

namespace BetterExperience.Test.Patches
{
    public class CookingEffectPreviewTests : IDisposable
    {
        private readonly uint[] _seeds = new uint[4];
        private readonly XorsMaker _random = (XorsMaker)typeof(X).GetField("Xors", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).GetValue(null);

        public CookingEffectPreviewTests()
        {
            _random.saveRandSeeds(_seeds);
            ResetRandom();
        }

        public void Dispose() => _random.init(_seeds, false);

        private void ResetRandom() => _random.init(false, 123u, 456u, 789u, 1011u);

        [Theory]
        [InlineData(0, 1, 0.1f)]
        [InlineData(4, 1, 0.3f)]
        [InlineData(4, 3, 0.3f)]
        public void NativeRecipeAggregation_PreservesIngredientGradeAndRandomCount(int grade, int count, float strength)
        {
            var mushroom = new NelItem("test_mushroom", 0, 10, 99);
            mushroom.RecipeInfo = new RCP.RecipeItemInfo(mushroom, null, 1, null, null);
            mushroom.RecipeInfo.Oeffect100[RCP.RPI_EFFECT.RANDOM] = 30f;
            var recipe = new RCP.Recipe("test_recipe");
            var source = new RCP.RecipeIngredient(null, null, RCP.RPI_CATEG.MUSH, NelItem.CATEG.OTHER, 100f, 1, 3, 0, 0, -2, true);
            recipe.AIng.Add(source);
            var rows = Rows(Enumerable.Range(0, count).Select(_ => new UiCraftBase.IngEntryRow(null, source, mushroom, grade)).ToArray());
            var dish = recipe.createDish(rows);
            var preview = new CookingEffectPreview();

            preview.Update(dish, rows);

            Assert.Equal(grade, dish.calced_grade);
            Assert.Equal(count, dish.OEffect[RCP.RPI_EFFECT.RANDOM].y);
            Assert.Equal(count, preview.GetEffects(true).OEffect.Count);
            Assert.All(preview.GetEffects(true).OEffect.Values, value => Assert.Equal(strength, value.x, 6));
            Assert.Equal(30f, mushroom.RecipeInfo.Oeffect100[RCP.RPI_EFFECT.RANDOM]);
        }

        [Fact]
        public void CookedIngredientEffects_AreFixedAndExcludedFromRerolls()
        {
            var existingFood = new RCP.RecipeDish().Create(new RCP.Recipe("cooked"));
            existingFood.OEffect[RCP.RPI_EFFECT.ATK] = new Vector2(0.3f, 1f);
            var dish = CreateDish(3);
            dish.addEffect(existingFood, 1f);
            var preview = new CookingEffectPreview();

            for (int i = 0; i < 5; i++)
            {
                preview.Update(dish, null, true);
                Assert.DoesNotContain(RCP.RPI_EFFECT.ATK, preview.GetEffects(true).OEffect.Keys);
                Assert.Equal(0.3f, preview.GetEffects(false).OEffect[RCP.RPI_EFFECT.ATK].x);
                Assert.Equal(0.3f, existingFood.OEffect[RCP.RPI_EFFECT.ATK].x);
            }
        }

        [Theory]
        [InlineData(1)]
        [InlineData(3)]
        [InlineData(30)]
        public void Update_MatchesNativeFinalization_AndDoesNotChangeOriginal(int count)
        {
            var dish = CreateDish(count);
            dish.OEffect[RCP.RPI_EFFECT.DISH_VARIABLE] = new Vector2(0.5f, 1f);
            var original = CookingEffectPreview.CopyEffects(dish.OEffect);
            var expected = new RCP.RecipeDish(dish) { OEffect = CookingEffectPreview.CopyEffects(original) };
            expected.finalizeDishEffect();
            ResetRandom();

            var preview = new CookingEffectPreview();
            Assert.True(preview.Update(dish, null));

            AssertEffectsEqual(expected, preview.Result);
            Assert.Equal(original.OrderBy(p => p.Key), dish.OEffect.OrderBy(p => p.Key));
            Assert.NotSame(dish.OEffect, preview.Result.OEffect);
            Assert.DoesNotContain(RCP.RPI_EFFECT.RANDOM, preview.Result.OEffect.Keys);
            Assert.DoesNotContain(RCP.RPI_EFFECT.DISH_VARIABLE, preview.Result.OEffect.Keys);
            Assert.Equal(Math.Min(count, 26), preview.GetEffects(true).OEffect.Count);
        }

        [Fact]
        public void RepeatedRerolls_StartFromRawEffects_AndApplyMultiplierOnce()
        {
            var dish = CreateDish(2);
            dish.OEffect[RCP.RPI_EFFECT.DISH_VARIABLE] = new Vector2(0.5f, 1f);
            var preview = new CookingEffectPreview();

            for (int i = 0; i < 15; i++)
            {
                preview.Update(dish, null, true);
                Assert.Equal(0.3f, preview.Result.OEffect[RCP.RPI_EFFECT.MAXHP].x, 6);
                Assert.Equal(2, preview.GetEffects(true).OEffect.Count);
                Assert.All(preview.GetEffects(true).OEffect.Values, value => Assert.Equal(0.15f, value.x, 6));
                Assert.Equal(0.2f, dish.OEffect[RCP.RPI_EFFECT.MAXHP].x);
            }
        }

        [Theory]
        [InlineData(8f, 1f)]
        [InlineData(-8f, -1f)]
        public void Update_UsesNativeEffectLimits(float strength, float expected)
        {
            var dish = CreateDish(1);
            dish.OEffect[RCP.RPI_EFFECT.RANDOM] = new Vector2(strength, 1f);
            var preview = new CookingEffectPreview();

            preview.Update(dish, null);

            Assert.Equal(expected, Assert.Single(preview.GetEffects(true).OEffect).Value.x);
        }

        [Fact]
        public void Update_UnchangedSelectionOrRebuiltDish_KeepsDisplayedResultAndRandomState()
        {
            var dish = CreateDish();
            var rows = Rows(Entry(1));
            var preview = new CookingEffectPreview();
            preview.Update(dish, rows);
            preview.Displayed = true;
            var result = preview.Result;
            var before = new uint[4];
            _random.saveRandSeeds(before);

            Assert.False(preview.Update(dish, rows));
            var rebuilt = new RCP.RecipeDish(dish) { OEffect = CookingEffectPreview.CopyEffects(dish.OEffect) };
            Assert.False(preview.Update(rebuilt, Rows(Entry(1))));

            var after = new uint[4];
            _random.saveRandSeeds(after);
            Assert.Equal(before, after);
            Assert.Same(result, preview.Result);
            Assert.True(preview.Displayed);
        }

        [Theory]
        [InlineData("grade")]
        [InlineData("count")]
        [InlineData("slot")]
        [InlineData("nested")]
        [InlineData("effect")]
        [InlineData("recipe")]
        [InlineData("item")]
        public void Update_ChangedInput_InvalidatesDisplayedResult(string change)
        {
            var dish = CreateDish();
            var rows = Rows(Entry(1));
            var preview = new CookingEffectPreview();
            preview.Update(dish, rows);
            preview.Displayed = true;
            var before = preview.Result;
            switch (change)
            {
                case "grade": rows[0][0] = Entry(4); break;
                case "count": rows[0].Add(Entry(1)); break;
                case "slot": rows.Add(rows[0]); rows[0] = new List<UiCraftBase.IngEntryRow>(); break;
                case "nested": rows[0][0].setInnerIngredientFromBinary(Rows(Entry(2))); break;
                case "effect": dish.OEffect[RCP.RPI_EFFECT.MAXHP] = new Vector2(0.8f, 1f); break;
                case "recipe": dish.Rcp = new RCP.Recipe("another"); break;
                case "item": rows[0][0] = new UiCraftBase.IngEntryRow(null, null, new NelItem("mushroom", 0, 10, 99), 1); break;
            }

            Assert.False(preview.Matches(dish, rows));
            Assert.Null(preview.BeginCommit(dish, rows));
            Assert.True(preview.Update(dish, rows));
            Assert.NotSame(before, preview.Result);
            Assert.False(preview.Displayed);
        }

        [Fact]
        public void Update_ChangedNestedGrade_IsDetectedWithoutReplacingParentRow()
        {
            var dish = CreateDish();
            var nested = Rows(Entry(1));
            var parent = Entry(0).setInnerIngredientFromBinary(nested);
            var rows = Rows(parent);
            var preview = new CookingEffectPreview();
            preview.Update(dish, rows);

            nested[0][0] = Entry(4);

            Assert.False(preview.Matches(dish, rows));
            Assert.True(preview.Update(dish, rows));
        }

        [Fact]
        public void Update_NullAndEmptySlots_AreComparedWithoutExceptions()
        {
            var dish = CreateDish();
            var rows = new List<List<UiCraftBase.IngEntryRow>> { null, new List<UiCraftBase.IngEntryRow> { null } };
            var preview = new CookingEffectPreview();
            preview.Update(dish, rows);
            Assert.True(preview.Matches(dish, rows));

            rows[0] = new List<UiCraftBase.IngEntryRow>();
            Assert.False(preview.Matches(dish, rows));
        }

        [Fact]
        public void BeginCommit_RequiresDisplayedMatchingPreview_AndKeepsNativeMetadata()
        {
            var dish = CreateDish(2);
            dish.price = 123;
            dish.calced_grade = 4;
            dish.fixCostInCreating(9);
            var preview = new CookingEffectPreview();
            preview.Update(dish, null);
            Assert.Null(preview.BeginCommit(dish, null));
            preview.Displayed = true;

            var commit = preview.BeginCommit(dish, null);
            Assert.NotNull(commit);
            dish.finalizeDishEffect();
            commit.Finish(true);

            AssertEffectsEqual(preview.Result, dish);
            Assert.NotSame(preview.Result.OEffect, dish.OEffect);
            Assert.Equal(123, dish.price);
            Assert.Equal(4, dish.calced_grade);
            Assert.Equal(9, dish.cost);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Finish_FailedBeforeStorage_RestoresOriginalEffectsForRetry(bool finalizeFirst)
        {
            var dish = CreateDish();
            var original = dish.OEffect;
            var preview = new CookingEffectPreview();
            preview.Update(dish, null);
            preview.Displayed = true;
            var commit = preview.BeginCommit(dish, null);
            if (finalizeFirst)
                dish.finalizeDishEffect();

            commit.Finish(false);

            Assert.Same(original, dish.OEffect);
            Assert.True(preview.Matches(dish, null));
            Assert.NotNull(preview.BeginCommit(dish, null));
        }

        [Fact]
        public void Finish_AfterStorageStarts_DoesNotRestoreRawEffectsIntoSharedProduct()
        {
            var dish = CreateDish();
            var preview = new CookingEffectPreview();
            preview.Update(dish, null);
            preview.Displayed = true;
            var commit = preview.BeginCommit(dish, null);
            commit.StorageStarted = true;
            var product = new RCP.RecipeDish(dish);

            commit.Finish(false);
            preview.Clear();

            Assert.DoesNotContain(RCP.RPI_EFFECT.RANDOM, dish.OEffect.Keys);
            AssertEffectsEqual(product, dish);
        }

        [Fact]
        public void Finish_DoesNotOverwriteAnotherPatchReplacingTheEffects()
        {
            var dish = CreateDish();
            var preview = new CookingEffectPreview();
            preview.Update(dish, null);
            preview.Displayed = true;
            var commit = preview.BeginCommit(dish, null);
            var replacement = CookingEffectPreview.CopyEffects(dish.OEffect);
            dish.OEffect = replacement;

            commit.Finish(false);

            Assert.Same(replacement, dish.OEffect);
        }

        [Fact]
        public void BatchProducts_KeepThePreview_AfterNextDishIsRerolled()
        {
            var dish = CreateDish(2);
            var preview = new CookingEffectPreview();
            preview.Update(dish, null);
            preview.Displayed = true;
            var expected = preview.Result;
            var commit = preview.BeginCommit(dish, null);
            dish.finalizeDishEffect();
            var products = Enumerable.Range(0, 5).Select(_ => new RCP.RecipeDish(dish)).ToArray();
            commit.StorageStarted = true;
            commit.Finish(true);
            preview.Clear();
            preview.Update(CreateDish(), null, true);

            Assert.All(products, product => AssertEffectsEqual(expected, product));
        }

        [Fact]
        public void Eligibility_ExcludesDisabledReadOnlyNonCookingAndFixedProducts()
        {
            var dish = CreateDish();
            Assert.True(CookingEffectPreview.IsEligible(true, false, dish));
            Assert.False(CookingEffectPreview.IsEligible(false, false, dish));
            Assert.False(CookingEffectPreview.IsEligible(true, true, dish));
            Assert.False(CookingEffectPreview.IsEligible(true, false, null));
            dish.Rcp.categ = RCP.RP_CATEG.ALOMA;
            Assert.False(CookingEffectPreview.IsEligible(true, false, dish));
            dish.Rcp.categ = RCP.RP_CATEG.COOK;
            dish.Rcp.Completion = new NelItem("fixed", 0, 10, 99);
            Assert.False(CookingEffectPreview.IsEligible(true, false, dish));
            dish.Rcp.Completion = null;
            dish.OEffect[RCP.RPI_EFFECT.RANDOM] = new Vector2(0.1f, 0f);
            Assert.False(CookingEffectPreview.IsEligible(true, false, dish));
            dish.OEffect.Remove(RCP.RPI_EFFECT.RANDOM);
            Assert.False(CookingEffectPreview.IsEligible(true, false, dish));
        }

        [Fact]
        public void Update_RemovingAllRandomIngredients_ClearsOldPreview()
        {
            var dish = CreateDish();
            var preview = new CookingEffectPreview();
            preview.Update(dish, null);
            dish.OEffect.Remove(RCP.RPI_EFFECT.RANDOM);

            Assert.False(preview.Update(dish, null));
            Assert.Null(preview.Result);
            Assert.Null(preview.BeginCommit(dish, null));
        }

        private static RCP.RecipeDish CreateDish(int randomCount = 1)
        {
            var dish = new RCP.RecipeDish().Create(new RCP.Recipe("test_cooking"));
            dish.OEffect[RCP.RPI_EFFECT.MAXHP] = new Vector2(0.2f, 1f);
            dish.OEffect[RCP.RPI_EFFECT.RANDOM] = new Vector2(0.1f * randomCount, randomCount);
            return dish;
        }

        private static UiCraftBase.IngEntryRow Entry(int grade) => new UiCraftBase.IngEntryRow(null, null, null, grade);

        private static List<List<UiCraftBase.IngEntryRow>> Rows(params UiCraftBase.IngEntryRow[] entries)
        {
            return new List<List<UiCraftBase.IngEntryRow>> { entries.ToList() };
        }

        private static void AssertEffectsEqual(RCP.RecipeDish expected, RCP.RecipeDish actual)
        {
            Assert.Equal(expected.OEffect.OrderBy(p => p.Key), actual.OEffect.OrderBy(p => p.Key));
        }
    }
}
