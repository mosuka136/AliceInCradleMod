using System;

namespace BetterExperience.HConfigGUI
{
    public interface IUiMetadata
    {
        Type MetadataType { get; }
    }

    public class UiSliderMetadata : IUiMetadata
    {
        public Type MetadataType => typeof(UiSliderMetadata);
        public float Min { get; }
        public float Max { get; }
        public float Step { get; }
        public UiSliderMetadata(float min, float max, float step)
        {
            Min = min;
            Max = max;
            Step = step;
        }
    }
}
