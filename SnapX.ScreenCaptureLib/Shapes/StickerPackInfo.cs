
// SPDX-License-Identifier: GPL-3.0-or-later


using System.IO;

namespace ShareX.ScreenCaptureLib
{
    public class StickerPackInfo
    {
        public string FolderPath { get; set; }
        public string Name { get; set; }

        public StickerPackInfo(string folderPath = "", string name = "")
        {
            FolderPath = folderPath;
            Name = name;
        }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(Name))
            {
                return Name;
            }

            if (!string.IsNullOrEmpty(FolderPath))
            {
                return Path.GetFileName(FolderPath);
            }

            return "";
        }
    }
}