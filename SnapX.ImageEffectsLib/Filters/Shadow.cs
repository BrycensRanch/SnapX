
// SPDX-License-Identifier: GPL-3.0-or-later


using ShareX.HelpersLib;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;

namespace ShareX.ImageEffectsLib
{
    internal class Shadow : ImageEffect
    {
        private float opacity;

        [DefaultValue(0.6f), Description("Choose a value between 0.1 and 1.0")]
        public float Opacity
        {
            get
            {
                return opacity;
            }
            set
            {
                opacity = value.Clamp(0.1f, 1.0f);
            }
        }

        private int size;

        [DefaultValue(10)]
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

        [DefaultValue(0f)]
        public float Darkness { get; set; }

        [DefaultValue(typeof(Color), "Black"), Editor(typeof(MyColorEditor), typeof(UITypeEditor)), TypeConverter(typeof(MyColorConverter))]
        public Color Color { get; set; }

        [DefaultValue(typeof(Point), "0, 0")]
        public Point Offset { get; set; }

        [DefaultValue(true)]
        public bool AutoResize { get; set; }

        public Shadow()
        {
            this.ApplyDefaultPropertyValues();
        }

        public override Bitmap Apply(Bitmap bmp)
        {
            return ImageHelpers.AddShadow(bmp, Opacity, Size, Darkness + 1, Color, Offset, AutoResize);
        }

        protected override string GetSummary()
        {
            return Size.ToString();
        }
    }
}