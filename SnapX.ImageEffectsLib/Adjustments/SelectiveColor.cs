
// SPDX-License-Identifier: GPL-3.0-or-later


using ShareX.HelpersLib;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;

namespace SnapX.ImageEffectsLib
{
    [Description("Selective color")]
    internal class SelectiveColor : ImageEffect
    {
        [DefaultValue(typeof(Color), "White"), Editor(typeof(MyColorEditor), typeof(UITypeEditor)), TypeConverter(typeof(MyColorConverter))]
        public Color LightColor { get; set; }

        [DefaultValue(typeof(Color), "Black"), Editor(typeof(MyColorEditor), typeof(UITypeEditor)), TypeConverter(typeof(MyColorConverter))]
        public Color DarkColor { get; set; }

        [DefaultValue(10)]
        public int PaletteSize { get; set; }

        public SelectiveColor()
        {
            this.ApplyDefaultPropertyValues();
        }

        public override Bitmap Apply(Bitmap bmp)
        {
            ImageHelpers.SelectiveColor(bmp, LightColor, DarkColor, MathHelpers.Clamp(PaletteSize, 2, 100));
            return bmp;
        }
    }
}