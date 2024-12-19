
// SPDX-License-Identifier: GPL-3.0-or-later


using ShareX.Core.Utils;

namespace ShareX.Core.Indexer;

public static class HtmlHelper
{
    public static string StartTag(string tag, string style = "", string otherFields = "")
    {
        string css = "";

        if (!string.IsNullOrEmpty(style))
        {
            css = $" style=\"{style}\"";
        }

        string fields = "";

        if (!string.IsNullOrEmpty(otherFields))
        {
            fields = $" {otherFields}";
        }

        return $"<{tag}{css}{fields}>";
    }

    public static string EndTag(string tag)
    {
        return $"</{tag}>";
    }

    public static string Tag(string tag, string content, string style = "", string otherFields = "")
    {
        return StartTag(tag, style, otherFields) + URLHelpers.HtmlEncode(content) + EndTag(tag);
    }
}

