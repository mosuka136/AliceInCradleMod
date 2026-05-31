using BetterExperience.HConfigGUI.Bindings;
using BetterExperience.HProvider;
using System;

namespace BetterExperience.HConfigGUI.Editor.ValueEditor
{
    public class NumberEditor : IValueEditor
    {
        public float DelayApplyDuration { get; set; } = 0.5f;

        public UnityGuiProvider UnityGui { get; }

        public NumberEditor(UnityGuiProvider unityGui)
        {
            UnityGui = unityGui;
        }

        public virtual bool CanEdit(IEntryBinding entry)
        {
            var type = entry.ValueType;
            return type.IsPrimitive && type != typeof(bool) && type != typeof(char);
        }

        public virtual void DrawValue(IEntryBinding entry, GuiStateStore state, EntryChangeSink changeSink)
        {
            if (!entry.ValueType.IsPrimitive || entry.ValueType == typeof(bool) || entry.ValueType == typeof(char))
                return;

            var key = state.GetKey(entry, "_number");
            var valueString = state.GetText(key, entry.Value.ToString());
            string newValueString = UnityGui.TextField(valueString, UnityGui.ExpandWidth(true));
            if (valueString != newValueString)
            {
                state.SetText(key, newValueString);
                SetValue(entry, newValueString, changeSink);
            }
        }

        public void SetValue(IEntryBinding entry, string newValueString, EntryChangeSink changeSink)
        {
            switch (entry.ValueType)
            {
                case Type t when t == typeof(byte):
                    if (byte.TryParse(newValueString, out var byteValue))
                        changeSink.SetValue(entry, byteValue, DelayApplyDuration);
                    break;
                case Type t when t == typeof(sbyte):
                    if (sbyte.TryParse(newValueString, out var sbyteValue))
                        changeSink.SetValue(entry, sbyteValue, DelayApplyDuration);
                    break;
                case Type t when t == typeof(short):
                    if (short.TryParse(newValueString, out var shortValue))
                        changeSink.SetValue(entry, shortValue, DelayApplyDuration);
                    break;
                case Type t when t == typeof(ushort):
                    if (ushort.TryParse(newValueString, out var ushortValue))
                        changeSink.SetValue(entry, ushortValue, DelayApplyDuration);
                    break;
                case Type t when t == typeof(int):
                    if (int.TryParse(newValueString, out var intValue))
                        changeSink.SetValue(entry, intValue, DelayApplyDuration);
                    break;
                case Type t when t == typeof(uint):
                    if (uint.TryParse(newValueString, out var uintValue))
                        changeSink.SetValue(entry, uintValue, DelayApplyDuration);
                    break;
                case Type t when t == typeof(long):
                    if (long.TryParse(newValueString, out var longValue))
                        changeSink.SetValue(entry, longValue, DelayApplyDuration);
                    break;
                case Type t when t == typeof(ulong):
                    if (ulong.TryParse(newValueString, out var ulongValue))
                        changeSink.SetValue(entry, ulongValue, DelayApplyDuration);
                    break;
                case Type t when t == typeof(float):
                    if (float.TryParse(newValueString, out var floatValue))
                        changeSink.SetValue(entry, floatValue, DelayApplyDuration);
                    break;
                case Type t when t == typeof(double):
                    if (double.TryParse(newValueString, out var doubleValue))
                        changeSink.SetValue(entry, doubleValue, DelayApplyDuration);
                    break;
                default:
                    break;
            }
        }

        public void DrawExtra(IEntryBinding entry, GuiStateStore state, EntryChangeSink changeSink)
        {
        }
    }
}
