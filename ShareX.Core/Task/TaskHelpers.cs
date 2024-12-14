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
using System.Reflection;
using System.Text;
using ShareX.Core.Capture;
using ShareX.Core.CLI;
using ShareX.Core.Media;
using ShareX.Core.Upload;
using ShareX.Core.Upload.Custom;
using ShareX.Core.Upload.SharingServices;
using ShareX.Core.Utils;
using ShareX.Core.Utils.Extensions;
using ShareX.Core.Utils.Miscellaneous;
using ShareX.Core.Utils.Parsers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using ZXing;
using ZXing.Common;
using ZXing.ImageSharp.Rendering;
using ZXing.QrCode;

namespace ShareX.Core.Task
{
    public static class TaskHelpers
    {
        public static async System.Threading.Tasks.Task ExecuteJob(HotkeyType job, CLICommand command = null)
        {
            await ExecuteJob(ShareX.DefaultTaskSettings, job, command);
        }

        public static async System.Threading.Tasks.Task ExecuteJob(TaskSettings taskSettings)
        {
            await ExecuteJob(taskSettings, taskSettings.Job);
        }

        public static async System.Threading.Tasks.Task ExecuteJob(TaskSettings taskSettings, HotkeyType job, CLICommand command = null)
        {
            if (job == HotkeyType.None) return;

            DebugHelper.WriteLine("Executing: " + job.GetLocalizedDescription());

            var safeTaskSettings = TaskSettings.GetSafeTaskSettings(taskSettings);

            switch (job)
            {
                // Upload
                case HotkeyType.FileUpload:
                    UploadManager.UploadFile(safeTaskSettings);
                    break;
                case HotkeyType.FolderUpload:
                    UploadManager.UploadFolder(safeTaskSettings);
                    break;
                case HotkeyType.ClipboardUpload:
                    UploadManager.ClipboardUpload(safeTaskSettings);
                    break;
                case HotkeyType.ClipboardUploadWithContentViewer:
                    // UploadManager.ClipboardUploadWithContentViewer(safeTaskSettings);
                    break;
                case HotkeyType.UploadText:
                    // UploadManager.UploadText(safeTaskSettings);
                    break;
                case HotkeyType.UploadURL:
                    UploadManager.UploadURL(safeTaskSettings);
                    break;
                case HotkeyType.DragDropUpload:
                    // OpenDropWindow(safeTaskSettings);
                    break;
                case HotkeyType.ShortenURL:
                    // UploadManager.ShowShortenURLDialog(safeTaskSettings);
                    break;
                case HotkeyType.TweetMessage:
                    TweetMessage();
                    break;
                case HotkeyType.StopUploads:
                    TaskManager.StopAllTasks();
                    break;
                // Screen capture
                case HotkeyType.PrintScreen:
                    new CaptureFullscreen().Capture(safeTaskSettings);
                    break;
                case HotkeyType.ActiveWindow:
                    new CaptureActiveWindow().Capture(safeTaskSettings);
                    break;
                case HotkeyType.ActiveMonitor:
                    new CaptureActiveMonitor().Capture(safeTaskSettings);
                    break;
                case HotkeyType.RectangleRegion:
                    new CaptureRegion().Capture(safeTaskSettings);
                    break;
                case HotkeyType.RectangleLight:
                    new CaptureRegion(RegionCaptureType.Light).Capture(safeTaskSettings);
                    break;
                case HotkeyType.RectangleTransparent:
                    new CaptureRegion(RegionCaptureType.Transparent).Capture(safeTaskSettings);
                    break;
                case HotkeyType.CustomRegion:
                    new CaptureCustomRegion().Capture(safeTaskSettings);
                    break;
                case HotkeyType.CustomWindow:
                    new CaptureCustomWindow().Capture(safeTaskSettings);
                    break;
                case HotkeyType.LastRegion:
                    new CaptureLastRegion().Capture(safeTaskSettings);
                    break;
                case HotkeyType.ScrollingCapture:
                    // await OpenScrollingCapture(safeTaskSettings);
                    break;
                case HotkeyType.AutoCapture:
                    // OpenAutoCapture(safeTaskSettings);
                    break;
                case HotkeyType.StartAutoCapture:
                    // StartAutoCapture(safeTaskSettings);
                    break;
                // Screen record
                case HotkeyType.ScreenRecorder:
                    // StartScreenRecording(ScreenRecordOutput.FFmpeg, ScreenRecordStartMethod.Region, safeTaskSettings);
                    break;
                case HotkeyType.ScreenRecorderActiveWindow:
                    // StartScreenRecording(ScreenRecordOutput.FFmpeg, ScreenRecordStartMethod.ActiveWindow, safeTaskSettings);
                    break;
                case HotkeyType.ScreenRecorderCustomRegion:
                    // StartScreenRecording(ScreenRecordOutput.FFmpeg, ScreenRecordStartMethod.CustomRegion, safeTaskSettings);
                    break;
                case HotkeyType.StartScreenRecorder:
                    // StartScreenRecording(ScreenRecordOutput.FFmpeg, ScreenRecordStartMethod.LastRegion, safeTaskSettings);
                    break;
                case HotkeyType.ScreenRecorderGIF:
                    // StartScreenRecording(ScreenRecordOutput.GIF, ScreenRecordStartMethod.Region, safeTaskSettings);
                    break;
                case HotkeyType.ScreenRecorderGIFActiveWindow:
                    // StartScreenRecording(ScreenRecordOutput.GIF, ScreenRecordStartMethod.ActiveWindow, safeTaskSettings);
                    break;
                case HotkeyType.ScreenRecorderGIFCustomRegion:
                    // StartScreenRecording(ScreenRecordOutput.GIF, ScreenRecordStartMethod.CustomRegion, safeTaskSettings);
                    break;
                case HotkeyType.StartScreenRecorderGIF:
                    // StartScreenRecording(ScreenRecordOutput.GIF, ScreenRecordStartMethod.LastRegion, safeTaskSettings);
                    break;
                case HotkeyType.StopScreenRecording:
                    StopScreenRecording();
                    break;
                case HotkeyType.PauseScreenRecording:
                    PauseScreenRecording();
                    break;
                case HotkeyType.AbortScreenRecording:
                    AbortScreenRecording();
                    break;
                // Tools
                case HotkeyType.ColorPicker:
                    // ShowScreenColorPickerDialog(safeTaskSettings);
                    break;
                case HotkeyType.ScreenColorPicker:
                    // OpenScreenColorPicker(safeTaskSettings);
                    break;
                case HotkeyType.Ruler:
                    // OpenRuler(safeTaskSettings);
                    break;
                case HotkeyType.PinToScreen:
                    PinToScreen(safeTaskSettings);
                    break;
                case HotkeyType.PinToScreenFromScreen:
                    // PinToScreenFromScreen(safeTaskSettings);
                    break;
                case HotkeyType.PinToScreenFromClipboard:
                    // PinToScreenFromClipboard(safeTaskSettings);
                    break;
                case HotkeyType.PinToScreenFromFile:
                    // PinToScreenFromFile(safeTaskSettings);
                    break;
                case HotkeyType.PinToScreenCloseAll:
                    // PinToScreenCloseAll(safeTaskSettings);
                    break;
                case HotkeyType.ImageEditor:
                    throw new NotImplementedException("ImageEditor not implemented");
                case HotkeyType.ImageBeautifier:
                    throw new NotImplementedException("ImageBeautifier not implemented");
                case HotkeyType.ImageEffects:
                    throw new NotImplementedException("ImageEffects not implemented");
                case HotkeyType.ImageViewer:
                    throw new NotImplementedException("ImageViewer not implemented");
                case HotkeyType.ImageCombiner:
                    // OpenImageCombiner(null, safeTaskSettings);
                    break;
                case HotkeyType.ImageSplitter:
                    // OpenImageSplitter();
                    break;
                case HotkeyType.ImageThumbnailer:
                    // OpenImageThumbnailer();
                    break;
                case HotkeyType.VideoConverter:
                    // OpenVideoConverter(safeTaskSettings);
                    break;
                case HotkeyType.VideoThumbnailer:
                    // OpenVideoThumbnailer(safeTaskSettings);
                    break;
                case HotkeyType.OCR:
                    await OCRImage(safeTaskSettings);
                    break;
                case HotkeyType.QRCode:
                    // OpenQRCode();
                    break;
                case HotkeyType.QRCodeDecodeFromScreen:
                    // OpenQRCodeDecodeFromScreen();
                    break;
                case HotkeyType.HashCheck:
                    // OpenHashCheck();
                    break;
                case HotkeyType.IndexFolder:
                    // UploadManager.IndexFolder();
                    break;
                case HotkeyType.ClipboardViewer:
                    // OpenClipboardViewer();
                    break;
                case HotkeyType.BorderlessWindow:
                    // OpenBorderlessWindow(safeTaskSettings);
                    break;
                case HotkeyType.ActiveWindowBorderless:
                    // MakeActiveWindowBorderless(safeTaskSettings);
                    break;
                case HotkeyType.ActiveWindowTopMost:
                    // MakeActiveWindowTopMost(safeTaskSettings);
                    break;
                case HotkeyType.InspectWindow:
                    // OpenInspectWindow();
                    break;
                case HotkeyType.MonitorTest:
                    // OpenMonitorTest();
                    break;
                case HotkeyType.DNSChanger:
                    // OpenDNSChanger();
                    break;
                // Other
                case HotkeyType.DisableHotkeys:
                    ToggleHotkeys();
                    break;
                case HotkeyType.OpenMainWindow:
                    // ShareX.MainForm.ForceActivate();
                    break;
                case HotkeyType.OpenScreenshotsFolder:
                    OpenScreenshotsFolder();
                    break;
                case HotkeyType.OpenHistory:
                    // OpenHistory();
                    break;
                case HotkeyType.OpenImageHistory:
                    // OpenImageHistory();
                    break;
                case HotkeyType.ToggleActionsToolbar:
                    // ToggleActionsToolbar();
                    break;
                case HotkeyType.ToggleTrayMenu:
                    // ToggleTrayMenu();
                    break;
                case HotkeyType.ExitShareX:
                    ShareX.quit();
                    break;
            }
        }

