using System;
using System.Collections.Generic;

namespace BetterExperience.Patches
{
    /// <summary>
    /// 兔女郎酒吧自动配送的纯决策。
    /// 按订单 id 从小到大送（连击要求后送的 id 大于上一单），手里已有下一单时就近先送；
    /// 只有第二杯在去客人的路上才顺路补拿。体力低于阈值时先把手腾空再吃零食回血。
    /// </summary>
    internal static class AutoBunServeLogic
    {
        internal const float PickupArrive = 0.85f;
        internal const float ServeArrive = 1.15f;
        internal const float TrashArrive = 0.9f;
        internal const float IdleArrive = 0.5f;
        internal const float EatStartHp = 70f;
        internal const float EatStopHp = 99f;
        internal const int CarryCap = 2;
        internal const int BartenderWalkStuckFrames = 60;
        internal const int BartenderShakeStandFrames = 8;
        internal const int BartenderProduceStuckFrames = 360;

        internal enum AutoBunAct
        {
            Wait,
            Pickup,
            Serve,
            Trash,
            Eat
        }

        internal readonly struct AutoBunDrink
        {
            internal readonly float X;
            internal readonly int Cocktail;

            internal AutoBunDrink(float x, int cocktail)
            {
                X = x;
                Cocktail = cocktail;
            }
        }

        internal readonly struct AutoBunCustomer
        {
            internal readonly float X;
            internal readonly int Cocktail;
            internal readonly float TimeLeft;
            internal readonly bool CanServe;
            internal readonly int OrderId;

            internal AutoBunCustomer(float x, int cocktail, float timeLeft, bool canServe, int orderId = 0)
            {
                X = x;
                Cocktail = cocktail;
                TimeLeft = timeLeft;
                CanServe = canServe;
                OrderId = orderId;
            }
        }

        internal readonly struct AutoBunAction
        {
            internal readonly AutoBunAct Act;
            internal readonly int Walk;
            internal readonly float TargetX;
            internal readonly int TargetIndex;

            internal AutoBunAction(AutoBunAct act, int walk, float targetX, int targetIndex)
            {
                Act = act;
                Walk = walk;
                TargetX = targetX;
                TargetIndex = targetIndex;
            }

            internal bool ShouldInteract =>
                Walk == 0 && (Act == AutoBunAct.Pickup || Act == AutoBunAct.Serve || Act == AutoBunAct.Trash);

            internal bool ShouldWalk => Walk != 0;
        }

        internal static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);

        internal static bool ShouldEat(float hp, bool eating)
        {
            if (!IsFinite(hp))
                return false;
            return eating ? hp < EatStopHp : hp <= EatStartHp;
        }

        internal static bool ShouldPressPendulum(bool accepting)
        {
            return accepting;
        }

        internal static bool ShouldFinishFrozenWalk(bool moveScriptActive, bool inBunGame)
        {
            return inBunGame && moveScriptActive;
        }

        internal static bool ShouldForceShakeStand(float walkY, int waitFrames)
        {
            return walkY == 0f && waitFrames >= BartenderShakeStandFrames;
        }

        internal static bool ShouldForceProduce(int notServed, int framesSinceProduce)
        {
            return notServed > 0 && framesSinceProduce >= BartenderProduceStuckFrames;
        }

        internal static bool ShouldUnstickIdleWalk(bool walking, float moved, int idleFrames)
        {
            return walking && moved < 0.01f && idleFrames >= BartenderWalkStuckFrames;
        }

        internal static AutoBunAction Wait(float playerX = 0f) =>
            new AutoBunAction(AutoBunAct.Wait, 0, playerX, -1);

