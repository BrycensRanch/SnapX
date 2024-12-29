
// SPDX-License-Identifier: GPL-3.0-or-later


using ShareX.HelpersLib;
using System;

namespace SnapX.ScreenCaptureLib
{
    internal class OpacityAnimation : BaseAnimation
    {
        private double opacity;

        public double Opacity
        {
            get
            {
                return opacity;
            }
            private set
            {
                opacity = value.Clamp(0, 1);
            }
        }

        public TimeSpan FadeInDuration { get; set; }
        public TimeSpan Duration { get; set; }
        public TimeSpan FadeOutDuration { get; set; }

        public TimeSpan TotalDuration => FadeInDuration + Duration + FadeOutDuration;

        public override bool Update()
        {
            if (IsActive)
            {
                if (Timer.Elapsed < FadeInDuration)
                {
                    Opacity = Timer.Elapsed.TotalMilliseconds / FadeInDuration.TotalMilliseconds;
                }
                else
                {
                    Opacity = 1 - ((Timer.Elapsed - (FadeInDuration + Duration)).TotalMilliseconds / FadeOutDuration.TotalMilliseconds);
                }

                if (Opacity == 0)
                {
                    Timer.Stop();
                }
            }

            return IsActive;
        }
    }
}