
// SPDX-License-Identifier: GPL-3.0-or-later


using ShareX.HelpersLib;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace ShareX.ImageEffectsLib
{
    internal class Crop : ImageEffect
    {
        private Padding margin;

        [DefaultValue(typeof(Padding), "0, 0, 0, 0")]
        public Padding Margin
        {
            get
            {
                return margin;
            }
            set
            {
                if (value.Top >= 0 && value.Right >= 0 && value.Bottom >= 0 && value.Left >= 0)
                {
                    margin = value;
                }
            }
        }

        public Crop()
        {
            this.ApplyDefaultPropertyValues();
        }

        public override Bitmap Apply(Bitmap bmp)
        {
            if (Margin.All == 0) return bmp;

            return ImageHelpers.CropBitmap(bmp, new Rectangle(Margin.Left, Margin.Top, bmp.Width - Margin.Horizontal, bmp.Height - Margin.Vertical));
        }

        protected override string GetSummary()
        {
            if (Margin.All == -1)
            {
                return $"{Margin.Left}, {Margin.Top}, {Margin.Right}, {Margin.Bottom}";
            }

            return Margin.All.ToString();
        }
    }
}