
// SPDX-License-Identifier: GPL-3.0-or-later



using ShareX.Core.Task;
using SixLabors.ImageSharp;

namespace ShareX.Core.Capture;
public class CaptureMonitor : CaptureBase
{
    public Rectangle MonitorRectangle { get; private set; }

    public CaptureMonitor(Rectangle monitorRectangle)
    {
        MonitorRectangle = monitorRectangle;
    }

    protected override TaskMetadata Execute(TaskSettings taskSettings)
    {
        var metadata = CreateMetadata(MonitorRectangle);
        metadata.Image = TaskHelpers.GetScreenshot().CaptureRectangle(MonitorRectangle);
        return metadata;
    }
}

