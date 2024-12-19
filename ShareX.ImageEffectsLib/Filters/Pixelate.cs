
// SPDX-License-Identifier: GPL-3.0-or-later


using ShareX.HelpersLib;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;

namespace ShareX.ImageEffectsLib
{
    internal class Pixelate : ImageEffect
    {
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
                size = value.Max(2);
            }
        }

        private int borderSize;

        [DefaultValue(0)]
        public int BorderSize
        {
            get
            {
                return borderSize;
            }
            set
            {
                borderSize = value.Max(0);
            }
        }

        [DefaultValue(typeof(Color), "Black"), Editor(typeof(MyColorEditor), typeof(UITypeEditor)), TypeConverter(typeof(MyColorConverter))]
        public Color BorderColor { get; set; }

        public Pixelate()
        {
            this.ApplyDefaultPropertyValues();
        }

        public override Bitmap Apply(Bitmap bmp)
        {
            ImageHelpers.Pixelate(bmp, Size, BorderSize, BorderColor);
            return bmp;
        }

        protected override string GetSummary()
        {
            return Size.ToString();
        }
    }
}