namespace BetterExperience.HConfigSpace
{
    public interface IConfigEntryAdapter
    {
        ConfigFileResult<string> Encode();
        ConfigFileResult<object> Decode(string content);
        ConfigFileResult<string> EncodeValueType();
    }
}
