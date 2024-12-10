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
using System;
using System.ComponentModel;

namespace ShareX
{
    public enum ShareXBuild
    {
        Debug,
        Release,
        Unknown
    }

    public enum UpdateChannel // Localized
    {
        Release,
        PreRelease,
        Dev
    }

    public enum SupportedLanguage
    {
        Automatic, // Localized
        [Description("Nederlands (Dutch)")]
        Dutch,
        [Description("English")]
        English,
        [Description("Français (French)")]
        French,
        [Description("Deutsch (German)")]
        German,
        [Description("עִברִית (Hebrew)")]
        Hebrew,
        [Description("Magyar (Hungarian)")]
        Hungarian,
        [Description("Bahasa Indonesia (Indonesian)")]
        Indonesian,
        [Description("Italiano (Italian)")]
        Italian,
        [Description("日本語 (Japanese)")]
        Japanese,
        [Description("한국어 (Korean)")]
        Korean,
        [Description("Español mexicano (Mexican Spanish)")]
        MexicanSpanish,
        [Description("فارسی (Persian)")]
        Persian,
        [Description("Polski (Polish)")]
        Polish,
        [Description("Português (Portuguese)")]
        Portuguese,
        [Description("Português-Brasil (Portuguese-Brazil)")]
        PortugueseBrazil,
        [Description("Română (Romanian)")]
        Romanian,
        [Description("Русский (Russian)")]
        Russian,
        [Description("简体中文 (Simplified Chinese)")]
        SimplifiedChinese,
        [Description("Español (Spanish)")]
        Spanish,
        [Description("繁體中文 (Traditional Chinese)")]
        TraditionalChinese,
        [Description("Türkçe (Turkish)")]
        Turkish,
        [Description("Українська (Ukrainian)")]
        Ukrainian,
        [Description("Tiếng Việt (Vietnamese)")]
        Vietnamese
    }

    public enum TaskJob
    {
        Job,
        DataUpload,
        FileUpload,
        TextUpload,
        ShortenURL,
        ShareURL,
        Download,
        DownloadUpload
    }

    public enum TaskStatus
    {
        InQueue,
        Preparing,
        Working,
        Stopping,
        Stopped,
        Failed,
        Completed,
        History
    }

    [Flags]
    public enum AfterCaptureTasks // Localized
    {
        None = 0,
        ShowQuickTaskMenu = 1,
        ShowAfterCaptureWindow = 1 << 1,
        BeautifyImage = 1 << 2,
        AddImageEffects = 1 << 3,
        AnnotateImage = 1 << 4,
        CopyImageToClipboard = 1 << 5,
        PinToScreen = 1 << 6,
        SendImageToPrinter = 1 << 7,
        SaveImageToFile = 1 << 8,
        SaveImageToFileWithDialog = 1 << 9,
        SaveThumbnailImageToFile = 1 << 10,
        PerformActions = 1 << 11,
        CopyFileToClipboard = 1 << 12,
        CopyFilePathToClipboard = 1 << 13,
        ShowInExplorer = 1 << 14,
        ScanQRCode = 1 << 15,
        DoOCR = 1 << 16,
        ShowBeforeUploadWindow = 1 << 17,
        UploadImageToHost = 1 << 18,
        DeleteFile = 1 << 19
    }

    [Flags]
    public enum AfterUploadTasks // Localized
    {
        None = 0,
        ShowAfterUploadWindow = 1,
        UseURLShortener = 1 << 1,
        ShareURL = 1 << 2,
        CopyURLToClipboard = 1 << 3,
        OpenURL = 1 << 4,
        ShowQRCode = 1 << 5
    }

    public enum CaptureType
    {
        Fullscreen,
        Monitor,
        ActiveMonitor,
        Window,
        ActiveWindow,
        Region,
        CustomRegion,
        LastRegion
    }

    public enum ScreenRecordStartMethod
    {
        Region,
        ActiveWindow,
        CustomRegion,
        LastRegion
    }

    public enum HotkeyType // Localized
    {
        None,
        ExitShareX
    }

    public enum ToastClickAction // Localized
    {
        CloseNotification,
        AnnotateImage,
        CopyImageToClipboard,
        CopyFile,
        CopyFilePath,
        CopyUrl,
        OpenFile,
        OpenFolder,
        OpenUrl,
        Upload,
        PinToScreen
    }

    public enum ThumbnailViewClickAction // Localized
    {
        Default,
        Select,
        OpenImageViewer,
        OpenFile,
        OpenFolder,
        OpenURL,
        EditImage
    }

