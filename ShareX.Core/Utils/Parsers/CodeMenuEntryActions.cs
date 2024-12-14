namespace ShareX.Core.Utils.Parsers;

public class CodeMenuEntryActions : CodeMenuEntry
{
    protected override string Prefix { get; } = "$";

    public static readonly CodeMenuEntryActions input = new CodeMenuEntryActions("input", "File path");
    public static readonly CodeMenuEntryActions output = new CodeMenuEntryActions("output", "File path with output file name extension");

    public CodeMenuEntryActions(string value, string description) : base(value, description)
    {
    }

    public static string Parse(string pattern, string inputPath, string outputPath)
    {
        var result = pattern;

        if (inputPath != null)
        {
            result = result.Replace(input.ToPrefixString("%"), '"' + inputPath + '"');
            result = result.Replace(input.ToPrefixString(), inputPath);
        }

        if (outputPath != null)
        {
            result = result.Replace(output.ToPrefixString("%"), '"' + outputPath + '"');
            result = result.Replace(output.ToPrefixString(), outputPath);
        }

        return result;
    }
}

