#region License Information (GPL v3)

/*
    ShareX - A program that allows you to take screenshots and share any file type
    Copyright (c) 2007-2024 ShareX Team

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program; if not, write to the Free Software
    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

    Optionally you can also view the license at <http://www.gnu.org/licenses/>.
*/

#endregion License Information (GPL v3)


namespace ShareX.Core.Task
{
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
                MaxCount = ShareX.Settings.RecentTasksMaxCount;

                if (ShareX.Settings.RecentTasks != null)
                {
                    Tasks = new Queue<RecentTask>(ShareX.Settings.RecentTasks.Take(MaxCount));
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

            if (ShareX.Settings.RecentTasksSave)
            {
                ShareX.Settings.RecentTasks = Tasks.ToArray();
            }
            else
            {
                ShareX.Settings.RecentTasks = null;
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

                ShareX.Settings.RecentTasks = null;

            }
        }
    }
}
