
// SPDX-License-Identifier: GPL-3.0-or-later


using ShareX.Core.Utils.Cryptographic;

namespace ShareX.Core.Upload.Custom.Functions
{
    // Example: Basic {base64:username:password}
    internal class CustomUploaderFunctionBase64 : CustomUploaderFunction
    {
        public override string Name { get; } = "base64";

        public override int MinParameterCount { get; } = 1;

        public override string Call(ShareXCustomUploaderSyntaxParser parser, string[] parameters)
        {
            string text = parameters[0];

            if (!string.IsNullOrEmpty(text))
            {
                return TranslatorHelper.TextToBase64(text);
            }

            return null;
        }
    }
}