    public enum FileExistAction // Localized
    {
        Ask,
        Overwrite,
        UniqueName,
        Cancel
    }

    public enum ImagePreviewVisibility // Localized
    {
        Show, Hide, Automatic
    }

    public enum ImagePreviewLocation // Localized
    {
        Side, Bottom
    }

    public enum ThumbnailTitleLocation // Localized
    {
        Top, Bottom
    }

    public enum RegionCaptureType
    {
        Default, Light, Transparent
    }

    public enum StartupState
    {
        Disabled = StartupTaskState.Disabled,
        DisabledByUser = StartupTaskState.DisabledByUser,
        Enabled = StartupTaskState.Enabled,
        DisabledByPolicy = StartupTaskState.DisabledByPolicy,
        EnabledByPolicy = StartupTaskState.EnabledByPolicy
    }

    public enum BalloonTipClickAction
    {
        None,
        OpenURL,
        OpenDebugLog
    }

    public enum TaskViewMode // Localized
    {
        ListView,
        ThumbnailView
    }

    public enum NativeMessagingAction
    {
        None,
        UploadImage,
        UploadVideo,
        UploadAudio,
        UploadText,
        ShortenURL
    }

    public enum NotificationSound
    {
        Capture,
        TaskCompleted,
        ActionCompleted,
        Error
    }
        public enum EDataType // Localized
    {
        Default,
        File,
        Image,
        Text,
        URL
    }

    public enum PNGBitDepth // Localized
    {
        Default,
        Automatic,
        Bit32,
        Bit24
    }

    public enum GIFQuality // Localized
    {
        Default,
        Bit8,
        Bit4,
        Grayscale
    }

    public enum EImageFormat
    {
        [Description("png")]
        PNG,
        [Description("jpg")]
        JPEG,
        [Description("gif")]
        GIF,
        [Description("bmp")]
        BMP,
        [Description("tif")]
        TIFF
    }

    public enum HashType
    {
        [Description("CRC-32")]
        CRC32,
        [Description("MD5")]
        MD5,
        [Description("SHA-1")]
        SHA1,
        [Description("SHA-256")]
        SHA256,
        [Description("SHA-384")]
        SHA384,
        [Description("SHA-512")]
        SHA512
    }

    public enum BorderType
    {
        Outside,
        Inside
    }

    public enum DownloaderFormStatus
    {
        Waiting,
        DownloadStarted,
        DownloadCompleted,
        InstallStarted
    }

    public enum InstallType
    {
        Default,
        Silent,
        VerySilent,
        Event
    }

    public enum ReleaseChannelType
    {
        [Description("Stable version")]
        Stable,
        [Description("Beta version")]
        Beta,
        [Description("Dev version")]
        Dev
    }

    public enum UpdateStatus
    {
        None,
        UpdateCheckFailed,
        UpdateAvailable,
        UpToDate
    }

    public enum PrintType
    {
        Image,
        Text
    }

    public enum DrawStyle
    {
        Hue,
        Saturation,
        Brightness,
        Red,
        Green,
        Blue
    }

    public enum ColorType
    {
        None, RGBA, HSB, CMYK, Hex, Decimal
    }

    public enum ColorFormat
    {
        RGB, RGBA, ARGB
    }

    public enum ProxyMethod // Localized
    {
        None,
        Manual,
        Automatic
    }

    public enum SlashType
    {
        Prefix,
        Suffix
    }

    public enum ScreenTearingTestMode
    {
        VerticalLines,
        HorizontalLines
    }

    public enum HotkeyStatus
    {
        Registered,
        Failed,
        NotConfigured
    }

    public enum ImageCombinerAlignment
    {
        LeftOrTop,
        Center,
        RightOrBottom
    }

    public enum ImageInterpolationMode
    {
        HighQualityBicubic,
        Bicubic,
        HighQualityBilinear,
        Bilinear,
        NearestNeighbor
    }

    public enum ArrowHeadDirection // Localized
    {
        End,
        Start,
        Both
    }

    public enum FFmpegArchitecture
    {
        win64,
        win32,
        macos64
    }

    public enum StepType // Localized
    {
        Numbers,
        LettersUppercase,
        LettersLowercase,
        RomanNumeralsUppercase,
        RomanNumeralsLowercase
    }

    public enum CutOutEffectType // Localized
    {
        None,
        ZigZag,
        TornEdge,
        Wave
    }
}
