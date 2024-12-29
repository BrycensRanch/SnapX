
// SPDX-License-Identifier: GPL-3.0-or-later


using ShareX.HelpersLib;
using System.Drawing;

namespace SnapX.ImageEffectsLib
{
    internal class Polaroid : ImageEffect
    {
        public override Bitmap Apply(Bitmap bmp)
        {
            using (bmp)
            {
                return ColorMatrixManager.Polaroid().Apply(bmp);
            }
        }
    }
}