        internal static AutoBunAction Decide(
            float playerX,
            IReadOnlyList<int> carrying,
            IReadOnlyList<AutoBunDrink> bar,
            IReadOnlyList<AutoBunCustomer> customers,
            float trashX,
            bool trashExists,
            float idleX,
            bool busy,
            int stickyPickup = -1,
            int stickyServe = -1,
            float hp = 100f,
            bool eating = false)
        {
            if (busy || !IsFinite(playerX))
                return Wait(playerX);

            int carryCount = carrying == null ? 0 : carrying.Count;
            int barCount = bar == null ? 0 : bar.Count;
            int customerCount = customers == null ? 0 : customers.Count;
            var upcoming = ListUpcoming(customers, customerCount);
            int next = upcoming.Count > 0 ? upcoming[0] : -1;
            bool haveNext = next >= 0 && CanServeWith(carrying, carryCount, customers[next]);
            int pick = PickUpcomingDrink(bar, barCount, carrying, carryCount, customers, upcoming, playerX, stickyPickup);
            bool needEat = ShouldEat(hp, eating);

            if (needEat)
            {
                if (carryCount == 0)
                    return new AutoBunAction(AutoBunAct.Eat, 0, playerX, -1);
                if (haveNext)
                    return WalkOrAct(playerX, customers[next].X, ServeArrive, AutoBunAct.Serve, next);
                if (trashExists && IsFinite(trashX))
                    return WalkOrAct(playerX, trashX, TrashArrive, AutoBunAct.Trash, -1);
                int forcedEatServe = PickServeCustomer(carrying, carryCount, customers, upcoming, stickyServe);
                if (forcedEatServe >= 0)
                    return WalkOrAct(playerX, customers[forcedEatServe].X, ServeArrive, AutoBunAct.Serve, forcedEatServe);
                return Wait(playerX);
            }

            if (haveNext)
            {
                if (pick >= 0 && carryCount < CarryCap &&
                    IsOnTheWay(playerX, bar[pick].X, customers[next].X))
                    return WalkOrAct(playerX, bar[pick].X, PickupArrive, AutoBunAct.Pickup, pick);
                int serve = stickyServe == next ? stickyServe : next;
                return WalkOrAct(playerX, customers[serve].X, ServeArrive, AutoBunAct.Serve, serve);
            }

            if (pick >= 0 && carryCount < CarryCap)
                return WalkOrAct(playerX, bar[pick].X, PickupArrive, AutoBunAct.Pickup, pick);

            if (carryCount > 0 && trashExists && IsFinite(trashX) &&
                HasUnmatchedCarrying(carrying, carryCount, customers, upcoming))
                return WalkOrAct(playerX, trashX, TrashArrive, AutoBunAct.Trash, -1);

            if (next >= 0 && carryCount < CarryCap)
                return WalkOrAct(playerX, IsFinite(idleX) ? idleX : playerX, IdleArrive, AutoBunAct.Wait, -1);

            int forcedServe = PickServeCustomer(carrying, carryCount, customers, upcoming, stickyServe);
            if (forcedServe >= 0)
                return WalkOrAct(playerX, customers[forcedServe].X, ServeArrive, AutoBunAct.Serve, forcedServe);

            if (carryCount > 0 && trashExists && IsFinite(trashX))
                return WalkOrAct(playerX, trashX, TrashArrive, AutoBunAct.Trash, -1);

            if (carryCount < CarryCap && trashExists && barCount > 0)
            {
                int leftover = PickNearestBar(bar, barCount, playerX, null);
                if (leftover >= 0)
                    return WalkOrAct(playerX, bar[leftover].X, PickupArrive, AutoBunAct.Pickup, leftover);
            }

            if (IsFinite(idleX))
                return WalkOrAct(playerX, idleX, IdleArrive, AutoBunAct.Wait, -1);

            return Wait(playerX);
        }

        internal static AutoBunAction WalkOrAct(
            float playerX,
            float targetX,
            float arrive,
            AutoBunAct act,
            int index)
        {
            if (!IsFinite(targetX) || !IsFinite(arrive) || arrive < 0f)
                return Wait(playerX);

            float dx = targetX - playerX;
            if (Math.Abs(dx) <= arrive)
                return new AutoBunAction(act, 0, targetX, index);
            return new AutoBunAction(act, dx > 0f ? 1 : -1, targetX, index);
        }

        /// <summary>
        /// 从玩家走向 dest 时，stop 是否在同一方向且不超过目的地；已经站在 stop 上视为顺路。
        /// </summary>
        internal static bool IsOnTheWay(float playerX, float stopX, float destX)
        {
            if (!IsFinite(playerX) || !IsFinite(stopX) || !IsFinite(destX))
                return false;

            float toDest = destX - playerX;
            float toStop = stopX - playerX;
            if (Math.Abs(toDest) <= ServeArrive)
                return false;
            if (Math.Abs(toStop) <= PickupArrive)
                return true;
            if (Math.Sign(toStop) != Math.Sign(toDest))
                return false;
            return Math.Abs(toStop) <= Math.Abs(toDest) + PickupArrive;
        }

        private static List<int> ListUpcoming(IReadOnlyList<AutoBunCustomer> customers, int customerCount)
        {
            var upcoming = new List<int>(customerCount);
            for (int i = 0; i < customerCount; i++)
            {
                if (customers[i].CanServe)
                    upcoming.Add(i);
            }
            upcoming.Sort((a, b) => CompareUpcoming(customers, a, b));
            return upcoming;
        }

