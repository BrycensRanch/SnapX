
// SPDX-License-Identifier: GPL-3.0-or-later



namespace SnapX.Core.Task;
public class RecentTaskManager
{
    private int maxCount = 10;

    public int MaxCount
    {
        get => maxCount;

        set
        {
            maxCount = Math.Clamp(value, 1, 100);

            lock (itemsLock)
            {
                while (Tasks.Count > maxCount)
                {
                    Tasks.Dequeue();
                }
            }
        }
    }

    public Queue<RecentTask> Tasks { get; private set; }

    private static readonly object itemsLock = new object();

    public RecentTaskManager()
    {
        Tasks = new Queue<RecentTask>();
    }

    public void InitItems()
    {
        lock (itemsLock)
        {
            MaxCount = SnapX.Settings.RecentTasksMaxCount;

            if (SnapX.Settings.RecentTasks != null)
            {
                Tasks = new Queue<RecentTask>(SnapX.Settings.RecentTasks.Take(MaxCount));
            }
        }
    }

    public void Add(WorkerTask task)
    {
        string info = task.Info.ToString();

        if (!string.IsNullOrEmpty(info))
        {
            RecentTask recentItem = new RecentTask()
            {
                FilePath = task.Info.FilePath,
                URL = task.Info.Result.URL,
                ThumbnailURL = task.Info.Result.ThumbnailURL,
                DeletionURL = task.Info.Result.DeletionURL,
                ShortenedURL = task.Info.Result.ShortenedURL
            };

            Add(recentItem);
        }

        if (SnapX.Settings.RecentTasksSave)
        {
            SnapX.Settings.RecentTasks = Tasks.ToList();
        }
        else
        {
            SnapX.Settings.RecentTasks = null;
        }
    }

    public void Add(RecentTask task)
    {
        lock (itemsLock)
        {
            while (Tasks.Count >= MaxCount)
            {
                Tasks.Dequeue();
            }

            Tasks.Enqueue(task);
        }
    }

    public void Clear()
    {
        lock (itemsLock)
        {
            Tasks.Clear();

            SnapX.Settings.RecentTasks = null;

        }
    }
}

