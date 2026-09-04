using BetterExperience.BConfigManager;
using BetterExperience.Patches;
using HarmonyLib;
using nel;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using UnityModBase.HConfigSpace;
using UnityModBase.HTranslatorSpace;
using XX;

namespace BetterExperience.Test.Patches
{
    public class BattleEnemyPreviewTests
    {
        private const string DarkCave = @"
#GRADE 1
%MAX_APPEAR 6
%EN RAMDA_0 !2 0.6 0.8
%EN MUSH_0 !1 0.6 0.8
%ENATTR ICE
%EN MUSH_0 !1 0.6 0.8
%ENATTR FIRE
%EN MUSH_0 !1 0.6 0.8
";

        private const string WindTower = @"
%MAX_APPEAR PUPPY_0 3
%MAX_APPEAR ~5+difficulty
%EN_HR _1 1
%EN PENTAPOD_0 3 0.6 0.8 '' 0.4
%EN PUPPY_0 2 0.2 0.3
%EN MUSH_0 2 0.6 0.8 '' 0.75 0.3
%EN SPONGE_0 0 0.8 0.8 '' 0.15 0.4
IF 'difficulty>=1' {
    %ENATTR MP_STABLE
    %EN SPONGE_0 !1 0.8 0.8 '' 0.15 0.4
}
%ENATTR INVISIBLE
%EN PUPPY_0 !1 0.4 0.6
IF 'difficulty==2' {
    %ENATTR BIG
    %EN PENTAPOD_0 0 0.4 0.5 '' 0.35 0.4
}
";

        private static BattleEnemyPreviewSnapshot Build(string script, BattleEnemyPreviewContext context = null)
        {
            context = context ?? new BattleEnemyPreviewContext();
            context.GetEnemyDescription = EnemyDescription;
            return BattleEnemyPreview.Build(script, context);
        }

        // 元数据测试桩取自 ver030d 的 NDAT；xUnit 环境无法实例化 Unity 魔物类型。
        private static NDAT.EnemyDescryption EnemyDescription(string key)
        {
            var id = Enum.Parse<ENEMYID>(key);
            return new NDAT.EnemyDescryption
            {
                id = (int)id,
                EnemyType = typeof(object),
                overdriveable = key.StartsWith("SLIME_") || key.StartsWith("MUSH_") || key.StartsWith("GOLEM_") || key.StartsWith("UNI_") || key.StartsWith("LEECH_"),
                nattr_decline = key.StartsWith("MAGE_") ? ENATTR._MATTR : ENATTR.NORMAL,
                nattr_addable = key.StartsWith("SPONGE_") ? (byte)32 : (byte)0
            };
        }

        private static BattleEnemyPreviewEntry Entry(BattleEnemyPreviewSnapshot snapshot, ENEMYID id, ENATTR attributes = ENATTR.NORMAL, bool od = false)
            => Assert.Single(snapshot.Entries.Where(entry => entry.EnemyId == id && entry.Attributes == attributes && entry.Overdrive == od));

        [Fact]
        public void DarkCave_FixedCountsSeparateInnateAttributes_DespiteExtraBudget()
        {
            var result = Build(DarkCave, new BattleEnemyPreviewContext { AdditionalCount = 12, DangerLevel = 2 });

            Assert.False(result.Incomplete);
            Assert.Equal(5, result.Minimum);
            Assert.Equal(5, result.Maximum);
            Assert.Equal(4, result.Entries.Count);
            Assert.Equal(2, Entry(result, ENEMYID.RAMDA_0).Minimum);
            Assert.Equal(1, Entry(result, ENEMYID.MUSH_0).Minimum);
            Assert.Equal(1, Entry(result, ENEMYID.MUSH_0, ENATTR.ICE).Minimum);
            Assert.Equal(1, Entry(result, ENEMYID.MUSH_0, ENATTR.FIRE).Minimum);
            Assert.All(result.Entries, entry => Assert.Equal(entry.Minimum, entry.Maximum));
        }

        [Theory]
        [InlineData(0, false)]
        [InlineData(1, true)]
        [InlineData(2, true)]
        public void WindTower_ResolvesDifficulty_WithoutTreatingAppearLimitAsTotal(int difficulty, bool stableSponge)
        {
            var context = new BattleEnemyPreviewContext();
            context.Values["difficulty"] = difficulty;

            var result = Build(WindTower, context);

            Assert.False(result.Incomplete);
            Assert.Equal(stableSponge, result.Entries.Any(entry => entry.EnemyId == ENEMYID.SPONGE_0 && entry.Attributes == ENATTR.MP_STABLE));
            Assert.Equal(1, Entry(result, ENEMYID.PUPPY_0, ENATTR.INVISIBLE).Minimum);
            Assert.Equal(stableSponge ? 9 : 8, result.Maximum);
        }

