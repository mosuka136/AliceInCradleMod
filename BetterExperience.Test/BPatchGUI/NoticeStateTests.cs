using BetterExperience.BPatchGUI;
using UnityModBase.HTranslatorSpace;

namespace BetterExperience.Test.BPatchGUI
{
    public class NoticeStateTests
    {
        private float _time = 10f;
        private static Translator Text(string value) => new Translator(value, value);

        [Fact]
        public void TimedNotice_ExpiresAtThreeSecondsByDefault()
        {
            var state = new NoticeState(() => _time);
            state.Show(null, Text("ready"));
            _time = 12.99f;
            Assert.Equal("ready", Assert.Single(state.GetVisible()).Message.ToString());
            _time = 13f;
            Assert.Empty(state.GetVisible());
        }

        [Fact]
        public void ReplacingOwnNotice_RefreshesDurationWithoutOverwritingAnotherOwner()
        {
            var state = new NoticeState(() => _time);
            var first = new object();
            var second = new object();
            state.Show(first, Text("old"));
            state.Show(second, Text("other"));
            _time = 12f;
            state.Show(first, Text("new"), 5f);

            Assert.Equal(new[] { "new", "other" }, state.GetVisible().Select(entry => entry.Message.ToString()));
            _time = 13f;
            Assert.Equal("new", Assert.Single(state.GetVisible()).Message.ToString());
            _time = 17f;
            Assert.Empty(state.GetVisible());
        }

        [Fact]
        public void PersistentStatus_PrecedesTimedNoticeAndDoesNotExpire()
        {
            var state = new NoticeState(() => _time);
            var owner = new object();
            state.Show(owner, Text("enabled"));
            state.SetStatus(owner, Text("running"));
            Assert.Equal(new[] { "running", "enabled" }, state.GetVisible().Select(entry => entry.Message.ToString()));

            _time = 100f;
            Assert.Equal("running", Assert.Single(state.GetVisible()).Message.ToString());
            state.SetStatus(owner, Text("updated"));
            Assert.Equal("updated", Assert.Single(state.GetVisible()).Message.ToString());
        }

        [Fact]
        public void RemoveStatus_KeepsTheOwnersTimedNotice()
        {
            var state = new NoticeState(() => _time);
            var owner = new object();
            state.SetStatus(owner, Text("running"));
            state.Show(owner, Text("stopped"));
            state.RemoveStatus(owner);
            Assert.Equal("stopped", Assert.Single(state.GetVisible()).Message.ToString());
        }

        [Fact]
        public void Clear_UsesOwnerIdentityAndLeavesOtherOwnersUntouched()
        {
            var state = new NoticeState(() => _time);
            object first = new string('x', 1);
            object second = new string('x', 1);
            Assert.Equal(first, second);
            Assert.NotSame(first, second);
            state.SetStatus(first, Text("running"));
            state.Show(first, Text("first"));
            state.Show(second, Text("second"));
            state.Clear(first);
            Assert.Equal("second", Assert.Single(state.GetVisible()).Message.ToString());
        }

        [Fact]
        public void DefaultChannelAndClearAll_HaveSeparateScopes()
        {
            var state = new NoticeState(() => _time);
            state.Show(null, Text("default"));
            state.SetStatus(new object(), Text("owned"));
            state.Clear(null);
            Assert.Equal("owned", Assert.Single(state.GetVisible()).Message.ToString());
            state.ClearAll();
            Assert.Empty(state.GetVisible());
        }

        [Fact]
        public void EmptyMessage_RemovesOnlyTheMatchingKind()
        {
            var state = new NoticeState(() => _time);
            var owner = new object();
            state.Show(owner, Text("notice"));
            state.SetStatus(owner, Text("status"));
            state.Show(owner, null);
            Assert.Equal("status", Assert.Single(state.GetVisible()).Message.ToString());
            state.SetStatus(owner, Text(" \t "));
            Assert.Empty(state.GetVisible());
        }

        [Theory]
        [InlineData(0f)]
        [InlineData(-1f)]
        [InlineData(float.NaN)]
        [InlineData(float.PositiveInfinity)]
        [InlineData(float.NegativeInfinity)]
        public void InvalidDuration_FallsBackToDefault(float duration)
        {
            var state = new NoticeState(() => _time);
            state.Show(null, Text("notice"), duration);
            _time = 12f;
            Assert.Single(state.GetVisible());
            _time = 13f;
            Assert.Empty(state.GetVisible());
        }

        [Fact]
        public void Translation_IsResolvedAtDisplayTime()
        {
            var state = new NoticeState(() => _time);
            var message = new Translator("完成", "Done") { LanguageType = LanguageType.English };
            state.Show(null, message);
            Assert.Equal("Done", Assert.Single(state.GetVisible()).Message.ToString());
            message.LanguageType = LanguageType.Chinese;
            Assert.Equal("完成", Assert.Single(state.GetVisible()).Message.ToString());
        }
    }
}
