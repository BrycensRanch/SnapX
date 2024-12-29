
// SPDX-License-Identifier: GPL-3.0-or-later


using ShareX.HelpersLib;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;

namespace SnapX.ImageEffectsLib
{
    [Description("Background")]
    public class DrawBackground : ImageEffect
    {
        [DefaultValue(typeof(Color), "Black"), Editor(typeof(MyColorEditor), typeof(UITypeEditor)), TypeConverter(typeof(MyColorConverter))]
        public Color Color { get; set; }

        [DefaultValue(false)]
        public bool UseGradient { get; set; }

        [Editor(typeof(GradientEditor), typeof(UITypeEditor))]
        public GradientInfo Gradient { get; set; }

        public DrawBackground()
        {
            this.ApplyDefaultPropertyValues();
            AddDefaultGradient();
        }

        private void AddDefaultGradient()
        {
            Gradient = new GradientInfo();
            Gradient.Colors.Add(new GradientStop(Color.FromArgb(68, 120, 194), 0f));
            Gradient.Colors.Add(new GradientStop(Color.FromArgb(13, 58, 122), 50f));
            Gradient.Colors.Add(new GradientStop(Color.FromArgb(6, 36, 78), 50f));
            Gradient.Colors.Add(new GradientStop(Color.FromArgb(23, 89, 174), 100f));
        }

        public override Bitmap Apply(Bitmap bmp)
        {
            using (bmp)
            {
                if (UseGradient && Gradient != null && Gradient.IsValid)
                {
                    return ImageHelpers.FillBackground(bmp, Gradient);
                }

                return ImageHelpers.FillBackground(bmp, Color);
            }
        }

        protected override string GetSummary()
        {
            if (!UseGradient)
            {
                return $"{Color.R}, {Color.G}, {Color.B}";
            }

            return null;
        }
    }
}