
// SPDX-License-Identifier: GPL-3.0-or-later


using ShareX.HelpersLib;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;

namespace ShareX.ImageEffectsLib
{
    [Description("Color matrix")]
    internal class MatrixColor : ImageEffect
    {
        [DefaultValue(1f), Description("Red = (Red * Rr) + (Green * Rg) + (Blue * Rb) + (Alpha * Ra) + Ro")]
        public float Rr { get; set; }
        [DefaultValue(0f)]
        public float Rg { get; set; }
        [DefaultValue(0f)]
        public float Rb { get; set; }
        [DefaultValue(0f)]
        public float Ra { get; set; }
        [DefaultValue(0f)]
        public float Ro { get; set; }

        [DefaultValue(0f)]
        public float Gr { get; set; }
        [DefaultValue(1f)]
        public float Gg { get; set; }
        [DefaultValue(0f)]
        public float Gb { get; set; }
        [DefaultValue(0f)]
        public float Ga { get; set; }
        [DefaultValue(0f)]
        public float Go { get; set; }

        [DefaultValue(0f)]
        public float Br { get; set; }
        [DefaultValue(0f)]
        public float Bg { get; set; }
        [DefaultValue(1f)]
        public float Bb { get; set; }
        [DefaultValue(0f)]
        public float Ba { get; set; }
        [DefaultValue(0f)]
        public float Bo { get; set; }

        [DefaultValue(0f)]
        public float Ar { get; set; }
        [DefaultValue(0f)]
        public float Ag { get; set; }
        [DefaultValue(0f)]
        public float Ab { get; set; }
        [DefaultValue(1f)]
        public float Aa { get; set; }
        [DefaultValue(0f)]
        public float Ao { get; set; }

        public MatrixColor()
        {
            this.ApplyDefaultPropertyValues();
        }

        public override Bitmap Apply(Bitmap bmp)
        {
            ColorMatrix colorMatrix = new ColorMatrix(new[]
            {
                new float[] { Rr, Gr, Br, Ar, 0 },
                new float[] { Rg, Gg, Bg, Ag, 0 },
                new float[] { Rb, Gb, Bb, Ab, 0 },
                new float[] { Ra, Ga, Ba, Aa, 0 },
                new float[] { Ro, Go, Bo, Ao, 1 }
            });

            using (bmp)
            {
                return colorMatrix.Apply(bmp);
            }
        }
    }
}