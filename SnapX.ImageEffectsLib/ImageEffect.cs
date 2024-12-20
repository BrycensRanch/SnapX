
// SPDX-License-Identifier: GPL-3.0-or-later


using Newtonsoft.Json;
using ShareX.HelpersLib;
using System.ComponentModel;
using System.Drawing;

namespace ShareX.ImageEffectsLib
{
    public abstract class ImageEffect
    {
        [DefaultValue(true), Browsable(false)]
        public bool Enabled { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        [DefaultValue(""), Browsable(false)]
        public string Name { get; set; }

        protected ImageEffect()
        {
            Enabled = true;
        }

        public abstract Bitmap Apply(Bitmap bmp);

        protected virtual string GetSummary()
        {
            return null;
        }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(Name))
            {
                return Name;
            }

            string name = GetType().GetDescription();
            string summary = GetSummary();

            if (!string.IsNullOrEmpty(summary))
            {
                name = $"{name}: {summary}";
            }

            return name;
        }
    }
}