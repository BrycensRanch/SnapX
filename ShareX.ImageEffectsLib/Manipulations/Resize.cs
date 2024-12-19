
// SPDX-License-Identifier: GPL-3.0-or-later


using ShareX.HelpersLib;
using System.ComponentModel;
using System.Drawing;

namespace ShareX.ImageEffectsLib
{
    public class Resize : ImageEffect
    {
        [DefaultValue(250), Description("Use width as 0 to automatically adjust width to maintain aspect ratio.")]
        public int Width { get; set; }

        [DefaultValue(0), Description("Use height as 0 to automatically adjust height to maintain aspect ratio.")]
        public int Height { get; set; }

        [DefaultValue(ResizeMode.ResizeAll)]
        public ResizeMode Mode { get; set; }

        public Resize()
        {
            this.ApplyDefaultPropertyValues();
        }

        public Resize(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public override Bitmap Apply(Bitmap bmp)
        {
            if (Width <= 0 && Height <= 0)
            {
                return bmp;
            }

            Size size = ImageHelpers.ApplyAspectRatio(Width, Height, bmp);

            if ((Mode == ResizeMode.ResizeIfBigger && bmp.Width <= size.Width && bmp.Height <= size.Height) ||
                (Mode == ResizeMode.ResizeIfSmaller && bmp.Width >= size.Width && bmp.Height >= size.Height))
            {
                return bmp;
            }

            return ImageHelpers.ResizeImage(bmp, size);
        }

        protected override string GetSummary()
        {
            string summary = Width.ToString();

            if (Width > 0)
            {
                summary += "px";
            }

            summary += ", " + Height.ToString();

            if (Height > 0)
            {
                summary += "px";
            }

            return summary;
        }
    }
}