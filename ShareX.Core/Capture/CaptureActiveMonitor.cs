
// SPDX-License-Identifier: GPL-3.0-or-later



using ShareX.Core.Task;
using ShareX.Core.Utils;

namespace ShareX.Core.Capture;

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

