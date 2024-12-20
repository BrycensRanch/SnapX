
// SPDX-License-Identifier: GPL-3.0-or-later


using ShareX.HelpersLib;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace ShareX.ScreenCaptureLib
{
    public class HardDiskCache : ImageCache
    {
        public int Count
        {
            get
            {
                if (indexList != null)
                {
                    return indexList.Count;
                }

                return 0;
            }
        }

        private FileStream fsCache;
        private List<LocationInfo> indexList;

        public HardDiskCache(ScreenRecordingOptions options)
        {
            Options = options;
            FileHelpers.CreateDirectoryFromFilePath(Options.OutputPath);
            fsCache = new FileStream(Options.OutputPath, FileMode.Create, FileAccess.Write, FileShare.Read);
            indexList = new List<LocationInfo>();
        }

        protected override void WriteFrame(Image img)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                img.Save(ms, ImageFormat.Bmp);
                long position = fsCache.Position;
                ms.CopyStreamTo(fsCache);
                indexList.Add(new LocationInfo(position, fsCache.Length - position));
            }
        }

        public override void Dispose()
        {
            if (fsCache != null)
            {
                fsCache.Dispose();
            }

            base.Dispose();
        }

        public IEnumerable<Image> GetImageEnumerator()
        {
            if (!IsWorking && File.Exists(Options.OutputPath) && indexList != null && indexList.Count > 0)
            {
                using (FileStream fsCache = new FileStream(Options.OutputPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    foreach (LocationInfo index in indexList)
                    {
                        using (MemoryStream ms = new MemoryStream())
                        {
                            fsCache.CopyStreamTo64(ms, index.Location, (int)index.Length);
                            yield return Image.FromStream(ms);
                        }
                    }
                }
            }
        }
    }
}