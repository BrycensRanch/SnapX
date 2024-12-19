
// SPDX-License-Identifier: GPL-3.0-or-later


using ShareX.HelpersLib;
using System.Drawing;

namespace ShareX.ImageEffectsLib
{
    internal class Sharpen : ImageEffect
    {
        public override Bitmap Apply(Bitmap bmp)
        {
            //return ImageHelpers.Sharpen(bmp, Strength);

            using (bmp)
            {
                return ConvolutionMatrixManager.Sharpen().Apply(bmp);
            }
        }
    }
}