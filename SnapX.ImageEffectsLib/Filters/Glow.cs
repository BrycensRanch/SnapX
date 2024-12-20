
// SPDX-License-Identifier: GPL-3.0-or-later


using ShareX.HelpersLib;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Linq;

namespace ShareX.ImageEffectsLib
{
    internal class Glow : ImageEffect
    {
        private int size;

        [DefaultValue(20)]
        public int Size
        {
            get
            {
                return size;
            }
            set
            {
                size = value.Max(0);
            }
        }

        private float strength;

        [DefaultValue(1f)]
        public float Strength
        {
            get
            {
                return strength;
            }
            set
            {
                strength = value.Max(0.1f);
            }
        }

        [DefaultValue(typeof(Color), "White"), Editor(typeof(MyColorEditor), typeof(UITypeEditor)), TypeConverter(typeof(MyColorConverter))]
        public Color Color { get; set; }

        [DefaultValue(false)]
        public bool UseGradient { get; set; }

        [Editor(typeof(GradientEditor), typeof(UITypeEditor))]
        public GradientInfo Gradient { get; set; }

        [DefaultValue(typeof(Point), "0, 0")]
        public Point Offset { get; set; }

        public Glow()
        {
            this.ApplyDefaultPropertyValues();
            Gradient = AddDefaultGradient();
        }

        private GradientInfo AddDefaultGradient()
        {
            GradientInfo gradientInfo = new GradientInfo();
            gradientInfo.Type = LinearGradientMode.ForwardDiagonal;

            switch (RandomFast.Next(0, 2))
            {
                case 0:
                    gradientInfo.Colors.Add(new GradientStop(Color.FromArgb(0, 187, 138), 0f));
                    gradientInfo.Colors.Add(new GradientStop(Color.FromArgb(0, 105, 163), 100f));
                    break;
                case 1:
                    gradientInfo.Colors.Add(new GradientStop(Color.FromArgb(255, 3, 135), 0f));
                    gradientInfo.Colors.Add(new GradientStop(Color.FromArgb(255, 143, 3), 100f));
                    break;
                case 2:
                    gradientInfo.Colors.Add(new GradientStop(Color.FromArgb(184, 11, 195), 0f));
                    gradientInfo.Colors.Add(new GradientStop(Color.FromArgb(98, 54, 255), 100f));
                    break;
            }

            return gradientInfo;
        }

        public override Bitmap Apply(Bitmap bmp)
        {
            return ImageHelpers.AddGlow(bmp, Size, Strength, Color, Offset, UseGradient ? Gradient : null);
        }

        protected override string GetSummary()
        {
            return Size.ToString();
        }
    }
}