        [Fact]
        public void DuplicateRows_ShareOneExtraPool_AndMergeIntoAnExactCount()
        {
            var result = Build("%EN SLIME_0 2\n%EN SLIME_0 3", new BattleEnemyPreviewContext { AdditionalCount = 4 });

            var entry = Assert.Single(result.Entries);
            Assert.Equal(9, entry.Minimum);
            Assert.Equal(9, entry.Maximum);
            Assert.Equal(9, result.Maximum);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(2)]
        public void MatchingDisplayKinds_MergeAfterAttributeEligibilityIsResolved(int attributeBudget)
        {
            var result = Build("%ENATTR !FIRE\n%EN MUSH_0 !2\n%ENATTR FIRE\n%EN MUSH_0 !3", new BattleEnemyPreviewContext
            {
                AttributeDanger = 60, AttributeBudget = attributeBudget
            });
            var entry = Assert.Single(result.Entries);
            Assert.Equal(5, entry.Minimum);
            Assert.Equal(5, entry.Maximum);
            Assert.Equal(attributeBudget > 0, entry.PossibleAttributes != ENATTR.NORMAL);
        }

        [Fact]
        public void OnlyOverdriveRows_DoNotEnableNativeWeatherConversionForExtraCopies()
        {
            var result = Build("%EN_OD SLIME_0 1", new BattleEnemyPreviewContext { AdditionalCount = 1, ThunderCapacity = 1 });
            Assert.Equal(1, Entry(result, ENEMYID.SLIME_0).Minimum);
            Assert.Equal(1, Entry(result, ENEMYID.SLIME_0, od: true).Maximum);
        }

        [Fact]
        public void OverdriveRowExtras_KeepThunderEligibility_WhenAnotherOrdinaryRowEnablesWeather()
        {
            var result = Build("%CANNOT_THUNDER_TO SLIME_0\n%EN_OD SLIME_0 1\n%EN MUSH_0 !1", new BattleEnemyPreviewContext
            {
                AdditionalCount = 1, ThunderCapacity = 2
            });
            Assert.DoesNotContain(result.Entries, entry => entry.EnemyId == ENEMYID.SLIME_0 && !entry.Overdrive);
            Assert.Equal(2, Entry(result, ENEMYID.SLIME_0, od: true).Minimum);
            Assert.Equal(1, Entry(result, ENEMYID.MUSH_0, od: true).Minimum);
        }

        [Fact]
        public void WeightedSpecies_HaveIndividualRanges_AndOneExactTotal()
        {
            var result = Build("%EN SLIME_0 2 0 0 '' 1\n%EN MUSH_0 3 0 0 '' 4", new BattleEnemyPreviewContext { AdditionalCount = 4 });

            Assert.Equal(2, Entry(result, ENEMYID.SLIME_0).Minimum);
            Assert.Equal(6, Entry(result, ENEMYID.SLIME_0).Maximum);
            Assert.Equal(3, Entry(result, ENEMYID.MUSH_0).Minimum);
            Assert.Equal(7, Entry(result, ENEMYID.MUSH_0).Maximum);
            Assert.Equal(9, result.Minimum);
            Assert.Equal(9, result.Maximum);
        }

        [Theory]
        [InlineData("0", 0f, 1, 0)]
        [InlineData("0", 1f, 1, 2)]
        [InlineData("-1", 1f, 1, 0)]
        [InlineData("-1", 2f, 1, 2)]
        [InlineData("!0", 2f, 5, 0)]
        [InlineData("0", 2f, 0, 0)]
        public void ZeroAndNegativeRows_UseNativeUnlockThreshold(string count, float danger, int extra, int expected)
        {
            var result = Build("%EN SLIME_0 " + count, new BattleEnemyPreviewContext { DangerLevel = danger, AdditionalCount = extra });
            Assert.Equal(expected, result.Maximum);
        }

