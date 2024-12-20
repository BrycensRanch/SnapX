
// SPDX-License-Identifier: GPL-3.0-or-later


namespace SnapX.Core.Indexer;

public class FolderInfo
{
    public string FolderPath { get; set; }
    public List<FileInfo> Files { get; set; }
    public List<FolderInfo> Folders { get; set; }
    public long Size { get; private set; }
    public int TotalFileCount { get; private set; }
    public int TotalFolderCount { get; private set; }
    public FolderInfo Parent { get; set; }

    public string FolderName
    {
        get
        {
            return Path.GetFileName(FolderPath);
        }
    }

    public bool IsEmpty
    {
        get
        {
            return TotalFileCount == 0 && TotalFolderCount == 0;
        }
    }

    public FolderInfo(string folderPath)
    {
        FolderPath = folderPath;
        Files = new List<FileInfo>();
        Folders = new List<FolderInfo>();
    }

    public void Update()
    {
        Folders.ForEach(x => x.Update());
        Folders.Sort((x, y) => x.FolderName.CompareTo(y.FolderName));
        Size = Folders.Sum(x => x.Size) + Files.Sum(x => x.Length);
        TotalFileCount = Files.Count + Folders.Sum(x => x.TotalFileCount);
        TotalFolderCount = Folders.Count + Folders.Sum(x => x.TotalFolderCount);
    }
}

