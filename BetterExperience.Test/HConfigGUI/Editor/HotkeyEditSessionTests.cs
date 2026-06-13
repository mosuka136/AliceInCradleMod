using BetterExperience.HConfigGUI;
using BetterExperience.HConfigGUI.Bindings;
using BetterExperience.HConfigGUI.Editor;
using BetterExperience.HotkeyManager;
using BetterExperience.HProvider;
using Moq;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace BetterExperience.Test.HConfigGUI.Editor
{
    public class HotkeyEditSessionTests
    {
        [Theory]
        [InlineData(false, HotkeyEditState.Expanded)]
        [InlineData(true, HotkeyEditState.Idle)]
        public void IsEditing_WhenEntryIsMissingOrStateIsIdle_ReturnsFalse(bool hasEntry, HotkeyEditState state)
        {
            var session = new HotkeyEditSession
            {
                Entry = hasEntry ? CreateEntryBinding(new Hotkey()).Object : null,
                State = state
            };

            var result = session.IsEditing;

            Assert.False(result);
        }

        [Fact]
        public void IsEditing_WhenEntryExistsAndStateIsNotIdle_ReturnsTrue()
        {
            var session = new HotkeyEditSession
            {
                Entry = CreateEntryBinding(new Hotkey()).Object,
                State = HotkeyEditState.Expanded
            };

            var result = session.IsEditing;

            Assert.True(result);
        }

        [Theory]
        [InlineData(HotkeyEditState.Idle)]
        [InlineData(HotkeyEditState.Expanded)]
        public void IsRecording_WhenStateIsIdleOrExpanded_ReturnsFalse(HotkeyEditState state)
        {
            var session = new HotkeyEditSession
            {
                State = state
            };

            var result = session.IsRecording;

            Assert.False(result);
        }

        [Theory]
        [InlineData(HotkeyEditState.WaitingPress)]
        [InlineData(HotkeyEditState.Recording)]
        [InlineData(HotkeyEditState.WaitingConfirm)]
        public void IsRecording_WhenStateRequiresInput_ReturnsTrue(HotkeyEditState state)
        {
            var session = new HotkeyEditSession
            {
                State = state
            };

            var result = session.IsRecording;

            Assert.True(result);
        }

        [Fact]
        public void Constructor_WhenEntryValueResetInvoked_CancelsEdit()
        {
            var session = new HotkeyEditSession
            {
                Entry = CreateEntryBinding(new Hotkey()).Object,
                State = HotkeyEditState.WaitingPress,
                OriginalValue = new Hotkey(),
                WorkingValue = new Hotkey(),
                WorkingChord = new HotkeyChord(new UnityProvider()),
                PreviewGamepadChord = new GamepadChord(new UnityProvider(), new GamepadTrigger(GamepadButton.DpadUp, new UnityProvider())),
                PreviewKeyboardChord = new KeyboardChord(new UnityProvider(), new KeyboardTrigger(Key.A, new UnityProvider()))
            };

            GuiPipe.InvokeOnEntryValueReset(null);

            Assert.Null(session.Entry);
            Assert.Equal(HotkeyEditState.Idle, session.State);
            Assert.Null(session.OriginalValue);
            Assert.Null(session.WorkingValue);
            Assert.Null(session.WorkingChord);
            Assert.Null(session.PreviewGamepadChord);
            Assert.Null(session.PreviewKeyboardChord);
        }

        [Theory]
        [InlineData(HotkeyEditState.Idle)]
        [InlineData(HotkeyEditState.Expanded)]
        public void Update_WhenStateIsIdleOrExpanded_ReturnsWithoutChangingState(HotkeyEditState state)
        {
            var session = new HotkeyEditSession
            {
                State = state
            };

            session.Update();

            Assert.Equal(state, session.State);
        }

        [Fact]
        public void Update_WhenStateIsWaitingPressWithoutInput_LeavesStateUnchanged()
        {
            var unityProvider = new UnityProvider();
            var session = new HotkeyEditSession
            {
                Entry = CreateEntryBinding(new Hotkey(unityProvider)).Object,
                State = HotkeyEditState.WaitingPress,
                WorkingValue = new Hotkey(unityProvider)
            };

            session.Update();

            Assert.Equal(HotkeyEditState.WaitingPress, session.State);
            Assert.Null(session.PreviewGamepadChord);
            Assert.Null(session.PreviewKeyboardChord);
        }

        [Fact]
        public void Update_WhenStateIsRecordingWithoutInput_MovesToWaitingConfirm()
        {
            var unityProvider = new UnityProvider();
            var session = new HotkeyEditSession
            {
                Entry = CreateEntryBinding(new Hotkey(unityProvider)).Object,
                State = HotkeyEditState.Recording,
                WorkingValue = new Hotkey(unityProvider)
            };

            session.Update();

            Assert.Equal(HotkeyEditState.WaitingConfirm, session.State);
        }

        [Fact]
        public void Update_WhenStateIsWaitingConfirmAndWorkingChordIsInvalid_ClearsPreviewsAndMovesToRecording()
        {
            var unityProvider = new UnityProvider();
            var session = new HotkeyEditSession
            {
                Entry = CreateEntryBinding(new Hotkey(unityProvider)).Object,
                State = HotkeyEditState.WaitingConfirm,
                WorkingValue = new Hotkey(unityProvider),
                WorkingChord = new HotkeyChord(unityProvider),
                PreviewGamepadChord = new GamepadChord(unityProvider, new GamepadTrigger(GamepadButton.DpadUp, unityProvider)),
                PreviewKeyboardChord = new KeyboardChord(unityProvider, new KeyboardTrigger(Key.A, unityProvider))
            };

            session.Update();

            Assert.Equal(HotkeyEditState.Recording, session.State);
            Assert.False(session.PreviewGamepadChord.IsValid);
            Assert.False(session.PreviewKeyboardChord.IsValid);
        }

        [Fact]
        public void Clear_WhenEntryContainsHotkey_ResetsHotkeyValidityAndSessionState()
        {
            var unityProvider = new UnityProvider();
            var hotkey = new Hotkey(unityProvider)
            {
                Valid = false
            };
            Hotkey.GlobalValid = false;
            var session = new HotkeyEditSession
            {
                Entry = CreateEntryBinding(hotkey).Object,
                State = HotkeyEditState.WaitingConfirm,
                OriginalValue = new Hotkey(unityProvider),
                WorkingValue = new Hotkey(unityProvider),
                WorkingChord = new HotkeyChord(unityProvider),
                PreviewGamepadChord = new GamepadChord(unityProvider, new GamepadTrigger(GamepadButton.DpadUp, unityProvider)),
                PreviewKeyboardChord = new KeyboardChord(unityProvider, new KeyboardTrigger(Key.A, unityProvider))
            };

            session.Clear();

            Assert.True(hotkey.Valid);
            Assert.True(Hotkey.GlobalValid);
            Assert.Null(session.Entry);
            Assert.Equal(HotkeyEditState.Idle, session.State);
            Assert.Null(session.OriginalValue);
            Assert.Null(session.WorkingValue);
            Assert.Null(session.WorkingChord);
            Assert.Null(session.PreviewGamepadChord);
            Assert.Null(session.PreviewKeyboardChord);
        }

        [Fact]
        public void Clear_WhenEntryIsNull_ResetsSessionStateAndGlobalValidity()
        {
            var unityProvider = new UnityProvider();
            Hotkey.GlobalValid = false;
            var session = new HotkeyEditSession
            {
                State = HotkeyEditState.WaitingPress,
                OriginalValue = new Hotkey(unityProvider),
                WorkingValue = new Hotkey(unityProvider),
                WorkingChord = new HotkeyChord(unityProvider),
                PreviewGamepadChord = new GamepadChord(unityProvider, new GamepadTrigger(GamepadButton.DpadUp, unityProvider)),
                PreviewKeyboardChord = new KeyboardChord(unityProvider, new KeyboardTrigger(Key.A, unityProvider))
            };

            session.Clear();

            Assert.True(Hotkey.GlobalValid);
            Assert.Null(session.Entry);
            Assert.Equal(HotkeyEditState.Idle, session.State);
            Assert.Null(session.OriginalValue);
            Assert.Null(session.WorkingValue);
            Assert.Null(session.WorkingChord);
            Assert.Null(session.PreviewGamepadChord);
            Assert.Null(session.PreviewKeyboardChord);
        }


        [Fact]
        public void BeginEdit_WhenEntryContainsHotkey_InitializesEditingState()
        {
            var unityProvider = new UnityProvider();
            var hotkey = new Hotkey(unityProvider);
            hotkey.Add(new HotkeyChord(unityProvider));
            var entry = CreateEntryBinding(hotkey);
            var session = new HotkeyEditSession
            {
                WorkingChord = new HotkeyChord(unityProvider),
                PreviewGamepadChord = new GamepadChord(unityProvider, new GamepadTrigger(GamepadButton.DpadUp, unityProvider)),
                PreviewKeyboardChord = new KeyboardChord(unityProvider, new KeyboardTrigger(Key.A, unityProvider))
            };

            session.BeginEdit(entry.Object);

            Assert.Same(entry.Object, session.Entry);
            Assert.Equal(HotkeyEditState.Expanded, session.State);
            Assert.Same(hotkey, session.OriginalValue);
            Assert.NotSame(hotkey, session.WorkingValue);
            Assert.Equal(hotkey.Count, session.WorkingValue.Count);
            Assert.False(session.WorkingValue.Valid);
            Assert.Null(session.WorkingChord);
            Assert.Null(session.PreviewGamepadChord);
            Assert.Null(session.PreviewKeyboardChord);
        }


        [Fact]
        public void BeginEdit_WhenEntryIsNull_DoesNothing()
        {
            var session = new HotkeyEditSession
            {
                State = HotkeyEditState.Idle
            };

            session.BeginEdit(null);

            Assert.Null(session.Entry);
            Assert.Equal(HotkeyEditState.Idle, session.State);
            Assert.Null(session.OriginalValue);
            Assert.Null(session.WorkingValue);
        }

        [Fact]
        public void BeginEdit_WhenSessionIsNotIdle_DoesNothing()
        {
            var unityProvider = new UnityProvider();
            var hotkey = new Hotkey(unityProvider);
            var entry = CreateEntryBinding(hotkey);
            var session = new HotkeyEditSession
            {
                State = HotkeyEditState.Expanded
            };

            session.BeginEdit(entry.Object);

            Assert.Null(session.Entry);
            Assert.Equal(HotkeyEditState.Expanded, session.State);
            Assert.Null(session.OriginalValue);
            Assert.Null(session.WorkingValue);
        }

        [Fact]
        public void BeginEdit_WhenEntryValueIsNotHotkey_DoesNothing()
        {
            var entry = CreateEntryBinding("value");
            var session = new HotkeyEditSession();

            session.BeginEdit(entry.Object);

            Assert.Null(session.Entry);
            Assert.Equal(HotkeyEditState.Idle, session.State);
            Assert.Null(session.OriginalValue);
            Assert.Null(session.WorkingValue);
        }

        [Fact]
        public void BeginRecord_WhenSessionIsNotExpanded_DoesNothing()
        {
            var unityProvider = new UnityProvider();
            var originalValue = new Hotkey(unityProvider)
            {
                Valid = true
            };
            var workingChord = new HotkeyChord(unityProvider);
            var entry = CreateEntryBinding(originalValue);
            var session = new HotkeyEditSession
            {
                Entry = entry.Object,
                State = HotkeyEditState.WaitingPress,
                OriginalValue = originalValue
            };
            Hotkey.GlobalValid = true;

            session.BeginRecord(workingChord);

            Assert.True(Hotkey.GlobalValid);
            Assert.True(originalValue.Valid);
            Assert.Null(session.WorkingChord);
            Assert.Equal(HotkeyEditState.WaitingPress, session.State);
        }

        [Fact]
        public void BeginRecord_WhenSessionIsExpanded_SetsRecordingState()
        {
            var unityProvider = new UnityProvider();
            var originalValue = new Hotkey(unityProvider)
            {
                Valid = true
            };
            var workingChord = new HotkeyChord(unityProvider);
            var entry = CreateEntryBinding(originalValue);
            var session = new HotkeyEditSession
            {
                Entry = entry.Object,
                State = HotkeyEditState.Expanded,
                OriginalValue = originalValue
            };
            Hotkey.GlobalValid = true;

            session.BeginRecord(workingChord);

            Assert.False(Hotkey.GlobalValid);
            Assert.False(originalValue.Valid);
            Assert.Same(workingChord, session.WorkingChord);
            Assert.Equal(HotkeyEditState.WaitingPress, session.State);
        }

        [Fact]
        public void CancelRecord_WhenSessionIsExpanded_DoesNothing()
        {
            var unityProvider = new UnityProvider();
            var originalValue = new Hotkey(unityProvider)
            {
                Valid = false
            };
            var previousWorkingValue = new Hotkey(unityProvider);
            var workingChord = new HotkeyChord(unityProvider);
            var entry = CreateEntryBinding(originalValue);
            var session = new HotkeyEditSession
            {
                Entry = entry.Object,
                State = HotkeyEditState.Expanded,
                OriginalValue = originalValue,
                WorkingValue = previousWorkingValue,
                WorkingChord = workingChord
            };
            Hotkey.GlobalValid = false;

            session.CancelRecord();

            Assert.False(Hotkey.GlobalValid);
            Assert.False(originalValue.Valid);
            Assert.Same(previousWorkingValue, session.WorkingValue);
            Assert.Same(workingChord, session.WorkingChord);
            Assert.Equal(HotkeyEditState.Expanded, session.State);
        }

        [Fact]
        public void CancelRecord_WhenRecording_RestoresEditingState()
        {
            var unityProvider = new UnityProvider();
            var originalValue = new Hotkey(unityProvider)
            {
                Valid = false
            };
            originalValue.Add(new HotkeyChord(new KeyboardChord(unityProvider, new KeyboardTrigger(Key.A, unityProvider)), unityProvider));
            var workingValue = new Hotkey(unityProvider);
            var workingChord = new HotkeyChord(unityProvider);
            var entry = CreateEntryBinding(originalValue);
            var session = new HotkeyEditSession
            {
                Entry = entry.Object,
                State = HotkeyEditState.WaitingPress,
                OriginalValue = originalValue,
                WorkingValue = workingValue,
                WorkingChord = workingChord
            };
            Hotkey.GlobalValid = false;

            session.CancelRecord();

            Assert.True(Hotkey.GlobalValid);
            Assert.True(originalValue.Valid);
            Assert.NotSame(workingValue, session.WorkingValue);
            Assert.NotSame(originalValue, session.WorkingValue);
            Assert.Equal(originalValue.Count, session.WorkingValue.Count);
            Assert.False(session.WorkingValue.Valid);
            Assert.Null(session.WorkingChord);
            Assert.Equal(HotkeyEditState.Expanded, session.State);
        }

        [Fact]
        public void CancelEdit_WhenCalled_ClearsSession()
        {
            var unityProvider = new UnityProvider();
            var hotkey = new Hotkey(unityProvider)
            {
                Valid = false
            };
            Hotkey.GlobalValid = false;
            var session = new HotkeyEditSession
            {
                Entry = CreateEntryBinding(hotkey).Object,
                State = HotkeyEditState.WaitingConfirm,
                OriginalValue = new Hotkey(unityProvider),
                WorkingValue = new Hotkey(unityProvider),
                WorkingChord = new HotkeyChord(unityProvider),
                PreviewGamepadChord = new GamepadChord(unityProvider, new GamepadTrigger(GamepadButton.DpadUp, unityProvider)),
                PreviewKeyboardChord = new KeyboardChord(unityProvider, new KeyboardTrigger(Key.A, unityProvider))
            };

            session.CancelEdit();

            Assert.True(hotkey.Valid);
            Assert.True(Hotkey.GlobalValid);
            Assert.Null(session.Entry);
            Assert.Equal(HotkeyEditState.Idle, session.State);
            Assert.Null(session.OriginalValue);
            Assert.Null(session.WorkingValue);
            Assert.Null(session.WorkingChord);
            Assert.Null(session.PreviewGamepadChord);
            Assert.Null(session.PreviewKeyboardChord);
        }

        [Fact]
        public void ConfirmEdit_WhenStateIsNotWaitingConfirm_ClearsSession()
        {
            var unityProvider = new UnityProvider();
            var hotkey = new Hotkey(unityProvider)
            {
                Valid = false
            };
            Hotkey.GlobalValid = false;
            var session = new HotkeyEditSession
            {
                Entry = CreateEntryBinding(hotkey).Object,
                State = HotkeyEditState.Expanded,
                OriginalValue = hotkey,
                WorkingValue = new Hotkey(unityProvider),
                WorkingChord = new HotkeyChord(unityProvider)
            };

            session.ConfirmEdit(new EntryChangeSink());

            Assert.True(hotkey.Valid);
            Assert.True(Hotkey.GlobalValid);
            Assert.Null(session.Entry);
            Assert.Equal(HotkeyEditState.Idle, session.State);
            Assert.Null(session.OriginalValue);
            Assert.Null(session.WorkingValue);
            Assert.Null(session.WorkingChord);
        }

        [Fact]
        public void ConfirmEdit_WhenStateIsWaitingConfirm_ConfirmsChangeAndClearsSession()
        {
            var unityProvider = new UnityProvider();
            var originalValue = new Hotkey(unityProvider)
            {
                Valid = false
            };
            var entry = CreateEntryBinding(originalValue);
            var session = new HotkeyEditSession
            {
                Entry = entry.Object,
                State = HotkeyEditState.WaitingConfirm,
                OriginalValue = originalValue,
                WorkingValue = new Hotkey(unityProvider),
                WorkingChord = new HotkeyChord(new KeyboardChord(unityProvider, new KeyboardTrigger(Key.A, unityProvider)), unityProvider)
            };
            Hotkey.GlobalValid = false;

            session.ConfirmEdit(new EntryChangeSink());

            var updatedValue = Assert.IsType<Hotkey>(entry.Object.Value);
            Assert.NotSame(originalValue, updatedValue);
            Assert.Equal(1, updatedValue.Count);
            Assert.True(updatedValue.Valid);
            Assert.True(Hotkey.GlobalValid);
            Assert.Null(session.Entry);
            Assert.Equal(HotkeyEditState.Idle, session.State);
            Assert.Null(session.OriginalValue);
            Assert.Null(session.WorkingValue);
            Assert.Null(session.WorkingChord);
            Assert.Null(session.PreviewGamepadChord);
            Assert.Null(session.PreviewKeyboardChord);
        }


        [Fact]
        public void ConfirmRecord_WhenGamepadPreviewIsValid_UpdatesEntryAndResetsEditingState()
        {
            var unityProvider = new UnityProvider();
            var originalValue = new Hotkey(unityProvider);
            var entry = CreateEntryBinding(originalValue);
            var workingChord = new HotkeyChord(unityProvider);
            var previewGamepadChord = new GamepadChord(unityProvider, new GamepadTrigger(GamepadButton.DpadUp, unityProvider));
            var session = new HotkeyEditSession
            {
                Entry = entry.Object,
                State = HotkeyEditState.WaitingConfirm,
                OriginalValue = originalValue,
                WorkingValue = originalValue.Clone(),
                WorkingChord = workingChord,
                PreviewGamepadChord = previewGamepadChord,
                PreviewKeyboardChord = new KeyboardChord(unityProvider)
            };
            Hotkey.GlobalValid = false;
            originalValue.Valid = false;

            session.ConfirmRecord(new EntryChangeSink());

            var updatedValue = Assert.IsType<Hotkey>(entry.Object.Value);
            var updatedChord = Assert.Single(updatedValue.Hotkeys);
            Assert.Same(previewGamepadChord, updatedChord.Chord);
            Assert.True(updatedValue.Valid);
            Assert.True(Hotkey.GlobalValid);
            Assert.True(session.OriginalValue.Valid);
            Assert.Null(session.WorkingChord);
            Assert.Null(session.PreviewGamepadChord);
            Assert.Null(session.PreviewKeyboardChord);
            Assert.NotSame(updatedValue, session.WorkingValue);
            Assert.False(session.WorkingValue.Valid);
            Assert.Equal(updatedValue.Count, session.WorkingValue.Count);
            Assert.Equal(HotkeyEditState.Expanded, session.State);
        }

        [Fact]
        public void ConfirmRecord_WhenSessionIsNotWaitingConfirm_CancelsRecord()
        {
            var unityProvider = new UnityProvider();
            var originalValue = new Hotkey(unityProvider)
            {
                Valid = false
            };
            originalValue.Add(new HotkeyChord(new KeyboardChord(unityProvider, new KeyboardTrigger(Key.A, unityProvider)), unityProvider));
            var previousWorkingValue = new Hotkey(unityProvider);
            var entry = CreateEntryBinding(originalValue);
            var session = new HotkeyEditSession
            {
                Entry = entry.Object,
                State = HotkeyEditState.WaitingPress,
                OriginalValue = originalValue,
                WorkingValue = previousWorkingValue,
                WorkingChord = new HotkeyChord(unityProvider)
            };
            Hotkey.GlobalValid = false;

            session.ConfirmRecord(new EntryChangeSink());

            Assert.True(Hotkey.GlobalValid);
            Assert.True(originalValue.Valid);
            Assert.NotSame(previousWorkingValue, session.WorkingValue);
            Assert.Equal(originalValue.Count, session.WorkingValue.Count);
            Assert.False(session.WorkingValue.Valid);
            Assert.Null(session.WorkingChord);
            Assert.Equal(HotkeyEditState.Expanded, session.State);
        }

        [Fact]
        public void ConfirmRecord_WhenKeyboardPreviewIsValid_UsesKeyboardChord()
        {
            var unityProvider = new UnityProvider();
            var originalValue = new Hotkey(unityProvider);
            var entry = CreateEntryBinding(originalValue);
            var workingChord = new HotkeyChord(unityProvider);
            var previewKeyboardChord = new KeyboardChord(unityProvider, new KeyboardTrigger(Key.B, unityProvider));
            var session = new HotkeyEditSession
            {
                Entry = entry.Object,
                State = HotkeyEditState.WaitingConfirm,
                OriginalValue = originalValue,
                WorkingValue = originalValue.Clone(),
                WorkingChord = workingChord,
                PreviewGamepadChord = new GamepadChord(unityProvider),
                PreviewKeyboardChord = previewKeyboardChord
            };

            session.ConfirmRecord(new EntryChangeSink());

            var updatedValue = Assert.IsType<Hotkey>(entry.Object.Value);
            var updatedChord = Assert.Single(updatedValue.Hotkeys);
            Assert.Same(previewKeyboardChord, updatedChord.Chord);
            Assert.Equal(HotkeyEditState.Expanded, session.State);
        }

        [Fact]
        public void ConfirmRecord_WhenBothPreviewsAreValid_KeepsExistingWorkingChordAndWritesResult()
        {
            var unityProvider = new UnityProvider();
            var originalValue = new Hotkey(unityProvider);
            var originalChord = new HotkeyChord(new KeyboardChord(unityProvider, new KeyboardTrigger(Key.C, unityProvider)), unityProvider);
            originalValue.Add(originalChord);
            var entry = CreateEntryBinding(originalValue);
            var workingValue = originalValue.Clone();
            var workingChord = new HotkeyChord(new KeyboardChord(unityProvider, new KeyboardTrigger(Key.C, unityProvider)), unityProvider);
            var session = new HotkeyEditSession
            {
                Entry = entry.Object,
                State = HotkeyEditState.WaitingConfirm,
                OriginalValue = originalValue,
                WorkingValue = workingValue,
                WorkingChord = workingChord,
                PreviewGamepadChord = new GamepadChord(unityProvider, new GamepadTrigger(GamepadButton.DpadUp, unityProvider)),
                PreviewKeyboardChord = new KeyboardChord(unityProvider, new KeyboardTrigger(Key.V, unityProvider))
            };

            session.ConfirmRecord(new EntryChangeSink());

            var updatedValue = Assert.IsType<Hotkey>(entry.Object.Value);
            Assert.Equal(2, updatedValue.Count);
            Assert.Equal("C,C", updatedValue.ToString());
            Assert.Equal("C", workingChord.ToString());
            Assert.Equal(HotkeyEditState.Expanded, session.State);
            Assert.Null(session.WorkingChord);
        }

        [Fact]
        public void ConfirmRecord_WhenResultHasNoValidChord_DoesNotWriteBackAndRefreshesWorkingCopy()
        {
            var unityProvider = new UnityProvider();
            var originalValue = new Hotkey(unityProvider);
            originalValue.Add(new HotkeyChord(new KeyboardChord(unityProvider, new KeyboardTrigger(Key.D, unityProvider)), unityProvider));
            var entry = CreateEntryBinding(originalValue);
            var session = new HotkeyEditSession
            {
                Entry = entry.Object,
                State = HotkeyEditState.WaitingConfirm,
                OriginalValue = originalValue,
                WorkingValue = new Hotkey(unityProvider),
                WorkingChord = new HotkeyChord(unityProvider),
                PreviewGamepadChord = new GamepadChord(unityProvider),
                PreviewKeyboardChord = new KeyboardChord(unityProvider)
            };

            session.ConfirmRecord(new EntryChangeSink());

            Assert.Same(originalValue, entry.Object.Value);
            Assert.Equal(originalValue.Count, session.WorkingValue.Count);
            Assert.False(session.WorkingValue.Valid);
            Assert.Equal(HotkeyEditState.Expanded, session.State);
        }

        [Fact]
        public void SetWorkingChord_WhenNotEditing_DoesNothing()
        {
            var unityProvider = new UnityProvider();
            var chord = new HotkeyChord(new KeyboardChord(unityProvider, new KeyboardTrigger(Key.E, unityProvider)), unityProvider);
            var session = new HotkeyEditSession
            {
                State = HotkeyEditState.Idle
            };

            session.SetWorkingChord(chord, new EntryChangeSink());

            Assert.Equal(HotkeyEditState.Idle, session.State);
            Assert.Null(session.WorkingChord);
        }

        [Fact]
        public void SetWorkingChord_WhenEditing_ReplacesCurrentChordAndBeginsRecording()
        {
            var unityProvider = new UnityProvider();
            var originalValue = new Hotkey(unityProvider);
            var originalChord = new HotkeyChord(new KeyboardChord(unityProvider, new KeyboardTrigger(Key.A, unityProvider)), unityProvider);
            originalValue.Add(originalChord);
            var entry = CreateEntryBinding(originalValue);
            entry.SetupGet(x => x.Key).Returns(nameof(SetWorkingChord_WhenEditing_ReplacesCurrentChordAndBeginsRecording));
            var newChord = new HotkeyChord(new KeyboardChord(unityProvider, new KeyboardTrigger(Key.B, unityProvider)), unityProvider);
            var session = new HotkeyEditSession
            {
                Entry = entry.Object,
                State = HotkeyEditState.WaitingConfirm,
                OriginalValue = originalValue,
                WorkingValue = originalValue.Clone(),
                WorkingChord = new HotkeyChord(new KeyboardChord(unityProvider, new KeyboardTrigger(Key.C, unityProvider)), unityProvider),
                PreviewGamepadChord = new GamepadChord(unityProvider),
                PreviewKeyboardChord = new KeyboardChord(unityProvider)
            };

            session.SetWorkingChord(newChord, new EntryChangeSink());

            Assert.Same(entry.Object.Value, session.OriginalValue);
            Assert.Same(newChord, session.WorkingChord);
            Assert.Equal(HotkeyEditState.WaitingPress, session.State);
            Assert.False(Hotkey.GlobalValid);
            Assert.False(session.OriginalValue.Valid);
        }

        [Fact]
        public void AddChord_WhenNotEditing_DoesNothing()
        {
            var unityProvider = new UnityProvider();
            var chord = new HotkeyChord(new KeyboardChord(unityProvider, new KeyboardTrigger(Key.F, unityProvider)), unityProvider);
            var session = new HotkeyEditSession();

            session.AddChord(chord, new EntryChangeSink());

            Assert.Equal(HotkeyEditState.Idle, session.State);
            Assert.Null(session.WorkingValue);
            Assert.Null(session.WorkingChord);
        }

        [Fact]
        public void AddChord_WhenEditing_AddsChordAndBeginsRecording()
        {
            var unityProvider = new UnityProvider();
            var originalValue = new Hotkey(unityProvider);
            originalValue.Add(new HotkeyChord(new KeyboardChord(unityProvider, new KeyboardTrigger(Key.A, unityProvider)), unityProvider));
            var entry = CreateEntryBinding(originalValue);
            var addedChord = new HotkeyChord(new KeyboardChord(unityProvider, new KeyboardTrigger(Key.G, unityProvider)), unityProvider);
            var session = new HotkeyEditSession
            {
                Entry = entry.Object,
                State = HotkeyEditState.WaitingConfirm,
                OriginalValue = originalValue,
                WorkingValue = originalValue.Clone(),
                WorkingChord = new HotkeyChord(new KeyboardChord(unityProvider, new KeyboardTrigger(Key.H, unityProvider)), unityProvider),
                PreviewGamepadChord = new GamepadChord(unityProvider),
                PreviewKeyboardChord = new KeyboardChord(unityProvider)
            };

            session.AddChord(addedChord, new EntryChangeSink());

            Assert.Contains(addedChord, session.WorkingValue.Hotkeys);
            Assert.Same(addedChord, session.WorkingChord);
            Assert.Equal(HotkeyEditState.WaitingPress, session.State);
        }

        [Fact]
        public void RemoveChord_WhenNotEditing_DoesNothing()
        {
            var unityProvider = new UnityProvider();
            var chord = new HotkeyChord(new KeyboardChord(unityProvider, new KeyboardTrigger(Key.I, unityProvider)), unityProvider);
            var session = new HotkeyEditSession();

            session.RemoveChord(chord, new EntryChangeSink());

            Assert.Equal(HotkeyEditState.Idle, session.State);
            Assert.Null(session.WorkingValue);
        }

        [Fact]
        public void DealWaitingPress_WhenNotEditingOrNotWaitingPress_DoesNothing()
        {
            var session = new HotkeyEditSession
            {
                State = HotkeyEditState.Expanded
            };

            session.DealWaitingPress();

            Assert.Equal(HotkeyEditState.Expanded, session.State);
            Assert.Null(session.PreviewGamepadChord);
            Assert.Null(session.PreviewKeyboardChord);
        }

        [Fact]
        public void DealWaitingPress_WhenNoInputPressed_LeavesStateUnchanged()
        {
            var unityProvider = new UnityProvider();
            var entry = CreateEntryBinding(new Hotkey(unityProvider));
            var session = new HotkeyEditSession
            {
                Entry = entry.Object,
                State = HotkeyEditState.WaitingPress,
                WorkingValue = new Hotkey(unityProvider)
            };

            session.DealWaitingPress();

            Assert.Equal(HotkeyEditState.WaitingPress, session.State);
            Assert.Null(session.PreviewGamepadChord);
            Assert.Null(session.PreviewKeyboardChord);
        }




        [Fact]
        public void DealRecording_WhenNotEditingOrNotRecording_DoesNothing()
        {
            var session = new HotkeyEditSession
            {
                State = HotkeyEditState.Expanded,
                PreviewGamepadChord = new GamepadChord(new UnityProvider()),
                PreviewKeyboardChord = new KeyboardChord(new UnityProvider()),
                WorkingChord = new HotkeyChord(new UnityProvider())
            };

            session.DealRecording();

            Assert.Equal(HotkeyEditState.Expanded, session.State);
            Assert.False(session.PreviewGamepadChord.IsValid);
            Assert.False(session.PreviewKeyboardChord.IsValid);
            Assert.False(session.WorkingChord.IsValid);
        }

        [Fact]
        public void DealRecording_WhenNoInputPressed_MovesToWaitingConfirm()
        {
            var unityProvider = new UnityProvider();
            var session = CreateEditingSession(unityProvider, HotkeyEditState.Recording);

            session.DealRecording();

            Assert.Equal(HotkeyEditState.WaitingConfirm, session.State);
        }


        [Fact]
        public void DealWaitingConfirm_WhenNotEditingOrNotWaitingConfirm_DoesNothing()
        {
            var session = new HotkeyEditSession
            {
                State = HotkeyEditState.Recording,
                PreviewGamepadChord = new GamepadChord(new UnityProvider()),
                PreviewKeyboardChord = new KeyboardChord(new UnityProvider()),
                WorkingChord = new HotkeyChord(new UnityProvider())
            };

            session.DealWaitingConfirm();

            Assert.Equal(HotkeyEditState.Recording, session.State);
            Assert.False(session.PreviewGamepadChord.IsValid);
            Assert.False(session.PreviewKeyboardChord.IsValid);
            Assert.False(session.WorkingChord.IsValid);
        }


        [Fact]
        public void DealWaitingConfirm_WhenWorkingChordIsInvalidAndNoInputPressed_ClearsPreviewsAndReturnsToRecording()
        {
            var unityProvider = new UnityProvider();
            var session = CreateEditingSession(unityProvider, HotkeyEditState.WaitingConfirm);
            session.PreviewGamepadChord = new GamepadChord(unityProvider, new GamepadTrigger(GamepadButton.DpadUp, unityProvider));
            session.PreviewKeyboardChord = new KeyboardChord(unityProvider, new KeyboardTrigger(Key.B, unityProvider));
            session.WorkingChord = new HotkeyChord(unityProvider);

            session.DealWaitingConfirm();

            Assert.Equal(HotkeyEditState.Recording, session.State);
            Assert.False(session.PreviewGamepadChord.IsValid);
            Assert.False(session.PreviewKeyboardChord.IsValid);
        }

        [Fact]
        public void DealWaitingConfirm_WhenWorkingChordIsValidAndNoInputPressed_LeavesStateUnchanged()
        {
            var unityProvider = new UnityProvider();
            var existingKeyboardPreview = new KeyboardChord(unityProvider, new KeyboardTrigger(Key.C, unityProvider));
            var existingGamepadPreview = new GamepadChord(unityProvider, new GamepadTrigger(GamepadButton.DpadUp, unityProvider));
            var workingChord = new HotkeyChord(existingKeyboardPreview, unityProvider);
            var session = CreateEditingSession(unityProvider, HotkeyEditState.WaitingConfirm);
            session.PreviewKeyboardChord = existingKeyboardPreview;
            session.PreviewGamepadChord = existingGamepadPreview;
            session.WorkingChord = workingChord;

            session.DealWaitingConfirm();

            Assert.Equal(HotkeyEditState.WaitingConfirm, session.State);
            Assert.Same(existingKeyboardPreview, session.PreviewKeyboardChord);
            Assert.Same(existingGamepadPreview, session.PreviewGamepadChord);
            Assert.Same(existingKeyboardPreview, session.WorkingChord.Chord);
        }

        [Fact]
        public void CaptureHotkeyInput_WhenEditingWithNoPressedInput_ReturnsEmptySnapshot()
        {
            var unityProvider = new UnityProvider();
            var session = CreateEditingSession(unityProvider, HotkeyEditState.Recording);

            var result = session.CaptureHotkeyInput();

            Assert.NotNull(result);
            Assert.False(result.HasAnyPressed);
            Assert.False(result.GamepadChord.IsValid);
            Assert.False(result.KeyboardChord.IsValid);
        }


        private static HotkeyEditSession CreateEditingSession(UnityProvider unityProvider, HotkeyEditState state)
        {
            return new HotkeyEditSession
            {
                Entry = CreateEntryBinding(new Hotkey(unityProvider)).Object,
                State = state,
                WorkingValue = new Hotkey(unityProvider),
                WorkingChord = new HotkeyChord(unityProvider),
                PreviewGamepadChord = new GamepadChord(unityProvider),
                PreviewKeyboardChord = new KeyboardChord(unityProvider)
            };
        }

        [Fact]
        public void CaptureHotkeyInput_WhenNotEditing_ReturnsNull()
        {
            var session = new HotkeyEditSession
            {
                State = HotkeyEditState.Idle
            };

            var result = session.CaptureHotkeyInput();

            Assert.Null(result);
        }


        private static Mock<IEntryBinding> CreateEntryBinding(object value)
        {
            var entry = new Mock<IEntryBinding>();
            entry.SetupProperty(x => x.Value, value);
            return entry;
        }
    }
}
