﻿#region License Information (GPL v3)

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

using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Web;
using ShareX.Core.Task;
using ShareX.Core.Utils;
using ShareX.Core.Utils.Extensions;
using ShareX.Core.Utils.Miscellaneous;
using ShareX.Core.Utils.Native;

namespace ShareX.Core.Upload
{
    public static class UploadManager
    {
        public static void UploadFile(string filePath, TaskSettings taskSettings = null)
        {
            if (taskSettings == null) taskSettings = TaskSettings.GetDefaultTaskSettings();

            if (!string.IsNullOrEmpty(filePath))
            {
                if (System.IO.File.Exists(filePath))
                {
                    WorkerTask task = WorkerTask.CreateFileUploaderTask(filePath, taskSettings);
                    TaskManager.Start(task);
                }
                else if (Directory.Exists(filePath))
                {
                    string[] files = Directory.GetFiles(filePath, "*.*", SearchOption.AllDirectories);
                    UploadFile(files, taskSettings);
                }
            }
        }

        public static void UploadFile(string[] files, TaskSettings taskSettings = null)
        {
            if (taskSettings == null) taskSettings = TaskSettings.GetDefaultTaskSettings();

            if (files != null && files.Length > 0)
            {
                if (files.Length <= 10 || IsUploadConfirmed(files.Length))
                {
                    foreach (string file in files)
                    {
                        UploadFile(file, taskSettings);
                    }
                }
            }
        }

        private static bool IsUploadConfirmed(int length)
        {
            if (ShareX.Settings.ShowMultiUploadWarning)
            {
                // using (MyMessageBox msgbox = new MyMessageBox(string.Format(Resources.UploadManager_IsUploadConfirmed_Are_you_sure_you_want_to_upload__0__files_, length),
                //     "ShareX - " + Resources.UploadManager_IsUploadConfirmed_Upload_files,
                //     MessageBoxButtons.YesNo, Resources.UploadManager_IsUploadConfirmed_Don_t_show_this_message_again_))
                // {
                //     msgbox.ShowDialog();
                //     ShareX.Settings.ShowMultiUploadWarning = !msgbox.IsChecked;
                //     return msgbox.DialogResult == DialogResult.Yes;
                // }
            }

            return true;
        }

        public static void UploadFile(TaskSettings taskSettings = null)
        {
            // using (OpenFileDialog ofd = new OpenFileDialog())
            // {
            //     ofd.Title = "ShareX - " + Resources.UploadManager_UploadFile_File_upload;
            //
            //     if (!string.IsNullOrEmpty(ShareX.Settings.FileUploadDefaultDirectory) && Directory.Exists(ShareX.Settings.FileUploadDefaultDirectory))
            //     {
            //         ofd.InitialDirectory = ShareX.Settings.FileUploadDefaultDirectory;
            //     }
            //     else
            //     {
            //         ofd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            //     }
            //
            //     ofd.Multiselect = true;
            //
            //     if (ofd.ShowDialog() == DialogResult.OK)
            //     {
            //         if (!string.IsNullOrEmpty(ofd.FileName))
            //         {
            //             ShareX.Settings.FileUploadDefaultDirectory = Path.GetDirectoryName(ofd.FileName);
            //         }
            //
            //         UploadFile(ofd.FileNames, taskSettings);
            //     }
            // }
        }

        public static void UploadFolder(TaskSettings taskSettings = null)
        {
            // using (FolderSelectDialog folderDialog = new FolderSelectDialog())
            // {
            //     folderDialog.Title = "ShareX - " + Resources.UploadManager_UploadFolder_Folder_upload;
            //
            //     if (!string.IsNullOrEmpty(ShareX.Settings.FileUploadDefaultDirectory) && Directory.Exists(ShareX.Settings.FileUploadDefaultDirectory))
            //     {
            //         folderDialog.InitialDirectory = ShareX.Settings.FileUploadDefaultDirectory;
            //     }
            //     else
            //     {
            //         folderDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            //     }
            //
            //     if (folderDialog.ShowDialog() && !string.IsNullOrEmpty(folderDialog.FileName))
            //     {
            //         ShareX.Settings.FileUploadDefaultDirectory = folderDialog.FileName;
            //         UploadFile(folderDialog.FileName, taskSettings);
            //     }
            // }
        }

