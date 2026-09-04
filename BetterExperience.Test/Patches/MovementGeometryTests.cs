using BetterExperience.Patches;
using UnityEngine;

namespace BetterExperience.Test.Patches
{
    public class MovementGeometryTests
    {
        [Theory]
        [InlineData(1280, 720)]
        [InlineData(1920, 1080)]
        [InlineData(2560, 1440)]
        [InlineData(3840, 2160)]
        public void MouseProjection_UsesWindowViewportInsteadOfRenderTexturePixels(int width, int height)
        {
            // 同一相对鼠标位置在不同窗口尺寸下必须得到相同目标；与内部渲染分辨率无关。
            var rect = new Rect(0f, 0f, width, height);
            Assert.True(MovementGeometry.TryScreenToViewport(new Vector2(width * 0.5f, height * 0.5f), rect, out var center));
            Assert.Equal(new Vector2(0.5f, 0.5f), center);
            Assert.True(MovementGeometry.TryScreenToViewport(new Vector2(width * 0.75f, height * 0.25f), rect, out var corner));
            Assert.Equal(new Vector2(0.75f, 0.25f), corner);
        }

        [Fact]
        public void MouseProjection_AccountsForViewportOffsetAndRejectsBlackBars()
        {
            var rect = new Rect(320f, 180f, 1280f, 720f);
            Assert.True(MovementGeometry.TryScreenToViewport(new Vector2(960f, 540f), rect, out var center));
            Assert.Equal(new Vector2(0.5f, 0.5f), center);
            Assert.False(MovementGeometry.TryScreenToViewport(new Vector2(319f, 540f), rect, out _));
            Assert.False(MovementGeometry.TryScreenToViewport(new Vector2(960f, 900f), rect, out _));
            Assert.False(MovementGeometry.TryScreenToViewport(new Vector2(float.NaN, 540f), rect, out _));
            Assert.False(MovementGeometry.TryScreenToViewport(Vector2.zero, new Rect(0f, 0f, 0f, 720f), out _));
        }

        [Theory]
        [InlineData(0, 0, true)]
        [InlineData(1919, 1079, true)]
        [InlineData(1920, 500, false)]
        [InlineData(500, 1080, false)]
        [InlineData(-1, 20, false)]
        [InlineData(20, -1, false)]
        [InlineData(float.NaN, 20, false)]
        [InlineData(20, float.PositiveInfinity, false)]
        public void ScreenCheck_RejectsOutsideAndInvalidCoordinates(float x, float y, bool expected)
        {
            Assert.Equal(expected, MovementGeometry.IsOnScreen(new Vector2(x, y), 1920, 1080));
        }

        [Theory]
        [InlineData(0.5f, 1f, true)]
        [InlineData(19.5f, 9f, true)]
        [InlineData(0.4f, 1f, false)]
        [InlineData(19.6f, 9f, false)]
        [InlineData(10f, 9.1f, false)]
        [InlineData(10f, 0.9f, false)]
        [InlineData(float.PositiveInfinity, 5f, false)]
        public void MapCheck_UsesFullBodyNotOnlyCenter(float x, float y, bool expected)
        {
            Assert.Equal(expected, MovementGeometry.IsInsideMap(new Vector2(x, y), 0.5f, 1f, 20, 10));
        }

        [Theory]
        [InlineData(0f)]
        [InlineData(-1f)]
        [InlineData(float.NaN)]
        [InlineData(float.PositiveInfinity)]
        public void InvalidBodySize_IsRejected(float size)
        {
            Assert.False(MovementGeometry.IsInsideMap(new Vector2(5f, 5f), size, 1f, 20, 10));
        }

        [Fact]
        public void FlightStep_HasEqualDiagonalSpeedAndPreservesAnalogInput()
        {
            var horizontal = MovementGeometry.FlightStep(new Vector2(1f, 0f), 12f, 1f);
            var diagonal = MovementGeometry.FlightStep(new Vector2(1f, -1f), 12f, 1f);
            var analog = MovementGeometry.FlightStep(new Vector2(0.5f, 0f), 12f, 1f);

            Assert.Equal(0.2f, horizontal.x, 5);
            Assert.Equal(horizontal.sqrMagnitude, diagonal.sqrMagnitude, 5);
            Assert.True(diagonal.y < 0f);
            Assert.Equal(0.1f, analog.x, 5);
        }

        [Fact]
        public void FlightStep_ClampsStallsAndRejectsInvalidInput()
        {
            Assert.Equal(0.8f, MovementGeometry.FlightStep(Vector2.right, 12f, 600f).x, 5);
            Assert.Equal(Vector2.zero, MovementGeometry.FlightStep(Vector2.right, 12f, float.NaN));
            Assert.Equal(Vector2.zero, MovementGeometry.FlightStep(new Vector2(float.NaN, 1f), 12f, 1f));
            Assert.Equal(8f / 60f, MovementGeometry.FlightStep(Vector2.right, float.PositiveInfinity, 1f).x, 5);
        }

        [Fact]
        public void ExitSearch_UsesCurrentPositionWhenValid()
        {
            var origin = new Vector2(5f, 5f);
            int checks = 0;
            Assert.True(MovementGeometry.TryFindNearby(origin, _ => { checks++; return true; }, out var target));
            Assert.Equal(origin, target);
            Assert.Equal(1, checks);
        }

        [Fact]
        public void ExitSearch_FindsNearbyOpeningWithoutCrossingMapBoundary()
        {
            bool Valid(Vector2 point) => MovementGeometry.IsInsideMap(point, 0.5f, 1f, 20, 10) && point.x >= 1f;
            Assert.True(MovementGeometry.TryFindNearby(new Vector2(0.5f, 3f), Valid, out var target));
            Assert.True(Valid(target));
            Assert.Equal(1f, target.x);
        }

        [Fact]
        public void ExitSearch_RechecksChangedTerrainAndIsBounded()
        {
            int checks = 0;
            Assert.False(MovementGeometry.TryFindNearby(new Vector2(5f, 5f), _ => { checks++; return false; }, out _));
            Assert.InRange(checks, 1, 289);
            Assert.False(MovementGeometry.TryFindNearby(new Vector2(float.NaN, 1f), _ => true, out _));
            Assert.False(MovementGeometry.TryFindNearby(Vector2.zero, null, out _));
        }
    }
}
