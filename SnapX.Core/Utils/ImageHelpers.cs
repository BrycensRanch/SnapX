using System.Numerics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SnapX.ImageEffectsLib;
using ResizeMode = SixLabors.ImageSharp.Processing.ResizeMode;

namespace SnapX.Core.Utils;

public static class ImageHelpers
{
    // This class consists of dark magic.
    // It is ugly.
    // If anyone has complaints,
    // Talk is cheap, send patches.
    public static Image AddSkew(Image img, int horizontal, int vertical)
    {
        img.Mutate(ctx => ctx.Skew((float)horizontal, (float)vertical));
        return img;
    }

    public static Size ApplyAspectRatio(int width, int height, Image img)
    {
        // Get the original aspect ratio of the image
        float aspectRatio = (float)img.Width / img.Height;

        // Determine the target aspect ratio (width to height ratio)
        float targetAspectRatio = (float)width / height;

        // Variables to hold the new width and height
        int newWidth = width;
        int newHeight = height;

        // Adjust the size based on the aspect ratio comparison
        if (aspectRatio > targetAspectRatio)
        {
            // The image is wider than the target aspect ratio, so adjust based on width
            newHeight = (int)(width / aspectRatio); // Calculate new height to maintain aspect ratio
        }
        else if (aspectRatio < targetAspectRatio)
        {
            // The image is taller than the target aspect ratio, so adjust based on height
            newWidth = (int)(height * aspectRatio); // Calculate new width to maintain aspect ratio
        }

        // Return the new size that maintains the aspect ratio
        return new Size(newWidth, newHeight);
    }
    // See how clean the code is?!
    // See how it's thread safe, and doesn't mess around with pointers!?!?
    // BLESSED.
    public static Image ResizeImage(Image img, Size size)
    {
        img.Mutate(x => x.Resize(size.Width, size.Height));

        return img;
    }
    public static Image ResizeImage(Image img, int targetWidth, int targetHeight, bool preserveAspectRatio, bool fill, Color growFillColor)
    {

        // Mutate the image to apply resizing and fill (if necessary)
        img.Mutate(ctx =>
        {
            if (preserveAspectRatio)
            {
                ctx.Resize(new ResizeOptions
                {
                    Mode = ResizeMode.Max,
                    Size = new Size(targetWidth, targetHeight)
                });
            }
            else
            {
                ctx.Resize(targetWidth, targetHeight);
            }

            if (fill)
            {
                ctx.BackgroundColor(growFillColor);
            }
        });

        return img;
    }

    public static Image RoundedCorners(Image img, float cornerRadius)
    {
        var size = img.Size;

        // Build the path collection for the corners
        var corners = BuildCorners(size.Width, size.Height, cornerRadius);

        // Apply graphics options (like antialiasing and transparency settings)
        img.Mutate(context =>
        {
            // Set the graphics options for smooth rendering
            context.SetGraphicsOptions(new GraphicsOptions
            {
                Antialias = true,
                AlphaCompositionMode = PixelAlphaCompositionMode.DestOut // Ensures the shape is punched out of the background
            });

            // Fill each corner path (with any color like Red, which will be clipped)
            foreach (var path in corners)
            {
                context.Fill(Color.Red, path);
            }
        });

        return img;
    }

    public static IPathCollection BuildCorners(int imageWidth, int imageHeight, float cornerRadius)
    {
        var rect = new RectangularPolygon(-0.5f, -0.5f, cornerRadius, cornerRadius);

        var cornerToptLeft = rect.Clip(new EllipsePolygon(cornerRadius - 0.5f, cornerRadius - 0.5f, cornerRadius));

        var center = new Vector2(imageWidth / 2F, imageHeight / 2F);

        var rightPos = imageWidth - cornerToptLeft.Bounds.Width + 1;
        var bottomPos = imageHeight - cornerToptLeft.Bounds.Height + 1;

        var cornerTopRight = cornerToptLeft.RotateDegree(90).Translate(rightPos, 0);
        var cornerBottomLeft = cornerToptLeft.RotateDegree(-90).Translate(0, bottomPos);
        var cornerBottomRight = cornerToptLeft.RotateDegree(180).Translate(rightPos, bottomPos);

        return new PathCollection(cornerToptLeft, cornerBottomLeft, cornerTopRight, cornerBottomRight);
    }

