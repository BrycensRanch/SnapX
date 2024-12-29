
// SPDX-License-Identifier: GPL-3.0-or-later


using ShareX.HelpersLib;
using System.Drawing;

namespace SnapX.ImageEffectsLib
{
    internal class Inverse : ImageEffect
    {
        public override Bitmap Apply(Bitmap bmp)
        {
            using (bmp)
            {
                return ColorMatrixManager.Inverse().Apply(bmp);
            }
        }
    }
}