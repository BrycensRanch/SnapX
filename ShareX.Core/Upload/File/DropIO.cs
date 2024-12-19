
// SPDX-License-Identifier: GPL-3.0-or-later


using System.Xml.Linq;
using ShareX.Core.Upload.BaseUploaders;
using ShareX.Core.Utils;
using ShareX.Core.Utils.Extensions;

namespace ShareX.Core.Upload.File
{
    public sealed class DropIO : FileUploader
    {
        public string DropName { get; set; }
        public string DropDescription { get; set; }

        public class Asset
        {
            public string Name { get; set; }
            public string OriginalFilename { get; set; }
        }

        public class Drop
        {
            public string Name { get; set; }
            public string AdminToken { get; set; }
        }

        private string APIKey;

        public DropIO(string apiKey)
        {
            APIKey = apiKey;
        }

        public override UploadResult Upload(Stream stream, string fileName)
        {
            DropName = "ShareX_" + Helpers.GetRandomAlphanumeric(10);
            DropDescription = "";
            Drop drop = CreateDrop(DropName, DropDescription, false, false, false);

            Dictionary<string, string> args = new Dictionary<string, string>();
            args.Add("version", "2.0");
            args.Add("api_key", APIKey);
            args.Add("format", "xml");
            args.Add("token", drop.AdminToken);
            args.Add("drop_name", drop.Name);

            UploadResult result = SendRequestFile("https://assets.drop.io/upload", stream, fileName, "file", args);

            if (result.IsSuccess)
            {
                Asset asset = ParseAsset(result.Response);
                result.URL = string.Format("https://drop.io/{0}/asset/{1}", drop.Name, asset.Name);
            }

            return result;
        }

        public Asset ParseAsset(string response)
        {
            XDocument doc = XDocument.Parse(response);
            XElement root = doc.Element("asset");
            if (root != null)
            {
                Asset asset = new Asset();
                asset.Name = root.GetElementValue("name");
                asset.OriginalFilename = root.GetElementValue("original-filename");
                return asset;
            }

            return null;
        }

        private Drop CreateDrop(string name, string description, bool guests_can_comment, bool guests_can_add, bool guests_can_delete)
        {
            Dictionary<string, string> args = new Dictionary<string, string>();
            args.Add("version", "2.0");
            args.Add("api_key", APIKey);
            args.Add("format", "xml");
            // this is the name of the drop and will become part of the URL of the drop
            args.Add("name", name);
            // a plain text description of a drop
            args.Add("description", description);
            // determines whether guests can comment on assets
            args.Add("guests_can_comment", guests_can_comment.ToString());
            // determines whether guests can add assets
            args.Add("guests_can_add", guests_can_add.ToString());
            // determines whether guests can delete assets
            args.Add("guests_can_delete", guests_can_delete.ToString());

            string response = SendRequestMultiPart("https://api.drop.io/drops", args);

            XDocument doc = XDocument.Parse(response);
            XElement root = doc.Element("drop");
            if (root != null)
            {
                Drop drop = new Drop();
                drop.Name = root.GetElementValue("name");
                drop.AdminToken = root.GetElementValue("admin_token");
                return drop;
            }

            return null;
        }
    }
}