        [Fact]
        public void AddCount_ChangesUnlockBudget_ButNativeAllocationRetainsOriginalBudget()
        {
            var context = new BattleEnemyPreviewContext { AdditionalCount = 2, DangerLevel = 1 };
            var result = Build("%ADD_COUNT !0\n%EN MUSH_0 0\n%EN SLIME_0 1", context);

            Assert.DoesNotContain(result.Entries, entry => entry.EnemyId == ENEMYID.MUSH_0);
            Assert.Equal(3, Entry(result, ENEMYID.SLIME_0).Maximum);
            Assert.Equal(2, context.AdditionalCount);
        }

        [Fact]
        public void QuantityCurves_UseCurrentDangerLevel()
        {
            var result = Build("%EN SLIME_0 !1|Z1<1.5..>|Z1<3..>", new BattleEnemyPreviewContext { DangerLevel = 2 });
            Assert.Equal(2, Assert.Single(result.Entries).Minimum);
        }

        [Fact]
        public void Tikuwar_TotalCapIsSeparateFromSpeciesCap()
        {
            var result = Build("%MAX_CNT !4\n%MAX_CNT SPONGE_0 !2\n%EN SPONGE_0 2 0 0 '' 1\n%EN MUSH_0 1 0 0 '' 4",
                new BattleEnemyPreviewContext { AdditionalCount = 5 });

            Assert.False(result.Incomplete);
            Assert.Equal(4, result.Maximum);
            Assert.True(Entry(result, ENEMYID.SPONGE_0).Maximum <= 2);
            Assert.True(Entry(result, ENEMYID.MUSH_0).Maximum <= 4);
        }

        [Fact]
        public void TotalCapCanRemoveFixedCountEnemies()
        {
            var result = Build("%MAX_CNT !2\n%EN SLIME_0 !2\n%EN MUSH_0 !2");

            Assert.Equal(2, result.Minimum);
            Assert.Equal(2, result.Maximum);
            Assert.All(result.Entries, entry => { Assert.Equal(0, entry.Minimum); Assert.Equal(2, entry.Maximum); });
        }

        [Fact]
        public void SpeciesCapMatchesFamilies_AndPrioritizesLaterRows()
        {
            var result = Build("%MAX_CNT MUSH !2\n%EN MUSH_0 !2\n%ENATTR FIRE\n%EN MUSH_0 !2");
            Assert.Equal(ENATTR.FIRE, Assert.Single(result.Entries).Attributes);
            Assert.Equal(2, result.Maximum);
        }

        [Fact]
        public void PollutionIsAConversion_AndDoesNotIncreaseTotal()
        {
            var result = Build("%EN SLIME_0 !3", new BattleEnemyPreviewContext { ThunderCapacity = 1 });

            Assert.Equal(2, Entry(result, ENEMYID.SLIME_0).Minimum);
            Assert.Equal(2, Entry(result, ENEMYID.SLIME_0).Maximum);
            Assert.Equal(1, Entry(result, ENEMYID.SLIME_0, od: true).Minimum);
            Assert.Equal(1, Entry(result, ENEMYID.SLIME_0, od: true).Maximum);
            Assert.Equal(3, result.Maximum);
        }

        [Fact]
        public void FixedPollutionIsSeparate_AndExtraCopiesAreOrdinary()
        {
            var result = Build("%EN_OD SLIME_0 2", new BattleEnemyPreviewContext { AdditionalCount = 3 });
            Assert.Equal(2, Entry(result, ENEMYID.SLIME_0, od: true).Maximum);
            Assert.Equal(3, Entry(result, ENEMYID.SLIME_0).Maximum);
        }

        [Fact]
        public void CannotThunderAndAttributeRestrictions_AreRespected()
        {
            var result = Build("%CANNOT_THUNDER_TO SLIME_0\n%ENATTR ! ATK\n%EN SLIME_0 !1", new BattleEnemyPreviewContext
            {
                ThunderCapacity = 3, AttributeDanger = 100, AttributeBudget = 10, AttributeKindMaximum = 3
            });
            var entry = Assert.Single(result.Entries);
            Assert.False(entry.Overdrive);
            Assert.Equal(ENATTR.ATK, entry.Attributes);
            Assert.Equal(ENATTR.NORMAL, entry.PossibleAttributes);
        }

        [Fact]
        public void RandomAttributes_ExposePossibilitiesWithoutPretendingToKnowAssignments()
        {
            var result = Build("%ENATTR FIRE\n%EN MUSH_0 !2", new BattleEnemyPreviewContext
            {
                AttributeDanger = 60, AttributeBudget = 2, AttributeKindMaximum = 2
            });
            var entry = Assert.Single(result.Entries);
            Assert.Equal(ENATTR.FIRE, entry.Attributes);
            Assert.NotEqual(ENATTR.NORMAL, entry.PossibleAttributes & ENATTR.ATK);
            Assert.Equal(ENATTR.NORMAL, entry.PossibleAttributes & ENATTR._MATTR);
        }

