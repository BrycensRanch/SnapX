
// SPDX-License-Identifier: GPL-3.0-or-later


using SnapX.Core.Job;

namespace SnapX.Core.Capture;

public class CaptureActiveMonitor : CaptureBase
{
    protected override TaskMetadata Execute(TaskSettings taskSettings)
    {
        DebugHelper.WriteLine("CaptureActiveMonitor started");
        var img = TaskHelpers.GetScreenshot(taskSettings).CaptureActiveMonitor();
        var metadata = CreateMetadata(img.Bounds);
        metadata.Image = img;
        return metadata;
    }
}

