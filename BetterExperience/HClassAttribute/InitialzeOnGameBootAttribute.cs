using System;

namespace BetterExperience.HClassAttribute
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class InitializeOnGameBootAttribute : Attribute
    {
    }
}
