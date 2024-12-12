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

using System.Diagnostics;
using ShareX.Core.Utils;
using ShareX.Core.Utils.Extensions;
using ShareX.Core.Utils.Random;

namespace ShareX.Core.Media
{
    public class VideoThumbnailer
    {
        public delegate void ProgressChangedEventHandler(int current, int length);
        public event ProgressChangedEventHandler ProgressChanged;

        public string FFmpegPath { get; private set; }
        public VideoThumbnailOptions Options { get; private set; }
        public string MediaPath { get; private set; }
        public VideoInfo VideoInfo { get; private set; }

        public VideoThumbnailer(string ffmpegPath, VideoThumbnailOptions options)
        {
            FFmpegPath = ffmpegPath;
            Options = options;
        }

        private void UpdateVideoInfo()
        {
            using (FFmpegCLIManager ffmpeg = new FFmpegCLIManager(FFmpegPath))
            {
                VideoInfo = ffmpeg.GetVideoInfo(MediaPath);
            }
        }

        public List<VideoThumbnailInfo> TakeThumbnails(string mediaPath)
        {
            MediaPath = mediaPath;

            UpdateVideoInfo();

            if (VideoInfo == null || VideoInfo.Duration == TimeSpan.Zero)
            {
                return null;
            }

            List<VideoThumbnailInfo> tempThumbnails = new List<VideoThumbnailInfo>();

            for (int i = 0; i < Options.ThumbnailCount; i++)
            {
                string mediaFileName = Path.GetFileNameWithoutExtension(MediaPath);

                int timeSliceElapsed;

                if (Options.RandomFrame)
                {
                    timeSliceElapsed = GetRandomTimeSlice(i);
                }
                else
                {
                    timeSliceElapsed = GetTimeSlice(Options.ThumbnailCount) * (i + 1);
                }

                string fileName = string.Format("{0}-{1}.{2}", mediaFileName, timeSliceElapsed, Options.ImageFormat.GetDescription());
                string tempThumbnailPath = Path.Combine(GetOutputDirectory(), fileName);

                using (Process process = new Process())
                {
                    ProcessStartInfo psi = new ProcessStartInfo()
                    {
                        FileName = FFmpegPath,
                        Arguments = $"-ss {timeSliceElapsed} -i \"{MediaPath}\" -f image2 -vframes 1 -y \"{tempThumbnailPath}\"",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    process.StartInfo = psi;
                    process.Start();
                    process.WaitForExit(1000 * 30);
                }

                if (System.IO.File.Exists(tempThumbnailPath))
                {
                    VideoThumbnailInfo screenshotInfo = new VideoThumbnailInfo(tempThumbnailPath)
                    {
                        Timestamp = TimeSpan.FromSeconds(timeSliceElapsed)
                    };

                    tempThumbnails.Add(screenshotInfo);
                }

                OnProgressChanged(i + 1, Options.ThumbnailCount);
            }

            return Finish(tempThumbnails);
        }

        private List<VideoThumbnailInfo> Finish(List<VideoThumbnailInfo> tempThumbnails)
        {
            List<VideoThumbnailInfo> thumbnails = new List<VideoThumbnailInfo>();

            if (tempThumbnails != null && tempThumbnails.Count > 0)
            {
                if (Options.CombineScreenshots)
                {
                        throw new NotImplementedException("VideoThumbnailer Combine screenshots is not implemented.");
                }
                else
                {
                    thumbnails.AddRange(tempThumbnails);
                }

                if (Options.OpenDirectory && thumbnails.Count > 0)
                {
                    FileHelpers.OpenFolderWithFile(thumbnails[0].FilePath);
                }
            }

            return thumbnails;
        }

        protected void OnProgressChanged(int current, int length)
        {
            ProgressChanged?.Invoke(current, length);
        }

        private string GetOutputDirectory()
        {
            string directory;

            switch (Options.OutputLocation)
            {
                default:
                case ThumbnailLocationType.DefaultFolder:
                    directory = Options.DefaultOutputDirectory;
                    break;
                case ThumbnailLocationType.ParentFolder:
                    directory = Path.GetDirectoryName(MediaPath);
                    break;
                case ThumbnailLocationType.CustomFolder:
                    directory = FileHelpers.ExpandFolderVariables(Options.CustomOutputDirectory);
                    break;
            }

            FileHelpers.CreateDirectory(directory);

            return directory;
        }

        private int GetTimeSlice(int count)
        {
            return (int)(VideoInfo.Duration.TotalSeconds / count);
        }

        private int GetRandomTimeSlice(int start)
        {
            List<int> mediaSeekTimes = new List<int>();

            for (int i = 1; i < Options.ThumbnailCount + 2; i++)
            {
                mediaSeekTimes.Add(GetTimeSlice(Options.ThumbnailCount + 2) * i);
            }

            return (int)((RandomFast.NextDouble() * (mediaSeekTimes[start + 1] - mediaSeekTimes[start])) + mediaSeekTimes[start]);
        }
    }
}
