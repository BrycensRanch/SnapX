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

using System.Net;
using System.Reflection;
using System.Text;
using ShareX.Core.Utils;

namespace ShareX.Core.Upload.OAuth;

public class OAuthListener : IDisposable
{
    public IOAuth2Loopback OAuth { get; private set; }

    private HttpListener listener;

    public OAuthListener(IOAuth2Loopback oauth)
    {
        OAuth = oauth;
    }

    public void Dispose()
    {
        if (listener != null)
        {
            listener.Close();
            listener = null;
        }
    }

    public async Task<bool> ConnectAsync()
    {
        Dispose();

        var ip = IPAddress.Loopback;
        var port = WebHelpers.GetRandomUnusedPort();
        var redirectURI = $"http://{ip}:{port}/";
        var state = Helpers.GetRandomAlphanumeric(32);

        OAuth.RedirectURI = redirectURI;
        OAuth.State = state;

        var url = OAuth.GetAuthorizationURL();

        if (string.IsNullOrEmpty(url))
        {
            DebugHelper.WriteLine("Authorization URL is empty.");
            return false;
        }

        URLHelpers.OpenURL(url);
        DebugHelper.WriteLine("Authorization URL is opened: " + url);

        try
        {
            using var listener = new HttpListener();
            listener.Prefixes.Add(redirectURI);
            listener.Start();

            var context = await listener.GetContextAsync();
            var queryCode = context.Request.QueryString.Get("code");
            var queryState = context.Request.QueryString.Get("state");

            using var response = context.Response;
            var status = (queryState == state && !string.IsNullOrEmpty(queryCode))
                ? "Authorization completed successfully."
                : queryState != state
                    ? "Invalid state parameter."
                    : "Authorization did not succeed.";

            var assembly = Assembly.GetExecutingAssembly();
            await using var stream = assembly.GetManifestResourceStream("OAuthCallbackPage.html");
            if (stream == null || stream.Length == 0) return false;
            using var reader = new StreamReader(stream);
            var oAuthCallbackPage = reader.ReadToEnd();
            var responseText = oAuthCallbackPage.Replace("{0}", status);
            var buffer = Encoding.UTF8.GetBytes(responseText);

            response.ContentLength64 = buffer.Length;
            response.KeepAlive = false;

            await using var responseOutput = response.OutputStream;
            await responseOutput.WriteAsync(buffer, 0, buffer.Length);
            await responseOutput.FlushAsync();

            if (queryState == state && !string.IsNullOrEmpty(queryCode))
            {
                return await System.Threading.Tasks.Task.Run(() => OAuth.GetAccessToken(queryCode));
            }
        }
        catch (ObjectDisposedException)
        {
            // Listener is DISPOSED.
        }
        finally
        {
            Dispose();
        }

        return false;
    }
}

