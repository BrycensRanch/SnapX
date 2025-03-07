
// SPDX-License-Identifier: GPL-3.0-or-later


using System.Text;
using SnapX.Core.Utils.Miscellaneous;

namespace SnapX.Core.Indexer;
public class IndexerText : Indexer
{
    protected StringBuilder sbContent = new StringBuilder();
    public IndexerText(IndexerSettings indexerSettings) : base(indexerSettings)
    {
    }

    public string Index(string folderPath)
    {
        StringBuilder sbTxtIndex = new StringBuilder();

        FolderInfo folderInfo = new FolderInfo(folderPath);
        folderInfo.Update();

        IndexFolder(folderInfo);
        string index = sbContent.ToString().Trim();

        sbTxtIndex.AppendLine(index);
        return sbTxtIndex.ToString().Trim();
    }

    protected override void IndexFolder(FolderInfo dir, int level = 0)
    {
        sbContent.AppendLine(GetFolderNameRow(dir, level));

        foreach (FolderInfo subdir in dir.Folders)
        {
            IndexFolder(subdir, level + 1);
        }

        if (dir.Files.Count > 0)
        {

            foreach (FileInfo fi in dir.Files)
            {
                sbContent.AppendLine(GetFileNameRow(fi, level + 1));
            }
        }
    }

    private string GetFolderNameRow(FolderInfo dir, int level)
    {
        string folderNameRow = string.Format("{0}{1}", level, dir.FolderName);

        return folderNameRow;
    }

    private string GetFileNameRow(FileInfo fi, int level)
    {
        // TODO: Reimplement whatever the fuck this is.
        return fi.Name + "_" + level;
    }

    private string GetFooter()
    {
        return $"Generated by SnapX Directory Indexer on {DateTime.UtcNow:yyyy-MM-dd 'at' HH:mm:ss 'UTC'}. Latest version can be downloaded from: {Links.Website}";
    }
}

