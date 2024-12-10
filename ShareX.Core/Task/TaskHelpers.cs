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

using ShareX.HelpersLib;
using ShareX.MediaLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ShareX.Core.Capture;
using ShareX.Core.CLI;
using ShareX.Core.Media;
using ShareX.Core.Upload;
using ShareX.Core.Upload.BaseServices;
using ShareX.Core.Upload.Custom;
using ShareX.Core.Upload.OAuth;
using ShareX.Core.Upload.SharingServices;
using ShareX.Core.Utils;
using ShareX.Core.Utils.Extensions;
using ShareX.Core.Utils.Miscellaneous;
using ShareX.Core.Utils.Random;
using ZXing;
using ZXing.Common;
using ZXing.QrCode;

namespace ShareX.Core.Task
{
    public static class TaskHelpers
    {
        public static async Task ExecuteJob(HotkeyType job, CLICommand command = null)
        {
            await ExecuteJob(ShareX.DefaultTaskSettings, job, command);
        }

        public static async Task ExecuteJob(TaskSettings taskSettings)
        {
            await ExecuteJob(taskSettings, taskSettings.Job);
        }

        public static async System.Threading.Tasks.Task ExecuteJob(TaskSettings taskSettings, HotkeyType job, CLICommand command = null)
        {
            if (job == HotkeyType.None) return;

            DebugHelper.WriteLine("Executing: " + job.GetLocalizedDescription());

            TaskSettings safeTaskSettings = TaskSettings.GetSafeTaskSettings(taskSettings);

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
                    UploadManager.ClipboardUploadWithContentViewer(safeTaskSettings);
                    break;
                case HotkeyType.UploadText:
                    UploadManager.ShowTextUploadDialog(safeTaskSettings);
                    break;
                case HotkeyType.UploadURL:
                    UploadManager.UploadURL(safeTaskSettings);
                    break;
                case HotkeyType.DragDropUpload:
                    OpenDropWindow(safeTaskSettings);
                    break;
                case HotkeyType.ShortenURL:
                    UploadManager.ShowShortenURLDialog(safeTaskSettings);
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
                    await OpenScrollingCapture(safeTaskSettings);
                    break;
                case HotkeyType.AutoCapture:
                    OpenAutoCapture(safeTaskSettings);
                    break;
                case HotkeyType.StartAutoCapture:
                    StartAutoCapture(safeTaskSettings);
                    break;
                // Screen record
                case HotkeyType.ScreenRecorder:
                    StartScreenRecording(ScreenRecordOutput.FFmpeg, ScreenRecordStartMethod.Region, safeTaskSettings);
                    break;
                case HotkeyType.ScreenRecorderActiveWindow:
                    StartScreenRecording(ScreenRecordOutput.FFmpeg, ScreenRecordStartMethod.ActiveWindow, safeTaskSettings);
                    break;
                case HotkeyType.ScreenRecorderCustomRegion:
                    StartScreenRecording(ScreenRecordOutput.FFmpeg, ScreenRecordStartMethod.CustomRegion, safeTaskSettings);
                    break;
                case HotkeyType.StartScreenRecorder:
                    StartScreenRecording(ScreenRecordOutput.FFmpeg, ScreenRecordStartMethod.LastRegion, safeTaskSettings);
                    break;
                case HotkeyType.ScreenRecorderGIF:
                    StartScreenRecording(ScreenRecordOutput.GIF, ScreenRecordStartMethod.Region, safeTaskSettings);
                    break;
                case HotkeyType.ScreenRecorderGIFActiveWindow:
                    StartScreenRecording(ScreenRecordOutput.GIF, ScreenRecordStartMethod.ActiveWindow, safeTaskSettings);
                    break;
                case HotkeyType.ScreenRecorderGIFCustomRegion:
                    StartScreenRecording(ScreenRecordOutput.GIF, ScreenRecordStartMethod.CustomRegion, safeTaskSettings);
                    break;
                case HotkeyType.StartScreenRecorderGIF:
                    StartScreenRecording(ScreenRecordOutput.GIF, ScreenRecordStartMethod.LastRegion, safeTaskSettings);
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
                    ShowScreenColorPickerDialog(safeTaskSettings);
                    break;
                case HotkeyType.ScreenColorPicker:
                    OpenScreenColorPicker(safeTaskSettings);
                    break;
                case HotkeyType.Ruler:
                    OpenRuler(safeTaskSettings);
                    break;
                case HotkeyType.PinToScreen:
                    PinToScreen(safeTaskSettings);
                    break;
                case HotkeyType.PinToScreenFromScreen:
                    PinToScreenFromScreen(safeTaskSettings);
                    break;
                case HotkeyType.PinToScreenFromClipboard:
                    PinToScreenFromClipboard(safeTaskSettings);
                    break;
                case HotkeyType.PinToScreenFromFile:
                    PinToScreenFromFile(safeTaskSettings);
                    break;
                case HotkeyType.PinToScreenCloseAll:
                    PinToScreenCloseAll(safeTaskSettings);
                    break;
                case HotkeyType.ImageEditor:
                    if (command != null && !string.IsNullOrEmpty(command.Parameter) && System.IO.File.Exists(command.Parameter))
                    {
                        AnnotateImageFromFile(command.Parameter, safeTaskSettings);
                    }
                    else
                    {
                        OpenImageEditor(safeTaskSettings);
                    }
                    break;
                case HotkeyType.ImageBeautifier:
                    if (command != null && !string.IsNullOrEmpty(command.Parameter) && System.IO.File.Exists(command.Parameter))
                    {
                        OpenImageBeautifier(command.Parameter, safeTaskSettings);
                    }
                    else
                    {
                        OpenImageBeautifier(safeTaskSettings);
                    }
                    break;
                case HotkeyType.ImageEffects:
                    if (command != null && !string.IsNullOrEmpty(command.Parameter) && System.IO.File.Exists(command.Parameter))
                    {
                        OpenImageEffects(command.Parameter, taskSettings);
                    }
                    else
                    {
                        OpenImageEffects(taskSettings);
                    }
                    break;
                case HotkeyType.ImageViewer:
                    if (command != null && !string.IsNullOrEmpty(command.Parameter) && System.IO.File.Exists(command.Parameter))
                    {
                        OpenImageViewer(command.Parameter);
                    }
                    else
                    {
                        OpenImageViewer();
                    }
                    break;
                case HotkeyType.ImageCombiner:
                    OpenImageCombiner(null, safeTaskSettings);
                    break;
                case HotkeyType.ImageSplitter:
                    OpenImageSplitter();
                    break;
                case HotkeyType.ImageThumbnailer:
                    OpenImageThumbnailer();
                    break;
                case HotkeyType.VideoConverter:
                    OpenVideoConverter(safeTaskSettings);
                    break;
                case HotkeyType.VideoThumbnailer:
                    OpenVideoThumbnailer(safeTaskSettings);
                    break;
                case HotkeyType.OCR:
                    await OCRImage(safeTaskSettings);
                    break;
                case HotkeyType.QRCode:
                    OpenQRCode();
                    break;
                case HotkeyType.QRCodeDecodeFromScreen:
                    OpenQRCodeDecodeFromScreen();
                    break;
                case HotkeyType.HashCheck:
                    OpenHashCheck();
                    break;
                case HotkeyType.IndexFolder:
                    UploadManager.IndexFolder();
                    break;
                case HotkeyType.ClipboardViewer:
                    OpenClipboardViewer();
                    break;
                case HotkeyType.BorderlessWindow:
                    OpenBorderlessWindow(safeTaskSettings);
                    break;
                case HotkeyType.ActiveWindowBorderless:
                    MakeActiveWindowBorderless(safeTaskSettings);
                    break;
                case HotkeyType.ActiveWindowTopMost:
                    MakeActiveWindowTopMost(safeTaskSettings);
                    break;
                case HotkeyType.InspectWindow:
                    OpenInspectWindow();
                    break;
                case HotkeyType.MonitorTest:
                    OpenMonitorTest();
                    break;
                case HotkeyType.DNSChanger:
                    OpenDNSChanger();
                    break;
                // Other
                case HotkeyType.DisableHotkeys:
                    ToggleHotkeys();
                    break;
                case HotkeyType.OpenMainWindow:
                    ShareX.MainForm.ForceActivate();
                    break;
                case HotkeyType.OpenScreenshotsFolder:
                    OpenScreenshotsFolder();
                    break;
                case HotkeyType.OpenHistory:
                    OpenHistory();
                    break;
                case HotkeyType.OpenImageHistory:
                    OpenImageHistory();
                    break;
                case HotkeyType.ToggleActionsToolbar:
                    ToggleActionsToolbar();
                    break;
                case HotkeyType.ToggleTrayMenu:
                    ToggleTrayMenu();
                    break;
                case HotkeyType.ExitShareX:
                    ShareX.MainForm.ForceClose();
                    break;
            }
        }

