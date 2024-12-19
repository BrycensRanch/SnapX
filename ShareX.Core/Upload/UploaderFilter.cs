
// SPDX-License-Identifier: GPL-3.0-or-later


using ShareX.Core.Upload.BaseServices;
using ShareX.Core.Utils;

namespace ShareX.Core.Upload;

public class UploaderFilter
{
    public string Uploader { get; set; }
    public List<string> Extensions { get; set; } = new();
    //public long Size { get; set; }

    public UploaderFilter()
    {
    }

    public UploaderFilter(string uploader, params string[] extensions)
    {
        Uploader = uploader;
        Extensions = extensions.ToList();
    }

    public bool IsValidFilter(string fileName)
    {
        var extension = FileHelpers.GetFileNameExtension(fileName);

        return !string.IsNullOrEmpty(extension) && Extensions.Any(x => x.TrimStart('.').Equals(extension, StringComparison.OrdinalIgnoreCase));
    }

    public IGenericUploaderService GetUploaderService()
    {
        return UploaderFactory.AllGenericUploaderServices.FirstOrDefault(x => x.ServiceIdentifier.Equals(Uploader, StringComparison.OrdinalIgnoreCase));
    }

    public void SetExtensions(string extensions)
    {
        Extensions = string.IsNullOrEmpty(extensions)
            ? new List<string>()
            : extensions.Split(',')
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrEmpty(x))
                .ToList();
    }

    public string GetExtensions()
    {
        return string.Join(", ", Extensions);
    }

    public override string ToString()
    {
        return Uploader;
    }
}

