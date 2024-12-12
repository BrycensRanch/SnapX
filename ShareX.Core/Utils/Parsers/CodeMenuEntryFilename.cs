namespace ShareX.Core.Utils.Parsers
{
    public class CodeMenuEntryFilename : CodeMenuEntry
    {
        protected override string Prefix { get; } = "%";

        public static readonly CodeMenuEntryFilename t = new CodeMenuEntryFilename("t", "Title of window", "Window");
        public static readonly CodeMenuEntryFilename pn = new CodeMenuEntryFilename("pn", "Process name of window", "Window");
        public static readonly CodeMenuEntryFilename y = new CodeMenuEntryFilename("y", "Current year", "Date and time");
        public static readonly CodeMenuEntryFilename yy = new CodeMenuEntryFilename("yy", "Year (2 digits)", "Date and time");
        public static readonly CodeMenuEntryFilename mo = new CodeMenuEntryFilename("mo", "Month", "Date and time");
        public static readonly CodeMenuEntryFilename mon = new CodeMenuEntryFilename("mon", "Month name (Local language)", "Date and time");
        public static readonly CodeMenuEntryFilename mon2 = new CodeMenuEntryFilename("mon2", "Month name (English)", "Date and time");
        public static readonly CodeMenuEntryFilename w = new CodeMenuEntryFilename("w", "Week name (Local language)", "Date and time");
        public static readonly CodeMenuEntryFilename w2 = new CodeMenuEntryFilename("w2", "Week name (English)", "Date and time");
        public static readonly CodeMenuEntryFilename wy = new CodeMenuEntryFilename("wy", "Week of year", "Date and time");
        public static readonly CodeMenuEntryFilename d = new CodeMenuEntryFilename("d", "Day", "Date and time");
        public static readonly CodeMenuEntryFilename h = new CodeMenuEntryFilename("h", "Hour", "Date and time");
        public static readonly CodeMenuEntryFilename mi = new CodeMenuEntryFilename("mi", "Minute", "Date and time");
        public static readonly CodeMenuEntryFilename s = new CodeMenuEntryFilename("s", "Second", "Date and time");
        public static readonly CodeMenuEntryFilename ms = new CodeMenuEntryFilename("ms", "Millisecond", "Date and time");
        public static readonly CodeMenuEntryFilename pm = new CodeMenuEntryFilename("pm", "AM/PM", "Date and time");
        public static readonly CodeMenuEntryFilename unix = new CodeMenuEntryFilename("unix", "Unix timestamp", "Date and time");
        public static readonly CodeMenuEntryFilename i = new CodeMenuEntryFilename("i", "Auto increment number", "Incremental");
        public static readonly CodeMenuEntryFilename ia = new CodeMenuEntryFilename("ia", "Auto increment alphanumeric case-insensitive (0 pad left using {n})", "Incremental");
        public static readonly CodeMenuEntryFilename iAa = new CodeMenuEntryFilename("iAa", "Auto increment alphanumeric case-sensitive (0 pad left using {n})", "Incremental");
        public static readonly CodeMenuEntryFilename ib = new CodeMenuEntryFilename("ib", "Auto increment by base {n} using alphanumeric (1 &lt; n &lt; 63)", "Incremental");
        public static readonly CodeMenuEntryFilename ix = new CodeMenuEntryFilename("ix", "Auto increment hexadecimal (0 pad left using {n})", "Incremental");
        public static readonly CodeMenuEntryFilename rn = new CodeMenuEntryFilename("rn", "Random number 0 to 9 (Repeat using {n})", "Random");
        public static readonly CodeMenuEntryFilename ra = new CodeMenuEntryFilename("ra", "Random alphanumeric char (Repeat using {n})", "Random");
        public static readonly CodeMenuEntryFilename rna = new CodeMenuEntryFilename("rna", "Random non ambiguous alphanumeric char (Repeat using {n})", "Random");
        public static readonly CodeMenuEntryFilename rx = new CodeMenuEntryFilename("rx", "Random hexadecimal char (Repeat using {n})", "Random");
        public static readonly CodeMenuEntryFilename guid = new CodeMenuEntryFilename("guid", "Random GUID", "Random");
        public static readonly CodeMenuEntryFilename rf = new CodeMenuEntryFilename("rf", "Random line from a file (Use {filepath} to determine the file)", "Random");
        public static readonly CodeMenuEntryFilename width = new CodeMenuEntryFilename("width", "Image width", "Image");
        public static readonly CodeMenuEntryFilename height = new CodeMenuEntryFilename("height", "Image height", "Image");
        public static readonly CodeMenuEntryFilename un = new CodeMenuEntryFilename("un", "Username", "Computer");
        public static readonly CodeMenuEntryFilename uln = new CodeMenuEntryFilename("uln", "User login name", "Computer");
        public static readonly CodeMenuEntryFilename cn = new CodeMenuEntryFilename("cn", "Computer name/HOSTNAME", "Computer");
        public static readonly CodeMenuEntryFilename n = new CodeMenuEntryFilename("n", "New line");

        public CodeMenuEntryFilename(string value, string description, string category = null) : base(value, description, category)
        {
        }
    }
}
