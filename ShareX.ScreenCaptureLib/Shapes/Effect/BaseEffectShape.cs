
// SPDX-License-Identifier: GPL-3.0-or-later


using ShareX.HelpersLib;
using ShareX.ScreenCaptureLib.Properties;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading.Tasks;

namespace ShareX.ScreenCaptureLib
{
    public abstract class BaseEffectShape : BaseShape
    {
        public override ShapeCategory ShapeCategory { get; } = ShapeCategory.Effect;

        public abstract string OverlayText { get; }

        private bool drawCache, isEffectCaching, isCachePending, isDisposePending;
        private Bitmap cachedEffect;

        public abstract void ApplyEffect(Bitmap bmp);

        public override BaseShape Duplicate()
        {
            Bitmap cachedEffectTemp = cachedEffect;
            cachedEffect = null;
            BaseEffectShape shape = (BaseEffectShape)base.Duplicate();
            cachedEffect = cachedEffectTemp;
            return shape;
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            if (isCachePending)
            {
                CacheEffect();
            }
        }

        public virtual void OnDraw(Graphics g)
        {
            if (drawCache && isEffectCaching)
            {
                OnDrawOverlay(g, Resources.Processing);
            }
            else if (drawCache && cachedEffect != null)
            {
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.DrawImage(cachedEffect, RectangleInsideCanvas);
                g.InterpolationMode = InterpolationMode.Bilinear;
            }
            else
            {
                OnDrawOverlay(g);
            }
        }

        public virtual void OnDrawOverlay(Graphics g)
        {
            OnDrawOverlay(g, OverlayText);
        }

        public void OnDrawOverlay(Graphics g, string overlayText)
        {
            using (Brush brush = new SolidBrush(Color.FromArgb(150, Color.Black)))
            {
                g.FillRectangle(brush, Rectangle);
            }

            g.DrawCornerLines(Rectangle.Offset(1), Pens.White, 25);

            using (Font font = new Font("Verdana", 12))
            {
                Size textSize = g.MeasureString(overlayText, font).ToSize();

                if (Rectangle.Width > textSize.Width && Rectangle.Height > textSize.Height)
                {
                    using (StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                    {
                        g.DrawString(overlayText, font, Brushes.White, Rectangle, sf);
                    }
                }
            }
        }

        public virtual void OnDrawFinal(Graphics g, Bitmap bmp)
        {
            Rectangle cropRect = System.Drawing.Rectangle.Intersect(new Rectangle(0, 0, bmp.Width, bmp.Height), Rectangle.Round());

            if (!cropRect.IsEmpty)
            {
                using (Bitmap croppedImage = ImageHelpers.CropBitmap(bmp, cropRect))
                {
                    ApplyEffect(croppedImage);

                    g.DrawImage(croppedImage, cropRect);
                }
            }
        }

        public override void OnCreated()
        {
            base.OnCreated();
            CacheEffect();
        }

        public override void OnMoving()
        {
            StopDrawCache();
        }

        public override void OnMoved()
        {
            CacheEffect();
        }

        public override void OnResizing()
        {
            StopDrawCache();
        }

        public override void OnResized()
        {
            CacheEffect();
        }

        private void CacheEffect()
        {
            if (!isEffectCaching)
            {
                isCachePending = false;
                drawCache = true;

                ClearCache();

                if (IsInsideCanvas)
                {
                    isEffectCaching = true;

                    cachedEffect = Manager.CropImage(RectangleInsideCanvas);

                    Task.Run(() =>
                    {
                        ApplyEffect(cachedEffect);

                        isEffectCaching = false;

                        if (isDisposePending)
                        {
                            Dispose();
                        }
                    });
                }
            }
            else
            {
                isCachePending = true;
            }
        }

        private void StopDrawCache()
        {
            drawCache = false;
            isCachePending = false;
        }

        private void ClearCache()
        {
            if (!isEffectCaching && cachedEffect != null)
            {
                cachedEffect.Dispose();
                cachedEffect = null;
            }
        }

        public override void Dispose()
        {
            if (isEffectCaching)
            {
                isDisposePending = true;
            }
            else
            {
                ClearCache();
            }
        }
    }
}