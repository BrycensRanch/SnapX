
// SPDX-License-Identifier: GPL-3.0-or-later


using ShareX.Core.Utils.Extensions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace ShareX.Core.Task;

public class TaskMetadata : IDisposable
{
    private const int WindowInfoMaxLength = 255;

    public Image<Rgba64> Image { get; set; }

    private string windowTitle;

    public string WindowTitle
    {
        get
        {
            return windowTitle;
        }
        set
        {
            windowTitle = value.Truncate(WindowInfoMaxLength);
        }
    }

    private string processName;

    public string ProcessName
    {
        get
        {
            return processName;
        }
        set
        {
            processName = value.Truncate(WindowInfoMaxLength);
        }
    }

    public TaskMetadata()
    {
    }

    public TaskMetadata(Image<Rgba64> image)
    {
        Image = image;
    }
    // This code should be removed after ShareX.Core compiles
    public void UpdateInfo<T>(T windowInfo)
    {
        // TODO: Migrate API consumers to ShareX.CommonUI
        // if (windowInfo != null)
        // {
        //     WindowTitle = windowInfo.Text;
        //     ProcessName = windowInfo.ProcessName;
        // }
    }

    public void Dispose()
    {
        Image?.Dispose();
    }
}

