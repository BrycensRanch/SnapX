
// SPDX-License-Identifier: GPL-3.0-or-later


using ShareX.Core.Task;
using ShareX.Core.Utils;
using SixLabors.ImageSharp;

namespace ShareX.Core.Capture;

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