        [Fact]
        public void ScriptAttributes_AreFilteredByEnemyDeclines()
        {
            var info = EnemyDescription("MAGE_0");
            var result = Build("%ENATTR ! ATK FIRE ICE THUNDER BIG MP_STABLE\n%EN MAGE_0 !1");
            Assert.Equal(ENATTR.NORMAL, Assert.Single(result.Entries).Attributes & (info.nattr_decline | ENATTR.__OPTIONAL));
        }

        [Fact]
        public void QuestReplacementAndMinimum_AreIncluded()
        {
            var result = Build("%EN SLIME_0 !1", new BattleEnemyPreviewContext { QuestEnemy = ENEMYID.MUSH_0, QuestMinimum = 3 });
            Assert.Equal(3, Assert.Single(result.Entries).Minimum);
            Assert.Equal(ENEMYID.MUSH_0, result.Entries[0].EnemyId);
        }

        [Fact]
        public void QuestAttributes_CanReplaceExistingCategories()
        {
            var result = Build("%ENATTR FIRE ATK\n%EN MUSH_0 !1", new BattleEnemyPreviewContext
            {
                QuestAttributes = ENATTR.ICE, QuestAttributeBudget = true
            });
            var entry = Assert.Single(result.Entries);
            Assert.Equal(ENATTR.ATK, entry.Attributes);
            Assert.Equal(ENATTR.FIRE | ENATTR.ICE, entry.PossibleAttributes);
        }

        [Fact]
        public void KnownBranchesAndPrivateVariables_LeaveInputsUntouched()
        {
            var context = new BattleEnemyPreviewContext();
            context.Variables["saved"] = "original";
            context.Values["PVV"] = 2;
            context.Values["GFC[NOE1]"] = 4;
            var result = Build("saved=changed\nIF 'PVV<=2&&GFC[NOE1]<15' {\n%EN SLIME_TUTORIAL !1\n} ELSE {\n%EN MUSH_0 !9\n}", context);

            Assert.Equal(ENEMYID.SLIME_TUTORIAL, Assert.Single(result.Entries).EnemyId);
            Assert.Equal("original", context.Variables["saved"]);
        }

        [Fact]
        public void Definitions_AndElseIf_SelectOnlyOneBranch()
        {
            var result = Build("n=2\nIFDEF n {\nIF '$n==1' {\n%EN SLIME_0 !1\n} ELSIF '$n==2' {\n%EN MUSH_0 !1\n} ELSE {\n%EN PUPPY_0 !1\n}\n}");
            Assert.Equal(ENEMYID.MUSH_0, Assert.Single(result.Entries).EnemyId);
        }

        [Fact]
        public void FollowerPools_AreConditional_AndDynamicManufacturingIsFlagged()
        {
            var result = Build("%EN GOLEM_0 !1\n%EN_HR _FOLLOW_TEST\n%EN MUSH_0 !4");
            var follower = Entry(result, ENEMYID.MUSH_0);
            Assert.Equal(BattleEnemySource.Follower, follower.Source);
            Assert.Equal(0, follower.Minimum);
            Assert.Equal(4, follower.Maximum);
            Assert.True(result.DynamicReinforcements);
            Assert.Equal(1, result.Minimum);
        }

