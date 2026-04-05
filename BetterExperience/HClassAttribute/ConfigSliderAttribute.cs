using System;

namespace BetterExperience.HClassAttribute
{
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public class ConfigSliderAttribute : Attribute
    {
        public float Min { get; set; }
        public float Max { get; set; }
        public float Step { get; set; }

        public ConfigSliderAttribute(float min, float max, float step = -1f)
        {
            Min = min;
            Max = max;
            Step = step;
        }
    }
}
