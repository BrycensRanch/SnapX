
// SPDX-License-Identifier: GPL-3.0-or-later


using SnapX.Core.Job;
using SnapX.Core.Utils;

namespace SnapX.Core.Capture;

public class CaptureActiveMonitor : CaptureBase
{
    protected override TaskMetadata Execute(TaskSettings taskSettings)
    {
        var rect = CaptureHelpers.GetActiveScreenWorkingArea();
        var metadata = CreateMetadata(rect);
        metadata.Image = TaskHelpers.GetScreenshot(taskSettings).CaptureActiveMonitor();
        return metadata;
    }
}

