using BetterExperience.BConfigManager;
using BetterExperience.BControlManager;
using BetterExperience.BPatchGUI;
using BetterExperience.Patches;
using System.Reflection;
using UnityModBase.HControlSpace;
using UnityModBase.HTranslatorSpace;
using static BetterExperience.Patches.AutoBunServeLogic;

namespace BetterExperience.Test.Patches
{
    public class AutoBunServeLogicTests
    {
        private static readonly AutoBunDrink[] NoBar = Array.Empty<AutoBunDrink>();
        private static readonly AutoBunCustomer[] NoCustomers = Array.Empty<AutoBunCustomer>();
        private static readonly int[] EmptyHands = Array.Empty<int>();

        [Fact]
        public void BusyOrInvalidPlayer_Waits()
        {
            var drink = new[] { new AutoBunDrink(10f, 0) };
            var customer = new[] { new AutoBunCustomer(20f, 0, 2000f, true) };

            Assert.Equal(AutoBunAct.Wait, Decide(5f, EmptyHands, drink, customer, 0f, true, 8f, true).Act);
            Assert.Equal(0, Decide(5f, EmptyHands, drink, customer, 0f, true, 8f, true).Walk);
            Assert.Equal(AutoBunAct.Wait, Decide(float.NaN, EmptyHands, drink, customer, 0f, true, 8f, false).Act);
        }

        [Fact]
        public void EmptyHands_WalkToNeededDrinkThenPickup()
        {
            var drink = new[] { new AutoBunDrink(10f, 0) };
            var customer = new[] { new AutoBunCustomer(20f, 0, 2000f, true) };

            var walk = Decide(5f, EmptyHands, drink, customer, 0f, true, 8f, false);
            Assert.Equal(AutoBunAct.Pickup, walk.Act);
            Assert.Equal(1, walk.Walk);
            Assert.Equal(0, walk.TargetIndex);

            var arrived = Decide(10f, EmptyHands, drink, customer, 0f, true, 8f, false);
            Assert.Equal(AutoBunAct.Pickup, arrived.Act);
            Assert.Equal(0, arrived.Walk);
            Assert.True(arrived.ShouldInteract);
        }

        [Fact]
        public void HoldingMatch_WalksToCustomerAndServes()
        {
            var customer = new[] { new AutoBunCustomer(20f, 0, 2000f, true) };

            var walk = Decide(10f, new[] { 0 }, NoBar, customer, 0f, true, 8f, false);
            Assert.Equal(AutoBunAct.Serve, walk.Act);
            Assert.Equal(1, walk.Walk);
            Assert.Equal(0, walk.TargetIndex);

            var arrived = Decide(20f, new[] { 0 }, NoBar, customer, 0f, true, 8f, false);
            Assert.Equal(AutoBunAct.Serve, arrived.Act);
            Assert.Equal(0, arrived.Walk);
            Assert.True(arrived.ShouldInteract);
        }

        [Fact]
        public void HoldingMatch_ServesNearbyInsteadOfGoingBackForSecondDrink()
        {
            var drinks = new[] { new AutoBunDrink(4f, 1) };
            var customers = new[]
            {
                new AutoBunCustomer(20f, 0, 2000f, true, 0),
                new AutoBunCustomer(24f, 1, 1800f, true, 1)
            };

            var action = Decide(10f, new[] { 0 }, drinks, customers, 0f, true, 8f, false);
            Assert.Equal(AutoBunAct.Serve, action.Act);
            Assert.Equal(1, action.Walk);
            Assert.Equal(0, action.TargetIndex);
        }

        [Fact]
        public void PicksSecondDrinkOnlyWhenItIsOnTheWayToTheCustomer()
        {
            var drinks = new[] { new AutoBunDrink(8f, 1) };
            var customers = new[]
            {
                new AutoBunCustomer(20f, 0, 2000f, true, 0),
                new AutoBunCustomer(24f, 1, 1800f, true, 1)
            };

            var action = Decide(5f, new[] { 0 }, drinks, customers, 0f, true, 8f, false);
            Assert.Equal(AutoBunAct.Pickup, action.Act);
            Assert.Equal(1, action.Walk);
            Assert.Equal(0, action.TargetIndex);
            Assert.True(IsOnTheWay(5f, 8f, 20f));
        }

