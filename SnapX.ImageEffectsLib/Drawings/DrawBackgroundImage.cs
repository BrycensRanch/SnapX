
// SPDX-License-Identifier: GPL-3.0-or-later


using ShareX.HelpersLib;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;

namespace ShareX.ImageEffectsLib
{
    [Description("Background image")]
    public class DrawBackgroundImage : ImageEffect
    {
        [DefaultValue(""), Editor(typeof(ImageFileNameEditor), typeof(UITypeEditor))]
        public string ImageFilePath { get; set; }

        [DefaultValue(true)]
        public bool Center { get; set; }

        [DefaultValue(false)]
        public bool Tile { get; set; }

        public DrawBackgroundImage()
        {
            this.ApplyDefaultPropertyValues();
        }

        public override Bitmap Apply(Bitmap bmp)
        {
            return ImageHelpers.DrawBackgroundImage(bmp, ImageFilePath, Center, Tile);
        }

        protected override string GetSummary()
        {
            if (!string.IsNullOrEmpty(ImageFilePath))
            {
                return FileHelpers.GetFileNameSafe(ImageFilePath);
            }

            return null;
        }
    }
}