        public static void ProcessImageUpload(SixLabors.ImageSharp.Image image, TaskSettings taskSettings)
        {
            if (image != null)
            {
                if (!taskSettings.AdvancedSettings.ProcessImagesDuringClipboardUpload)
                {
                    taskSettings.AfterCaptureJob = AfterCaptureTasks.UploadImageToHost;
                }

                RunImageTask(image, taskSettings);
            }
        }

        public static void ProcessTextUpload(string text, TaskSettings taskSettings)
        {
            if (!string.IsNullOrEmpty(text))
            {
                string url = text.Trim();

                if (URLHelpers.IsValidURL(url))
                {
                    if (taskSettings.UploadSettings.ClipboardUploadURLContents)
                    {
                        DownloadAndUploadFile(url, taskSettings);
                        return;
                    }

                    if (taskSettings.UploadSettings.ClipboardUploadShortenURL)
                    {
                        ShortenURL(url, taskSettings);
                        return;
                    }

                    if (taskSettings.UploadSettings.ClipboardUploadShareURL)
                    {
                        ShareURL(url, taskSettings);
                        return;
                    }
                }

                if (taskSettings.UploadSettings.ClipboardUploadAutoIndexFolder && text.Length <= 260 && Directory.Exists(text))
                {
                    IndexFolder(text, taskSettings);
                }
                else
                {
                    UploadText(text, taskSettings, true);
                }
            }
        }

        public static void ProcessFilesUpload(string[] files, TaskSettings taskSettings)
        {
            if (files != null && files.Length > 0)
            {
                UploadFile(files, taskSettings);
            }
        }

        public static void ClipboardUpload(TaskSettings taskSettings = null)
        {
            if (taskSettings == null) taskSettings = TaskSettings.GetDefaultTaskSettings();

            try
            {
                if (Clipboard.ContainsImage())
                {
                    SixLabors.ImageSharp.Image image;


                    image = Clipboard.GetImage();


                    ProcessImageUpload(image, taskSettings);
                }
                else if (Clipboard.ContainsText())
                {
                    string text = Clipboard.GetText();

                    ProcessTextUpload(text, taskSettings);
                }
                else if (Clipboard.ContainsFileDropList())
                {
                    string[] files = Clipboard.GetFileDropList().Cast<string>().ToArray();

                    ProcessFilesUpload(files, taskSettings);
                }
            }
            catch (ExternalException e)
            {
                DebugHelper.WriteException(e);
                // Basic retries. Should use Polly Nuget package
                ClipboardUpload(taskSettings);

            }
            catch (Exception e)
            {
                DebugHelper.WriteException(e);
            }
        }

        public static void ClipboardUploadWithContentViewer(TaskSettings taskSettings = null)
        {
            if (taskSettings == null) taskSettings = TaskSettings.GetDefaultTaskSettings();

            using (ClipboardUploadForm clipboardUploadForm = new ClipboardUploadForm(taskSettings))
            {
                clipboardUploadForm.ShowDialog();
            }
        }

        public static void ClipboardUploadMainWindow(TaskSettings taskSettings = null)
        {
            if (taskSettings == null) taskSettings = TaskSettings.GetDefaultTaskSettings();

            ClipboardUpload(taskSettings);

        }