        public static ImageData PrepareImage(Image<Rgba64> img, TaskSettings taskSettings)
        {
            var imageData = new ImageData();
            imageData.ImageStream = SaveImageAsStream(img, taskSettings.ImageSettings.ImageFormat, taskSettings);
            imageData.ImageFormat = taskSettings.ImageSettings.ImageFormat;

            if (taskSettings.ImageSettings.ImageAutoUseJPEG && taskSettings.ImageSettings.ImageFormat != EImageFormat.JPEG &&
                imageData.ImageStream.Length > taskSettings.ImageSettings.ImageAutoUseJPEGSize * 1000)
            {
                imageData.ImageStream.Dispose();

                // using (Bitmap newImage = MediaTypeNames.Image.FillBackground(img, Color.White))
                // {
                //     if (taskSettings.ImageSettings.ImageAutoJPEGQuality)
                //     {
                //         imageData.ImageStream = MediaTypeNames.Image.SaveJPEGAutoQuality(newImage, taskSettings.ImageSettings.ImageAutoUseJPEGSize * 1000, 2, 70, 100);
                //     }
                //     else
                //     {
                //         imageData.ImageStream = MediaTypeNames.Image.SaveJPEG(newImage, taskSettings.ImageSettings.ImageJPEGQuality);
                //     }
                // }

                imageData.ImageFormat = EImageFormat.JPEG;
            }

            return imageData;
        }

