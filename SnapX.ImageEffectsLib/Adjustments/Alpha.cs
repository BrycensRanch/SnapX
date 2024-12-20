
// SPDX-License-Identifier: GPL-3.0-or-later


using ShareX.HelpersLib;
using System.ComponentModel;
using System.Drawing;

namespace ShareX.ImageEffectsLib
{
    internal class Alpha : ImageEffect
    {
        [DefaultValue(1f), Description("Pixel alpha = Pixel alpha * Value\r\nExample 0.5 will decrease alpha of pixel 50%")]
        public float Value { get; set; }

        [DefaultValue(0f), Description("Pixel alpha = Pixel alpha + Addition\r\nExample 0.5 will increase alpha of pixel 127.5")]
        public float Addition { get; set; }

        public Alpha()
        {
            this.ApplyDefaultPropertyValues();
        }

        public override Bitmap Apply(Bitmap bmp)
        {
            using (bmp)
            {
                return ColorMatrixManager.Alpha(Value, Addition).Apply(bmp);
            }
        }

        protected override string GetSummary()
        {
            return $"{Value}, {Addition}";
        }
    }
}