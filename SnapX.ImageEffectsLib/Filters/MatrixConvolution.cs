
// SPDX-License-Identifier: GPL-3.0-or-later


using ShareX.HelpersLib;
using System.ComponentModel;
using System.Drawing;

namespace ShareX.ImageEffectsLib
{
    [Description("Convolution matrix")]
    internal class MatrixConvolution : ImageEffect
    {
        [DefaultValue(0)]
        public int X0Y0 { get; set; }
        [DefaultValue(0)]
        public int X1Y0 { get; set; }
        [DefaultValue(0)]
        public int X2Y0 { get; set; }

        [DefaultValue(0)]
        public int X0Y1 { get; set; }
        [DefaultValue(1)]
        public int X1Y1 { get; set; }
        [DefaultValue(0)]
        public int X2Y1 { get; set; }

        [DefaultValue(0)]
        public int X0Y2 { get; set; }
        [DefaultValue(0)]
        public int X1Y2 { get; set; }
        [DefaultValue(0)]
        public int X2Y2 { get; set; }

        [DefaultValue(1.0)]
        public double Factor { get; set; }

        [DefaultValue((byte)0)]
        public byte Offset { get; set; }

        public MatrixConvolution()
        {
            this.ApplyDefaultPropertyValues();
        }

        public override Bitmap Apply(Bitmap bmp)
        {
            using (bmp)
            {
                ConvolutionMatrix cm = new ConvolutionMatrix();
                cm[0, 0] = X0Y0 / Factor;
                cm[0, 1] = X1Y0 / Factor;
                cm[0, 2] = X2Y0 / Factor;
                cm[1, 0] = X0Y1 / Factor;
                cm[1, 1] = X1Y1 / Factor;
                cm[1, 2] = X2Y1 / Factor;
                cm[2, 0] = X0Y2 / Factor;
                cm[2, 1] = X1Y2 / Factor;
                cm[2, 2] = X2Y2 / Factor;
                cm.Offset = Offset;
                return cm.Apply(bmp);
            }
        }
    }
}