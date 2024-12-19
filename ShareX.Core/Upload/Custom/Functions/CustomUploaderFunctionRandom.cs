
// SPDX-License-Identifier: GPL-3.0-or-later


using ShareX.Core.Utils.Random;

namespace ShareX.Core.Upload.Custom.Functions
{
    // Example: {random:domain1.com|domain2.com}
    internal class CustomUploaderFunctionRandom : CustomUploaderFunction
    {
        public override string Name { get; } = "random";

        public override int MinParameterCount { get; } = 2;

        public override string Call(ShareXCustomUploaderSyntaxParser parser, string[] parameters)
        {
            return RandomFast.Pick(parameters);
        }
    }
}
