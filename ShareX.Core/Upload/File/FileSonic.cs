
// SPDX-License-Identifier: GPL-3.0-or-later


using System.Xml.Linq;
using ShareX.Core.Upload.BaseUploaders;
using ShareX.Core.Utils.Extensions;

namespace ShareX.Core.Upload.File
{
    public class FileSonic : FileUploader
    {
        public string Username { get; set; }

        public string Password { get; set; }

        private const string APIURL = "https://api.filesonic.com/upload";

        public FileSonic(string username, string password)
        {
            Username = username;
            Password = password;
        }

        public override UploadResult Upload(Stream stream, string fileName)
        {
            UploadResult result = null;

            string url = GetUploadURL();

            if (!string.IsNullOrEmpty(url))
            {
                result = SendRequestFile(url, stream, fileName, "file");

                if (!string.IsNullOrEmpty(result.Response))
                {
                    result.URL = result.Response;
                }
            }
            else
            {
                Errors.Add("GetUploadURL failed.");
            }

            return result;
        }

        public string GetUploadURL()
        {
            Dictionary<string, string> args = new Dictionary<string, string>();
            args.Add("method", "getUploadUrl");
            args.Add("format", "xml");
            args.Add("u", Username);
            args.Add("p", Password);

            string response = SendRequest(HttpMethod.Get, APIURL, args);

            XDocument xd = XDocument.Parse(response);
            return xd.GetValue("FSApi_Upload/getUploadUrl/response/url");
        }
    }
}