        public static ImageData PrepareImage(Image img, TaskSettings taskSettings)
        {
            ImageData imageData = new ImageData();
            imageData.ImageStream = SaveImageAsStream(img, taskSettings.ImageSettings.ImageFormat, taskSettings);
            imageData.ImageFormat = taskSettings.ImageSettings.ImageFormat;

            if (taskSettings.ImageSettings.ImageAutoUseJPEG && taskSettings.ImageSettings.ImageFormat != EImageFormat.JPEG &&
                imageData.ImageStream.Length > taskSettings.ImageSettings.ImageAutoUseJPEGSize * 1000)
            {
                imageData.ImageStream.Dispose();

                using (Bitmap newImage = Image.FillBackground(img, Color.White))
                {
                    if (taskSettings.ImageSettings.ImageAutoJPEGQuality)
                    {
                        imageData.ImageStream = Image.SaveJPEGAutoQuality(newImage, taskSettings.ImageSettings.ImageAutoUseJPEGSize * 1000, 2, 70, 100);
                    }
                    else
                    {
                        imageData.ImageStream = Image.SaveJPEG(newImage, taskSettings.ImageSettings.ImageJPEGQuality);
                    }
                }

                imageData.ImageFormat = EImageFormat.JPEG;
            }

            return imageData;
        }

        public static string CreateThumbnail(Bitmap bmp, string folder, string fileName, TaskSettings taskSettings)
        {
            if ((taskSettings.ImageSettings.ThumbnailWidth > 0 || taskSettings.ImageSettings.ThumbnailHeight > 0) && (!taskSettings.ImageSettings.ThumbnailCheckSize ||
                (bmp.Width > taskSettings.ImageSettings.ThumbnailWidth && bmp.Height > taskSettings.ImageSettings.ThumbnailHeight)))
            {
                string thumbnailFileName = Path.GetFileNameWithoutExtension(fileName) + taskSettings.ImageSettings.ThumbnailName + ".jpg";
                string thumbnailFilePath = HandleExistsFile(folder, thumbnailFileName, taskSettings);

                if (!string.IsNullOrEmpty(thumbnailFilePath))
                {
                    using (Bitmap thumbnail = (Bitmap)bmp.Clone())
                    using (Bitmap resizedImage = new Resize(taskSettings.ImageSettings.ThumbnailWidth, taskSettings.ImageSettings.ThumbnailHeight).Apply(thumbnail))
                    using (Bitmap newImage = Image.FillBackground(resizedImage, Color.White))
                    {
                        Image.SaveJPEG(newImage, thumbnailFilePath, 90);
                        return thumbnailFilePath;
                    }
                }
            }

            return null;
        }

        public static MemoryStream SaveImageAsStream(Image img, EImageFormat imageFormat, TaskSettings taskSettings)
        {
            return SaveImageAsStream(img, imageFormat, taskSettings.ImageSettings.ImagePNGBitDepth,
                taskSettings.ImageSettings.ImageJPEGQuality, taskSettings.ImageSettings.ImageGIFQuality);
        }