        // public static string CreateThumbnail(Bitmap bmp, string folder, string fileName, TaskSettings taskSettings)
        // {
        //     if ((taskSettings.ImageSettings.ThumbnailWidth > 0 || taskSettings.ImageSettings.ThumbnailHeight > 0) && (!taskSettings.ImageSettings.ThumbnailCheckSize ||
        //         (bmp.Width > taskSettings.ImageSettings.ThumbnailWidth && bmp.Height > taskSettings.ImageSettings.ThumbnailHeight)))
        //     {
        //         string thumbnailFileName = Path.GetFileNameWithoutExtension(fileName) + taskSettings.ImageSettings.ThumbnailName + ".jpg";
        //         string thumbnailFilePath = HandleExistsFile(folder, thumbnailFileName, taskSettings);
        //
        //         if (!string.IsNullOrEmpty(thumbnailFilePath))
        //         {
        //             using (Bitmap thumbnail = (Bitmap)bmp.Clone())
        //             using (Bitmap resizedImage = new Resize(taskSettings.ImageSettings.ThumbnailWidth, taskSettings.ImageSettings.ThumbnailHeight).Apply(thumbnail))
        //             using (Bitmap newImage = MediaTypeNames.Image.FillBackground(resizedImage, Color.White))
        //             {
        //                 MediaTypeNames.Image.SaveJPEG(newImage, thumbnailFilePath, 90);
        //                 return thumbnailFilePath;
        //             }
        //         }
        //     }
        //
        //     return null;
        // }

