using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace SnapX.Core.ScreenCapture.SharpCapture.Windows;

[SupportedOSPlatform("windows")]
public class WindowsCapture : BaseCapture
{

    private bool IsSupportedFeatureLevel(IDXGIAdapter1 adapter, Vortice.Direct3D.FeatureLevel featureLevel, DeviceCreationFlags creationFlags)
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
    public override async Task<Image> CaptureFullscreen()
    {
        // # of graphics card adapter
        const int numAdapter = 0;

        // # of output device (i.e. monitor)
        const int numOutput = 0;

        var factory = DXGI.CreateDXGIFactory1<IDXGIFactory1>()!;
        IDXGIAdapter1 Adapter;

        for (uint adapterIndex = 0; factory.EnumAdapters1(adapterIndex, out Adapter).Success; adapterIndex++)
        {
            var desc = Adapter.Description1;

            // Don't select the Basic Render Driver adapter.
            if ((desc.Flags & AdapterFlags.Software) != AdapterFlags.None)
            {
                Adapter.Dispose();

                continue;
            }

            if (IsSupportedFeatureLevel(Adapter, FeatureLevel.Level_11_1, DeviceCreationFlags.BgraSupport))
            {
                break;
            }

        }

        IDXGIOutput Output;
        if (Adapter == null)
        {
            DebugHelper.WriteException(new InvalidOperationException("Adapter is not initialized."));
            factory.EnumAdapters1(0, out Adapter);

        }
        var output = Adapter.EnumOutputs(0, out Output);
        if (output == null || Output == null)
        {
            throw new InvalidOperationException("Failed to enumerate outputs or output is null.");
        }
        var firstOutput = Output.QueryInterface<IDXGIOutput1>();
        var width = 1920;
        var height = 1080;

        D3D11.D3D11CreateDevice(Adapter, DriverType.Unknown, DeviceCreationFlags.None, new[] { FeatureLevel.Level_11_1 }, out var device);
        var bounds = firstOutput.Description.DesktopCoordinates;
        var textureDesc = new Texture2DDescription
        {
            CPUAccessFlags = CpuAccessFlags.Read,
            BindFlags = BindFlags.None,
            Format = Format.B8G8R8A8_UNorm,
            Width = (uint)(bounds.Right - bounds.Left),
            Height = (uint)(bounds.Bottom - bounds.Top),
            MiscFlags = ResourceOptionFlags.None,
            MipLevels = 1,
            ArraySize = 1,
            SampleDescription = { Count = 1, Quality = 0 },
            Usage = ResourceUsage.Staging
        };
        var duplication = firstOutput.DuplicateOutput(device);
        var currentFrame = device.CreateTexture2D(textureDesc);
        Thread.Sleep(100);
        duplication.AcquireNextFrame(500, out var frameInfo, out var desktopResource);
        var tempTexture = desktopResource.QueryInterface<ID3D11Texture2D>();
        device.ImmediateContext.CopyResource(currentFrame, tempTexture);
        var dataBox = device.ImmediateContext.Map(currentFrame, 0, MapMode.Read, Vortice.Direct3D11.MapFlags.None);
        var screenShotBytes = GetDataAsByteArray(dataBox.DataPointer, (int)dataBox.RowPitch, width, height);
        return Image.LoadPixelData<Rgba32>(screenShotBytes, width, height);

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
