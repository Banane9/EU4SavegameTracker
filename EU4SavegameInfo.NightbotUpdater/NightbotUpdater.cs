using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace EU4SavegameInfo.NightbotUpdater
{
    internal sealed class NightbotUpdater
    {
        private readonly HttpClient httpClient = new HttpClient();
        private readonly Settings settings;

        public NightbotUpdater(Settings settings)
        {
            this.settings = settings;
        }

        public async Task<bool> Connect()
        {
            if (settings.AccessToken != null && settings.ExpiresAt > DateTime.Now.AddHours(1))
                return true;

            if (settings.RefreshToken != null && settings.ExpiresAt.AddDays(30) > DateTime.Now)
                if (await refreshAccessToken())
                    return true;

            var browserWindow = new OAuthBrowserWindow();
            browserWindow.ShowDialog();

            var authorizeCode = browserWindow.DataContext as string;
            if (authorizeCode == null || string.IsNullOrWhiteSpace(authorizeCode))
                return false;

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.nightbot.tv/oauth2/token");

            var formContent = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "client_id", "cac06b4cc0205a7fa9c4f223d42e19e7" },
                { "client_secret", "74c9cf88e9bdccbb37aa8807e525cf65" },
                { "code", authorizeCode },
                { "grant_type", "authorization_code" },
                { "redirect_uri", "https://banane9.github.io/eu4savegameauthenticate" }
            });

            request.Content = formContent;

            var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);

            if (!response.IsSuccessStatusCode)
                return false;

            var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(await response.Content.ReadAsStringAsync());

            if (!tokenResponse.Scope.Contains("commands"))
                return false;

            settings.Update(tokenResponse);

            return true;
        }

        public async Task<bool> GetIsConnected()
        {
            return settings.AccessToken != null && (settings.ExpiresAt > DateTime.Now || await Connect());
        }

        public async Task<bool> RevokeAccess()
        {
            if (settings.AccessToken == null)
                return true;

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.nightbot.tv/oauth2/token/revoke");

            var formContent = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "token", settings.AccessToken },
            });

            request.Content = formContent;

            var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);

            settings.AccessToken = null;
            settings.RefreshToken = null;
            settings.ExpiresAt = default(DateTime);
            settings.Update();

            return true;
        }

        private async Task<bool> refreshAccessToken()
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.nightbot.tv/oauth2/token");

            var formContent = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "client_id", "cac06b4cc0205a7fa9c4f223d42e19e7" },
                { "client_secret", "74c9cf88e9bdccbb37aa8807e525cf65" },
                { "refresh_token", settings.RefreshToken },
                { "grant_type", "refresh_token" },
                { "redirect_uri", "https://banane9.github.io/eu4savegameauthenticate" }
            });

            request.Content = formContent;

            var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);

            if (!response.IsSuccessStatusCode)
                return false;

            var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(await response.Content.ReadAsStringAsync());

            if (!tokenResponse.Scope.Contains("commands"))
                return false;

            settings.Update(tokenResponse);
            return true;
        }
    }
}