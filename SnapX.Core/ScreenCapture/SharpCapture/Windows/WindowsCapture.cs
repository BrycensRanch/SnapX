using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace SnapX.Core.ScreenCapture.SharpCapture.Windows;

[SupportedOSPlatform("windows")]
public class WindowsCapture : BaseCapture
{

    private bool IsSupportedFeatureLevel(IDXGIAdapter1 adapter, FeatureLevel featureLevel, DeviceCreationFlags creationFlags)
    {
        ID3D11Device device;
        FeatureLevel supportedFeatureLevel;

        // Call D3D11CreateDevice to check if the feature level is supported by the adapter
        var result = D3D11.D3D11CreateDevice(
            adapter,
            DriverType.Hardware,
            creationFlags,
            new[] { featureLevel },
            out device,
            out supportedFeatureLevel,
            out _);

        // Return true if the device was created successfully and the feature level matches
        if (result.Success && supportedFeatureLevel == featureLevel)
        {
            device?.Dispose(); // Clean up the created device
            return true; // The feature level is supported
        }

        device?.Dispose(); // Clean up the created device
        return false; // The feature level is not supported
    }
    public override async Task<Image?> CaptureFullscreen()
    {
        var factory = DXGI.CreateDXGIFactory1<IDXGIFactory1>()!;

        var adapters = EnumerateAdapters(factory);

        if (adapters.Count == 0)
        {
            return null; // No suitable adapters found
        }

        var outputs = EnumerateOutputs(adapters);

        if (outputs.Count == 0)
        {
            return null; // No outputs found
        }

        int totalWidth = 0;
        int totalHeight = 0;

        // Calculate the total width and height required for the combined image
        foreach (var (output, x, y, width, height, _) in outputs)
        {
            totalWidth = Math.Max(totalWidth, x + width);
            totalHeight = Math.Max(totalHeight, y + height);
        }

        var combinedImage = new Image<Rgba32>(totalWidth, totalHeight);

        foreach (var (output, x, y, width, height, adapter) in outputs)
        {
            var bounds = new Rectangle(x, y, width, height);

            var monitorImage = await CaptureOutputImage(output, adapter, bounds);

            if (monitorImage != null)
            {
                combinedImage.Mutate(ctx => ctx.DrawImage(monitorImage, new Point(x, y), 1f));
            }

            output.Dispose();
        }

        foreach (var adapter in adapters)
        {
            adapter.Dispose();
        }

        return combinedImage;
    }


    public override async Task<Image?> CaptureScreen(Point? pos)
    {
        var factory = DXGI.CreateDXGIFactory1<IDXGIFactory1>()!;

        var adapters = EnumerateAdapters(factory);

        if (adapters.Count == 0)
        {
            return null;
        }

        var outputs = EnumerateOutputs(adapters);

        if (outputs.Count == 0)
        {
            return null;
        }

        if (pos.HasValue)
        {
            var targetOutput = outputs.FirstOrDefault(output =>
                pos.Value.X >= output.X && pos.Value.X < output.X + output.Width &&
                pos.Value.Y >= output.Y && pos.Value.Y < output.Y + output.Height);

            if (targetOutput.Equals(default))
            {
                return null;
            }

            var output = targetOutput.Output;
            var adapter = targetOutput.Adapter;
            var bounds = new Rectangle(targetOutput.X, targetOutput.Y, targetOutput.Width, targetOutput.Height);

            return await CaptureOutputImage(output, adapter, bounds);
        }

        var defaultOutput = outputs.FirstOrDefault();
        if (defaultOutput.Equals(default))
        {
            return null;
        }

        var defaultBounds = new Rectangle(defaultOutput.X, defaultOutput.Y, defaultOutput.Width, defaultOutput.Height);
        return await CaptureOutputImage(defaultOutput.Output, defaultOutput.Adapter, defaultBounds);
    }