        [Fact]
        public void NeverServesMismatchedCocktail()
        {
            var customer = new[] { new AutoBunCustomer(20f, 0, 200f, true) };

            var action = Decide(20f, new[] { 1 }, NoBar, customer, 0f, true, 8f, false);
            Assert.Equal(AutoBunAct.Trash, action.Act);
            Assert.NotEqual(AutoBunAct.Serve, action.Act);
        }

        [Fact]
        public void UnmatchedCarrying_WalksToTrash()
        {
            var walk = Decide(10f, new[] { 1 }, NoBar, NoCustomers, 0f, true, 8f, false);
            Assert.Equal(AutoBunAct.Trash, walk.Act);
            Assert.Equal(-1, walk.Walk);

            var arrived = Decide(0.4f, new[] { 1 }, NoBar, NoCustomers, 0f, true, 8f, false);
            Assert.Equal(AutoBunAct.Trash, arrived.Act);
            Assert.Equal(0, arrived.Walk);
            Assert.True(arrived.ShouldInteract);
        }

        [Fact]
        public void LeftoverBarDrink_PickedUpToTrashWhenNothingToServe()
        {
            var drink = new[] { new AutoBunDrink(10f, 3) };

            var action = Decide(5f, EmptyHands, drink, NoCustomers, 0f, true, 8f, false);
            Assert.Equal(AutoBunAct.Pickup, action.Act);
            Assert.Equal(0, action.TargetIndex);
        }

        [Fact]
        public void IdleWalksToBarWhenNothingToDo()
        {
            var walk = Decide(2f, EmptyHands, NoBar, NoCustomers, 0f, true, 8f, false);
            Assert.Equal(AutoBunAct.Wait, walk.Act);
            Assert.Equal(1, walk.Walk);
            Assert.False(walk.ShouldInteract);

            var arrived = Decide(8.2f, EmptyHands, NoBar, NoCustomers, 0f, true, 8f, false);
            Assert.Equal(AutoBunAct.Wait, arrived.Act);
            Assert.Equal(0, arrived.Walk);
        }

        [Fact]
        public void StickyPickupKeepsDrinkOnlyIfItIsTheNextOrder()
        {
            var drinks = new[] { new AutoBunDrink(10f, 0), new AutoBunDrink(16f, 1) };
            var customers = new[]
            {
                new AutoBunCustomer(20f, 0, 2000f, true, 0),
                new AutoBunCustomer(24f, 1, 2000f, true, 1)
            };

            var next = Decide(5f, EmptyHands, drinks, customers, 0f, true, 8f, false, stickyPickup: 1);
            Assert.Equal(AutoBunAct.Pickup, next.Act);
            Assert.Equal(0, next.TargetIndex);

            var keep = Decide(14f, EmptyHands, drinks, customers, 0f, true, 8f, false, stickyPickup: 0);
            Assert.Equal(AutoBunAct.Pickup, keep.Act);
            Assert.Equal(0, keep.TargetIndex);
        }

        [Fact]
        public void ServesLowestOrderIdEvenIfAnotherMatchIsCloser()
        {
            var customers = new[]
            {
                new AutoBunCustomer(12f, 0, 800f, true, 5),
                new AutoBunCustomer(20f, 0, 2000f, true, 2)
            };

            var action = Decide(10f, new[] { 0 }, NoBar, customers, 0f, true, 8f, false, stickyServe: 0);
            Assert.Equal(AutoBunAct.Serve, action.Act);
            Assert.Equal(1, action.TargetIndex);
        }

        [Fact]
        public void GoesBackForNextOrderDrinkBeforeServingALaterTicket()
        {
            var drinks = new[] { new AutoBunDrink(4f, 0) };
            var customers = new[]
            {
                new AutoBunCustomer(20f, 0, 2000f, true, 0),
                new AutoBunCustomer(12f, 1, 2000f, true, 1)
            };

            var action = Decide(10f, new[] { 1 }, drinks, customers, 0f, true, 8f, false);
            Assert.Equal(AutoBunAct.Pickup, action.Act);
            Assert.Equal(0, action.TargetIndex);
            Assert.Equal(-1, action.Walk);
        }

