using BetterExperience.Patches;
using nel;

namespace BetterExperience.Test.Patches
{
    public sealed class PortraitPreviewTests
    {
        private readonly object owner = new object();
        private readonly PortraitPreviewSession preview = new PortraitPreviewSession();
        private readonly PortraitSelection original = new PortraitSelection(UIEMOT.STAND,
            UIPictureBase.EMSTATE.TORNED, UIPictureBase.EMSTATE_ADD.FEAR);
        private readonly PortraitSelection bench = new PortraitSelection(UIEMOT.BENCH);
        private readonly PortraitSelection down = new PortraitSelection(UIEMOT.DOWN);
        private readonly List<PortraitSelection> applied = new List<PortraitSelection>();

        private PortraitPreviewResult Tick(float now, PortraitPreviewReadiness ready = PortraitPreviewReadiness.Ready) =>
            preview.Tick(now, value => value, (value, _) => applied.Add(value), () => ready);

        [Fact]
        public void Preview_TimesOnlyActualReplacementDisplayAndRestoresOriginalStates()
        {
            preview.Begin(owner, original, bench, 0);
            Assert.Equal(PortraitPreviewResult.Loading, preview.Tick(1, _ => null,
                (_, _) => Assert.Fail("Game resources are still loading."), () => PortraitPreviewReadiness.Ready));
            Assert.Empty(applied);
            Assert.Equal(PortraitPreviewResult.Loading, Tick(5, PortraitPreviewReadiness.Loading));
            Assert.Equal(PortraitPreviewResult.Loading, Tick(6, PortraitPreviewReadiness.Loading));
            Assert.Empty(applied); // 首次启用不能先显示该姿态的原版。
            Assert.Equal(PortraitPreviewResult.Showing, Tick(7));
            Assert.Equal(PortraitPreviewResult.Showing, Tick(8.9f));
            Assert.Single(applied);
            Assert.Equal(PortraitPreviewResult.Restored, Tick(9));
            Assert.Equal(new[] { bench, original }, applied);
            Assert.False(preview.Active);
            Assert.False(preview.Control.Blocks(owner));
            Assert.Null(preview.Control.OverrideFor(owner));
        }

        [Fact]
        public void Preview_RapidEnableKeepsFirstOriginalAndResetsLatestTimer()
        {
            preview.Begin(owner, original, bench, 0);
            Tick(0);
            preview.Begin(owner, bench, down, 1);
            Assert.Equal(original, preview.Original);
            Tick(1, PortraitPreviewReadiness.Loading);
            Assert.Equal(new[] { bench }, applied);
            Tick(2);
            Assert.Equal(PortraitPreviewResult.Showing, Tick(3));
            Assert.Equal(PortraitPreviewResult.Restored, Tick(4));
            Assert.Equal(new[] { bench, down, original }, applied);
        }

        [Fact]
        public void Preview_RestoreWhileLoadingDiscardsPendingPose()
        {
            preview.Begin(owner, original, bench, 0);
            preview.Restore(1);
            Assert.Equal(PortraitPreviewResult.Restored, Tick(1));
            Assert.Equal(new[] { original }, applied);
            Assert.Equal(PortraitPreviewResult.None, Tick(2));
        }

