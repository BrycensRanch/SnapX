
// SPDX-License-Identifier: GPL-3.0-or-later


using SnapX.Core.Upload.BaseServices;
using SnapX.Core.Utils;

namespace SnapX.Core.Upload;

public static class UploaderFactory
{
    public static List<IUploaderService> AllServices { get; } = new List<IUploaderService>();
    public static List<IGenericUploaderService> AllGenericUploaderServices { get; } = new List<IGenericUploaderService>();
    public static Dictionary<ImageDestination, ImageUploaderService> ImageUploaderServices { get; } = CacheServices<ImageDestination, ImageUploaderService>();
    public static Dictionary<TextDestination, TextUploaderService> TextUploaderServices { get; } = CacheServices<TextDestination, TextUploaderService>();
    public static Dictionary<FileDestination, FileUploaderService> FileUploaderServices { get; } = CacheServices<FileDestination, FileUploaderService>();
    public static Dictionary<UrlShortenerType, URLShortenerService> URLShortenerServices { get; } = CacheServices<UrlShortenerType, URLShortenerService>();
    public static Dictionary<URLSharingServices, URLSharingService> URLSharingServices { get; } = CacheServices<URLSharingServices, URLSharingService>();

    private static Dictionary<T, T2> CacheServices<T, T2>() where T2 : UploaderService<T>
    {
        var instances = Helpers.GetInstances<T2>();

        AllServices.AddRange(instances.OfType<IUploaderService>());
        AllGenericUploaderServices.AddRange(instances.OfType<IGenericUploaderService>());

        return instances.ToDictionary(x => x.EnumValue, x => x);
    }
}