    public static Image RotateImage(Image img, float angle, bool upSize, bool clip)
    {
        var newSize = img.Size;

        if (upSize)
        {
            // Calculate the new size based on the rotation angle
            newSize = CalculateRotatedSize(img.Width, img.Height, angle);
        }
        img.Mutate(ctx =>
        {
            ctx.Rotate(angle);

            if (upSize)
            {
                ctx.Resize(newSize.Width, newSize.Height);
            }

            if (clip)
            {
                ctx.Crop(new Rectangle(0, 0, newSize.Width, newSize.Height));
            }
        });

        return img;
    }

    // Helper function to calculate the new size after rotation
    private static Size CalculateRotatedSize(int width, int height, float angle)
    {
        var radians = Math.PI * angle / 180.0;  // Convert angle to radians
        var cos = Math.Abs(Math.Cos(radians));
        var sin = Math.Abs(Math.Sin(radians));

        // Calculate the new width and height after rotation
        var newWidth = (int)Math.Ceiling(width * cos + height * sin);
        var newHeight = (int)Math.Ceiling(width * sin + height * cos);

        return new Size(newWidth, newHeight);
    }

    public static Image CropImage(Image img, Rectangle cropRectangle)
    {
        img.Mutate(ctx =>
        {
            ctx.Crop(cropRectangle);  // Crop the image to the specified rectangle
        });

        return img;
    }
    public static Image AddCanvas(Image img, Padding canvasMargin, Rgba32 canvasColor)
    {
        // Get the original image dimensions
        int originalWidth = img.Width;
        int originalHeight = img.Height;

        // Calculate the new dimensions (original size + margins)
        int newWidth = originalWidth + canvasMargin.Left + canvasMargin.Right;
        int newHeight = originalHeight + canvasMargin.Top + canvasMargin.Bottom;

        // Mutate the existing image and resize it to the new size
        img.Mutate(ctx =>
        {
            // First, fill the new area (the canvas) with the background color
            ctx.Fill(canvasColor);

            // Draw the original image into the center of the new canvas area
            var xOffset = canvasMargin.Left;
            var yOffset = canvasMargin.Top;
            ctx.DrawImage(img, new Point(xOffset, yOffset), 1f);  // Draw original image on the new canvas
        });

        return img;
    }
    public static Image AutoCropImage(
        Image img,
        bool shouldCrop,
        AnchorStyles sides,
        int padding)
    {
        if (!shouldCrop)
        {
            // If cropping is not needed, just return the original image
            return img;
        }

        // Get the original image dimensions
        int originalWidth = img.Width;
        int originalHeight = img.Height;

        // Calculate the new width and height by applying the padding
        int newWidth = originalWidth - (padding * 2);  // Padding on left and right
        int newHeight = originalHeight - (padding * 2); // Padding on top and bottom

        // Ensure the new width and height are not smaller than 0
        newWidth = Math.Max(newWidth, 1);
        newHeight = Math.Max(newHeight, 1);

        // Create a rectangle for cropping, starting with the full image
        Rectangle cropArea = new Rectangle(0, 0, newWidth, newHeight);

        // Adjust the cropping area based on the AnchorStyles
        if ((sides & AnchorStyles.Top) == 0) cropArea.Y += padding;
        if ((sides & AnchorStyles.Bottom) == 0) cropArea.Height -= padding;
        if ((sides & AnchorStyles.Left) == 0) cropArea.X += padding;
        if ((sides & AnchorStyles.Right) == 0) cropArea.Width -= padding;

        // Crop the image to the desired area
        img.Mutate(ctx => ctx.Crop(cropArea));

        return img;
    }
    public static Image BoxBlur(Image img, int radius)
    {
        if (radius <= 0) return img;

        img.Mutate(ctx => ctx.BoxBlur(radius));
        return img;
    }
    public static Image ColorDepth(Image img, int bitsPerChannel = 4)
    {
        if (bitsPerChannel < 1 || bitsPerChannel > 8)
        {
            return img; // Return the original image if invalid bitsPerChannel
        }

        var colorsPerChannel = Math.Pow(2, bitsPerChannel);
        var colorInterval = 255.0 / (colorsPerChannel - 1.0);

        img = img.Clone(context =>
        {
            context.ProcessPixelRowsAsVector4(row =>
            {
                for (int x = 0; x < row.Length; x++)
                {
                    row[x].X = Remap(row[x].X, colorInterval);
                    row[x].Y = Remap(row[x].Y, colorInterval);
                    row[x].Z = Remap(row[x].Z, colorInterval);
                }
            });
        });

        return img;
    }

