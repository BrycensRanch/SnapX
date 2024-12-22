
// SPDX-License-Identifier: GPL-3.0-or-later


using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SnapX.Core.Utils.Extensions;

namespace SnapX.Core.Task;

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
    // This code should be removed after SnapX.Core compiles
    public void UpdateInfo<T>(T windowInfo)
    {
        // TODO: Migrate API consumers to SnapX.CommonUI
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

