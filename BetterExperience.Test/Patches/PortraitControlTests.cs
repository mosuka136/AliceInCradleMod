using BetterExperience.Patches;
using nel;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace BetterExperience.Test.Patches
{
    public class PortraitControlTests
    {
        private static FieldInfo Field(Type type, string name) => type.GetField(name,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        private static MethodInfo Method(Type type, string name) => type.GetMethods(
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).Single(method => method.Name == name);
        [Theory]
        [InlineData(UIEMOT.STAND, true)]
        [InlineData(UIEMOT.BENCH, true)]
        [InlineData(UIEMOT.DOWN_B, true)]
        [InlineData(UIEMOT.TORTURE_SLIME_0, true)]
        [InlineData(UIEMOT.CUTS_COW_0, false)]
        [InlineData(UIEMOT.CUTS_LAYEGG, false)]
        [InlineData(UIEMOT._OFFLINE, false)]
        [InlineData(UIEMOT.__MAX, false)]
        [InlineData((UIEMOT)1000, false)]
        public void MainPoses_ExcludeSentinelsAndCutins(UIEMOT pose, bool expected)
            => Assert.Equal(expected, PortraitControlLogic.IsMainPose(pose));

        [Fact]
        public void EditableFlags_KeepIndependentStatesAndExcludeSystemBits()
        {
            var result = PortraitControlLogic.Editable(new PortraitSelection(UIEMOT.STAND,
                UIPictureBase.EMSTATE.TORNED | UIPictureBase.EMSTATE.LOWHP | UIPictureBase.EMSTATE.SP_SENSITIVE,
                UIPictureBase.EMSTATE_ADD.FROZEN | UIPictureBase.EMSTATE_ADD.SENSITIVE | UIPictureBase.EMSTATE_ADD._ALL));
            Assert.Equal(UIPictureBase.EMSTATE.TORNED | UIPictureBase.EMSTATE.LOWHP, result.State);
            Assert.Equal(UIPictureBase.EMSTATE_ADD.FROZEN, result.Additional);
        }

        [Fact]
        public void FlagRows_CanCombineAndClearFlags_RejectEditedLabels()
        {
            var rows = PortraitControlLogic.FlagRows(typeof(UIPictureBase.EMSTATE), PortraitControlLogic.MainMask, 0);
            Assert.DoesNotContain(rows, row => row.Display.Contains("_ALL") || row.Display.Contains("SP_SENSITIVE"));
            for (int i = 0; i < rows.Count; i++)
                rows[i] = (rows[i].Display, rows[i].Display.Contains("[LOWHP]") || rows[i].Display.Contains("[TORNED]"));
            uint expected = (uint)(UIPictureBase.EMSTATE.LOWHP | UIPictureBase.EMSTATE.TORNED);
            Assert.Equal(expected, PortraitControlLogic.ReadFlags(typeof(UIPictureBase.EMSTATE), PortraitControlLogic.MainMask, 0, rows));
            rows[0] = ("edited", true);
            Assert.Equal(expected, PortraitControlLogic.ReadFlags(typeof(UIPictureBase.EMSTATE), PortraitControlLogic.MainMask, expected, rows));
            Assert.Equal(0u, PortraitControlLogic.ReadFlags(typeof(UIPictureBase.EMSTATE), PortraitControlLogic.MainMask, expected,
                PortraitControlLogic.FlagRows(typeof(UIPictureBase.EMSTATE), PortraitControlLogic.MainMask, 0)));
        }

        [Fact]
        public void SingleSelection_SelectsNewTick_RejectsNullStaleAndReorderedRows()
        {
            string[] labels = { "站立 [STAND]", "坐姿 [BENCH]" };
            Assert.Equal(1, PortraitControlLogic.SelectOne(new[] { (labels[0], true), (labels[1], true) }, labels, 0));
            Assert.Equal(-1, PortraitControlLogic.SelectOne(new[] { (labels[0], false), (labels[1], false) }, labels, 0));
            Assert.Equal(0, PortraitControlLogic.SelectOne(null, labels, 0));
            Assert.Equal(0, PortraitControlLogic.SelectOne(new[] { (labels[0], false) }, labels, 0));
            Assert.Equal(0, PortraitControlLogic.SelectOne(new[] { (labels[1], true), (labels[0], false) }, labels, 0));
        }

        [Fact]
        public void Presets_DeduplicateNormalizedValuesWithoutLosingAdditionalStates()
        {
            var normal = new PortraitSelection(UIEMOT.STAND);
            var fear = new PortraitSelection(UIEMOT.STAND, 0, UIPictureBase.EMSTATE_ADD.FEAR);
            var presets = PortraitControlLogic.Presets(UIEMOT.STAND, new[]
            {
                normal, normal, fear, new PortraitSelection(UIEMOT.BENCH),
                new PortraitSelection(UIEMOT.STAND, UIPictureBase.EMSTATE.SP_SENSITIVE)
            });
            Assert.Equal(new[] { normal, fear }, presets);
            Assert.Empty(PortraitControlLogic.Presets(UIEMOT.STAND, null));
        }

        [Fact]
        public void Catalog_RequiresDrawableBody_CombinesRegisteredAndViewerStates()
        {
            var body = (UIPictureBodySpine)RuntimeHelpers.GetUninitializedObject(typeof(UIPictureBodySpine));
            Field(typeof(UIPictureBodySpine), "Spv").SetValue(body,
                RuntimeHelpers.GetUninitializedObject(typeof(SpineViewerNel)));
            body.AViewerEntrySrc.Add(new UIPictureBodySpine.ViewerEntry(UIPictureBase.EMSTATE.LOWHP, UIPictureBase.EMSTATE_ADD.FEAR));
            var emotion = new UIPictureBase.PrEmotion(UIEMOT.STAND, null, null);
            emotion.Default = new UIPictureBase.PrEmotion.BodyPaint { Body = body };
            emotion.ODrawer.Add(0, emotion.Default);
            emotion.ODrawer.Add(UIPictureBase.EMSTATE.TORNED, emotion.Default);
            var empty = new UIPictureBase.PrEmotion(UIEMOT.BENCH, null, null);
            var cutin = new UIPictureBase.PrEmotion(UIEMOT.CUTS_COW_0, null, null) { Default = emotion.Default };
            cutin.ODrawer.Add(0, emotion.Default);
            var catalog = PortraitControlCatalog.Build(new[] { null, emotion, empty, cutin });
            Assert.Single(catalog);
            Assert.Equal(UIEMOT.STAND, catalog[0].Pose);
            Assert.Equal(3, catalog[0].Presets.Count);
            Assert.Contains(new PortraitSelection(UIEMOT.STAND, UIPictureBase.EMSTATE.LOWHP, UIPictureBase.EMSTATE_ADD.FEAR), catalog[0].Presets);
            Assert.Empty(PortraitControlCatalog.Build(null));
        }

        [Fact]
        public void AdditionalSupport_CombinesLayerAndSpineRules_ExcludesSystemBits()
        {
            var body = (UIPictureBodySpine)RuntimeHelpers.GetUninitializedObject(typeof(UIPictureBodySpine));
            body.Alay_attr = new[] { UIPictureBase.EMSTATE_ADD.FROZEN, UIPictureBase.EMSTATE_ADD.SENSITIVE };
            var animationField = Field(typeof(UIPictureBodySpine), "AAnim");
            var list = (System.Collections.IList)Activator.CreateInstance(animationField.FieldType);
            var variationType = animationField.FieldType.GetGenericArguments()[0];
            var variation = Activator.CreateInstance(variationType);
            Field(variationType, "st_add").SetValue(variation, UIPictureBase.EMSTATE_ADD.FEAR);
            list.Add(variation);
            animationField.SetValue(body, list);
            Assert.Equal((uint)(UIPictureBase.EMSTATE_ADD.FROZEN | UIPictureBase.EMSTATE_ADD.FEAR), PortraitControlCatalog.AdditionalMask(body));
        }

        [Fact]
        public void SamePoseAndState_DifferentAdditionalFlagsAreDifferentSelections()
        {
            Assert.NotEqual(new PortraitSelection(UIEMOT.STAND), new PortraitSelection(UIEMOT.STAND, 0, UIPictureBase.EMSTATE_ADD.FROZEN));
        }

        [Fact]
        public void PatchTargets_ExistWithExpectedInterfacesInReferencedGame()
        {
            Assert.NotNull(Method(typeof(COOK), "readBinaryContent"));
            Assert.NotNull(Method(typeof(COOK), nameof(COOK.newGame)));
            Assert.NotNull(Field(typeof(UIPictureBase), "pre_sta"));
            foreach (string field in new[] { "Next", "t_ground", "t", "t_lock", "timeout_skip" })
                Assert.NotNull(Field(typeof(UIPictureFader), field));
            foreach (string method in new[] { "run", "runGroundLevel", "Explode" })
            {
                var target = Method(typeof(UIPictureFader), method);
                Assert.Equal(typeof(UIPictureFader.UIP_RES), target.ReturnType);
                Assert.Contains(target.GetParameters(), parameter => parameter.Name == "PCon" && parameter.ParameterType == typeof(UIPictureBase));
            }
        }
    }

    public class PortraitControlSessionTests
    {
        private readonly object owner = new object();
        private readonly PortraitControlSession session = new PortraitControlSession();
        private readonly PortraitSelection stand = new PortraitSelection(UIEMOT.STAND);
        private readonly PortraitSelection bench = new PortraitSelection(UIEMOT.BENCH);

        public PortraitControlSessionTests() => session.Bind(owner);
        private PortraitApplyResult Apply(float now = 0) => session.Tick(now, value => value, (_, _) => { });

        [Fact]
        public void Loading_ProtectsOnlyOwner_LeavesPreviousDisplayUntilReady()
        {
            session.Queue(bench, true, 0);
            bool applied = false;
            Assert.Equal(PortraitApplyResult.Loading, session.Tick(1, _ => null, (_, _) => applied = true));
            Assert.False(applied);
            Assert.Null(session.Active);
            Assert.False(session.Locked);
            Assert.True(session.LockRequested);
            Assert.True(session.Blocks(owner));
            Assert.False(session.Blocks(new object()));
            Assert.False(session.Blocks(null));
            Assert.Equal(PortraitApplyResult.Applied, Apply(2));
            Assert.Equal(bench, session.Active);
            Assert.True(session.Locked);
        }

        [Fact]
        public void NewRequest_SupersedesLoadingRequest()
        {
            session.Queue(stand, true, 0);
            session.Tick(0, _ => null, (_, _) => Assert.Fail("Loading cannot apply."));
            session.Queue(bench, true, 1);
            PortraitSelection? applied = null;
            session.Tick(1, value => value, (value, _) => applied = value);
            Assert.Equal(bench, applied);
        }

        [Fact]
        public void RequestReplacedDuringPreparation_CannotCommitStaleSelection()
        {
            session.Queue(stand, true, 0);
            var result = session.Tick(0, value => { session.Queue(bench, true, 0); return value; },
                (_, _) => Assert.Fail("Superseded request cannot apply."));
            Assert.Equal(PortraitApplyResult.None, result);
            Assert.Null(session.Active);
            Assert.True(session.HasPending);
            Apply();
            Assert.Equal(bench, session.Active);
        }

        [Fact]
        public void ApplyScope_AllowsOwnSwitchAndProvidesAdditionalOverride()
        {
            var selected = new PortraitSelection(UIEMOT.STAND, 0, UIPictureBase.EMSTATE_ADD.FEAR);
            session.Queue(selected, true, 0);
            session.Tick(0, value => value, (value, _) =>
            {
                Assert.False(session.Blocks(owner));
                Assert.Equal(selected, session.OverrideFor(owner));
                Assert.Null(session.OverrideFor(new object()));
            });
            Assert.True(session.Blocks(owner));
            Assert.Equal(selected, session.OverrideFor(owner));
            Assert.Null(session.Applying);
        }

        [Fact]
        public void UnchangedLockedSelection_DoesNotRestartAnimation_AdditionalEditDoes()
        {
            int count = 0;
            void Tick() => session.Tick(0, value => value, (_, _) => count++);
            session.Queue(stand, true, 0);
            Tick();
            session.Queue(stand, true, 0);
            Tick();
            session.Tick(1, _ => throw new Exception("No work expected"), (_, _) => count++);
            Assert.Equal(1, count);
            var edited = new PortraitSelection(UIEMOT.STAND, 0, UIPictureBase.EMSTATE_ADD.FEAR);
            session.Queue(edited, true, 0);
            Tick();
            Assert.Equal(2, count);
            Assert.Equal(edited, session.Active);
        }

        [Fact]
        public void ResourceRefresh_ReappliesEvenWhenSelectionIsUnchanged()
        {
            session.Queue(stand, true, 0);
            Apply();
            session.Queue(stand, true, 0, force: true);
            bool called = false;
            session.Tick(0, value => value, (_, force) => { called = true; Assert.True(force); });
            Assert.True(called);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void LoadOrApplyFailure_PreservesLastSuccessfulLock(bool applyFails)
        {
            session.Queue(stand, true, 0);
            Apply();
            session.Queue(bench, true, 0);
            var result = session.Tick(0, value => applyFails ? value : throw new InvalidOperationException("prepare"),
                (_, _) => throw new InvalidOperationException("apply"));
            Assert.Equal(PortraitApplyResult.Failed, result);
            Assert.Equal(stand, session.Active);
            Assert.True(session.Locked);
            Assert.True(session.Blocks(owner));
            Assert.Null(session.Applying);
            Assert.False(session.HasPending);
            Assert.NotNull(session.Error);
        }

        [Fact]
        public void LoadingTimeout_ReleasesFirstRequestWithoutInventingActiveLock()
        {
            session.Queue(stand, true, 2);
            var result = session.Tick(2 + PortraitControlSession.LoadTimeout, _ => null, (_, _) => Assert.Fail("Not loaded"));
            Assert.Equal(PortraitApplyResult.Failed, result);
            Assert.IsType<TimeoutException>(session.Error);
            Assert.False(session.Blocks(owner));
            Assert.False(session.LockRequested);
            Assert.Null(session.Active);
        }

        [Fact]
        public void ApplyOnce_ReleasesControl_AndCanReapplySameValueLater()
        {
            int applied = 0;
            for (int i = 0; i < 2; i++)
            {
                session.Queue(stand, false, i);
                session.Tick(i, value => value, (_, _) => applied++);
                Assert.False(session.Blocks(owner));
                Assert.Null(session.OverrideFor(owner));
            }
            Assert.Equal(2, applied);
        }

        [Fact]
        public void Reset_CancelsLoadingAndClearsLockAndOverrides()
        {
            session.Queue(stand, true, 0);
            Apply();
            session.Queue(bench, true, 0);
            session.Reset();
            Assert.False(session.HasPending);
            Assert.False(session.Locked);
            Assert.Null(session.Owner);
            Assert.Null(session.Active);
            Assert.Null(session.RequestedActive);
            Assert.Null(session.OverrideFor(owner));
            Assert.Equal(PortraitApplyResult.None, Apply());
        }

        [Fact]
        public void NormalizedResultIsReportedButOriginalRequestRemainsAvailableForRefresh()
        {
            var requested = new PortraitSelection(UIEMOT.STAND, UIPictureBase.EMSTATE.TORNED);
            session.Queue(requested, true, 0);
            session.Tick(0, _ => stand, (_, _) => { });
            Assert.Equal(stand, session.Active);
            Assert.Equal(requested, session.RequestedActive);
        }

        [Fact]
        public void OwnerReplacement_ClearsPreviousSession()
        {
            session.Queue(stand, true, 0);
            Apply();
            object replacement = new object();
            session.Bind(replacement);
            Assert.Same(replacement, session.Owner);
            Assert.False(session.Blocks(owner));
            Assert.Null(session.Active);
            Assert.False(session.Locked);
        }

        [Fact]
        public void InvalidPoseOrMissingOwner_RejectsRequest()
        {
            Assert.Throws<ArgumentException>(() => session.Queue(new PortraitSelection(UIEMOT._OFFLINE), true, 0));
            session.Reset();
            Assert.Throws<InvalidOperationException>(() => session.Queue(stand, true, 0));
        }
    }
}
