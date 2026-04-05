using System;
using System.Collections.Concurrent;
using System.Linq;

namespace BetterExperience.HClassAttribute
{
    public class ClassHelper
    {
        private static ConcurrentDictionary<(Type classType, string propertyName, Type attributeType), Attribute> _attributeCache = new ConcurrentDictionary<(Type classType, string propertyName, Type attributeType), Attribute>();

        public static TAttribute GetAttribute<TClass, TAttribute>(string propertyName) where TAttribute : Attribute
        {
            var key = (typeof(TClass), propertyName, typeof(TAttribute));
            if (_attributeCache.TryGetValue(key, out var cachedAttribute))
            {
                return (TAttribute)cachedAttribute;
            }
            var propertyInfo = typeof(TClass).GetProperty(propertyName);
            if (propertyInfo == null)
            {
                throw new ArgumentException($"Property '{propertyName}' not found in class '{typeof(TClass).FullName}'.");
            }
            var attribute = propertyInfo.GetCustomAttributes(typeof(TAttribute), false).FirstOrDefault() as TAttribute;
            _attributeCache[key] = attribute;
            return attribute;
        }

        public static (float Min, float Max, float Step)? GetSliderInfo<TClass>(string propertyName)
        {
            var sliderAttribute = GetAttribute<TClass, ConfigSliderAttribute>(propertyName);
            if (sliderAttribute != null)
            {
                return (sliderAttribute.Min, sliderAttribute.Max, sliderAttribute.Step);
            }
            return null;
        }
    }
}