        public static MemoryStream SaveImageAsStream(SixLabors.ImageSharp.Image img, EImageFormat imageFormat, TaskSettings taskSettings)
        {
            return SaveImageAsStream(img, imageFormat, taskSettings.ImageSettings.ImagePNGBitDepth,
                taskSettings.ImageSettings.ImageJPEGQuality, taskSettings.ImageSettings.ImageGIFQuality);
        }

        public static MemoryStream SaveImageAsStream(SixLabors.ImageSharp.Image img, EImageFormat imageFormat, PNGBitDepth pngBitDepth = PNGBitDepth.Automatic,
            int jpegQuality = 90, GIFQuality gifQuality = GIFQuality.Default)
        {
            MemoryStream ms = new MemoryStream();

            try
            {
                switch (imageFormat)
                {
                    case EImageFormat.PNG:

                        if (ShareX.Settings.PNGStripColorSpaceInformation)
                        {
                            using (ms)
                            {
                                // return MediaTypeNames.Image.PNGStripColorSpaceInformation(ms);
                            }
                        }
                        break;
                    case EImageFormat.JPEG:
                        // using (Bitmap newImage = MediaTypeNames.Image.FillBackground(img, Color.White))
                        // {
                        //     MediaTypeNames.Image.SaveJPEG(newImage, ms, jpegQuality);
                        // }
                        break;
                    case EImageFormat.GIF:
                        // MediaTypeNames.Image.SaveGIF(img, ms, gifQuality);
                        break;
                    case EImageFormat.BMP:
                        // img.Save(ms, ImageFormat.Bmp);
                        break;
                    case EImageFormat.TIFF:
                        // img.Save(ms, ImageFormat.Tiff);
                        break;
                }
            }
            catch (Exception e)
            {
                DebugHelper.WriteException(e);
                e.ShowError();
            }

            return ms;
        }

        // public static void SaveImageAsFile(Bitmap bmp, TaskSettings taskSettings, bool overwriteFile = false)
        // {
        //     using (ImageData imageData = PrepareImage(bmp, taskSettings))
        //     {
        //         string screenshotsFolder = GetScreenshotsFolder(taskSettings);
        //         string fileName = GetFileName(taskSettings, imageData.ImageFormat.GetDescription(), bmp);
        //         string filePath = Path.Combine(screenshotsFolder, fileName);
        //
        //         if (!overwriteFile)
        //         {
        //             filePath = HandleExistsFile(filePath, taskSettings);
        //         }
        //
        //         if (!string.IsNullOrEmpty(filePath))
        //         {
        //             imageData.Write(filePath);
        //             DebugHelper.WriteLine("Image saved to file: " + filePath);
        //         }
        //     }
        // }
        public static string HandleExistsFile(string filePath, TaskSettings taskSettings)
        {
            if (File.Exists(filePath))
            {
                switch (taskSettings.ImageSettings.FileExistAction)
                {
                    case FileExistAction.Ask:
                        throw new NotImplementedException("FileExistAction.Ask not implemented");
                        break;
                    case FileExistAction.UniqueName:
                        filePath = FileHelpers.GetUniqueFilePath(filePath);
                        break;
                    case FileExistAction.Cancel:
                        filePath = "";
                        break;
                }
            }

            return filePath;
        }
        public static string HandleExistsFile(string folderPath, string fileName, TaskSettings taskSettings)
        {
            var filePath = Path.Combine(folderPath, fileName);
            return HandleExistsFile(filePath, taskSettings);
        }
        public static string GetFileName(TaskSettings taskSettings, string extension, Image<Rgba64> bmp)
        {
            var metadata = new TaskMetadata(bmp);
            return GetFileName(taskSettings, extension, metadata);
        }

