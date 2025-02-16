
// SPDX-License-Identifier: GPL-3.0-or-later


using SnapX.Core.Job;

namespace SnapX.Core.Capture;

public class CaptureFullscreen : CaptureBase
{
    protected override TaskMetadata Execute(TaskSettings taskSettings)
    {
        DebugHelper.WriteLine("CaptureFullscreen");
        var img = TaskHelpers.GetScreenshot(taskSettings).CaptureFullscreen();
        var metadata = CreateMetadata(img.Bounds);
        metadata.Image = img;

        return metadata;
    }
}

