
// SPDX-License-Identifier: GPL-3.0-or-later


using ShareX.HelpersLib;
using System;
using System.Drawing;

namespace ShareX.ScreenCaptureLib
{
    internal class PointAnimation : BaseAnimation
    {
        public Point FromPosition { get; set; }
        public Point ToPosition { get; set; }
        public TimeSpan Duration { get; set; }

        public Point CurrentPosition { get; private set; }

        public override bool Update()
        {
            if (IsActive)
            {
                base.Update();

                float amount = (float)Timer.Elapsed.Ticks / Duration.Ticks;
                amount = Math.Min(amount, 1);

                CurrentPosition = (Point)MathHelpers.Lerp(FromPosition, ToPosition, amount);

                if (amount >= 1)
                {
                    Stop();
                }
            }

            return IsActive;
        }
    }
}