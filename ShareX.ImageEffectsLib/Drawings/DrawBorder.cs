
// SPDX-License-Identifier: GPL-3.0-or-later


using ShareX.HelpersLib;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;

namespace ShareX.ImageEffectsLib
{
    [Description("Border")]
    public class DrawBorder : ImageEffect
    {
        [DefaultValue(BorderType.Outside)]
        public BorderType Type { get; set; }

        private int size;

        [DefaultValue(1)]
        public int Size
        {
            get
            {
                return size;
            }
            set
            {
                size = value.Max(1);
            }
        }

        [DefaultValue(DashStyle.Solid), TypeConverter(typeof(EnumProperNameConverter))]
        public DashStyle DashStyle { get; set; }

        [DefaultValue(typeof(Color), "Black"), Editor(typeof(MyColorEditor), typeof(UITypeEditor)), TypeConverter(typeof(MyColorConverter))]
        public Color Color { get; set; }

        [DefaultValue(false)]
        public bool UseGradient { get; set; }

        [Editor(typeof(GradientEditor), typeof(UITypeEditor))]
        public GradientInfo Gradient { get; set; }

        public DrawBorder()
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
            if (UseGradient && Gradient != null && Gradient.IsValid)
            {
                return ImageHelpers.DrawBorder(bmp, Gradient, Size, Type, DashStyle);
            }

            return ImageHelpers.DrawBorder(bmp, Color, Size, Type, DashStyle);
        }

        protected override string GetSummary()
        {
            return Size + "px";
        }
    }
}