        [Fact]
        public void Preview_RestorationWaitsForOriginalResourceAndThenReleasesControl()
        {
            preview.Begin(owner, original, bench, 0);
            Tick(0);
            Assert.Equal(PortraitPreviewResult.Loading, preview.Tick(2, _ => null,
                (_, _) => Assert.Fail("Original resources are still loading."), () => PortraitPreviewReadiness.Ready));
            Assert.True(preview.Restoring);
            Assert.True(preview.Control.Blocks(owner));
            Assert.Equal(PortraitPreviewResult.Restored, Tick(3));
            Assert.Equal(original, applied.Last());
            Assert.False(preview.Control.Blocks(owner));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Preview_ResourceFailureOrTimeoutRestoresOriginal(bool timeout)
        {
            preview.Begin(owner, original, bench, 0);
            Tick(0, PortraitPreviewReadiness.Loading);
            Tick(timeout ? PortraitControlSession.LoadTimeout : 1,
                timeout ? PortraitPreviewReadiness.Loading : PortraitPreviewReadiness.Failed);
            Assert.True(preview.Restoring);
            Assert.Equal(PortraitPreviewResult.Failed, Tick(16));
            Assert.False(preview.Active);
            Assert.False(preview.RestoreFailed);
            Assert.NotNull(preview.Error);
            Assert.Equal(original, applied.Last());
        }

        [Fact]
        public void Preview_ApplyFailureReturnsToOriginalAndDoesNotKeepTemporaryLock()
        {
            preview.Begin(owner, original, bench, 0);
            preview.Tick(0, value => value, (_, _) => throw new InvalidOperationException("drawing failed"),
                () => PortraitPreviewReadiness.Ready);
            Assert.Equal(PortraitPreviewResult.Failed, Tick(1));
            Assert.Equal(new[] { original }, applied);
            Assert.False(preview.Control.Blocks(owner));
        }

        [Fact]
        public void Preview_RestoreTimeoutReportsNeedForGameFallback()
        {
            preview.Begin(owner, original, bench, 0);
            Tick(0);
            preview.Restore(1);
            var result = preview.Tick(16, _ => null, (_, _) => Assert.Fail("Not ready"),
                () => PortraitPreviewReadiness.Ready);
            Assert.Equal(PortraitPreviewResult.Failed, result);
            Assert.True(preview.RestoreFailed);
            Assert.False(preview.Active);
        }

        [Fact]
        public void Preview_ResetAndOwnerChangeDiscardOldRequests()
        {
            preview.Begin(owner, original, bench, 0);
            Tick(0);
            preview.Reset();
            Assert.Equal(PortraitPreviewResult.None, Tick(10));
            Assert.Single(applied);
            var secondOwner = new object();
            preview.Begin(secondOwner, down, bench, 11);
            Assert.False(preview.Control.Blocks(owner));
            Assert.True(preview.Control.Blocks(secondOwner));
            Assert.Equal(down, preview.Original);
        }

        [Fact]
        public void Preview_RequestReplacedDuringPreparationCannotApplyOrStartOldTimer()
        {
            preview.Begin(owner, original, bench, 0);
            var result = preview.Tick(0, value => { preview.Begin(owner, bench, down, 0); return value; },
                (_, _) => Assert.Fail("Stale preview cannot apply."), () => PortraitPreviewReadiness.Ready);
            Assert.Equal(PortraitPreviewResult.None, result);
            Tick(0);
            Assert.Equal(new[] { down }, applied);
            Assert.Equal(original, preview.Original);
        }

        [Fact]
        public void Preview_PreparedGamePoseCannotApplyWhileReplacementIsStillLoading()
        {
            preview.Begin(owner, original, bench, 0);
            for (int frame = 0; frame < 5; frame++)
            {
                Assert.Equal(PortraitPreviewResult.Loading, Tick(frame, PortraitPreviewReadiness.Loading));
                Assert.Empty(applied);
                Assert.True(preview.Control.Blocks(owner));
            }
            Assert.Equal(PortraitPreviewResult.Showing, Tick(5));
            Assert.Equal(new[] { bench }, applied);
        }

        [Fact]
        public void Preview_SamePoseForNewResourceStillCommitsNewPreparedContent()
        {
            preview.Begin(owner, original, bench, 0);
            Tick(0);
            preview.Begin(owner, bench, bench, 1);
            Tick(1);
            Assert.Equal(new[] { bench, bench }, applied);
            Assert.Equal(original, preview.Original);
            Assert.Equal(PortraitPreviewResult.Restored, Tick(3));
        }

        [Fact]
        public void Preview_ReplacementPreparationFailureNeverDisplaysFallbackPose()
        {
            preview.Begin(owner, original, bench, 0);
            Tick(0, PortraitPreviewReadiness.Failed);
            Assert.Empty(applied);
            Assert.True(preview.Restoring);
            Tick(1);
            Assert.Equal(new[] { original }, applied);
        }

        [Fact]
        public void Preview_SupersededDuringResourcePreparationCannotCommitOldResources()
        {
            preview.Begin(owner, original, bench, 0);
            var result = preview.Tick(0, value => value, (_, _) => Assert.Fail("Obsolete preview cannot apply."), () =>
            {
                preview.Begin(owner, bench, down, 1);
                return PortraitPreviewReadiness.Ready;
            });
            Assert.Equal(PortraitPreviewResult.None, result);
            Tick(1);
            Assert.Equal(new[] { down }, applied);
            Assert.Equal(original, preview.Original);
        }

        [Fact]
        public void Preview_OwnCallsBypassManualLockWithoutChangingPanelLock()
        {
            var picture = new object();
            var other = new object();
            var manual = new PortraitControlSession();
            var temporary = new PortraitPreviewSession();
            var show = new PortraitSelection(UIEMOT.BENCH, 0, UIPictureBase.EMSTATE_ADD.FROZEN);
            manual.Bind(picture);
            manual.Queue(original, true, 0);
            manual.Tick(0, value => value, (_, _) => { });
            temporary.Begin(picture, original, show, 0);
            Assert.True(manual.LockRequested);
            Assert.True(temporary.ControlFor(picture, manual).Blocks(picture));
            Assert.False(temporary.ControlFor(other, manual).Blocks(other));
            temporary.Tick(0, value => value, (_, _) =>
            {
                var control = temporary.ControlFor(picture, manual);
                Assert.False(control.Blocks(picture));
                Assert.Equal(show.Additional, control.OverrideFor(picture)?.Additional);
            }, () => PortraitPreviewReadiness.Ready);
            temporary.Tick(2, value => value, (_, _) => { }, () => PortraitPreviewReadiness.Ready);
            Assert.True(manual.LockRequested);
            Assert.Same(manual, temporary.ControlFor(picture, manual));
            Assert.Equal(original.Additional, temporary.ControlFor(picture, manual).OverrideFor(picture)?.Additional);
            Assert.Equal(original, manual.Active);
            Assert.Equal(original, manual.RequestedActive);
        }

        [Fact]
        public void Preview_RejectsMissingOwnerAndInvalidPosesWithoutStarting()
        {
            Assert.Throws<ArgumentNullException>(() => preview.Begin(null, original, bench, 0));
            Assert.Throws<ArgumentException>(() => preview.Begin(owner, original, new PortraitSelection(UIEMOT._OFFLINE), 0));
            Assert.False(preview.Active);
        }
    }
}