        [Theory]
        [InlineData("%EN SLIME_0 rand(9)")]
        [InlineData("IF 'rand(3)>1' {\n%EN SLIME_0 !1\n}")]
        [InlineData("%EN SLIME_0 $missing")]
        [InlineData("%EN UNKNOWN_CREATURE !1")]
        [InlineData("%MAX_CNT rand(3)\n%EN SLIME_0 !1")]
        [InlineData("GOTO restart\n%EN SLIME_0 !1")]
        public void UnknownInputs_PreserveCandidateNames_WithoutInventingCounts(string script)
        {
            var result = Build(script);
            Assert.True(result.Incomplete);
            Assert.Null(result.Maximum);
            Assert.NotEmpty(result.Entries);
            Assert.All(result.Entries, entry =>
            {
                Assert.Null(entry.Maximum);
                Assert.False(string.IsNullOrEmpty(entry.UndeterminedReason));
            });
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void MissingScripts_HaveUnknownTotals(string script)
        {
            var result = Build(script);
            Assert.True(result.Incomplete);
            Assert.Null(result.Maximum);
        }

        [Fact]
        public void NextScriptsAndRescue_DoNotClaimAnExactWholeBattleTotal()
        {
            var next = Build("%EN SLIME_0 !1\n%NEXT_SCRIPT later");
            var rescue = Build("%EN SLIME_0 !1", new BattleEnemyPreviewContext { SpecialBattle = true });
            Assert.Null(next.Maximum);
            Assert.True(next.DynamicReinforcements);
            Assert.Null(rescue.Maximum);
        }

        [Fact]
        public void Preview_DoesNotConsumeEitherGameRandomStream_OrMoveAnExistingReader()
        {
            var random = (XorsMaker)typeof(X).GetField("Xors", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).GetValue(null);
            var beforeGlobal = new uint[4];
            var beforeNight = new uint[4];
            random.saveRandSeeds(beforeGlobal);
            NightController.Xors.saveRandSeeds(beforeNight);
            var variables = new CsvVariableContainer(new[] { new CsvVariableItem { name = "n", val = "3" } });
            var reader = new CsvReaderA("%EN SLIME_0 !3", variables);
            reader.seek_set(1);
            int line = reader.get_cur_line();
            var context = new BattleEnemyPreviewContext { AdditionalCount = 4, ThunderCapacity = 2, AttributeDanger = 100, AttributeBudget = 5 };
            context.Variables["n"] = variables.Get("n");

            Build("n=4\n%EN SLIME_0 $n\n%EN MUSH_0 2", context);
            Build("IF 'rand(5)>1' {\n%EN SLIME_0 rand(3)\n}", context);

            var after = new uint[4];
            random.saveRandSeeds(after);
            Assert.Equal(beforeGlobal, after);
            NightController.Xors.saveRandSeeds(after);
            Assert.Equal(beforeNight, after);
            Assert.Equal(line, reader.get_cur_line());
            Assert.Equal("3", variables.Get("n"));
        }

        [Theory]
        [InlineData(LanguageType.Chinese)]
        [InlineData(LanguageType.English)]
        public void Formatter_ShowsCountsAttributesAndUnknowns(LanguageType language)
        {
            var original = Translator.DefaultLanguage;
            try
            {
                Translator.DefaultLanguage = language;
                var text = BattleEnemyPreviewPatch.Format(Build(DarkCave));
                Assert.Contains("× 2", text);
                Assert.Contains("× 1", text);
                Assert.DoesNotContain("MUSH_0", text);
                var unknown = BattleEnemyPreviewPatch.Format(Build("%EN UNKNOWN_CREATURE rand(4)"));
                Assert.Contains("UNKNOWN_CREATURE", unknown);
                Assert.DoesNotContain("× 0", unknown);
            }
            finally { Translator.DefaultLanguage = original; }
        }

        [Fact]
        public void LayoutRewrite_IsScopedAndFailsClosedOnUnsupportedLayout()
        {
            var method = typeof(M2BoxOneLine).GetMethod(nameof(M2BoxOneLine.fineBoxPosOnMapWH));
            var original = Enumerable.Range(0, 4).Select(_ => new CodeInstruction(OpCodes.Call, method)).ToList();
            Assert.True(BattleEnemyPreviewPatch.TryRewriteLayout(original, out var rewritten));
            Assert.Equal(4, rewritten.Count(code => code.opcode == OpCodes.Ldarg_0));
            Assert.False(BattleEnemyPreviewPatch.TryRewriteLayout(original.Take(3).ToList(), out _));
            Assert.All(original, code => Assert.True(code.Calls(method)));
            Assert.Equal(typeof(ConfigEntry<bool>), typeof(ConfigManager).GetProperty(nameof(ConfigManager.EnableBattleEnemyPreview)).PropertyType);
        }

        [Fact]
        public void ScrollbarOpacity_DelayedPanelHidesInitiallyOpaqueSkinImmediately()
        {
            var skin = (RecordingScrollSkin)RuntimeHelpers.GetUninitializedObject(typeof(RecordingScrollSkin));
            skin.alpha = 1f;
            skin.Fine();

            BattleEnemyPreviewPatch.SetScrollBarAlpha(skin, 0f, 0xCC646566, 0xEEC8C9CA);

            Assert.Equal(0f, skin.RenderedAlpha);
            Assert.Equal(0, skin.RenderedNormalAlpha);
            Assert.Equal(0, skin.RenderedPushedAlpha);
            Assert.Equal(2, skin.DrawCount);
        }

        [Fact]
        public void ScrollbarOpacity_FollowsFadeAndReopening_WithoutWaitingForButtonUpdate()
        {
            var skin = (RecordingScrollSkin)RuntimeHelpers.GetUninitializedObject(typeof(RecordingScrollSkin));
            foreach (float alpha in new[] { 0f, 0.1f, 0.5f, 0.995f, 0.4f, 0f, 0f, 0.2f })
            {
                BattleEnemyPreviewPatch.SetScrollBarAlpha(skin, alpha, 0xCC646566, 0xEEC8C9CA);
                Assert.Equal(alpha, skin.RenderedAlpha);
                Assert.Equal((byte)(204 * alpha), skin.RenderedNormalAlpha);
                Assert.Equal((byte)(238 * alpha), skin.RenderedPushedAlpha);
            }
            Assert.Equal(8, skin.DrawCount);
        }

        // 跳过依赖 Unity 对象的构造，只记录实际重绘时采用的透明度。
        private sealed class RecordingScrollSkin : ButtonSkinMeterScroll
        {
            private RecordingScrollSkin() : base(null) { }
            internal float RenderedAlpha;
            internal byte RenderedNormalAlpha;
            internal byte RenderedPushedAlpha;
            internal int DrawCount;

            public override ButtonSkin Fine()
            {
                RenderedAlpha = alpha;
                RenderedNormalAlpha = ((UnityEngine.Color32)typeof(ButtonSkinMeterScroll).GetField("BarColor", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this)).a;
                RenderedPushedAlpha = ((UnityEngine.Color32)typeof(ButtonSkinMeterScroll).GetField("BarPushedColor", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this)).a;
                DrawCount++;
                return this;
            }
        }

        [Theory]
        [InlineData(0, 3, 0)]
        [InlineData(0, 3, 2)]
        [InlineData(1, 3, 1)]
        [InlineData(2, 3, 3)]
        [InlineData(3, 4, 2)]
        [InlineData(99, 4, 2)]
        public void RangesContainEveryAllocationAndTruncation_ForSmallBattles(int cap, int extra, int thunder)
        {
            var preview = Build($"%MAX_CNT !{cap}\n%EN SLIME_0 2\n%EN MUSH_0 2", new BattleEnemyPreviewContext
            {
                AdditionalCount = extra, ThunderCapacity = thunder
            });
            // 枚举所有可能的编排结果。原版先分配污染体，再保留最多 cap + OD 只普通体，
            // 最后放回不参与裁减的污染体；这里不依赖随机抽样。
            for (int extraSlime = 0; extraSlime <= extra; extraSlime++)
            {
                int slime = 2 + extraSlime;
                int mush = 2 + extra - extraSlime;
                int od = Math.Min(thunder, slime + mush);
                for (int slimeOd = Math.Max(0, od - mush); slimeOd <= Math.Min(od, slime); slimeOd++)
                {
                    int mushOd = od - slimeOd;
                    int retained = Math.Min(slime + mush - od, cap + od);
                    for (int slimeRetained = Math.Max(0, retained - (mush - mushOd)); slimeRetained <= Math.Min(retained, slime - slimeOd); slimeRetained++)
                    {
                        AssertBound(preview, ENEMYID.SLIME_0, false, slimeRetained);
                        AssertBound(preview, ENEMYID.MUSH_0, false, retained - slimeRetained);
                        AssertBound(preview, ENEMYID.SLIME_0, true, slimeOd);
                        AssertBound(preview, ENEMYID.MUSH_0, true, mushOd);
                        Assert.InRange(retained + od, preview.Minimum, preview.Maximum.Value);
                    }
                }
            }
        }

        private static void AssertBound(BattleEnemyPreviewSnapshot preview, ENEMYID id, bool od, int count)
        {
            var entry = preview.Entries.SingleOrDefault(entry => entry.EnemyId == id && entry.Overdrive == od);
            Assert.InRange(count, entry?.Minimum ?? 0, entry?.Maximum ?? 0);
        }

        [Theory]
        [InlineData("0.5", 0)]
        [InlineData("1.5", 1)]
        [InlineData("0&&0||1", 0)]
        [InlineData("(0&&0)||1", 1)]
        public void SpawnConditions_MatchNativeIntegerAndLogicalEvaluation(string condition, int count)
        {
            var result = Build($"%EN SLIME_0 !1 0 0 '{condition}'");
            Assert.Equal(count, result.Maximum);
        }
    }
}
