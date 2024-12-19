
// SPDX-License-Identifier: GPL-3.0-or-later


using ShareX.HelpersLib;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;

namespace ShareX.ImageEffectsLib
{
    internal class Canvas : ImageEffect
    {
        [DefaultValue(typeof(Padding), "0, 0, 0, 0")]
        public Padding Margin { get; set; }

        [DefaultValue(CanvasMarginMode.AbsoluteSize), Description("How the margin around the canvas will be calculated."), TypeConverter(typeof(EnumDescriptionConverter))]
        public CanvasMarginMode MarginMode { get; set; }

        [DefaultValue(typeof(Color), "Transparent"), Editor(typeof(MyColorEditor), typeof(UITypeEditor)), TypeConverter(typeof(MyColorConverter))]
        public Color Color { get; set; }

        public Canvas()
        {
            this.ApplyDefaultPropertyValues();
        }

        public enum CanvasMarginMode
        {
            AbsoluteSize,
            PercentageOfCanvas
        }

        public override Bitmap Apply(Bitmap bmp)
        {
            Padding canvasMargin;

            if (MarginMode == CanvasMarginMode.PercentageOfCanvas)
            {
                canvasMargin = new Padding();
                canvasMargin.Left = (int)Math.Round(Margin.Left / 100f * bmp.Width);
                canvasMargin.Right = (int)Math.Round(Margin.Right / 100f * bmp.Width);
                canvasMargin.Top = (int)Math.Round(Margin.Top / 100f * bmp.Height);
                canvasMargin.Bottom = (int)Math.Round(Margin.Bottom / 100f * bmp.Height);
            }
            else
            {
                canvasMargin = Margin;
            }

            Bitmap bmpResult = ImageHelpers.AddCanvas(bmp, canvasMargin, Color);

            if (bmpResult == null)
            {
                return bmp;
            }

            bmp.Dispose();
            return bmpResult;
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