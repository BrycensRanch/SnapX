
// SPDX-License-Identifier: GPL-3.0-or-later


using ShareX.HelpersLib;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace ShareX.ImageEffectsLib
{
    [Description("Torn edge")]
    internal class TornEdge : ImageEffect
    {
        [DefaultValue(15)]
        public int Depth { get; set; }

        [DefaultValue(20)]
        public int Range { get; set; }

        [DefaultValue(AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right)]
        public AnchorStyles Sides { get; set; }

        [DefaultValue(true)]
        public bool CurvedEdges { get; set; }

        public TornEdge()
        {
            this.ApplyDefaultPropertyValues();
        }

        public override Bitmap Apply(Bitmap bmp)
        {
            return ImageHelpers.TornEdges(bmp, Depth, Range, Sides, CurvedEdges, true);
        }

        protected override string GetSummary()
        {
            return $"{Depth}, {Range}";
        }
    }
}