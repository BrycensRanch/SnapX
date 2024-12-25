
// SPDX-License-Identifier: GPL-3.0-or-later


using SnapX.Core.History;
using SnapX.Core.Upload;
using SnapX.Core.Upload.Utils;
using SnapX.Core.Utils;
using SnapX.Core.Utils.Extensions;

namespace SnapX.Core.Job;

public static class TaskManager
{
    public static List<WorkerTask> Tasks { get; } = new List<WorkerTask>();
    public static RecentTaskManager RecentManager { get; } = new RecentTaskManager();
    public static bool IsBusy => Tasks.Count > 0 && Tasks.Any(task => task.IsBusy);

    private static int lastIconStatus = -1;

    public static void Start(WorkerTask task)
    {
        if (task != null)
        {
            Tasks.Add(task);

            if (task.Status != TaskStatus.History)
            {
                task.StatusChanged += Task_StatusChanged;
                task.ImageReady += Task_ImageReady;
                task.UploadStarted += Task_UploadStarted;
                task.UploadProgressChanged += Task_UploadProgressChanged;
                task.UploadCompleted += Task_UploadCompleted;
                task.TaskCompleted += Task_TaskCompleted;
            }

            if (task.Status != TaskStatus.History)
            {
                StartTasks();
            }
        }
    }

    public static void Remove(WorkerTask task)
    {
        if (task != null)
        {
            task.Stop();
            Tasks.Remove(task);
            task.Dispose();
        }
    }

    private static void StartTasks()
    {
        int workingTasksCount = Tasks.Count(x => x.IsWorking);
        WorkerTask[] inQueueTasks = Tasks.Where(x => x.Status == TaskStatus.InQueue).ToArray();

        if (inQueueTasks.Length > 0)
        {
            int len;

            if (SnapX.Settings.UploadLimit == 0)
            {
                len = inQueueTasks.Length;
            }
            else
            {
                len = (SnapX.Settings.UploadLimit - workingTasksCount).Clamp(0, inQueueTasks.Length);
            }

            for (int i = 0; i < len; i++)
            {
                inQueueTasks[i].Start();
            }
        }
    }

    public static void StopAllTasks()
    {
        foreach (WorkerTask task in Tasks)
        {
            if (task != null) task.Stop();
        }
    }


    private static void Task_StatusChanged(WorkerTask task)
    {
        DebugHelper.WriteLine("Task status: " + task.Status);
    }

    private static void Task_ImageReady(WorkerTask task, SixLabors.ImageSharp.Image image)
    {
    }

    private static void Task_UploadStarted(WorkerTask task)
    {
        TaskInfo info = task.Info;

        string status = string.Format("Upload started. File name: {0}", info.FileName);
        if (!string.IsNullOrEmpty(info.FilePath)) status += ", File path: " + info.FilePath;
        DebugHelper.WriteLine(status);

    }

    private static void Task_UploadProgressChanged(WorkerTask task)
    {
    }

    private static void Task_UploadCompleted(WorkerTask task)
    {
        TaskInfo info = task.Info;

        if (info != null && info.Result != null && !info.Result.IsError)
        {
            string url = info.Result.ToString();

            if (!string.IsNullOrEmpty(url))
            {
                string text = $"Upload completed. URL: {url}";

                if (info.UploadDuration != null)
                {
                    text += $", Duration: {info.UploadDuration.ElapsedMilliseconds} ms";
                }

                DebugHelper.WriteLine(text);
            }
        }
    }

    private static void Task_TaskCompleted(WorkerTask task)
    {
        try
        {
            if (task != null)
            {
                task.KeepImage = false;

                TaskInfo info = task.Info;

                if (info != null && info.Result != null)
                {

                    if (task.Status == TaskStatus.Stopped)
                    {
                        DebugHelper.WriteLine($"Task stopped. File name: {info.FileName}");
                    }
                    else if (task.Status == TaskStatus.Failed)
                    {
                        string errors = info.Result.Errors.ToString();

                        DebugHelper.WriteLine($"Task failed. File name: {info.FileName}, Errors:\r\n{errors}");

                        TaskHelpers.PlayNotificationSoundAsync(NotificationSound.Error, info.TaskSettings);

                        if (info.Result.Errors.Count > 0)
                        {
                            UploaderErrorInfo error = info.Result.Errors.Errors[0];

                            string title = error.Title;
                            if (string.IsNullOrEmpty(title))
                            {
                                title = "Error";
                            }
                            DebugHelper.WriteLine(title);
                        }
                    }
                    else
                    {
                        DebugHelper.WriteLine($"Task completed. File name: {info.FileName}, Duration: {(long)info.TaskDuration.TotalMilliseconds} ms");

                        string result = info.ToString();

                        if (!task.StopRequested && !string.IsNullOrEmpty(result))
                        {
                            if (SnapX.Settings.HistorySaveTasks && (!SnapX.Settings.HistoryCheckURL ||
                               (!string.IsNullOrEmpty(info.Result.URL) || !string.IsNullOrEmpty(info.Result.ShortenedURL))))
                            {
                                HistoryItem historyItem = info.GetHistoryItem();
                                AppendHistoryItemAsync(historyItem);
                            }

                            RecentManager.Add(task);

                            if (info.Job != TaskJob.ShareURL)
                            {
                                TaskHelpers.PlayNotificationSoundAsync(NotificationSound.TaskCompleted, info.TaskSettings);

                                if (!string.IsNullOrEmpty(info.TaskSettings.AdvancedSettings.BalloonTipContentFormat))
                                {
                                    result = new UploadInfoParser().Parse(info, info.TaskSettings.AdvancedSettings.BalloonTipContentFormat);
                                }

                                if (info.TaskSettings.GeneralSettings.ShowToastNotificationAfterTaskCompleted && !string.IsNullOrEmpty(result) &&
                                    (!info.TaskSettings.GeneralSettings.DisableNotificationsOnFullscreen || !CaptureHelpers.IsActiveWindowFullscreen()))
                                {
                                    task.KeepImage = true;


                                    if (info.TaskSettings.AfterUploadJob.HasFlag(AfterUploadTasks.ShowAfterUploadWindow) && info.IsUploadJob)
                                    {
                                        throw new NotImplementedException("After upload job AfterUploadTasks.ShowAfterUploadWindow");
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        finally
        {

            StartTasks();

            if (SnapX.Settings.SaveSettingsAfterTaskCompleted && !IsBusy)
            {
                SettingManager.SaveAllSettingsAsync();
            }
        }
    }


    private static void AppendHistoryItemAsync(HistoryItem historyItem)
    {
        Task.Run(() =>
        {
            HistoryManager history = new HistoryManagerJSON(SnapX.HistoryFilePath)
            {
                BackupFolder = SettingManager.SnapshotFolder,
                CreateBackup = false,
                CreateWeeklyBackup = true
            };

            history.AppendHistoryItem(historyItem);
        });
    }
}
