
// SPDX-License-Identifier: GPL-3.0-or-later


using ShareX.HelpersLib;
using System;
using System.Drawing;

namespace ShareX.ScreenCaptureLib
{
    internal class RectangleAnimation : BaseAnimation
    {
        public RectangleF FromRectangle { get; set; }
        public RectangleF ToRectangle { get; set; }
        public TimeSpan Duration { get; set; }

        public RectangleF CurrentRectangle { get; private set; }

        public override bool Update()
        {
            if (IsActive)
            {
                base.Update();

                float amount = (float)Timer.Elapsed.Ticks / Duration.Ticks;
                amount = Math.Min(amount, 1);

                float x = MathHelpers.Lerp(FromRectangle.X, ToRectangle.X, amount);
                float y = MathHelpers.Lerp(FromRectangle.Y, ToRectangle.Y, amount);
                float width = MathHelpers.Lerp(FromRectangle.Width, ToRectangle.Width, amount);
                float height = MathHelpers.Lerp(FromRectangle.Height, ToRectangle.Height, amount);

                CurrentRectangle = new RectangleF(x, y, width, height);

                if (amount >= 1)
                {
                    Stop();
                }
            }

            return IsActive;
        }
    }
}