        public static MemoryStream SaveImageAsStream(Image img, EImageFormat imageFormat, PNGBitDepth pngBitDepth = PNGBitDepth.Automatic,
            int jpegQuality = 90, GIFQuality gifQuality = GIFQuality.Default)
        {
            MemoryStream ms = new MemoryStream();

            try
            {
                switch (imageFormat)
                {
                    case EImageFormat.PNG:
                        Image.SavePNG(img, ms, pngBitDepth);

                        if (ShareX.Settings.PNGStripColorSpaceInformation)
                        {
                            using (ms)
                            {
                                return Image.PNGStripColorSpaceInformation(ms);
                            }
                        }
                        break;
                    case EImageFormat.JPEG:
                        using (Bitmap newImage = Image.FillBackground(img, Color.White))
                        {
                            Image.SaveJPEG(newImage, ms, jpegQuality);
                        }
                        break;
                    case EImageFormat.GIF:
                        Image.SaveGIF(img, ms, gifQuality);
                        break;
                    case EImageFormat.BMP:
                        img.Save(ms, ImageFormat.Bmp);
                        break;
                    case EImageFormat.TIFF:
                        img.Save(ms, ImageFormat.Tiff);
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

        public static void SaveImageAsFile(Bitmap bmp, TaskSettings taskSettings, bool overwriteFile = false)
        {
            using (ImageData imageData = PrepareImage(bmp, taskSettings))
            {
                string screenshotsFolder = GetScreenshotsFolder(taskSettings);
                string fileName = GetFileName(taskSettings, imageData.ImageFormat.GetDescription(), bmp);
                string filePath = Path.Combine(screenshotsFolder, fileName);

                if (!overwriteFile)
                {
                    filePath = HandleExistsFile(filePath, taskSettings);
                }

                if (!string.IsNullOrEmpty(filePath))
                {
                    imageData.Write(filePath);
                    DebugHelper.WriteLine("Image saved to file: " + filePath);
                }
            }
        }

        public static string GetFileName(TaskSettings taskSettings, string extension, Bitmap bmp)
        {
            TaskMetadata metadata = new TaskMetadata(bmp);
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

            return File.GetAbsolutePath(screenshotsFolder);
        }

        public static bool ShowAfterCaptureForm(TaskSettings taskSettings, out string fileName, TaskMetadata metadata = null, string filePath = null)
        {
            fileName = null;

            if (taskSettings.AfterCaptureJob.HasFlag(AfterCaptureTasks.ShowAfterCaptureWindow))
            {
                AfterCaptureForm afterCaptureForm = null;

                try
                {
                    if (!string.IsNullOrEmpty(filePath))
                    {
                        afterCaptureForm = new AfterCaptureForm(filePath, taskSettings);
                    }
                    else
                    {
                        afterCaptureForm = new AfterCaptureForm(metadata, taskSettings);
                    }

                    if (afterCaptureForm.ShowDialog() == DialogResult.Cancel)
                    {
                        metadata?.Dispose();

                        return false;
                    }

                    fileName = afterCaptureForm.FileName;
                }
                finally
                {
                    afterCaptureForm.Dispose();
                }
            }

            return true;
        }

        public static void PrintImage(Image img)
        {
            if (ShareX.Settings.DontShowPrintSettingsDialog)
            {
                using (PrintHelper printHelper = new PrintHelper(img))
                {
                    printHelper.Settings = ShareX.Settings.PrintSettings;
                    printHelper.Print();
                }
            }
            else
            {
                using (PrintForm printForm = new PrintForm(img, ShareX.Settings.PrintSettings))
                {
                    printForm.ShowDialog();
                }
            }
        }

        public static Bitmap ApplyImageEffects(Bitmap bmp, TaskSettingsImage taskSettingsImage)
        {
            if (bmp != null)
            {
                bmp = Image.NonIndexedBitmap(bmp);

                if (taskSettingsImage.ShowImageEffectsWindowAfterCapture)
                {
                    using (ImageEffectsForm imageEffectsForm = new ImageEffectsForm(bmp, taskSettingsImage.ImageEffectPresets,
                        taskSettingsImage.SelectedImageEffectPreset))
                    {
                        imageEffectsForm.ShowDialog();
                        taskSettingsImage.SelectedImageEffectPreset = imageEffectsForm.SelectedPresetIndex;
                    }
                }

                ImageEffectPreset imageEffect = null;

                if (taskSettingsImage.UseRandomImageEffect)
                {
                    imageEffect = RandomFast.Pick(taskSettingsImage.ImageEffectPresets);
                }
                else if (taskSettingsImage.ImageEffectPresets.IsValidIndex(taskSettingsImage.SelectedImageEffectPreset))
                {
                    imageEffect = taskSettingsImage.ImageEffectPresets[taskSettingsImage.SelectedImageEffectPreset];
                }

                if (imageEffect != null)
                {
                    using (bmp)
                    {
                        return imageEffect.ApplyEffects(bmp);
                    }
                }
            }

            return bmp;
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
            if (!taskSettings.ExternalPrograms.Exists(x => x.Name == name))
            {
                try
                {
                    string filePath = RegistryHelpers.SearchProgramPath(fileName);

                    if (!string.IsNullOrEmpty(filePath))
                    {
                        ExternalProgram externalProgram = new ExternalProgram(name, filePath);
                        taskSettings.ExternalPrograms.Add(externalProgram);
                    }
                }
                catch (Exception e)
                {
                    DebugHelper.WriteException(e);
                }
            }
        }

        public static string HandleExistsFile(string folder, string fileName, TaskSettings taskSettings)
        {
            string filePath = Path.Combine(folder, fileName);
            return HandleExistsFile(filePath, taskSettings);
        }

        public static string HandleExistsFile(string filePath, TaskSettings taskSettings)
        {
            if (System.IO.File.Exists(filePath))
            {
                switch (taskSettings.ImageSettings.FileExistAction)
                {
                    case FileExistAction.Ask:
                        using (FileExistForm form = new FileExistForm(filePath))
                        {
                            form.ShowDialog();
                            filePath = form.FilePath;
                        }
                        break;
                    case FileExistAction.UniqueName:
                        filePath = File.GetUniqueFilePath(filePath);
                        break;
                    case FileExistAction.Cancel:
                        filePath = "";
                        break;
                }
            }

            return filePath;
        }

        public static void OpenDropWindow(TaskSettings taskSettings = null)
        {
            DropForm.GetInstance(ShareX.Settings.DropSize, ShareX.Settings.DropOffset, ShareX.Settings.DropAlignment, ShareX.Settings.DropOpacity,
                ShareX.Settings.DropHoverOpacity, taskSettings).ForceActivate();
        }

        public static void StartScreenRecording(ScreenRecordOutput outputType, ScreenRecordStartMethod startMethod, TaskSettings taskSettings = null)
        {
            if (taskSettings == null) taskSettings = TaskSettings.GetDefaultTaskSettings();

            ScreenRecordManager.StartStopRecording(outputType, startMethod, taskSettings);
        }

        public static void StopScreenRecording()
        {
            ScreenRecordManager.StopRecording();
        }

        public static void PauseScreenRecording()
        {
            ScreenRecordManager.PauseScreenRecording();
        }

        public static void AbortScreenRecording()
        {
            ScreenRecordManager.AbortRecording();
        }

        public static async Task OpenScrollingCapture(TaskSettings taskSettings = null)
        {
            if (taskSettings == null) taskSettings = TaskSettings.GetDefaultTaskSettings();

            await ScrollingCaptureForm.StartStopScrollingCapture(taskSettings.CaptureSettingsReference.ScrollingCaptureOptions,
                img => UploadManager.RunImageTask(img, taskSettings));
        }

        public static void OpenAutoCapture(TaskSettings taskSettings = null)
        {
            if (taskSettings == null) taskSettings = TaskSettings.GetDefaultTaskSettings();

            AutoCaptureForm.Instance.TaskSettings = taskSettings;
            AutoCaptureForm.Instance.ForceActivate();
        }

        public static void StartAutoCapture(TaskSettings taskSettings = null)
        {
            if (taskSettings == null) taskSettings = TaskSettings.GetDefaultTaskSettings();

            if (!AutoCaptureForm.IsRunning)
            {
                AutoCaptureForm form = AutoCaptureForm.Instance;
                form.TaskSettings = taskSettings;
                form.Show();
                form.Execute();
            }
        }

        public static void OpenScreenshotsFolder()
        {
            string screenshotsFolder = GetScreenshotsFolder();

            if (Directory.Exists(screenshotsFolder))
            {
                File.OpenFolder(screenshotsFolder);
            }
            else
            {
                File.OpenFolder(ShareX.ScreenshotsParentFolder);
            }
        }

        public static void OpenHistory()
        {
            HistoryForm historyForm = new HistoryForm(ShareX.HistoryFilePath, ShareX.Settings.HistorySettings,
                filePath => UploadManager.UploadFile(filePath),
                filePath => AnnotateImageFromFile(filePath),
                filePath => PinToScreen(filePath));

            historyForm.Show();
        }

        public static void OpenImageHistory()
        {
            ImageHistoryForm imageHistoryForm = new ImageHistoryForm(ShareX.HistoryFilePath, ShareX.Settings.ImageHistorySettings,
                filePath => UploadManager.UploadFile(filePath),
                filePath => AnnotateImageFromFile(filePath),
                filePath => PinToScreen(filePath));

            imageHistoryForm.Show();
        }

        public static void OpenDebugLog()
        {
            DebugForm form = DebugForm.GetFormInstance(DebugHelper.Logger);

            if (!form.HasUploadRequested)
            {
                form.UploadRequested += text =>
                {
                    if (MessageBox.Show(form, Resources.MainForm_UploadDebugLogWarning, "ShareX", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                    {
                        UploadManager.UploadText(text);
                    }
                };
            }

            form.ForceActivate();
        }

        public static void ShowScreenColorPickerDialog(TaskSettings taskSettings = null)
        {
            if (taskSettings == null) taskSettings = TaskSettings.GetDefaultTaskSettings();
            taskSettings.CaptureSettings.SurfaceOptions.ScreenColorPickerInfoText = taskSettings.ToolsSettings.ScreenColorPickerInfoText;

            RegionCaptureTasks.ShowScreenColorPickerDialog(taskSettings.CaptureSettingsReference.SurfaceOptions);
        }

        public static void OpenScreenColorPicker(TaskSettings taskSettings = null)
        {
            if (taskSettings == null) taskSettings = TaskSettings.GetDefaultTaskSettings();
            taskSettings.CaptureSettings.SurfaceOptions.ScreenColorPickerInfoText = taskSettings.ToolsSettings.ScreenColorPickerInfoText;

            PointInfo pointInfo = RegionCaptureTasks.GetPointInfo(taskSettings.CaptureSettings.SurfaceOptions);

            if (pointInfo != null)
            {
                string input;

                if (Control.ModifierKeys == Keys.Control)
                {
                    input = taskSettings.ToolsSettings.ScreenColorPickerFormatCtrl;
                }
                else
                {
                    input = taskSettings.ToolsSettings.ScreenColorPickerFormat;
                }

                if (!string.IsNullOrEmpty(input))
                {
                    string text = CodeMenuEntryPixelInfo.Parse(input, pointInfo.Color, pointInfo.Position);
                    ClipboardHelpers.CopyText(text);

                    PlayNotificationSoundAsync(NotificationSound.ActionCompleted, taskSettings);

                    if (taskSettings.GeneralSettings.ShowToastNotificationAfterTaskCompleted)
                    {
                        ShowNotificationTip(string.Format(Resources.TaskHelpers_OpenQuickScreenColorPicker_Copied_to_clipboard___0_, text),
                            "ShareX - " + Resources.ScreenColorPicker);
                    }
                }
            }
        }

        public static void OpenHashCheck()
        {
            new HashCheckerForm().Show();
        }

        public static void OpenDirectoryIndexer(TaskSettings taskSettings = null)
        {
            if (taskSettings == null) taskSettings = TaskSettings.GetDefaultTaskSettings();

            IndexerSettings indexerSettings = taskSettings.ToolsSettingsReference.IndexerSettings;
            indexerSettings.BinaryUnits = ShareX.Settings.BinaryUnits;
            DirectoryIndexerForm form = new DirectoryIndexerForm(indexerSettings);
            form.UploadRequested += source =>
            {
                WorkerTask task = WorkerTask.CreateTextUploaderTask(source, taskSettings);
                task.Info.FileName = Path.ChangeExtension(task.Info.FileName, indexerSettings.Output.ToString().ToLowerInvariant());
                TaskManager.Start(task);
            };
            form.Show();
        }

        public static void OpenImageCombiner(IEnumerable<string> imageFiles = null, TaskSettings taskSettings = null)
        {
            if (taskSettings == null) taskSettings = TaskSettings.GetDefaultTaskSettings();

            ImageCombinerForm imageCombinerForm = new ImageCombinerForm(taskSettings.ToolsSettingsReference.ImageCombinerOptions, imageFiles);
            imageCombinerForm.ProcessRequested += bmp => UploadManager.RunImageTask(bmp, taskSettings);
            imageCombinerForm.Show();
        }

        public static void CombineImages(IEnumerable<string> imageFiles, Orientation orientation, TaskSettings taskSettings = null)
        {
            if (taskSettings == null) taskSettings = TaskSettings.GetDefaultTaskSettings();

            Bitmap output = Image.CombineImages(imageFiles, orientation, taskSettings.ToolsSettings.ImageCombinerOptions.Alignment,
                taskSettings.ToolsSettings.ImageCombinerOptions.Space, taskSettings.ToolsSettings.ImageCombinerOptions.WrapAfter,
                taskSettings.ToolsSettings.ImageCombinerOptions.AutoFillBackground);

            if (output != null)
            {
                UploadManager.RunImageTask(output, taskSettings);
            }
        }

        public static void OpenImageSplitter()
        {
            ImageSplitterForm imageSplitterForm = new ImageSplitterForm();
            imageSplitterForm.Show();
        }

        public static void OpenImageThumbnailer()
        {
            ImageThumbnailerForm imageThumbnailerForm = new ImageThumbnailerForm();
            imageThumbnailerForm.Show();
        }

        public static void OpenVideoConverter(TaskSettings taskSettings = null)
        {
            if (taskSettings == null) taskSettings = TaskSettings.GetDefaultTaskSettings();

            if (!CheckFFmpeg(taskSettings))
            {
                return;
            }

            VideoConverterForm videoConverterForm = new VideoConverterForm(taskSettings.CaptureSettings.FFmpegOptions.FFmpegPath,
                taskSettings.ToolsSettingsReference.VideoConverterOptions);
            videoConverterForm.Show();
        }

        public static void OpenVideoThumbnailer(TaskSettings taskSettings = null)
        {
            if (taskSettings == null) taskSettings = TaskSettings.GetDefaultTaskSettings();

            if (!CheckFFmpeg(taskSettings))
            {
                return;
            }

            taskSettings.ToolsSettingsReference.VideoThumbnailOptions.DefaultOutputDirectory = GetScreenshotsFolder(taskSettings);
            VideoThumbnailerForm thumbnailerForm = new VideoThumbnailerForm(taskSettings.CaptureSettings.FFmpegOptions.FFmpegPath,
                taskSettings.ToolsSettingsReference.VideoThumbnailOptions);
            thumbnailerForm.ThumbnailsTaken += thumbnails =>
            {
                if (taskSettings.ToolsSettingsReference.VideoThumbnailOptions.UploadThumbnails)
                {
                    foreach (VideoThumbnailInfo thumbnailInfo in thumbnails)
                    {
                        UploadManager.UploadFile(thumbnailInfo.FilePath, taskSettings);
                    }
                }
            };
            thumbnailerForm.Show();
        }

        public static void OpenBorderlessWindow(TaskSettings taskSettings = null)
        {
            if (taskSettings == null) taskSettings = TaskSettings.GetDefaultTaskSettings();

            BorderlessWindowSettings settings = taskSettings.ToolsSettingsReference.BorderlessWindowSettings;
            BorderlessWindowForm borderlessWindowForm = new BorderlessWindowForm(settings);
            borderlessWindowForm.Show();
        }

        public static void MakeActiveWindowBorderless(TaskSettings taskSettings = null)
        {
            if (taskSettings == null) taskSettings = TaskSettings.GetDefaultTaskSettings();

            try
            {
                IntPtr handle = NativeMethods.GetForegroundWindow();

                if (handle.ToInt32() > 0)
                {
                    BorderlessWindowManager.ToggleBorderlessWindow(handle, taskSettings.ToolsSettings.BorderlessWindowSettings.ExcludeTaskbarArea);

                    PlayNotificationSoundAsync(NotificationSound.ActionCompleted, taskSettings);
                }
            }
            catch (Exception e)
            {
                e.ShowError();
            }
        }

        public static void MakeActiveWindowTopMost(TaskSettings taskSettings = null)
        {
            if (taskSettings == null) taskSettings = TaskSettings.GetDefaultTaskSettings();

            try
            {
                IntPtr handle = NativeMethods.GetForegroundWindow();

                if (handle.ToInt32() > 0)
                {
                    WindowInfo windowInfo = new WindowInfo(handle);
                    windowInfo.TopMost = !windowInfo.TopMost;

                    PlayNotificationSoundAsync(NotificationSound.ActionCompleted, taskSettings);
                }
            }
            catch (Exception e)
            {
                e.ShowError();
            }
        }

        public static void OpenInspectWindow()
        {
            InspectWindowForm inspectWindowForm = new InspectWindowForm();
            inspectWindowForm.Show();
        }

        public static void OpenClipboardViewer()
        {
            ClipboardViewerForm clipboardViewerForm = new ClipboardViewerForm();
            clipboardViewerForm.Show();
        }

        public static void OpenImageEditor(TaskSettings taskSettings = null)
        {
            if (taskSettings == null) taskSettings = TaskSettings.GetDefaultTaskSettings();

            using (EditorStartupForm editorStartupForm = new EditorStartupForm(taskSettings.CaptureSettingsReference.SurfaceOptions))
            {
                if (editorStartupForm.ShowDialog() == DialogResult.OK)
                {
                    AnnotateImageAsync(editorStartupForm.Image, editorStartupForm.ImageFilePath, taskSettings);
                }
            }
        }

        public static void AnnotateImageFromFile(string filePath, TaskSettings taskSettings = null)
        {
            if (!string.IsNullOrEmpty(filePath) && System.IO.File.Exists(filePath))
            {
                if (taskSettings == null) taskSettings = TaskSettings.GetDefaultTaskSettings();

                Bitmap bmp = Image.LoadImage(filePath);

                AnnotateImageAsync(bmp, filePath, taskSettings);
            }
            else
            {
                MessageBox.Show("File does not exist:" + Environment.NewLine + filePath, "ShareX", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        public static void AnnotateImageAsync(Bitmap bmp, string filePath, TaskSettings taskSettings)
        {
            ThreadWorker worker = new ThreadWorker();

            worker.DoWork += () =>
            {
                bmp = AnnotateImage(bmp, filePath, taskSettings);
            };

            worker.Completed += () =>
            {
                if (bmp != null)
                {
                    UploadManager.RunImageTask(bmp, taskSettings);
                }
            };

            worker.Start(ApartmentState.STA);
        }

        public static Bitmap AnnotateImage(Bitmap bmp, string filePath, TaskSettings taskSettings, bool taskMode = false)
        {
            if (bmp != null)
            {
                bmp = Image.NonIndexedBitmap(bmp);

                using (bmp)
                {
                    RegionCaptureMode mode = taskMode ? RegionCaptureMode.TaskEditor : RegionCaptureMode.Editor;
                    RegionCaptureOptions options = taskSettings.CaptureSettingsReference.SurfaceOptions;

                    using (RegionCaptureForm form = new RegionCaptureForm(mode, options, bmp))
                    {
                        form.ImageFilePath = filePath;

                        form.SaveImageRequested += (output, newFilePath) =>
                        {
                            using (output)
                            {
                                if (string.IsNullOrEmpty(newFilePath))
                                {
                                    string screenshotsFolder = GetScreenshotsFolder(taskSettings);
                                    string fileName = GetFileName(taskSettings, taskSettings.ImageSettings.ImageFormat.GetDescription(), output);
                                    newFilePath = Path.Combine(screenshotsFolder, fileName);
                                }

                                Image.SaveImage(output, newFilePath);
                            }

                            return newFilePath;
                        };

                        form.SaveImageAsRequested += (output, newFilePath) =>
                        {
                            using (output)
                            {
                                if (string.IsNullOrEmpty(newFilePath))
                                {
                                    string screenshotsFolder = GetScreenshotsFolder(taskSettings);
                                    string fileName = GetFileName(taskSettings, taskSettings.ImageSettings.ImageFormat.GetDescription(), output);
                                    newFilePath = Path.Combine(screenshotsFolder, fileName);
                                }

                                newFilePath = Image.SaveImageFileDialog(output, newFilePath);
                            }

                            return newFilePath;
                        };

                        form.CopyImageRequested += MainFormCopyImage;
                        form.UploadImageRequested += output => MainFormUploadImage(output, taskSettings);
                        form.PrintImageRequested += MainFormPrintImage;
                        form.ShowDialog();

                        switch (form.Result)
                        {
                            case RegionResult.Close: // Esc
                            case RegionResult.AnnotateCancelTask:
                                return null;
                            case RegionResult.Region: // Enter
                            case RegionResult.AnnotateRunAfterCaptureTasks:
                                return form.GetResultImage();
                            case RegionResult.Fullscreen: // Space or right click
                            case RegionResult.AnnotateContinueTask:
                                return (Bitmap)form.Canvas.Clone();
                        }
                    }
                }
            }

            return null;
        }

        public static void MainFormCopyImage(Bitmap bmp)
        {
            ShareX.MainForm.InvokeSafe(() =>
            {
                ClipboardHelpers.CopyImage(bmp);
            });
        }

        public static void MainFormUploadImage(Bitmap bmp, TaskSettings taskSettings = null)
        {
            ShareX.MainForm.InvokeSafe(() =>
            {
                UploadManager.UploadImage(bmp, taskSettings);
            });
        }

        public static void MainFormPrintImage(Bitmap bmp)
        {
            ShareX.MainForm.InvokeSafe(() =>
            {
                using (bmp)
                {
                    PrintImage(bmp);
                }
            });
        }

        public static void OpenImageBeautifier(TaskSettings taskSettings = null)
        {
            string filePath = Image.OpenImageFileDialog();

            OpenImageBeautifier(filePath, taskSettings);
        }

        public static void OpenImageBeautifier(string filePath, TaskSettings taskSettings = null)
        {
            if (!string.IsNullOrEmpty(filePath))
            {
                if (taskSettings == null) taskSettings = TaskSettings.GetDefaultTaskSettings();

                ImageBeautifierForm imageBeautifierForm = new ImageBeautifierForm(filePath, taskSettings.ToolsSettingsReference.ImageBeautifierOptions);
                imageBeautifierForm.UploadImageRequested += output => MainFormUploadImage(output, taskSettings);
                imageBeautifierForm.PrintImageRequested += MainFormPrintImage;
                imageBeautifierForm.Show();
            }
        }

        public static Bitmap BeautifyImage(Bitmap bmp, TaskSettings taskSettings = null)
        {
            if (bmp != null)
            {
                if (taskSettings == null) taskSettings = TaskSettings.GetDefaultTaskSettings();

                using (ImageBeautifierForm imageBeautifierForm = new ImageBeautifierForm(bmp, taskSettings.ToolsSettingsReference.ImageBeautifierOptions))
                {
                    imageBeautifierForm.UploadImageRequested += output => MainFormUploadImage(output, taskSettings);
                    imageBeautifierForm.PrintImageRequested += MainFormPrintImage;
                    imageBeautifierForm.ShowDialog();

                    return (Bitmap)imageBeautifierForm.PreviewImage.Clone();
                }
            }

            return null;
        }

        public static void OpenImageEffects(TaskSettings taskSettings = null)
        {
            string filePath = Image.OpenImageFileDialog();

            OpenImageEffects(filePath, taskSettings);
        }

        public static void OpenImageEffects(string filePath, TaskSettings taskSettings = null)
        {
            if (!string.IsNullOrEmpty(filePath))
            {
                Bitmap bmp = Image.LoadImage(filePath);

                if (bmp != null)
                {
                    bmp = Image.NonIndexedBitmap(bmp);

                    if (taskSettings == null) taskSettings = ShareX.DefaultTaskSettings;

                    using (ImageEffectsForm imageEffectsForm = new ImageEffectsForm(bmp, taskSettings.ImageSettings.ImageEffectPresets,
                        taskSettings.ImageSettings.SelectedImageEffectPreset))
                    {
                        imageEffectsForm.EnableToolMode(x => UploadManager.RunImageTask(x, taskSettings), filePath);
                        imageEffectsForm.ShowDialog();
                        //taskSettings.ImageSettings.SelectedImageEffectPreset = imageEffectsForm.SelectedPresetIndex;
                    }
                }
            }
        }

        public static ImageEffectsForm OpenImageEffectsSingleton(TaskSettings taskSettings = null)
        {
            if (taskSettings == null) taskSettings = ShareX.DefaultTaskSettings;

            bool firstInstance = !ImageEffectsForm.IsInstanceActive;

            ImageEffectsForm imageEffectsForm = ImageEffectsForm.GetFormInstance(taskSettings.ImageSettings.ImageEffectPresets,
                taskSettings.ImageSettings.SelectedImageEffectPreset);

            if (firstInstance)
            {
                imageEffectsForm.FormClosed += (sender, e) => taskSettings.ImageSettings.SelectedImageEffectPreset = imageEffectsForm.SelectedPresetIndex;
                imageEffectsForm.Show();
            }
            else
            {
                imageEffectsForm.ForceActivate();
            }

            return imageEffectsForm;
        }

        public static void OpenImageViewer()
        {
            string filePath = Image.OpenImageFileDialog();
            OpenImageViewer(filePath);
        }

        public static void OpenImageViewer(string filePath)
        {
            if (!string.IsNullOrEmpty(filePath) && System.IO.File.Exists(filePath))
            {
                string folderPath = Path.GetDirectoryName(filePath);
                string[] files = Directory.GetFiles(folderPath);

                if (files != null && files.Length > 0)
                {
                    int imageIndex = Array.IndexOf(files, filePath);
                    ImageViewer.ShowImage(files, imageIndex);
                }
            }
        }

        public static void OpenMonitorTest()
        {
            using (MonitorTestForm monitorTestForm = new MonitorTestForm())
            {
                monitorTestForm.ShowDialog();
            }
        }

        public static void OpenDNSChanger()
        {
#if MicrosoftStore
            MessageBox.Show("Not supported in Microsoft Store build.", "ShareX", MessageBoxButtons.OK, MessageBoxIcon.Information);
#else
            if (Helpers.IsAdministrator())
            {
                new DNSChangerForm().Show();
            }
            else
            {
                RunShareXAsAdmin("-dnschanger");
            }
#endif
        }

        public static void RunShareXAsAdmin(string arguments = null)
        {
            try
            {
                using (Process process = new Process())
                {
                    ProcessStartInfo psi = new ProcessStartInfo()
                    {
                        FileName = "sharex",
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

        public static void OpenQRCode()
        {
            QRCodeForm.GenerateQRCodeFromClipboard().Show();
        }

        public static void OpenQRCodeDecodeFromScreen()
        {
            QRCodeForm.OpenFormScanFromScreen();
        }

        public static void OpenRuler(TaskSettings taskSettings = null)
        {
            if (taskSettings == null) taskSettings = TaskSettings.GetDefaultTaskSettings();

            RegionCaptureTasks.ShowScreenRuler(taskSettings.CaptureSettings.SurfaceOptions);
        }

        public static void SearchImageUsingGoogleLens(string url)
        {
            new GoogleLensSharingService().CreateSharer(null, null).ShareURL(url);
        }

        public static void SearchImageUsingBing(string url)
        {
            new BingVisualSearchSharingService().CreateSharer(null, null).ShareURL(url);
        }

        public static async Task OCRImage(TaskSettings taskSettings = null)
        {
            if (taskSettings == null) taskSettings = TaskSettings.GetDefaultTaskSettings();

            using (Bitmap bmp = RegionCaptureTasks.GetRegionImage(taskSettings.CaptureSettings.SurfaceOptions))
            {
                await OCRImage(bmp, taskSettings);
            }
        }

        public static async Task OCRImage(string filePath, TaskSettings taskSettings = null)
        {
            if (!string.IsNullOrEmpty(filePath) && System.IO.File.Exists(filePath))
            {
                using (Bitmap bmp = Image.LoadImage(filePath))
                {
                    await OCRImage(bmp, filePath, taskSettings);
                }
            }
        }

        public static async Task OCRImage(Bitmap bmp, TaskSettings taskSettings = null)
        {
            await OCRImage(bmp, null, taskSettings);
        }

        public static async Task OCRImage(Bitmap bmp, string filePath = null, TaskSettings taskSettings = null)
        {
            if (taskSettings == null) taskSettings = TaskSettings.GetDefaultTaskSettings();

            OCROptions options = taskSettings.CaptureSettingsReference.OCROptions;

            try
            {
                OCRHelper.ThrowIfNotSupported();

                if (bmp != null)
                {
                    if (options.Silent)
                    {
                        await AsyncOCRImage(bmp, filePath, taskSettings);
                    }
                    else
                    {
                        using (OCRForm form = new OCRForm(bmp, options))
                        {
                            form.ShowDialog();

                            if (!string.IsNullOrEmpty(form.Result) && !string.IsNullOrEmpty(filePath))
                            {
                                string textFilePath = Path.ChangeExtension(filePath, "txt");
                                System.IO.File.WriteAllText(textFilePath, form.Result, Encoding.UTF8);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                e.ShowError(false);
            }
        }

        private static async Task AsyncOCRImage(Bitmap bmp, string filePath = null, TaskSettings taskSettings = null)
        {
            if (bmp != null)
            {
                if (taskSettings == null) taskSettings = TaskSettings.GetDefaultTaskSettings();

                OCROptions options = taskSettings.CaptureSettingsReference.OCROptions;

                string result = await OCRHelper.OCR(bmp, options.Language, options.ScaleFactor, options.SingleLine);

                if (!string.IsNullOrEmpty(result))
                {
                    ShareX.MainForm.InvokeSafe(() =>
                    {
                        ClipboardHelpers.CopyText(result);
                    });

                    if (!string.IsNullOrEmpty(filePath))
                    {
                        string textFilePath = Path.ChangeExtension(filePath, "txt");
                        System.IO.File.WriteAllText(textFilePath, result, Encoding.UTF8);
                    }
                }
                else
                {
                    ShareX.MainForm.InvokeSafe(() =>
                    {
                        ClipboardHelpers.Clear();
                    });
                }

                PlayNotificationSoundAsync(NotificationSound.ActionCompleted, taskSettings);
            }
        }

        public static void PinToScreen(TaskSettings taskSettings = null)
        {
            using (PinToScreenStartupForm form = new PinToScreenStartupForm())
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    PinToScreen(form.Image, form.PinToScreenLocation, taskSettings);
                }
            }
        }

        public static void PinToScreen(Image image, TaskSettings taskSettings = null)
        {
            PinToScreen(image, null, taskSettings);
        }

        public static void PinToScreen(Image image, Point? location, TaskSettings taskSettings = null)
        {
            if (image != null)
            {
                if (taskSettings == null) taskSettings = TaskSettings.GetDefaultTaskSettings();

                PinToScreenOptions options = taskSettings.ToolsSettingsReference.PinToScreenOptions;
                options.BackgroundColor = ShareXResources.Theme.LightBackgroundColor;

                PinToScreenForm.PinToScreenAsync(image, options, location);

                PlayNotificationSoundAsync(NotificationSound.ActionCompleted, taskSettings);
            }
        }

        public static void PinToScreen(string filePath, TaskSettings taskSettings = null)
        {
            Image image = Image.LoadImage(filePath);

            PinToScreen(image, taskSettings);
        }

        public static void PinToScreenFromScreen(TaskSettings taskSettings = null)
        {
            Image image = RegionCaptureTasks.GetRegionImage(out Rectangle rect);

            PinToScreen(image, rect.Location, taskSettings);
        }

        public static void PinToScreenFromClipboard(TaskSettings taskSettings = null)
        {
            Image image = ClipboardHelpers.TryGetImage();

            if (image != null)
            {
                PinToScreen(image, taskSettings);
            }
            else
            {
                MessageBox.Show(Resources.ClipboardDoesNotContainAnImage, "ShareX - " + Resources.PinToScreen, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        public static void PinToScreenFromFile(TaskSettings taskSettings = null)
        {
            Image image = Image.LoadImageWithFileDialog();

            if (image != null)
            {
                PinToScreen(image, taskSettings);
            }
        }

        public static void PinToScreenCloseAll(TaskSettings taskSettings = null)
        {
            if (taskSettings == null) taskSettings = TaskSettings.GetDefaultTaskSettings();

            PinToScreenForm.CloseAll();

            PlayNotificationSoundAsync(NotificationSound.ActionCompleted, taskSettings);
        }

        public static void TweetMessage()
        {
            if (IsUploadAllowed())
            {
                if (ShareX.UploadersConfig != null && ShareX.UploadersConfig.TwitterOAuthInfoList != null)
                {
                    OAuthInfo twitterOAuth = ShareX.UploadersConfig.TwitterOAuthInfoList.ReturnIfValidIndex(ShareX.UploadersConfig.TwitterSelectedAccount);

                    if (twitterOAuth != null && OAuthInfo.CheckOAuth(twitterOAuth))
                    {
                        Task.Run(() =>
                        {
                            using (TwitterTweetForm twitter = new TwitterTweetForm(twitterOAuth))
                            {
                                if (twitter.ShowDialog() == DialogResult.OK && twitter.IsTweetSent)
                                {
                                    ShowNotificationTip(Resources.TaskHelpers_TweetMessage_Tweet_successfully_sent_);
                                }
                            }
                        });

                        return;
                    }
                }

                MessageBox.Show(Resources.TaskHelpers_TweetMessage_Unable_to_find_valid_Twitter_account_, "ShareX", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        public static EDataType FindDataType(string filePath, TaskSettings taskSettings)
        {
            if (File.CheckExtension(filePath, taskSettings.AdvancedSettings.ImageExtensions))
            {
                return EDataType.Image;
            }

            if (File.CheckExtension(filePath, taskSettings.AdvancedSettings.TextExtensions))
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
            ShareX.MainForm.UpdateToggleHotkeyButton();

            ShowNotificationTip(disableHotkeys ? Resources.TaskHelpers_ToggleHotkeys_Hotkeys_disabled_ : Resources.TaskHelpers_ToggleHotkeys_Hotkeys_enabled_);
        }

        public static bool CheckFFmpeg(TaskSettings taskSettings)
        {
            if (!Environment.Is64BitOperatingSystem &&
                !taskSettings.CaptureSettings.FFmpegOptions.OverrideCLIPath) return false;

            string ffmpegPath = taskSettings.CaptureSettings.FFmpegOptions.FFmpegPath;

            if (!System.IO.File.Exists(ffmpegPath)) return false;

            return true;
        }

        public static void PlayNotificationSoundAsync(NotificationSound notificationSound, TaskSettings taskSettings = null)
        {
            if (taskSettings == null) taskSettings = TaskSettings.GetDefaultTaskSettings();

            switch (notificationSound)
            {
                case NotificationSound.Capture:
                    if (taskSettings.GeneralSettings.PlaySoundAfterCapture)
                    {
                        if (taskSettings.GeneralSettings.UseCustomCaptureSound && !string.IsNullOrEmpty(taskSettings.GeneralSettings.CustomCaptureSoundPath))
                        {
                            Helpers.PlaySoundAsync(taskSettings.GeneralSettings.CustomCaptureSoundPath);
                        }
                        else
                        {
                            Helpers.PlaySoundAsync(Resources.CaptureSound);
                        }
                    }
                    break;
                case NotificationSound.TaskCompleted:
                    if (taskSettings.GeneralSettings.PlaySoundAfterUpload)
                    {
                        if (taskSettings.GeneralSettings.UseCustomTaskCompletedSound && !string.IsNullOrEmpty(taskSettings.GeneralSettings.CustomTaskCompletedSoundPath))
                        {
                            Helpers.PlaySoundAsync(taskSettings.GeneralSettings.CustomTaskCompletedSoundPath);
                        }
                        else
                        {
                            Helpers.PlaySoundAsync(Resources.TaskCompletedSound);
                        }
                    }
                    break;
                case NotificationSound.ActionCompleted:
                    if (taskSettings.GeneralSettings.PlaySoundAfterAction)
                    {
                        if (taskSettings.GeneralSettings.UseCustomActionCompletedSound && !string.IsNullOrEmpty(taskSettings.GeneralSettings.CustomActionCompletedSoundPath))
                        {
                            Helpers.PlaySoundAsync(taskSettings.GeneralSettings.CustomActionCompletedSoundPath);
                        }
                        else
                        {
                            Helpers.PlaySoundAsync(Resources.ActionCompletedSound);
                        }
                    }
                    break;
                case NotificationSound.Error:
                    if (taskSettings.GeneralSettings.PlaySoundAfterUpload)
                    {
                        if (taskSettings.GeneralSettings.UseCustomErrorSound && !string.IsNullOrEmpty(taskSettings.GeneralSettings.CustomErrorSoundPath))
                        {
                            Helpers.PlaySoundAsync(taskSettings.GeneralSettings.CustomErrorSoundPath);
                        }
                        else
                        {
                            Helpers.PlaySoundAsync(Resources.ErrorSound);
                        }
                    }
                    break;
            }
        }

        public static Screenshot GetScreenshot(TaskSettings taskSettings = null)
        {
            if (taskSettings == null) taskSettings = TaskSettings.GetDefaultTaskSettings();

            Screenshot screenshot = new Screenshot()
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

        public static void ImportImageEffect(string filePath)
        {
            string configJson = null;

            try
            {
                configJson = ImageEffectPackager.ExtractPackage(filePath, ShareX.ImageEffectsFolder);
            }
            catch (Exception ex)
            {
                ex.ShowError(false);
            }

            if (!string.IsNullOrEmpty(configJson))
            {
                ImageEffectsForm imageEffectsForm = OpenImageEffectsSingleton(ShareX.DefaultTaskSettings);

                if (imageEffectsForm != null)
                {
                    imageEffectsForm.ImportImageEffect(configJson);
                }

                if (!ShareX.DefaultTaskSettings.AfterCaptureJob.HasFlag(AfterCaptureTasks.AddImageEffects) &&
                    MessageBox.Show(Resources.WouldYouLikeToEnableImageEffects,
                    "ShareX", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    ShareX.DefaultTaskSettings.AfterCaptureJob = ShareX.DefaultTaskSettings.AfterCaptureJob.Add(AfterCaptureTasks.AddImageEffects);
                    ShareX.MainForm.UpdateCheckStates();
                }
            }
        }

        public static async Task HandleNativeMessagingInput(string filePath, TaskSettings taskSettings = null)
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
                                Bitmap bmp = WebHelpers.DataURLToImage(nativeMessagingInput.URL);

                                if (bmp == null && taskSettings.AdvancedSettings.ProcessImagesDuringExtensionUpload)
                                {
                                    try
                                    {
                                        bmp = await WebHelpers.DownloadImageAsync(nativeMessagingInput.URL);
                                    }
                                    catch
                                    {
                                    }
                                }

                                if (bmp != null)
                                {
                                    UploadManager.RunImageTask(bmp, taskSettings);
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

        public static async Task DownloadDevBuild()
        {
            GitHubUpdateChecker updateChecker = new GitHubUpdateChecker("ShareX", "DevBuilds")
            {
                IsDev = true,
                IsPortable = ShareX.Portable
            };

            await updateChecker.CheckUpdateAsync();

            if (updateChecker.Status == UpdateStatus.UpdateAvailable)
            {
                UpdateMessageBox.Start(updateChecker);
            }
            else if (updateChecker.Status == UpdateStatus.UpToDate)
            {
                MessageBox.Show(Resources.ShareXIsUpToDate, "ShareX", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        public static async Task DownloadAppVeyorBuild()
        {
            AppVeyorUpdateChecker updateChecker = new AppVeyorUpdateChecker()
            {
                IsDev = true,
                IsPortable = ShareX.Portable,
                Branch = "develop"
            };

            await updateChecker.CheckUpdateAsync();

            UpdateMessageBox.Start(updateChecker);
        }

        public static Image GenerateQRCode(string text, int size)
        {
            if (CheckQRCodeContent(text))
            {
                try
                {
                    BarcodeWriter writer = new BarcodeWriter
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
                        Renderer = new BitmapRenderer()
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

        public static string[] BarcodeScan(Bitmap bmp, bool scanQRCodeOnly = false)
        {
            try
            {
                BarcodeReader barcodeReader = new BarcodeReader
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

        public static void ShowBalloonTip(string text, ToolTipIcon icon, int timeout, string title = "ShareX", BalloonTipAction clickAction = null)
        {
            if (ShareX.MainForm != null && !ShareX.MainForm.IsDisposed && ShareX.MainForm.niTray != null && ShareX.MainForm.niTray.Visible)
            {
                ShareX.MainForm.niTray.Tag = clickAction;
                ShareX.MainForm.niTray.ShowBalloonTip(timeout, title, text, icon);
            }
        }

        public static void ShowNotificationTip(string text, string title = "ShareX", int duration = -1)
        {
            if (duration < 0)
            {
                duration = (int)(ShareX.DefaultTaskSettings.GeneralSettings.ToastWindowDuration * 1000);
            }

            NotificationFormConfig toastConfig = new NotificationFormConfig()
            {
                Duration = duration,
                FadeDuration = (int)(ShareX.DefaultTaskSettings.GeneralSettings.ToastWindowFadeDuration * 1000),
                Placement = ShareX.DefaultTaskSettings.GeneralSettings.ToastWindowPlacement,
                Size = ShareX.DefaultTaskSettings.GeneralSettings.ToastWindowSize,
                Title = title,
                Text = text
            };

            ShareX.MainForm.InvokeSafe(() =>
            {
                NotificationForm.Show(toastConfig);
            });
        }

        public static void ToggleTrayMenu()
        {
            ContextMenuStrip cmsTray = ShareX.MainForm.niTray.ContextMenuStrip;

            if (cmsTray != null && !cmsTray.IsDisposed)
            {
                if (cmsTray.Visible)
                {
                    cmsTray.Close();
                }
                else
                {
                    NativeMethods.SetForegroundWindow(cmsTray.Handle);
                    cmsTray.Show(Cursor.Position);
                }
            }
        }

        public static bool IsUploadAllowed()
        {
            if (SystemOptions.DisableUpload)
            {
                MessageBox.Show(Resources.YourSystemAdminDisabledTheUploadFeature, "ShareX", MessageBoxButtons.OK, MessageBoxIcon.Information);

                return false;
            }

            if (ShareX.Settings.DisableUpload)
            {
                MessageBox.Show(Resources.ThisFeatureWillNotWorkWhenDisableUploadOptionIsEnabled, "ShareX", MessageBoxButtons.OK, MessageBoxIcon.Information);

                return false;
            }

            return true;
        }
    }
}
