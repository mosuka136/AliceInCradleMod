namespace BetterExperience.HConfigSpace
{
    /// <summary>
    /// 自定义配置值类型的文件编码适配器。
    /// 实现类型需要提供无参构造函数，因为解码和类型说明生成会通过反射创建实例。
    /// </summary>
    public interface IConfigEntryAdapter
    {
        /// <summary>
        /// 将当前对象编码为配置文件中的单个值文本。
        /// </summary>
        ConfigFileResult<string> Encode();
        /// <summary>
        /// 从配置文件中的单个值文本解码对象。
        /// </summary>
        ConfigFileResult<object> Decode(string content);
        /// <summary>
        /// 返回写入配置文件注释中的类型说明。
        /// </summary>
        ConfigFileResult<string> EncodeValueType();
    }
}