        [Fact]
        public void WaitsAtBarForNextOrderInsteadOfServingALaterTicket()
        {
            var customers = new[]
            {
                new AutoBunCustomer(20f, 0, 2000f, true, 0),
                new AutoBunCustomer(12f, 1, 2000f, true, 1)
            };

            var action = Decide(10f, new[] { 1 }, NoBar, customers, 0f, true, 8f, false);
            Assert.Equal(AutoBunAct.Wait, action.Act);
            Assert.Equal(-1, action.Walk);
            Assert.NotEqual(AutoBunAct.Serve, action.Act);
        }

        [Fact]
        public void EmptyHands_PicksDrinkForEarliestOrderNotTheNearestGlass()
        {
            var drinks = new[] { new AutoBunDrink(6f, 1), new AutoBunDrink(14f, 0) };
            var customers = new[]
            {
                new AutoBunCustomer(20f, 0, 2000f, true, 0),
                new AutoBunCustomer(8f, 1, 2000f, true, 1)
            };

            var action = Decide(5f, EmptyHands, drinks, customers, 0f, true, 8f, false);
            Assert.Equal(AutoBunAct.Pickup, action.Act);
            Assert.Equal(1, action.TargetIndex);
        }

        [Fact]
        public void IsOnTheWay_SameDirectionBeforeDestination()
        {
            Assert.True(IsOnTheWay(5f, 8f, 20f));
            Assert.False(IsOnTheWay(10f, 4f, 20f));
            Assert.False(IsOnTheWay(19f, 8f, 20f));
            Assert.True(IsOnTheWay(7.5f, 8f, 20f));
        }

        [Fact]
        public void WalkOrAct_UsesArriveWindowAndDirection()
        {
            var left = WalkOrAct(10f, 4f, PickupArrive, AutoBunAct.Pickup, 0);
            Assert.Equal(-1, left.Walk);
            Assert.Equal(AutoBunAct.Pickup, left.Act);

            var right = WalkOrAct(4f, 10f, PickupArrive, AutoBunAct.Pickup, 0);
            Assert.Equal(1, right.Walk);

            var arrived = WalkOrAct(10f, 10.5f, PickupArrive, AutoBunAct.Pickup, 2);
            Assert.Equal(0, arrived.Walk);
            Assert.Equal(2, arrived.TargetIndex);
            Assert.True(arrived.ShouldInteract);
        }

        [Fact]
        public void WalkOrAct_RejectsInvalidTarget()
        {
            var action = WalkOrAct(10f, float.NaN, PickupArrive, AutoBunAct.Pickup, 0);
            Assert.Equal(AutoBunAct.Wait, action.Act);
            Assert.Equal(0, action.Walk);
        }

        [Fact]
        public void FullHands_SkipPickupEvenIfDrinksRemain()
        {
            var drinks = new[] { new AutoBunDrink(4f, 2) };
            var customers = new[]
            {
                new AutoBunCustomer(20f, 0, 2000f, true),
                new AutoBunCustomer(24f, 1, 2000f, true)
            };

            var action = Decide(10f, new[] { 0, 1 }, drinks, customers, 0f, true, 8f, false);
            Assert.Equal(AutoBunAct.Serve, action.Act);
            Assert.NotEqual(AutoBunAct.Pickup, action.Act);
        }

        [Fact]
        public void LowHp_EmptyHandsEatsInsteadOfPickingUp()
        {
            var drink = new[] { new AutoBunDrink(10f, 0) };
            var customer = new[] { new AutoBunCustomer(20f, 0, 2000f, true, 0) };

            var action = Decide(5f, EmptyHands, drink, customer, 0f, true, 8f, false, hp: EatStartHp);
            Assert.Equal(AutoBunAct.Eat, action.Act);
            Assert.Equal(0, action.Walk);
            Assert.False(action.ShouldInteract);
        }