    private static byte Remap(float color, double interval)
    {
        return (byte)Math.Round(Math.Round(color / interval) * interval);
    }

    public static Image GaussianBlur(Image img, int radius)
    {
        img.Mutate(ctx => ctx.GaussianBlur(radius));
        return img;
    }
    public static Image AddGlow(Image img, int size, float strength, Color color, Point offset, GradientBrush? gradient = null)
    {
        var expandedWidth = img.Width + size * 2; // Expand the width to simulate the glow
        var expandedHeight = img.Height + size * 2; // Expand the height to simulate the glow

        // Step 2: Resize the cloned image to create space for the glow
        img.Mutate(ctx => ctx.Resize(expandedWidth, expandedHeight));

        // Step 3: Apply a blur to simulate the glow effect
        img.Mutate(ctx => ctx.GaussianBlur(strength));

        // Step 4: Blend the original image and the blurred image (the glow)
        img.Mutate(ctx => ctx.DrawImage(img, new Point(-size + offset.X, -size + offset.Y), 1));

        // Step 5: Apply the color or gradient to the glow
        if (gradient != null)
        {
            img.Mutate(ctx => ctx.Fill(gradient, new RectangleF(0, 0, img.Width, img.Height)));
        }
        else
        {
            img.Mutate(ctx => ctx.Fill(color, new RectangleF(0, 0, img.Width, img.Height)));
        }

        return img;
    }
    public static Image Outline(Image img, int size, Color color, int padding, bool outlineOnly)
    {
        var width = img.Width + 2 * padding;
        var height = img.Height + 2 * padding;

        var result = new Image<Rgba32>(width, height);

        result.Mutate(ctx =>
        {
            ctx.DrawImage(img, new Point(padding, padding), 1f);
        });

        var outlineRectangle = new Rectangle(0, 0, width, height);

        result.Mutate(ctx =>
        {
            if (outlineOnly)
            {
                ctx.DrawPolygon(color, 1f, new PointF[] {
                    new PointF(0, 0),
                    new PointF(width, 0),
                    new PointF(width, height),
                    new PointF(0, height),
                    new PointF(0, 0)
                });
            }
            else
            {
                ctx.Fill(color, outlineRectangle);
            }
        });

        return result;
    }
    public static Image Pixelate(Image img, int size, int borderSize, Color borderColor)
    {
        // Ensure the size is positive and non-zero
        if (size <= 0) throw new ArgumentException("Size must be greater than zero.", nameof(size));

        // Create a new image with the same dimensions as the original
        var width = img.Width;
        var height = img.Height;

        // First, resize the image to a smaller version (pixelation effect)
        var pixelatedImage = img.Clone(ctx => ctx.Resize(width / size, height / size));

        // Resize it back to the original size to create the pixelated effect
        pixelatedImage.Mutate(ctx => ctx.Resize(width, height));

        // If a border size is specified, add borders around each pixel block
        if (borderSize > 0)
        {
            var borderColorArgb = borderColor.ToPixel<Rgba32>();

            pixelatedImage.Mutate(ctx =>
            {
                for (int y = 0; y < height; y += size)
                {
                    for (int x = 0; x < width; x += size)
                    {
                        var pixelBlock = new Rectangle(x, y, size, size);
                        ctx.Fill(borderColorArgb, pixelBlock);
                    }
                }
            });
        }

        return pixelatedImage;
    }
      public static Image DrawReflection(
        Image img,
        float percentage,
        float maxAlpha,
        float minAlpha,
        float offset,
        bool skew, // Skew is now a boolean
        float skewSize)
    {
        // Validate parameters
        if (percentage < 0 || percentage > 1)
            throw new ArgumentOutOfRangeException(nameof(percentage), "Percentage should be between 0 and 1.");
        if (maxAlpha < 0 || maxAlpha > 255 || minAlpha < 0 || minAlpha > 255)
            throw new ArgumentOutOfRangeException("Alpha values should be between 0 and 255.");

        // Calculate the height of the reflection based on the percentage
        int reflectionHeight = (int)(img.Height * percentage);

        // Clone the original image for the reflection
        var reflection = img.CloneAs<Rgba32>();

        // Create a gradient for the reflection fading effect
        var gradient = CreateAlphaGradient(reflectionHeight, maxAlpha, minAlpha);

        // Apply skew (if specified)
        if (skew)
        {
            var transform = new AffineTransformBuilder()
                .AppendSkewDegrees(0, skewSize); // Apply skew horizontally with the specified skewSize


            reflection.Mutate(ctx => ctx.Transform(transform));
        }

        // Create a new image to hold the original and the reflection
        var finalImage = new Image<Rgba32>(img.Width, img.Height + reflectionHeight + (int)offset);

        // Place the original image at the top of the final image
        finalImage.Mutate(ctx => ctx.DrawImage(img, new Point(0, 0), 1f));

        // Place the reflection below the original image with the applied gradient
        finalImage.Mutate(ctx =>
        {
            for (int y = 0; y < reflectionHeight; y++)
            {
                float alpha = gradient[y];
                var color = new Rgba32(255, 255, 255, (byte)alpha);  // White with varying transparency
                ctx.Fill(color, new Rectangle(0, img.Height + (int)offset + y, img.Width, 1));
            }
        });

        return finalImage;
    }

