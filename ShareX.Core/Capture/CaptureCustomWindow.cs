
// SPDX-License-Identifier: GPL-3.0-or-later



using ShareX.Core.Task;

namespace ShareX.Core.Capture;
public class CaptureCustomWindow : CaptureWindow
{
    protected override TaskMetadata Execute(TaskSettings taskSettings)
    {
        string windowTitle = taskSettings.CaptureSettings.CaptureCustomWindow;

        if (!string.IsNullOrEmpty(windowTitle))
        {
            // TODO: Reimplement w/ Windows support & Linux (X11, and KDE Plasma Wayland)
            // IntPtr hWnd = NativeMethods.SearchWindow(windowTitle);
            //
            // if (hWnd == IntPtr.Zero)
            // {
            //     MessageBox.Show(Resources.UnableToFindAWindowWithSpecifiedWindowTitle, "ShareX", MessageBoxButtons.OK, MessageBoxIcon.Information);
            // }
            // else
            // {
            //     WindowHandle = hWnd;
            //
            //     return base.Execute(taskSettings);
            // }
        }

        return null;
    }
}

