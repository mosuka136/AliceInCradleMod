using System;
using UnityEngine;

namespace BetterExperience.Patches
{
    /// <summary>与 Unity 场景无关的坐标校验、飞行步长和退出落点搜索。</summary>
    internal static class MovementGeometry
    {
        internal static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);

        internal static bool IsOnScreen(Vector2 point, float width, float height)
        {
            return IsFinite(point.x) && IsFinite(point.y) && IsFinite(width) && IsFinite(height) &&
                width > 0f && height > 0f &&
                point.x >= 0f && point.y >= 0f && point.x < width && point.y < height;
        }

        internal static bool IsInsideMap(Vector2 point, float halfWidth, float halfHeight, int columns, int rows)
        {
            return IsFinite(point.x) && IsFinite(point.y) && IsFinite(halfWidth) && IsFinite(halfHeight) &&
                halfWidth > 0f && halfHeight > 0f && columns > 0 && rows > 0 &&
                point.x - halfWidth >= 0f && point.x + halfWidth <= columns &&
                point.y - halfHeight >= 0f && point.y + halfHeight <= rows;
        }

        internal static bool TryScreenToViewport(Vector2 screen, Rect pixelRect, out Vector2 viewport)
        {
            viewport = Vector2.zero;
            if (!IsFinite(pixelRect.x) || !IsFinite(pixelRect.y))
                return false;

            var local = new Vector2(screen.x - pixelRect.x, screen.y - pixelRect.y);
            if (!IsOnScreen(local, pixelRect.width, pixelRect.height))
                return false;

            viewport = new Vector2(local.x / pixelRect.width, local.y / pixelRect.height);
            return true;
        }

        internal static Vector2 FlightStep(Vector2 direction, float speed, float frameCount)
        {
            if (!IsFinite(direction.x) || !IsFinite(direction.y) || !IsFinite(frameCount) || frameCount <= 0f)
                return Vector2.zero;
            if (!IsFinite(speed) || speed < 1f || speed > 30f)
                speed = 8f;

            // 游戏 fcnt 以 60 Hz 帧为单位；限制单次步长，避免卡顿后突然跳出地图。
            double length = Math.Sqrt((double)direction.x * direction.x + (double)direction.y * direction.y);
            double scale = speed * Math.Min(frameCount, 4f) / 60.0 / Math.Max(1.0, length);
            return new Vector2((float)(direction.x * scale), (float)(direction.y * scale));
        }

        internal static bool TryFindNearby(Vector2 origin, Func<Vector2, bool> canOccupy, out Vector2 result)
        {
            result = origin;
            if (canOccupy == null || !IsFinite(origin.x) || !IsFinite(origin.y))
                return false;
            if (canOccupy(origin))
                return true;

            // 从近到远枚举 0.25 格网格的方环，最多搜索 2 格。每次重新校验动态障碍。
            for (int ring = 1; ring <= 8; ring++)
            {
                for (int x = -ring; x <= ring; x++)
                {
                    for (int y = -ring; y <= ring; y++)
                    {
                        if (Math.Abs(x) != ring && Math.Abs(y) != ring)
                            continue;
                        var candidate = new Vector2(origin.x + x * 0.25f, origin.y + y * 0.25f);
                        if (!canOccupy(candidate))
                            continue;
                        result = candidate;
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