        public static string GetFileName(TaskSettings taskSettings, string extension = null, TaskMetadata metadata = null)
        {
            string fileName;

            NameParser nameParser = new NameParser(NameParserType.FileName)
            {
                AutoIncrementNumber = ShareX.Settings.NameParserAutoIncrementNumber,
                MaxNameLength = taskSettings.AdvancedSettings.NamePatternMaxLength,
                MaxTitleLength = taskSettings.AdvancedSettings.NamePatternMaxTitleLength,
                CustomTimeZone = taskSettings.UploadSettings.UseCustomTimeZone ? taskSettings.UploadSettings.CustomTimeZone : null
            };

            if (metadata != null)
            {
                if (metadata.Image != null)
                {
                    nameParser.ImageWidth = metadata.Image.Width;
                    nameParser.ImageHeight = metadata.Image.Height;
                }

                nameParser.WindowText = metadata.WindowTitle;
                nameParser.ProcessName = metadata.ProcessName;
            }

            if (!string.IsNullOrEmpty(taskSettings.UploadSettings.NameFormatPatternActiveWindow) && !string.IsNullOrEmpty(nameParser.WindowText))
            {
                fileName = nameParser.Parse(taskSettings.UploadSettings.NameFormatPatternActiveWindow);
            }
            else
            {
                fileName = nameParser.Parse(taskSettings.UploadSettings.NameFormatPattern);
            }

            ShareX.Settings.NameParserAutoIncrementNumber = nameParser.AutoIncrementNumber;

            if (!string.IsNullOrEmpty(extension))
            {
                fileName += "." + extension.TrimStart('.');
            }

            return fileName;
        }

        public static string GetScreenshotsFolder(TaskSettings taskSettings = null, TaskMetadata metadata = null)
        {
            string screenshotsFolder;

            NameParser nameParser = new NameParser(NameParserType.FilePath);

            if (metadata != null)
            {
                if (metadata.Image != null)
                {
                    nameParser.ImageWidth = metadata.Image.Width;
                    nameParser.ImageHeight = metadata.Image.Height;
                }

                nameParser.WindowText = metadata.WindowTitle;
                nameParser.ProcessName = metadata.ProcessName;
            }

            if (taskSettings != null && taskSettings.OverrideScreenshotsFolder && !string.IsNullOrEmpty(taskSettings.ScreenshotsFolder))
            {
                screenshotsFolder = nameParser.Parse(taskSettings.ScreenshotsFolder);
            }
            else
            {
                string subFolderPattern;

                if (!string.IsNullOrEmpty(ShareX.Settings.SaveImageSubFolderPatternWindow) && !string.IsNullOrEmpty(nameParser.WindowText))
                {
                    subFolderPattern = ShareX.Settings.SaveImageSubFolderPatternWindow;
                }
                else
                {
                    subFolderPattern = ShareX.Settings.SaveImageSubFolderPattern;
                }

                string subFolderPath = nameParser.Parse(subFolderPattern);
                screenshotsFolder = Path.Combine(ShareX.ScreenshotsParentFolder, subFolderPath);
            }

            return FileHelpers.GetAbsolutePath(screenshotsFolder);
        }

        public static void AddDefaultExternalPrograms(TaskSettings taskSettings)
        {
            if (taskSettings.ExternalPrograms == null)
            {
                taskSettings.ExternalPrograms = new List<ExternalProgram>();
            }

            AddExternalProgramFromRegistry(taskSettings, "Paint", "mspaint.exe");
            AddExternalProgramFromRegistry(taskSettings, "Paint.NET", "PaintDotNet.exe");
            AddExternalProgramFromRegistry(taskSettings, "Adobe Photoshop", "Photoshop.exe");
            AddExternalProgramFromRegistry(taskSettings, "IrfanView", "i_view32.exe");
            AddExternalProgramFromRegistry(taskSettings, "XnView", "xnview.exe");
        }

        private static void AddExternalProgramFromRegistry(TaskSettings taskSettings, string name, string fileName)
        {
            // if (!taskSettings.ExternalPrograms.Exists(x => x.Name == name))
            // {
            //     try
            //     {
            //         string filePath = RegistryHelpers.SearchProgramPath(fileName);
            //
            //         if (!string.IsNullOrEmpty(filePath))
            //         {
            //             ExternalProgram externalProgram = new ExternalProgram(name, filePath);
            //             taskSettings.ExternalPrograms.Add(externalProgram);
            //         }
            //     }
            //     catch (Exception e)
            //     {
            //         DebugHelper.WriteException(e);
            //     }
            // }
        }

        // public static void StartScreenRecording(ScreenRecordOutput outputType, ScreenRecordStartMethod startMethod, TaskSettings taskSettings = null)
        // {
        //     if (taskSettings == null) taskSettings = TaskSettings.GetDefaultTaskSettings();
        //
        //     ScreenRecordManager.StartStopRecording(outputType, startMethod, taskSettings);
        // }