    private List<IDXGIAdapter1> EnumerateAdapters(IDXGIFactory1 factory)
    {
        var adapters = new List<IDXGIAdapter1>();

        for (uint adapterIndex = 0; factory.EnumAdapters1(adapterIndex, out var adapter).Success; adapterIndex++)
        {
            var desc = adapter.Description1;

            if ((desc.Flags & AdapterFlags.Software) != AdapterFlags.None)
            {
                adapter.Dispose();
                continue;
            }

            if (IsSupportedFeatureLevel(adapter, FeatureLevel.Level_11_1, DeviceCreationFlags.BgraSupport))
            {
                DebugHelper.WriteLine($"Feature level {FeatureLevel.Level_11_1} not supported. Skipping Adapter {adapter.Description}");
                adapter.Dispose();
                continue;
            }

            adapters.Add(adapter);
        }

        return adapters;
    }

    private List<(IDXGIOutput1 Output, int X, int Y, int Width, int Height, IDXGIAdapter Adapter)> EnumerateOutputs(List<IDXGIAdapter1> adapters)
    {
        var outputs = new List<(IDXGIOutput1 Output, int X, int Y, int Width, int Height, IDXGIAdapter Adapter)>();

        foreach (var adapter in adapters)
        {
            for (uint outputIndex = 0; adapter.EnumOutputs(outputIndex, out var output).Success; outputIndex++)
            {
                var firstOutput = output.QueryInterface<IDXGIOutput1>();
                var bounds = firstOutput.Description.DesktopCoordinates;

                int width = bounds.Right - bounds.Left;
                int height = bounds.Bottom - bounds.Top;
                int x = bounds.Left;
                int y = bounds.Top;

                outputs.Add((firstOutput, x, y, width, height, adapter));
            }
        }

        return outputs;
    }

    private async Task<Image?> CaptureOutputImage(IDXGIOutput1 output, IDXGIAdapter adapter, Rectangle bounds)
    {
        D3D11.D3D11CreateDevice(adapter, DriverType.Unknown, DeviceCreationFlags.None, new[] { FeatureLevel.Level_11_1 }, out var device);

        var textureDesc = new Texture2DDescription
        {
            CPUAccessFlags = CpuAccessFlags.Read,
            BindFlags = BindFlags.None,
            Format = Format.B8G8R8A8_UNorm,
            Width = (uint)bounds.Width,
            Height = (uint)bounds.Height,
            MiscFlags = ResourceOptionFlags.None,
            MipLevels = 1,
            ArraySize = 1,
            SampleDescription = { Count = 1, Quality = 0 },
            Usage = ResourceUsage.Staging
        };

        var duplication = output.DuplicateOutput(device);
        var currentFrame = device.CreateTexture2D(textureDesc);

        Thread.Sleep(100);

        duplication.AcquireNextFrame(500, out var frameInfo, out var desktopResource);
        var tempTexture = desktopResource.QueryInterface<ID3D11Texture2D>();

        device.ImmediateContext.CopyResource(currentFrame, tempTexture);
        var dataBox = device.ImmediateContext.Map(currentFrame, 0);

        var screenshotBytes = GetDataAsByteArray(dataBox.DataPointer, (int)dataBox.RowPitch, (int)bounds.Width, (int)bounds.Height);

        return Image.LoadPixelData<Rgba32>(screenshotBytes, (int)bounds.Width, (int)bounds.Height);
    }
    private byte[] GetDataAsByteArray(IntPtr dataPointer, int rowPitch, int width, int height)
    {
        // Create a byte[] array to hold the pixel data
        var pixelData = new byte[height * rowPitch];

        // Copy the data from unmanaged memory to the byte array
        for (var y = 0; y < height; y++)
        {
            // Pointer arithmetic to calculate the address of each row
            var rowPointer = IntPtr.Add(dataPointer, y * rowPitch);

            // Copy the row from unmanaged memory to the byte array
            Marshal.Copy(rowPointer, pixelData, y * width * 4, width * 4); // Assuming 4 bytes per pixel (RGBA)
        }

        for (var i = 0; i < pixelData.Length; i += 4)
        {
            // Deconstruct the RGBA values and swap the red and blue channels
            (pixelData[i + 2], pixelData[i]) = (pixelData[i], pixelData[i + 2]); // Swap Blue (index 0) and Red (index 2)
        }

        return pixelData;
    }
}
