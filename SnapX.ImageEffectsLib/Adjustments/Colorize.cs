
// SPDX-License-Identifier: GPL-3.0-or-later


using ShareX.HelpersLib;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;

namespace SnapX.ImageEffectsLib
{
    internal class Colorize : ImageEffect
    {
        [DefaultValue(typeof(Color), "Red"), Editor(typeof(MyColorEditor), typeof(UITypeEditor)), TypeConverter(typeof(MyColorConverter))]
        public Color Color { get; set; }

        [DefaultValue(0f)]
        public float Value { get; set; }

        public Colorize()
        {
            this.ApplyDefaultPropertyValues();
        }

        public override Bitmap Apply(Bitmap bmp)
        {
            using (bmp)
            {
                return ColorMatrixManager.Colorize(Color, Value).Apply(bmp);
            }
        }

        protected override string GetSummary()
        {
            return $"{Color.R}, {Color.G}, {Color.B}";
        }
    }
}