        [Fact]
        public void LowHp_DeliversHeldNextOrderBeforeEating()
        {
            var customer = new[] { new AutoBunCustomer(20f, 0, 2000f, true, 0) };

            var action = Decide(10f, new[] { 0 }, NoBar, customer, 0f, true, 8f, false, hp: 40f);
            Assert.Equal(AutoBunAct.Serve, action.Act);
            Assert.Equal(0, action.TargetIndex);
        }

        [Fact]
        public void LowHp_TrashesUnmatchedBeforeEating()
        {
            var action = Decide(10f, new[] { 1 }, NoBar, NoCustomers, 0f, true, 8f, false, hp: 40f);
            Assert.Equal(AutoBunAct.Trash, action.Act);
            Assert.Equal(-1, action.Walk);
        }

        [Fact]
        public void EatingContinuesUntilStopHpAndDoesNotRestartAboveStart()
        {
            Assert.True(ShouldEat(80f, true));
            Assert.False(ShouldEat(EatStopHp, true));
            Assert.False(ShouldEat(80f, false));
            Assert.True(ShouldEat(EatStartHp, false));
        }

        [Fact]
        public void Pendulum_PressesOnlyInsideAcceptWindow()
        {
            Assert.True(ShouldPressPendulum(true));
            Assert.False(ShouldPressPendulum(false));
        }

        [Fact]
        public void Bartender_UnsticksFrozenWalkAndForcesProduce()
        {
            Assert.True(ShouldFinishFrozenWalk(true, true));
            Assert.False(ShouldFinishFrozenWalk(true, false));
            Assert.False(ShouldFinishFrozenWalk(false, true));

            Assert.False(ShouldForceShakeStand(0f, BartenderShakeStandFrames - 1));
            Assert.True(ShouldForceShakeStand(0f, BartenderShakeStandFrames));
            Assert.False(ShouldForceShakeStand(-1f, BartenderShakeStandFrames));

            Assert.False(ShouldForceProduce(1, BartenderProduceStuckFrames - 1));
            Assert.True(ShouldForceProduce(1, BartenderProduceStuckFrames));
            Assert.False(ShouldForceProduce(0, BartenderProduceStuckFrames));

            Assert.True(ShouldUnstickIdleWalk(true, 0f, BartenderWalkStuckFrames));
            Assert.False(ShouldUnstickIdleWalk(true, 0.05f, BartenderWalkStuckFrames));
            Assert.False(ShouldUnstickIdleWalk(false, 0f, BartenderWalkStuckFrames));
        }

        [Fact]
        public void AutoBunServeSwitch_IsSessionControlWithoutConfigOrHotkey()
        {
            var control = typeof(ControlManager).GetProperty(
                nameof(ControlManager.SetAutoBunServe),
                BindingFlags.NonPublic | BindingFlags.Static);
            Assert.NotNull(control);
            Assert.Equal(typeof(ControlEntry<bool>), control.PropertyType);

            Assert.Null(typeof(ConfigManager).GetProperty(nameof(ControlManager.SetAutoBunServe)));
            Assert.Null(typeof(ConfigManager).GetProperty("EnableAutoBunServe"));
            Assert.Null(typeof(ConfigManager).GetProperty("ToggleAutoBunServeHotkey"));
        }

        [Theory]
        [InlineData(LanguageType.Chinese, "自动配送酒水已开启：酒吧小游戏中会走路取酒并送到对应客人。", "自动配送酒水已关闭。", "自动配送酒水中")]
        [InlineData(LanguageType.English, "Auto drink serving on: in the bar minigame it will walk, pick up drinks and deliver them.", "Auto drink serving off.", "Auto drink serving active")]
        public void AutoBunServeLabels_FollowConfiguredLanguage(
            LanguageType language, string enabled, string disabled, string active)
        {
            var original = Translator.DefaultLanguage;
            try
            {
                Translator.DefaultLanguage = language;
                Assert.Equal(enabled, TranslatorResource.AutoBunServeEnabled.ToString());
                Assert.Equal(disabled, TranslatorResource.AutoBunServeDisabled.ToString());
                Assert.Equal(active, TranslatorResource.AutoBunServeActive.ToString());
            }
            finally
            {
                Translator.DefaultLanguage = original;
            }
        }
    }
}
