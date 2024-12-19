
// SPDX-License-Identifier: GPL-3.0-or-later


using ShareX.Core.Task;

namespace ShareX.Core.Capture;

public class CaptureCustomRegion : CaptureBase
{
    protected override TaskMetadata Execute(TaskSettings taskSettings)
    {
        var rect = taskSettings.CaptureSettings.CaptureCustomRegion;
        var metadata = CreateMetadata(rect);
        metadata.Image = TaskHelpers.GetScreenshot(taskSettings).CaptureRectangle(rect);
        return metadata;
    }
}

