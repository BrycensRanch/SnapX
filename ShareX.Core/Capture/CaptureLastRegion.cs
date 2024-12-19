
// SPDX-License-Identifier: GPL-3.0-or-later



using ShareX.Core.Task;

namespace ShareX.Core.Capture;
public class CaptureLastRegion : CaptureRegion
{
    protected override TaskMetadata Execute(TaskSettings taskSettings)
    {
        switch (lastRegionCaptureType)
        {
            default:
            case RegionCaptureType.Default: return ExecuteRegionCapture(taskSettings);
            case RegionCaptureType.Light: return ExecuteRegionCaptureLight(taskSettings);
            case RegionCaptureType.Transparent: return ExecuteRegionCaptureTransparent(taskSettings);
        }
    }
}