        public static void ShowTextUploadDialog(TaskSettings taskSettings = null)
        {
            if (taskSettings == null) taskSettings = TaskSettings.GetDefaultTaskSettings();

            using (TextUploadForm form = new TextUploadForm())
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    string text = form.Content;

                    if (!string.IsNullOrEmpty(text))
                    {
                        UploadText(text, taskSettings);
                    }
                }
            }
        }

        public static void DragDropUpload(IDataObject data, TaskSettings taskSettings = null)
        {
            if (taskSettings == null) taskSettings = TaskSettings.GetDefaultTaskSettings();

            if (data.GetDataPresent(DataFormats.FileDrop, false))
            {
                string[] files = data.GetData(DataFormats.FileDrop, false) as string[];
                UploadFile(files, taskSettings);
            }
            else if (data.GetDataPresent(DataFormats.Bitmap, false))
            {
                Bitmap bmp = data.GetData(DataFormats.Bitmap, false) as Bitmap;
                RunImageTask(bmp, taskSettings);
            }
            else if (data.GetDataPresent(DataFormats.Text, false))
            {
                string text = data.GetData(DataFormats.Text, false) as string;
                UploadText(text, taskSettings, true);
            }
        }

        public static void UploadURL(TaskSettings taskSettings = null)
        {
            if (taskSettings == null) taskSettings = TaskSettings.GetDefaultTaskSettings();

            string inputText = null;

            string text = ClipboardHelpers.GetText(true);

            if (URLHelpers.IsValidURL(text))
            {
                inputText = text;
            }

            string url = InputBox.Show(Resources.UploadManager_UploadURL_URL_to_download_from_and_upload, inputText);

            if (!string.IsNullOrEmpty(url))
            {
                DownloadAndUploadFile(url, taskSettings);
            }
        }

        public static void RunImageTask(SixLabors.ImageSharp.Image image, TaskSettings taskSettings, bool skipQuickTaskMenu = false, bool skipAfterCaptureWindow = false)
        {
            TaskMetadata metadata = new TaskMetadata(image);
            RunImageTask(metadata, taskSettings, skipQuickTaskMenu, skipAfterCaptureWindow);
        }

        public static void RunImageTask(TaskMetadata metadata, TaskSettings taskSettings, bool skipQuickTaskMenu = false, bool skipAfterCaptureWindow = false)
        {
            if (taskSettings == null) taskSettings = TaskSettings.GetDefaultTaskSettings();

            if (metadata != null && metadata.Image != null && taskSettings != null)
            {
                if (!skipQuickTaskMenu && taskSettings.AfterCaptureJob.HasFlag(AfterCaptureTasks.ShowQuickTaskMenu))
                {
                    RunImageTask(metadata, taskSettings, true);
                    return;
                }

                string customFileName = null;

                if (!skipAfterCaptureWindow && !TaskHelpers.ShowAfterCaptureForm(taskSettings, out customFileName, metadata))
                {
                    return;
                }

                WorkerTask task = WorkerTask.CreateImageUploaderTask(metadata, taskSettings, customFileName);
                TaskManager.Start(task);
            }
        }

        public static void UploadImage(SixLabors.ImageSharp.Image image, TaskSettings taskSettings = null)
        {
            if (image != null)
            {
                if (taskSettings == null)
                {
                    taskSettings = TaskSettings.GetDefaultTaskSettings();
                }

                if (taskSettings.IsSafeTaskSettings)
                {
                    taskSettings.UseDefaultAfterCaptureJob = false;
                    taskSettings.AfterCaptureJob = AfterCaptureTasks.UploadImageToHost;
                }

                RunImageTask(image, taskSettings);
            }
        }

        public static void UploadImage(SixLabors.ImageSharp.Image image, ImageDestination imageDestination, FileDestination imageFileDestination, TaskSettings taskSettings = null)
        {
            if (image != null)
            {
                if (taskSettings == null)
                {
                    taskSettings = TaskSettings.GetDefaultTaskSettings();
                }

                if (taskSettings.IsSafeTaskSettings)
                {
                    taskSettings.UseDefaultAfterCaptureJob = false;
                    taskSettings.AfterCaptureJob = AfterCaptureTasks.UploadImageToHost;
                    taskSettings.UseDefaultDestinations = false;
                    taskSettings.ImageDestination = imageDestination;
                    taskSettings.ImageFileDestination = imageFileDestination;
                }

                RunImageTask(image, taskSettings);
            }
        }

        public static void UploadText(string text, TaskSettings taskSettings = null, bool allowCustomText = false)
        {
            if (taskSettings == null) taskSettings = TaskSettings.GetDefaultTaskSettings();

            if (!string.IsNullOrEmpty(text))
            {
                if (allowCustomText)
                {
                    string input = taskSettings.AdvancedSettings.TextCustom;

                    if (!string.IsNullOrEmpty(input))
                    {
                        if (taskSettings.AdvancedSettings.TextCustomEncodeInput)
                        {
                            text = HttpUtility.HtmlEncode(text);
                        }

                        text = input.Replace("%input", text);
                    }
                }

                WorkerTask task = WorkerTask.CreateTextUploaderTask(text, taskSettings);
                TaskManager.Start(task);
            }
        }

        public static void UploadImageStream(Stream stream, string fileName, TaskSettings taskSettings = null)
        {
            if (taskSettings == null) taskSettings = TaskSettings.GetDefaultTaskSettings();

            if (stream != null && stream.Length > 0 && !string.IsNullOrEmpty(fileName))
            {
                WorkerTask task = WorkerTask.CreateDataUploaderTask(EDataType.Image, stream, fileName, taskSettings);
                TaskManager.Start(task);
            }
        }

        public static void ShortenURL(string url, TaskSettings taskSettings = null)
        {
            if (!string.IsNullOrEmpty(url))
            {
                if (taskSettings == null) taskSettings = TaskSettings.GetDefaultTaskSettings();

                WorkerTask task = WorkerTask.CreateURLShortenerTask(url, taskSettings);
                TaskManager.Start(task);
            }
        }

        public static void ShortenURL(string url, UrlShortenerType urlShortener)
        {
            if (!string.IsNullOrEmpty(url))
            {
                TaskSettings taskSettings = TaskSettings.GetDefaultTaskSettings();
                taskSettings.URLShortenerDestination = urlShortener;

                WorkerTask task = WorkerTask.CreateURLShortenerTask(url, taskSettings);
                TaskManager.Start(task);
            }
        }

        public static void ShareURL(string url, TaskSettings taskSettings = null)
        {
            if (!string.IsNullOrEmpty(url))
            {
                if (taskSettings == null) taskSettings = TaskSettings.GetDefaultTaskSettings();

                WorkerTask task = WorkerTask.CreateShareURLTask(url, taskSettings);
                TaskManager.Start(task);
            }
        }

        public static void ShareURL(string url, URLSharingServices urlSharingService)
        {
            if (!string.IsNullOrEmpty(url))
            {
                TaskSettings taskSettings = TaskSettings.GetDefaultTaskSettings();
                taskSettings.URLSharingServiceDestination = urlSharingService;

                WorkerTask task = WorkerTask.CreateShareURLTask(url, taskSettings);
                TaskManager.Start(task);
            }
        }

        public static void DownloadFile(string url, TaskSettings taskSettings = null)
        {
            DownloadFile(url, false, taskSettings);
        }

        public static void DownloadAndUploadFile(string url, TaskSettings taskSettings = null)
        {
            DownloadFile(url, true, taskSettings);
        }

        private static void DownloadFile(string url, bool upload, TaskSettings taskSettings = null)
        {
            if (!string.IsNullOrEmpty(url))
            {
                if (taskSettings == null) taskSettings = TaskSettings.GetDefaultTaskSettings();

                WorkerTask task = WorkerTask.CreateDownloadTask(url, upload, taskSettings);

                if (task != null)
                {
                    TaskManager.Start(task);
                }
            }
        }

        public static void IndexFolder(TaskSettings taskSettings = null)
        {
            using (FolderSelectDialog dlg = new FolderSelectDialog())
            {
                if (dlg.ShowDialog())
                {
                    IndexFolder(dlg.FileName, taskSettings);
                }
            }
        }

        public static void IndexFolder(string folderPath, TaskSettings taskSettings = null)
        {
            if (!string.IsNullOrEmpty(folderPath) && Directory.Exists(folderPath))
            {
                if (taskSettings == null) taskSettings = TaskSettings.GetDefaultTaskSettings();

                taskSettings.ToolsSettings.IndexerSettings.BinaryUnits = ShareX.Settings.BinaryUnits;

                string source = null;

                Task.Run(() =>
                {
                    source = Indexer.Index(folderPath, taskSettings.ToolsSettings.IndexerSettings);
                }).ContinueInCurrentContext(() =>
                {
                    if (!string.IsNullOrEmpty(source))
                    {
                        WorkerTask task = WorkerTask.CreateTextUploaderTask(source, taskSettings);
                        task.Info.FileName = Path.ChangeExtension(task.Info.FileName, taskSettings.ToolsSettings.IndexerSettings.Output.ToString().ToLower());
                        TaskManager.Start(task);
                    }
                });
            }
        }
    }
}
