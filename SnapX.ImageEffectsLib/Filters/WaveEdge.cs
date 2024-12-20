
// SPDX-License-Identifier: GPL-3.0-or-later


using ShareX.HelpersLib;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace ShareX.ImageEffectsLib
{
    [Description("Wave edge")]
    internal class WaveEdge : ImageEffect
    {
        [DefaultValue(15)]
        public int Depth { get; set; }

        [DefaultValue(20)]
        public int Range { get; set; }

        [DefaultValue(AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right)]
        public AnchorStyles Sides { get; set; }

        public WaveEdge()
        {
            this.ApplyDefaultPropertyValues();
        }

        public override Bitmap Apply(Bitmap bmp)
        {
            return ImageHelpers.WavyEdges(bmp, Depth, Range, Sides);
        }

        protected override string GetSummary()
        {
            return $"{Depth}, {Range}";
        }
    }
}