        public static void StopScreenRecording()
        {
            // ScreenRecordManager.StopRecording();
        }

        public static void PauseScreenRecording()
        {
            ScreenRecordManager.PauseScreenRecording();
        }

        public static void AbortScreenRecording()
        {
            ScreenRecordManager.AbortRecording();
        }

        public static void OpenScreenshotsFolder()
        {
            string screenshotsFolder = GetScreenshotsFolder();

            if (Directory.Exists(screenshotsFolder))
            {
                FileHelpers.OpenFolder(screenshotsFolder);
            }
            else
            {
                FileHelpers.OpenFolder(ShareX.ScreenshotsParentFolder);
            }
        }


        public static void RunShareXAsAdmin(string arguments = null)
        {
            try
            {
                using (Process process = new Process())
                {
                    ProcessStartInfo psi = new ProcessStartInfo()
                    {
                        FileName = Assembly.GetExecutingAssembly().Location,
                        Arguments = arguments,
                        UseShellExecute = true,
                        Verb = "runas"
                    };

                    process.StartInfo = psi;
                    process.Start();
                }
            }
            catch
            {
            }
        }

        public static void SearchImageUsingGoogleLens(string url)
        {
            new GoogleLensSharingService().CreateSharer(null, null).ShareURL(url);
        }

        public static void SearchImageUsingBing(string url)
        {
            new BingVisualSearchSharingService().CreateSharer(null, null).ShareURL(url);
        }

        public static async System.Threading.Tasks.Task OCRImage(TaskSettings taskSettings = null)
        {
            if (taskSettings == null) taskSettings = TaskSettings.GetDefaultTaskSettings();
            throw new NotImplementedException("OCRImage not implemented");
        }

        public static async System.Threading.Tasks.Task OCRImage(string filePath, TaskSettings taskSettings = null)
        {
            if (!string.IsNullOrEmpty(filePath) && System.IO.File.Exists(filePath))
            {
                throw new NotImplementedException("OCRImage not implemented");
            }
        }

        public static async System.Threading.Tasks.Task OCRImage(SixLabors.ImageSharp.Image image, TaskSettings taskSettings = null)
        {
            await OCRImage(image, null, taskSettings);
        }

        public static async System.Threading.Tasks.Task OCRImage(SixLabors.ImageSharp.Image image, string filePath = null, TaskSettings taskSettings = null)
        {
            if (taskSettings == null) taskSettings = TaskSettings.GetDefaultTaskSettings();

            throw new NotImplementedException("OCRImage not implemented");
        }

        private static async System.Threading.Tasks.Task AsyncOCRImage(SixLabors.ImageSharp.Image bmp, string filePath = null, TaskSettings taskSettings = null)
        {
            throw new NotImplementedException("AsyncOCRImage not implemented");
        }

        public static void PinToScreen(TaskSettings taskSettings = null)
        {
            throw new NotImplementedException("PinToScreen is not implemented");
        }

        public static void TweetMessage()
        {
            throw new NotImplementedException("TweetMessage is not implemented");
        }

        public static EDataType FindDataType(string filePath, TaskSettings taskSettings)
        {
            if (FileHelpers.CheckExtension(filePath, taskSettings.AdvancedSettings.ImageExtensions))
            {
                return EDataType.Image;
            }

            if (FileHelpers.CheckExtension(filePath, taskSettings.AdvancedSettings.TextExtensions))
            {
                return EDataType.Text;
            }

            return EDataType.File;
        }

        public static bool ToggleHotkeys()
        {
            bool disableHotkeys = !ShareX.Settings.DisableHotkeys;
            ToggleHotkeys(disableHotkeys);
            return disableHotkeys;
        }

        public static void ToggleHotkeys(bool disableHotkeys)
        {
            ShareX.Settings.DisableHotkeys = disableHotkeys;
            ShareX.HotkeyManager.ToggleHotkeys(disableHotkeys);
        }

        public static bool CheckFFmpeg(TaskSettings taskSettings)
        {
            return true;
        }

        public static void PlayNotificationSoundAsync(NotificationSound notificationSound, TaskSettings taskSettings = null)
        {
            if (taskSettings == null) taskSettings = TaskSettings.GetDefaultTaskSettings();
            throw new NotImplementedException("PlayNotificationSoundAsync is not implemented");
        }

