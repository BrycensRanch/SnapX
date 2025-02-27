
// SPDX-License-Identifier: GPL-3.0-or-later


using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using FluentFTP;
using FluentFTP.Exceptions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SnapX.Core.Upload.BaseServices;
using SnapX.Core.Upload.BaseUploaders;
using SnapX.Core.Upload.Utils;
using SnapX.Core.Utils;
using SnapX.Core.Utils.Extensions;
using SnapX.Core.Utils.Parsers;

namespace SnapX.Core.Upload.File;

public class FTPFileUploaderService : FileUploaderService
{
    public override FileDestination EnumValue { get; } = FileDestination.FTP;

    public override bool CheckConfig(UploadersConfig config)
    {
        return config.FTPAccountList != null && config.FTPAccountList.IsValidIndex(config.FTPSelectedFile);
    }

    public override GenericUploader CreateUploader(UploadersConfig config, TaskReferenceHelper taskInfo)
    {
        int index;

        if (taskInfo.OverrideFTP)
        {
            index = taskInfo.FTPIndex.BetweenOrDefault(0, config.FTPAccountList.Count - 1);
        }
        else
        {
            switch (taskInfo.DataType)
            {
                case EDataType.Image:
                    index = config.FTPSelectedImage;
                    break;
                case EDataType.Text:
                    index = config.FTPSelectedText;
                    break;
                default:
                case EDataType.File:
                    index = config.FTPSelectedFile;
                    break;
            }
        }

        var account = config.FTPAccountList.ReturnIfValidIndex(index);

        if (account != null)
        {
            if (account.Protocol == FTPProtocol.FTP || account.Protocol == FTPProtocol.FTPS)
            {
                return new FTP(account);
            }
            else if (account.Protocol == FTPProtocol.SFTP)
            {
                return new SFTP(account);
            }
        }

        return null;
    }
}

public sealed class FTP : FileUploader, IDisposable
{
    public FTPAccount Account { get; private set; }

    public bool IsConnected
    {
        get
        {
            return client != null && client.IsConnected;
        }
    }

    private FtpClient client;

    public FTP(FTPAccount account)
    {
        Account = account;

        client = new FtpClient()
        {
            Host = Account.Host,
            Port = Account.Port,
            Credentials = new NetworkCredential(Account.Username, Account.Password)
        };

        if (account.IsActive)
        {
            client.Config.DataConnectionType = FtpDataConnectionType.AutoActive;
        }
        else
        {
            client.Config.DataConnectionType = FtpDataConnectionType.AutoPassive;
        }

        if (account.Protocol == FTPProtocol.FTPS)
        {
            switch (Account.FTPSEncryption)
            {
                default:
                case FTPSEncryption.Explicit:
                    client.Config.EncryptionMode = FtpEncryptionMode.Explicit;
                    break;
                case FTPSEncryption.Implicit:
                    client.Config.EncryptionMode = FtpEncryptionMode.Implicit;
                    break;
            }
            // The Operating System knows best about what SSL Protocols it should use with .NET
            // client.Config.SslProtocols = SslProtocols.Tls | SslProtocols.Tls11 | SslProtocols.Tls12;
            client.Config.DataConnectionEncryption = true;

            if (!string.IsNullOrEmpty(account.FTPSCertificateLocation) && System.IO.File.Exists(account.FTPSCertificateLocation))
            {
                var cert = X509CertificateLoader.LoadCertificateFromFile(account.FTPSCertificateLocation);
                client.Config.ClientCertificates.Add(cert);
            }
            else
            {
                client.ValidateCertificate += (control, e) =>
                {
                    if (e.PolicyErrors != SslPolicyErrors.None)
                    {
                        e.Accept = true;
                    }
                };
            }
        }
    }

    public override UploadResult Upload(Stream stream, string fileName)
    {
        UploadResult result = new UploadResult();

        string subFolderPath = Account.GetSubFolderPath(null, NameParserType.FilePath);
        string path = URLHelpers.CombineURL(subFolderPath, fileName);
        string url = Account.GetUriPath(fileName, subFolderPath);

        OnEarlyURLCopyRequested(url);

        try
        {
            IsUploading = true;
            bool uploadResult = UploadData(stream, path);

            if (uploadResult && !StopUploadRequested && !IsError)
            {
                result.URL = url;
            }
        }
        finally
        {
            Dispose();
            IsUploading = false;
        }

        return result;
    }

    public override void StopUpload()
    {
        if (IsUploading && !StopUploadRequested)
        {
            StopUploadRequested = true;

            try
            {
                Disconnect();
            }
            catch (Exception e)
            {
                DebugHelper.WriteException(e);
            }
        }
    }

    public bool Connect()
    {
        if (!client.IsConnected)
        {
            client.Connect();
        }

        return client.IsConnected;
    }

    public void Disconnect()
    {
        if (client != null)
        {
            client.Disconnect();
        }
    }

    public bool UploadData(Stream localStream, string remotePath)
    {
        if (Connect())
        {
            try
            {
                return UploadDataInternal(localStream, remotePath);
            }
            catch (FtpCommandException e)
            {
                // Directly likely doesn't exist, try creating it
                if (e.CompletionCode == "550" || e.CompletionCode == "553")
                {
                    CreateMultiDirectory(URLHelpers.GetDirectoryPath(remotePath));

                    return UploadDataInternal(localStream, remotePath);
                }

                throw;
            }
        }

        return false;
    }

