
// SPDX-License-Identifier: GPL-3.0-or-later


using ShareX.Core.Utils;

namespace ShareX.Core.Upload.Custom.Functions
{
    // Example: {input}
    internal class CustomUploaderFunctionInput : CustomUploaderFunction
    {
        public override string Name { get; } = "input";

        public override string Call(ShareXCustomUploaderSyntaxParser parser, string[] parameters)
        {
            if (parser.URLEncode)
            {
                return URLHelpers.URLEncode(parser.Input);
            }

            return parser.Input;
        }
    }
}
