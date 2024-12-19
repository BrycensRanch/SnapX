
// SPDX-License-Identifier: GPL-3.0-or-later


using ShareX.HelpersLib;
using System.Drawing;

namespace ShareX.ImageEffectsLib
{
    internal class Smooth : ImageEffect
    {
        public override Bitmap Apply(Bitmap bmp)
        {
            using (bmp)
            {
                return ConvolutionMatrixManager.Smooth().Apply(bmp);
            }
        }
    }
}