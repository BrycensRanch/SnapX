namespace ShareX.Core.Utils.Parsers
{
    public class CodeMenuEntryActions : CodeMenuEntry
    {
        protected override string Prefix { get; } = "$";

        public static readonly CodeMenuEntryActions input = new CodeMenuEntryActions("input", Resources.ActionsCodeMenuEntry_FilePath_File_path);
        public static readonly CodeMenuEntryActions output = new CodeMenuEntryActions("output", Resources.ActionsCodeMenuEntry_OutputFilePath_File_path_without_extension____Output_file_name_extension_);

        public CodeMenuEntryActions(string value, string description) : base(value, description)
        {
        }

        public static string Parse(string pattern, string inputPath, string outputPath)
        {
            string result = pattern;

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
}
