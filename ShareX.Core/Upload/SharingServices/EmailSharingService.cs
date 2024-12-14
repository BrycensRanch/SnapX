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

using ShareX.Core.Upload.BaseServices;
using ShareX.Core.Upload.BaseUploaders;
using ShareX.Core.Upload.File;
using ShareX.Core.Upload.Utils;
using ShareX.Core.Utils;

namespace ShareX.Core.Upload.SharingServices;

public class EmailSharingService : URLSharingService
{
    public override URLSharingServices EnumValue => URLSharingServices.Email;

    public override bool CheckConfig(UploadersConfig config)
    {
        return !string.IsNullOrEmpty(config.EmailSmtpServer) && config.EmailSmtpPort > 0 && !string.IsNullOrEmpty(config.EmailFrom) && !string.IsNullOrEmpty(config.EmailPassword);
    }

    public override URLSharer CreateSharer(UploadersConfig config, TaskReferenceHelper taskInfo)
    {
        return new EmailSharer(config);
    }
}

public sealed class EmailSharer : URLSharer
{
    private UploadersConfig config;

    public EmailSharer(UploadersConfig config)
    {
        this.config = config;
    }

    public override UploadResult ShareURL(string url)
    {
        var result = new UploadResult { URL = url, IsURLExpected = false };

        if (config.EmailAutomaticSend && !string.IsNullOrEmpty(config.EmailAutomaticSendTo))
        {
            var email = new Email()
            {
                SmtpServer = config.EmailSmtpServer,
                SmtpPort = config.EmailSmtpPort,
                FromEmail = config.EmailFrom,
                Password = config.EmailPassword,
                ToEmail = config.EmailAutomaticSendTo,
                Subject = config.EmailDefaultSubject,
                Body = url
            };

            email.Send();
        }
        else
        {
            // TODO: Reimplement Email Service
            // I wonder if anyone even uses this shit 😭😭😭
            // using (EmailForm emailForm = new EmailForm(config.EmailRememberLastTo ? config.EmailLastTo : "", config.EmailDefaultSubject, url))
            // {
            //     if (emailForm.ShowDialog() == DialogResult.OK)
            //     {
            //         if (config.EmailRememberLastTo)
            //         {
            //             config.EmailLastTo = emailForm.ToEmail;
            //         }
            //
            //         Email email = new Email()
            //         {
            //             SmtpServer = config.EmailSmtpServer,
            //             SmtpPort = config.EmailSmtpPort,
            //             FromEmail = config.EmailFrom,
            //             Password = config.EmailPassword,
            //             ToEmail = emailForm.ToEmail,
            //             Subject = emailForm.Subject,
            //             Body = emailForm.Body
            //         };
            //
            //         email.Send();
            //     }
            // }
        }

        URLHelpers.OpenURL("mailto:?body=" + URLHelpers.URLEncode(url));

        return result;
    }

}