        public static Screenshot GetScreenshot(TaskSettings taskSettings = null)
        {
            if (taskSettings == null) taskSettings = TaskSettings.GetDefaultTaskSettings();

            var screenshot = new Screenshot()
            {
                CaptureCursor = taskSettings.CaptureSettings.ShowCursor,
                CaptureClientArea = taskSettings.CaptureSettings.CaptureClientArea,
                RemoveOutsideScreenArea = true,
                CaptureShadow = taskSettings.CaptureSettings.CaptureShadow,
                ShadowOffset = taskSettings.CaptureSettings.CaptureShadowOffset,
                AutoHideTaskbar = taskSettings.CaptureSettings.CaptureAutoHideTaskbar
            };

            return screenshot;
        }

        public static void ImportCustomUploader(string filePath)
        {
            if (ShareX.UploadersConfig != null)
            {
                try
                {
                    CustomUploaderItem cui = JsonHelpers.DeserializeFromFile<CustomUploaderItem>(filePath);

                    if (cui != null)
                    {
                        bool activate = false;

                        if (cui.DestinationType == CustomUploaderDestinationType.None)
                        {
                        }
                        else
                        {
                            List<string> destinations = new List<string>();
                            if (cui.DestinationType.HasFlag(CustomUploaderDestinationType.ImageUploader)) destinations.Add("images");
                            if (cui.DestinationType.HasFlag(CustomUploaderDestinationType.TextUploader)) destinations.Add("texts");
                            if (cui.DestinationType.HasFlag(CustomUploaderDestinationType.FileUploader)) destinations.Add("files");
                            if (cui.DestinationType.HasFlag(CustomUploaderDestinationType.URLShortener) ||
                                cui.DestinationType.HasFlag(CustomUploaderDestinationType.URLSharingService)) destinations.Add("urls");

                            string destinationsText = string.Join("/", destinations);
                            activate = true;
                        }

                        cui.CheckBackwardCompatibility();
                        ShareX.UploadersConfig.CustomUploadersList.Add(cui);

                        if (activate)
                        {
                            int index = ShareX.UploadersConfig.CustomUploadersList.Count - 1;

                            if (cui.DestinationType.HasFlag(CustomUploaderDestinationType.ImageUploader))
                            {
                                ShareX.UploadersConfig.CustomImageUploaderSelected = index;
                                ShareX.DefaultTaskSettings.ImageDestination = ImageDestination.CustomImageUploader;
                            }

                            if (cui.DestinationType.HasFlag(CustomUploaderDestinationType.TextUploader))
                            {
                                ShareX.UploadersConfig.CustomTextUploaderSelected = index;
                                ShareX.DefaultTaskSettings.TextDestination = TextDestination.CustomTextUploader;
                            }

                            if (cui.DestinationType.HasFlag(CustomUploaderDestinationType.FileUploader))
                            {
                                ShareX.UploadersConfig.CustomFileUploaderSelected = index;
                                ShareX.DefaultTaskSettings.FileDestination = FileDestination.CustomFileUploader;
                            }

                            if (cui.DestinationType.HasFlag(CustomUploaderDestinationType.URLShortener))
                            {
                                ShareX.UploadersConfig.CustomURLShortenerSelected = index;
                                ShareX.DefaultTaskSettings.URLShortenerDestination = UrlShortenerType.CustomURLShortener;
                            }

                            if (cui.DestinationType.HasFlag(CustomUploaderDestinationType.URLSharingService))
                            {
                                ShareX.UploadersConfig.CustomURLSharingServiceSelected = index;
                                ShareX.DefaultTaskSettings.URLSharingServiceDestination = URLSharingServices.CustomURLSharingService;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    DebugHelper.WriteException(e);
                    e.ShowError();
                }
            }
        }

        public static async System.Threading.Tasks.Task HandleNativeMessagingInput(string filePath, TaskSettings taskSettings = null)
        {
            if (!string.IsNullOrEmpty(filePath) && System.IO.File.Exists(filePath))
            {
                NativeMessagingInput nativeMessagingInput = null;

                try
                {
                    nativeMessagingInput = JsonHelpers.DeserializeFromFile<NativeMessagingInput>(filePath);
                }
                catch (Exception e)
                {
                    DebugHelper.WriteException(e);
                }
                finally
                {
                    System.IO.File.Delete(filePath);
                }

                if (nativeMessagingInput != null)
                {
                    if (taskSettings == null) taskSettings = TaskSettings.GetDefaultTaskSettings();

                    PlayNotificationSoundAsync(NotificationSound.ActionCompleted, taskSettings);

                    switch (nativeMessagingInput.Action)
                    {
                        // TEMP: For backward compatibility
                        default:
                            if (!string.IsNullOrEmpty(nativeMessagingInput.URL))
                            {
                                UploadManager.DownloadAndUploadFile(nativeMessagingInput.URL, taskSettings);
                            }
                            else if (!string.IsNullOrEmpty(nativeMessagingInput.Text))
                            {
                                UploadManager.UploadText(nativeMessagingInput.Text, taskSettings);
                            }
                            break;
                        case NativeMessagingAction.UploadImage:
                            if (!string.IsNullOrEmpty(nativeMessagingInput.URL))
                            {
                                var image = await WebHelpers.DataURLToImage(nativeMessagingInput.URL);

                                if (image == null && taskSettings.AdvancedSettings.ProcessImagesDuringExtensionUpload)
                                {
                                    try
                                    {
                                        image = await WebHelpers.DownloadImageAsync(nativeMessagingInput.URL);
                                    }
                                    catch
                                    {
                                    }
                                }

                                if (image != null)
                                {
                                    UploadManager.RunImageTask(image, taskSettings);
                                }
                                else
                                {
                                    UploadManager.DownloadAndUploadFile(nativeMessagingInput.URL, taskSettings);
                                }
                            }
                            break;
                        case NativeMessagingAction.UploadVideo:
                        case NativeMessagingAction.UploadAudio:
                            if (!string.IsNullOrEmpty(nativeMessagingInput.URL))
                            {
                                UploadManager.DownloadAndUploadFile(nativeMessagingInput.URL, taskSettings);
                            }
                            break;
                        case NativeMessagingAction.UploadText:
                            if (!string.IsNullOrEmpty(nativeMessagingInput.Text))
                            {
                                UploadManager.UploadText(nativeMessagingInput.Text, taskSettings);
                            }
                            break;
                        case NativeMessagingAction.ShortenURL:
                            if (!string.IsNullOrEmpty(nativeMessagingInput.URL))
                            {
                                UploadManager.ShortenURL(nativeMessagingInput.URL, taskSettings);
                            }
                            break;
                    }
                }
            }
        }
        public static SixLabors.ImageSharp.Image<Rgba64> GenerateQRCode(string text, int size)
        {
            if (CheckQRCodeContent(text))
            {
                try
                {
                    var writer = new ZXing.ImageSharp.BarcodeWriter<Rgba64>
                    {
                        Format = BarcodeFormat.QR_CODE,
                        Options = new QrCodeEncodingOptions
                        {
                            Width = size,
                            Height = size,
                            CharacterSet = "UTF-8",
                            PureBarcode = true,
                            NoPadding = false,
                            Margin = 1
                        },
                        Renderer = new ImageSharpRenderer<Rgba64>()
                    };

                    return writer.Write(text);
                }
                catch (Exception e)
                {
                    e.ShowError();
                }
            }

            return null;
        }

        public static string[] BarcodeScan(Image<Rgba64> bmp, bool scanQRCodeOnly = false)
        {
            try
            {
                var barcodeReader = new ZXing.ImageSharp.BarcodeReader<Rgba64>
                {
                    AutoRotate = true,
                    Options = new DecodingOptions
                    {
                        TryHarder = true,
                        TryInverted = true
                    }
                };

                if (scanQRCodeOnly)
                {
                    barcodeReader.Options.PossibleFormats = new List<BarcodeFormat>() { BarcodeFormat.QR_CODE };
                }

                Result[] results = barcodeReader.DecodeMultiple(bmp);

                if (results != null)
                {
                    return results.Where(x => x != null && !string.IsNullOrEmpty(x.Text)).Select(x => x.Text).ToArray();
                }
            }
            catch (Exception e)
            {
                e.ShowError();
            }

            return null;
        }

        public static bool CheckQRCodeContent(string content)
        {
            return !string.IsNullOrEmpty(content) && Encoding.UTF8.GetByteCount(content) <= 2952;
        }

        public static bool IsUploadAllowed()
        {
            if (ShareX.Settings.DisableUpload)
            {

                return false;
            }

            return true;
        }
    }
}
