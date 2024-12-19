
// SPDX-License-Identifier: GPL-3.0-or-later


using ShareX.Core.Task;
using ShareX.Core.Upload;
using ShareX.Core.Utils.Extensions;
using ShareX.Core.Utils.Native;
using SixLabors.ImageSharp;

namespace ShareX.Core.Capture;
public abstract class CaptureBase
{
    public bool AllowAutoHideForm { get; set; } = true;
    public bool AllowAnnotation { get; set; } = true;

    public void Capture(bool autoHideForm)
    {
        Capture(null, autoHideForm);
    }

    public void Capture(TaskSettings taskSettings = null, bool autoHideForm = false)
    {
        if (taskSettings == null) taskSettings = TaskSettings.GetDefaultTaskSettings();

        // TODO: Reimplement taskSettings.GeneralSettings.ToastWindowAutoHide
        // if (taskSettings.GeneralSettings.ToastWindowAutoHide)
        // {
        //     NotificationForm.CloseActiveForm();
        // }

        if (taskSettings.CaptureSettings.ScreenshotDelay > 0)
        {
            int delay = (int)(taskSettings.CaptureSettings.ScreenshotDelay * 1000);

            System.Threading.Tasks.Task.Delay(delay).ContinueInCurrentContext(() =>
            {
                CaptureInternal(taskSettings, autoHideForm);
            });
        }
        else
        {
            CaptureInternal(taskSettings, autoHideForm);
        }
    }

    protected abstract TaskMetadata Execute(TaskSettings taskSettings);

    private void CaptureInternal(TaskSettings taskSettings, bool autoHideForm)
    {
        if (autoHideForm && AllowAutoHideForm)
        {
            // ShareX.MainWindow.Hide();
            Thread.Sleep(250);
        }

        TaskMetadata metadata = null;

        try
        {
            AllowAnnotation = true;
            metadata = Execute(taskSettings);
        }
        catch (Exception ex)
        {
            DebugHelper.WriteException(ex);
        }
        finally
        {
            if (autoHideForm && AllowAutoHideForm)
            {
                // ShareX.MainWindow.ForceActivate();
            }

            AfterCapture(metadata, taskSettings);
        }
    }

    private void AfterCapture(TaskMetadata metadata, TaskSettings taskSettings)
    {
        if (metadata != null && metadata.Image != null)
        {
            TaskHelpers.PlayNotificationSoundAsync(NotificationSound.Capture, taskSettings);

            if (taskSettings.AfterCaptureJob.HasFlag(AfterCaptureTasks.AnnotateImage) && !AllowAnnotation)
            {
                taskSettings.AfterCaptureJob = taskSettings.AfterCaptureJob.Remove(AfterCaptureTasks.AnnotateImage);
            }

            if (taskSettings.ImageSettings.ImageEffectOnlyRegionCapture &&
                GetType() != typeof(CaptureRegion) && GetType() != typeof(CaptureLastRegion))
            {
                taskSettings.AfterCaptureJob = taskSettings.AfterCaptureJob.Remove(AfterCaptureTasks.AddImageEffects);
            }

            UploadManager.RunImageTask(metadata, taskSettings);
        }
    }

    protected TaskMetadata CreateMetadata()
    {
        return CreateMetadata(Rectangle.Empty, null);
    }

    protected TaskMetadata CreateMetadata(Rectangle insideRect)
    {
        return CreateMetadata(insideRect, "explorer");
    }

    protected TaskMetadata CreateMetadata(Rectangle insideRect, string ignoreProcess)
    {
        var metadata = new TaskMetadata();

        var handle = Methods.GetForeground();
        // var windowInfo = new WindowInfo(handle);
        //
        // if ((ignoreProcess == null || !windowInfo.ProcessName.Equals(ignoreProcess, StringComparison.OrdinalIgnoreCase)) &&
        //     (insideRect.IsEmpty || windowInfo.Rectangle.Contains(insideRect)))
        // {
        //     metadata.UpdateInfo(windowInfo);
        // }

        return metadata;
    }
}

