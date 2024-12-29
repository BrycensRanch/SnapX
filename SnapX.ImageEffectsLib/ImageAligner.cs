using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace SnapX.ImageEffectsLib;

public enum ContentAlignment
{
    TopLeft,
    TopCenter,
    TopRight,
    MiddleLeft,
    Center,
    MiddleRight,
    BottomLeft,
    BottomCenter,
    BottomRight
}

public class ImageAligner
{
    public static Point GetAlignedPosition(ContentAlignment alignment, int containerWidth, int containerHeight, int contentWidth, int contentHeight)
    {
        int x = 0;
        int y = 0;

        // Calculate X position
        switch (alignment)
        {
            case ContentAlignment.TopLeft:
            case ContentAlignment.MiddleLeft:
            case ContentAlignment.BottomLeft:
                x = 0;
                break;
            case ContentAlignment.TopCenter:
            case ContentAlignment.Center:
            case ContentAlignment.BottomCenter:
                x = (containerWidth - contentWidth) / 2;
                break;
            case ContentAlignment.TopRight:
            case ContentAlignment.MiddleRight:
            case ContentAlignment.BottomRight:
                x = containerWidth - contentWidth;
                break;
        }

        // Calculate Y position
        switch (alignment)
        {
            case ContentAlignment.TopLeft:
            case ContentAlignment.TopCenter:
            case ContentAlignment.TopRight:
                y = 0;
                break;
            case ContentAlignment.MiddleLeft:
            case ContentAlignment.Center:
            case ContentAlignment.MiddleRight:
                y = (containerHeight - contentHeight) / 2;
                break;
            case ContentAlignment.BottomLeft:
            case ContentAlignment.BottomCenter:
            case ContentAlignment.BottomRight:
                y = containerHeight - contentHeight;
                break;
        }

        return new Point(x, y);
    }

    public static void AlignImage(Image containerImage, Image contentImage, ContentAlignment alignment)
    {
        var alignedPosition = GetAlignedPosition(alignment, containerImage.Width, containerImage.Height, contentImage.Width, contentImage.Height);
        containerImage.Mutate(ctx => ctx.DrawImage(contentImage, alignedPosition, 1f));
    }
}
