using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BetterExperience.HClassAttribute
{
    /// <summary>
    /// 基于特性的反射辅助工具。
    /// 该类型集中处理配置 UI 元数据、游戏启动注册等反射查询，并缓存属性特性以减少每帧 GUI 查询成本。
    /// </summary>
    public class ClassHelper
    {
        // 属性特性按“类型 + 属性名 + 特性类型”缓存；未找到时也缓存 null，避免重复反射。
        private static ConcurrentDictionary<(Type classType, string propertyName, Type attributeType), Attribute> _attributeCache = new ConcurrentDictionary<(Type classType, string propertyName, Type attributeType), Attribute>();

        /// <summary>
        /// 获取指定属性上的特性。
        /// </summary>
        /// <typeparam name="TClass">声明属性的类型。</typeparam>
        /// <typeparam name="TAttribute">要读取的特性类型。</typeparam>
        /// <param name="propertyName">拥有指定特性的属性名。</param>
        /// <returns>找到的特性；属性存在但未标记时返回 <c>null</c>。</returns>
        /// <exception cref="ArgumentException">属性不存在时抛出。</exception>
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

        /// <summary>
        /// 获取程序集内标记了指定特性的类型。
        /// </summary>
        public static Type[] GetClasses<TAttribute>(Assembly assembly) where TAttribute : Attribute
        {
            var result = new List<Type>();

            foreach (var type in GetTypeSafe(assembly))
            {
                try
                {
                    if (type.GetCustomAttributes(typeof(TAttribute), false).Length > 0)
                        result.Add(type);
                }
                catch (Exception ex)
                {
                    HLog.Error($"Failed to get attributes from type '{type.FullName}'.", ex);
                }
            }

            return result.ToArray();
        }

        /// <summary>
        /// 获取程序集内标记了指定特性的方法。
        /// </summary>
        public static MethodInfo[] GetMethods<TAttribute>(Assembly assembly) where TAttribute : Attribute
        {
            var result = new List<MethodInfo>();

            foreach (var type in GetTypeSafe(assembly))
            {
                MethodInfo[] methods;

                try
                {
                    methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                }
                catch (Exception ex)
                {
                    HLog.Error($"Failed to get methods from type '{type.FullName}'.", ex);
                    continue;
                }

                foreach (var method in methods)
                {
                    try
                    {
                        if (method.GetCustomAttributes(typeof(TAttribute), false).Length > 0)
                            result.Add(method);
                    }
                    catch (Exception ex)
                    {
                        HLog.Error($"Failed to get attributes from method '{method.Name}' in type '{type.FullName}'.", ex);
                    }
                }
            }

            return result.ToArray();
        }

        public static Type[] GetTypeSafe(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                HLog.Error($"Failed to load some types from assembly '{assembly.FullName}'. Loaded {ex.Types.Length} types, {ex.LoaderExceptions.Length} loader exceptions.", ex);
                return ex.Types.Where(t => t != null).ToArray();
            }
            catch (Exception ex)
            {
                HLog.Error($"Failed to load types from assembly '{assembly.FullName}'.", ex);
                return Array.Empty<Type>();
            }
        }

        /// <summary>
        /// 读取配置属性上的滑条元数据。
        /// </summary>
        public static (float Min, float Max, float Step)? GetSliderInfo<TClass>(string propertyName)
        {
            var sliderAttribute = GetAttribute<TClass, ConfigSliderAttribute>(propertyName);
            if (sliderAttribute != null)
            {
                return (sliderAttribute.Min, sliderAttribute.Max, sliderAttribute.Step);
            }
            return null;
        }

        public static Type[] GetRegisterOnGameBootClasses(Assembly assembly)
        {
            return GetClasses<RegisterOnGameBootAttribute>(assembly);
        }

        public static MethodInfo[] GetInitializeOnGameBootMethods(Assembly assembly)
        {
            return GetMethods<InitializeOnGameBootAttribute>(assembly);
        }
    }
}
