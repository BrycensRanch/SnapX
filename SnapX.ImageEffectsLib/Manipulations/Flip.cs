
// SPDX-License-Identifier: GPL-3.0-or-later


using ShareX.HelpersLib;
using System.ComponentModel;
using System.Drawing;

namespace ShareX.ImageEffectsLib
{
    internal class Flip : ImageEffect
    {
        [DefaultValue(false)]
        public bool Horizontally { get; set; }

        [DefaultValue(false)]
        public bool Vertically { get; set; }

        public Flip()
        {
            this.ApplyDefaultPropertyValues();
        }

        public override Bitmap Apply(Bitmap bmp)
        {
            RotateFlipType flipType = RotateFlipType.RotateNoneFlipNone;

            if (Horizontally && Vertically)
            {
                flipType = RotateFlipType.RotateNoneFlipXY;
            }
            else if (Horizontally)
            {
                flipType = RotateFlipType.RotateNoneFlipX;
            }
            else if (Vertically)
            {
                flipType = RotateFlipType.RotateNoneFlipY;
            }

            if (flipType != RotateFlipType.RotateNoneFlipNone)
            {
                bmp.RotateFlip(flipType);
            }

            return bmp;
        }

        protected override string GetSummary()
        {
            return $"{Horizontally}, {Vertically}";
        }
    }
}