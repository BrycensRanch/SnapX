
// SPDX-License-Identifier: GPL-3.0-or-later


using ShareX.HelpersLib;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;

namespace ShareX.ImageEffectsLib
{
    [Description("Replace color")]
    internal class ReplaceColor : ImageEffect
    {
        [DefaultValue(typeof(Color), "White"), Editor(typeof(MyColorEditor), typeof(UITypeEditor)), TypeConverter(typeof(MyColorConverter))]
        public Color SourceColor { get; set; }

        [DefaultValue(false)]
        public bool AutoSourceColor { get; set; }

        [DefaultValue(typeof(Color), "Transparent"), Editor(typeof(MyColorEditor), typeof(UITypeEditor)), TypeConverter(typeof(MyColorConverter))]
        public Color TargetColor { get; set; }

        [DefaultValue(0)]
        public int Threshold { get; set; }

        public ReplaceColor()
        {
            this.ApplyDefaultPropertyValues();
        }

        public override Bitmap Apply(Bitmap bmp)
        {
            ImageHelpers.ReplaceColor(bmp, SourceColor, TargetColor, AutoSourceColor, Threshold);
            return bmp;
        }
    }
}