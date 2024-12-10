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


using ShareX.Core.Utils;

namespace ShareX.Core.History
{
    public abstract class HistoryManager
    {
        public string FilePath { get; private set; }
        public string BackupFolder { get; set; }
        public bool CreateBackup { get; set; }
        public bool CreateWeeklyBackup { get; set; }

        public HistoryManager(string filePath)
        {
            FilePath = filePath;
        }

        public List<HistoryItem> GetHistoryItems()
        {
            try
            {
                return Load();
            }
            catch (Exception e)
            {
                DebugHelper.WriteException(e);
            }

            return new List<HistoryItem>();
        }

        public async Task<List<HistoryItem>> GetHistoryItemsAsync()
        {
            return await Task.Run(() => GetHistoryItems());
        }

        public bool AppendHistoryItem(HistoryItem historyItem)
        {
            return AppendHistoryItems(new HistoryItem[] { historyItem });
        }

        public bool AppendHistoryItems(IEnumerable<HistoryItem> historyItems)
        {
            try
            {
                return Append(historyItems.Where(x => IsValidHistoryItem(x)));
            }
            catch (Exception e)
            {
                DebugHelper.WriteException(e);
            }

            return false;
        }

        private bool IsValidHistoryItem(HistoryItem historyItem)
        {
            return historyItem != null && !string.IsNullOrEmpty(historyItem.FileName) && historyItem.DateTime != DateTime.MinValue &&
                (!string.IsNullOrEmpty(historyItem.URL) || !string.IsNullOrEmpty(historyItem.FilePath));
        }

        protected List<HistoryItem> Load()
        {
            return Load(FilePath);
        }

        protected abstract List<HistoryItem> Load(string filePath);

        protected bool Append(IEnumerable<HistoryItem> historyItems)
        {
            return Append(FilePath, historyItems);
        }

        protected abstract bool Append(string filePath, IEnumerable<HistoryItem> historyItems);

        protected void Backup(string filePath)
        {
            if (!string.IsNullOrEmpty(BackupFolder))
            {
                if (CreateBackup)
                {
                    FileHelpers.CopyFile(filePath, BackupFolder);
                }

                if (CreateWeeklyBackup)
                {
                    FileHelpers.BackupFileWeekly(filePath, BackupFolder);
                }
            }
        }

        public void Test(int itemCount)
        {
            Test(FilePath, itemCount);
        }

        public void Test(string filePath, int itemCount)
        {
            HistoryItem historyItem = new HistoryItem()
            {
                FileName = "Example.png",
                FilePath = "/home/romvnly/Pictures/Mister_Brit.png",
                DateTime = DateTime.Now,
                Type = "Image",
                Host = "Imgur",
                URL = "https://example.com/Example.png",
                ThumbnailURL = "https://example.com/Example.png",
                DeletionURL = "https://example.com/Example.png",
                ShortenedURL = "https://example.com/Example.png"
            };

            HistoryItem[] historyItems = new HistoryItem[itemCount];
            for (int i = 0; i < itemCount; i++)
            {
                historyItems[i] = historyItem;
            }
            // TODO: Investigate this architectural flaw
            // Should be fixed when ORMLite is brought into the equation
            Thread.Sleep(1000);

            Append(filePath, historyItems);

            Load(filePath);
        }
    }
}