    private static float[] CreateAlphaGradient(int height, float maxAlpha, float minAlpha)
    {
        float[] gradient = new float[height];
        for (int i = 0; i < height; i++)
        {
            // Create a gradient from maxAlpha to minAlpha
            gradient[i] = maxAlpha - ((maxAlpha - minAlpha) * (i / (float)height));
        }
        return gradient;
    }
    public static Image AddShadow(Image img, float opacity, int size, float darkness, Rgba32 color, Point offset, bool autoResize)
    {
        // Clone the original image to create a new image for shadow
        var width = img.Width;
        var height = img.Height;
        var shadowImage = new Image<Rgba32>(width + size * 2, height + size * 2);

        // Create the shadow color with the specified opacity
        var shadowColor = new Rgba32(color.R, color.G, color.B, (byte)(opacity * 255));

        // Fill the shadow image with the shadow color
        shadowImage.Mutate(ctx => ctx.Fill(shadowColor));

        // Apply the original image to the shadow image at the given offset
        shadowImage.Mutate(ctx => ctx.DrawImage(img, new Point(offset.X + size, offset.Y + size), 1f));

        if (autoResize)
        {
            // Resize the image if autoResize is enabled, to fit the shadow effect
            shadowImage.Mutate(ctx => ctx.Resize(new Size(width + size * 2, height + size * 2)));
        }

        // Apply the darkness effect (saturation control)
        shadowImage.Mutate(ctx => ctx.Saturate(darkness));

        // Return the image with the shadow effect applied
        return shadowImage;
    }
    public static Image Slice(Image img, int minSliceHeight, int maxSliceHeight, int minSliceShift, int maxSliceShift)
    {
        var random = new System.Random();
        int width = img.Width;
        int height = img.Height;

        // Create a new image to store the sliced result
        var resultImage = new Image<Rgba32>(width, height);

        // Initialize the Y position for the next slice
        int currentY = 0;

        while (currentY < height)
        {
            // Randomly determine the height and horizontal shift for this slice
            int sliceHeight = random.Next(minSliceHeight, maxSliceHeight + 1);
            int sliceShift = random.Next(minSliceShift, maxSliceShift + 1);

            // Make sure the slice doesn't extend past the bottom of the image
            sliceHeight = Math.Min(sliceHeight, height - currentY);

            // Create a rectangle that represents the area of the current slice
            var sliceRect = new Rectangle(0, currentY, width, sliceHeight);

            // Crop this slice from the original image
            var slice = img.Clone(x => x.Crop(sliceRect)).CloneAs<Rgba32>();

            // Create a new image for this slice with the shift applied
            var shiftedSlice = new Image<Rgba32>(width, sliceHeight);

            shiftedSlice.Mutate(ctx => ctx.Fill(Color.Transparent)); // Fill with transparent background

            // Copy the slice pixels to the new image with horizontal shift
            for (int y = 0; y < sliceHeight; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int newX = (x + sliceShift) % width;  // Ensure wrapping around at the edges
                    shiftedSlice[newX, y] = slice[x, y];
                }
            }

            // Paste the shifted slice onto the result image
            resultImage.Mutate(ctx => ctx.DrawImage(shiftedSlice, new Point(0, currentY), 1f)); // Use correct overload

            // Update the Y position for the next slice
            currentY += sliceHeight;
        }

        return resultImage;
    }
}