    private bool UploadDataInternal(Stream localStream, string remotePath)
    {
        bool result;
        using (Stream remoteStream = client.OpenWrite(remotePath))
        {
            result = TransferData(localStream, remoteStream);
        }
        FtpReply ftpReply = client.GetReply();
        return result && ftpReply.Success;
    }

    public void UploadData(byte[] data, string remotePath)
    {
        using (MemoryStream stream = new MemoryStream(data, false))
        {
            UploadData(stream, remotePath);
        }
    }

    public void UploadFile(string localPath, string remotePath)
    {
        using (FileStream stream = new FileStream(localPath, FileMode.Open))
        {
            UploadData(stream, remotePath);
        }
    }

    public void UploadImage(Image<Rgba64> image, string remotePath)
    {
        using (MemoryStream stream = new MemoryStream())
        {
            image.Save(remotePath);
            UploadData(stream, remotePath);
        }
    }

    public void UploadText(string text, string remotePath)
    {
        using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(text), false))
        {
            UploadData(stream, remotePath);
        }
    }

    public void UploadFiles(string[] localPaths, string remotePath)
    {
        foreach (string file in localPaths)
        {
            if (!string.IsNullOrEmpty(file))
            {
                string fileName = Path.GetFileName(file);

                if (System.IO.File.Exists(file))
                {
                    UploadFile(file, URLHelpers.CombineURL(remotePath, fileName));
                }
                else if (Directory.Exists(file))
                {
                    List<string> filesList = [.. Directory.GetFiles(file), .. Directory.GetDirectories(file)];
                    string path = URLHelpers.CombineURL(remotePath, fileName);
                    CreateDirectory(path);
                    UploadFiles(filesList.ToArray(), path);
                }
            }
        }
    }

    public void DownloadFile(string remotePath, Stream localStream)
    {
        if (Connect())
        {
            using (Stream remoteStream = client.OpenRead(remotePath))
            {
                TransferData(remoteStream, localStream);
            }
            client.GetReply();
        }
    }

    public void DownloadFile(string remotePath, string localPath)
    {
        using (FileStream fs = new FileStream(localPath, FileMode.Create))
        {
            DownloadFile(remotePath, fs);
        }
    }

    public void DownloadFiles(IEnumerable<FtpListItem> files, string localPath, bool recursive = true)
    {
        foreach (FtpListItem file in files)
        {
            if (file != null && !string.IsNullOrEmpty(file.Name))
            {
                if (recursive && file.Type == FtpObjectType.Directory)
                {
                    FtpListItem[] newFiles = GetListing(file.FullName);
                    string directoryPath = Path.Combine(localPath, file.Name);

                    if (!Directory.Exists(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }

                    DownloadFiles(newFiles, directoryPath);
                }
                else if (file.Type == FtpObjectType.File)
                {
                    string filePath = Path.Combine(localPath, file.Name);
                    DownloadFile(file.FullName, filePath);
                }
            }
        }
    }

    public FtpListItem[] GetListing(string remotePath)
    {
        return client.GetListing(remotePath);
    }

    public bool DirectoryExists(string remotePath)
    {
        if (Connect())
        {
            return client.DirectoryExists(remotePath);
        }

        return false;
    }

    public bool CreateDirectory(string remotePath)
    {
        if (Connect())
        {
            try
            {
                client.CreateDirectory(remotePath);
                return true;
            }
            catch (Exception e)
            {
                DebugHelper.WriteException(e);
            }
        }

        return false;
    }

    public List<string> CreateMultiDirectory(string remotePath)
    {
        List<string> paths = URLHelpers.GetPaths(remotePath);

        foreach (string path in paths)
        {
            if (CreateDirectory(path))
            {
                DebugHelper.WriteLine($"FTP directory created: {path}");
            }
        }

        return paths;
    }

    public void Rename(string fromRemotePath, string toRemotePath)
    {
        if (Connect())
        {
            client.Rename(fromRemotePath, toRemotePath);
        }
    }

    public void DeleteFile(string remotePath)
    {
        if (Connect())
        {
            client.DeleteFile(remotePath);
        }
    }

    public void DeleteFiles(IEnumerable<FtpListItem> files)
    {
        foreach (FtpListItem file in files)
        {
            if (file != null && !string.IsNullOrEmpty(file.Name))
            {
                if (file.Type == FtpObjectType.Directory)
                {
                    DeleteDirectory(file.FullName);
                }
                else if (file.Type == FtpObjectType.File)
                {
                    DeleteFile(file.FullName);
                }
            }
        }
    }

    public void DeleteDirectory(string remotePath)
    {
        if (Connect())
        {
            string fileName = URLHelpers.GetFileName(remotePath);
            if (fileName == "." || fileName == "..")
            {
                return;
            }

            FtpListItem[] files = GetListing(remotePath);

            DeleteFiles(files);

            client.DeleteDirectory(remotePath);
        }
    }

    public bool SendCommand(string command)
    {
        if (Connect())
        {
            try
            {
                client.Execute(command);
                return true;
            }
            catch (Exception e)
            {
                DebugHelper.WriteException(e);
            }
        }

        return false;
    }

    public void Dispose()
    {
        if (client != null)
        {
            try
            {
                client.Dispose();
            }
            catch (Exception e)
            {
                DebugHelper.WriteException(e);
            }
        }
    }
}
