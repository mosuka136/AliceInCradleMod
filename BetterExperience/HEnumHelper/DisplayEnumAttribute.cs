using System;

namespace BetterExperience.HEnumHelper
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class DisplayEnumAttribute : Attribute
    {
        public bool IsDisplay { get; set; }
        public DisplayEnumAttribute(bool isDisplay = true)
        {
            IsDisplay = isDisplay;
        }
    }
}