        private static int CompareUpcoming(IReadOnlyList<AutoBunCustomer> customers, int a, int b)
        {
            int id = customers[a].OrderId.CompareTo(customers[b].OrderId);
            if (id != 0)
                return id;
            float ta = IsFinite(customers[a].TimeLeft) ? customers[a].TimeLeft : float.MaxValue;
            float tb = IsFinite(customers[b].TimeLeft) ? customers[b].TimeLeft : float.MaxValue;
            int time = ta.CompareTo(tb);
            if (time != 0)
                return time;
            return a.CompareTo(b);
        }

        private static int PickServeCustomer(
            IReadOnlyList<int> carrying,
            int carryCount,
            IReadOnlyList<AutoBunCustomer> customers,
            List<int> upcoming,
            int stickyServe)
        {
            int first = -1;
            for (int i = 0; i < upcoming.Count; i++)
            {
                int index = upcoming[i];
                if (!CanServeWith(carrying, carryCount, customers[index]))
                    continue;
                first = index;
                break;
            }
            return stickyServe == first ? stickyServe : first;
        }

        private static int PickUpcomingDrink(
            IReadOnlyList<AutoBunDrink> bar,
            int barCount,
            IReadOnlyList<int> carrying,
            int carryCount,
            IReadOnlyList<AutoBunCustomer> customers,
            List<int> upcoming,
            float playerX,
            int stickyPickup)
        {
            if (barCount <= 0 || upcoming.Count == 0)
                return -1;

            var remaining = CopyCarrying(carrying, carryCount);
            for (int i = 0; i < upcoming.Count; i++)
            {
                int cocktail = customers[upcoming[i]].Cocktail;
                if (TryConsume(remaining, cocktail))
                    continue;
                int found = FindBarDrink(bar, barCount, cocktail, playerX, stickyPickup);
                if (found >= 0)
                    return found;
                return -1;
            }
            return -1;
        }

        private static int FindBarDrink(
            IReadOnlyList<AutoBunDrink> bar,
            int barCount,
            int cocktail,
            float playerX,
            int stickyPickup)
        {
            if (stickyPickup >= 0 && stickyPickup < barCount &&
                bar[stickyPickup].Cocktail == cocktail && IsFinite(bar[stickyPickup].X))
                return stickyPickup;

            int best = -1;
            float bestDist = float.MaxValue;
            for (int i = 0; i < barCount; i++)
            {
                if (bar[i].Cocktail != cocktail || !IsFinite(bar[i].X))
                    continue;
                float dist = Math.Abs(bar[i].X - playerX);
                if (best < 0 || dist < bestDist)
                {
                    best = i;
                    bestDist = dist;
                }
            }
            return best;
        }

        private static int PickNearestBar(
            IReadOnlyList<AutoBunDrink> bar,
            int barCount,
            float playerX,
            bool[] needed)
        {
            int best = -1;
            float bestDist = float.MaxValue;
            for (int i = 0; i < barCount; i++)
            {
                if (needed != null && !needed[i])
                    continue;
                if (!IsFinite(bar[i].X))
                    continue;
                float dist = Math.Abs(bar[i].X - playerX);
                if (best < 0 || dist < bestDist)
                {
                    best = i;
                    bestDist = dist;
                }
            }
            return best;
        }

        private static List<int> CopyCarrying(IReadOnlyList<int> carrying, int carryCount)
        {
            var remaining = new List<int>(carryCount);
            for (int i = 0; i < carryCount; i++)
                remaining.Add(carrying[i]);
            return remaining;
        }

        private static bool TryConsume(List<int> remaining, int cocktail)
        {
            int index = remaining.IndexOf(cocktail);
            if (index < 0)
                return false;
            remaining.RemoveAt(index);
            return true;
        }

        private static bool HasUnmatchedCarrying(
            IReadOnlyList<int> carrying,
            int carryCount,
            IReadOnlyList<AutoBunCustomer> customers,
            List<int> upcoming)
        {
            var remaining = CopyCarrying(carrying, carryCount);
            for (int i = 0; i < upcoming.Count; i++)
                TryConsume(remaining, customers[upcoming[i]].Cocktail);
            return remaining.Count > 0;
        }

        private static bool CanServeWith(IReadOnlyList<int> carrying, int carryCount, AutoBunCustomer customer)
        {
            if (!customer.CanServe || carrying == null)
                return false;
            for (int i = 0; i < carryCount; i++)
            {
                if (carrying[i] == customer.Cocktail)
                    return true;
            }
            return false;
        }
    }
}
