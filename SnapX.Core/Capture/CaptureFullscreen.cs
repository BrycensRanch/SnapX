
// SPDX-License-Identifier: GPL-3.0-or-later


using SixLabors.ImageSharp;
using SnapX.Core.Job;
using SnapX.Core.Utils;

namespace SnapX.Core.Capture;

public class CaptureFullscreen : CaptureBase
{
    protected override TaskMetadata Execute(TaskSettings taskSettings)
    {
        Rectangle rect = CaptureHelpers.GetScreenWorkingArea();
        TaskMetadata metadata = CreateMetadata(rect);
        metadata.Image = TaskHelpers.GetScreenshot(taskSettings).CaptureFullscreen();
        return metadata;
    }
}

