using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using EU4SavegameInfo.NightbotUpdater.OAuth;
using Newtonsoft.Json;

namespace EU4SavegameInfo.NightbotUpdater
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        private readonly HttpClient httpClient = new HttpClient();
        private OAuthBrowserWindow browserWindow;
        private MenuItem nightbotAccess;
        private NotifyIcon notifyIcon;
        private Settings settings;
        private SavegameTracker tracker;
        private MenuItem trackPath;
        private MenuItem trackSaves;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            MainWindow = new MainWindow();

            settings = File.Exists("settings.json") ? JsonConvert.DeserializeObject<Settings>(File.ReadAllText("settings.json")) : new Settings();

            nightbotAccess = new MenuItem("Nightbot Access", establishNightbotAccess) { Checked = settings.ExpiresAt > DateTime.Now };

            if (settings.AccessToken != null && settings.ExpiresAt < DateTime.Now)
                refreshNightbotToken();

            tracker = new SavegameTracker(updateSettings, settings.SavegamePath) { IsTracking = true };
            trackPath = new MenuItem(tracker.SavegamePath);

            trackSaves = new MenuItem("Track Saves", trackSaves_OnClick) { Checked = true };
            trackSaves.MenuItems.Add(trackPath);

            var menuItems = new[]
            {
                trackSaves,
                new MenuItem("-"),
                nightbotAccess,
                new MenuItem("-"),
                new MenuItem("Exit", (_, __) => Shutdown())
            };

            var icon = new Icon(Assembly.GetExecutingAssembly().GetManifestResourceStream("EU4SavegameInfo.NightbotUpdater.nightbot.ico"));

            notifyIcon = new NotifyIcon()
            {
                Icon = icon,
                ContextMenu = new ContextMenu(menuItems),
                Visible = true
            };
        }

        private async void browserWindow_Closing(object sender, CancelEventArgs e)
        {
            browserWindow.Closing -= browserWindow_Closing;
            browserWindow = null;

            if (!(((OAuthBrowserWindow)sender).DataContext is string))
                return;

            if (await getNightbotTokens((string)((OAuthBrowserWindow)sender).DataContext))
                nightbotAccess.Checked = true;
        }

        private void establishNightbotAccess(object sender, EventArgs e)
        {
            if (nightbotAccess.Checked)
                return;

            browserWindow = new OAuthBrowserWindow();
            browserWindow.Closing += browserWindow_Closing;
            browserWindow.Show();
        }

        private async Task<bool> getNightbotTokens(string code)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.nightbot.tv/oauth2/token");

            var formContent = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "client_id", "cac06b4cc0205a7fa9c4f223d42e19e7" },
                { "client_secret", "74c9cf88e9bdccbb37aa8807e525cf65" },
                { "code", code },
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

            updateSettings(tokenResponse);

            return true;
        }

        private async Task refreshNightbotToken()
        {
            if (settings.ExpiresAt.AddDays(30) < DateTime.Now)
            {
                establishNightbotAccess(null, null);
                return;
            }

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.nightbot.tv/oauth2/token");

            var formContent = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "client_id", "cac06b4cc0205a7fa9c4f223d42e19e7" },
                { "client_secret", "74c9cf88e9bdccbb37aa8807e525cf65" },
                { "code", settings.RefreshToken },
                { "grant_type", "refresh_token" },
                { "redirect_uri", "https://banane9.github.io/eu4savegameauthenticate" }
            });

            request.Content = formContent;

            var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);

            if (!response.IsSuccessStatusCode)
                return;

            var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(await response.Content.ReadAsStringAsync());

            if (!tokenResponse.Scope.Contains("commands"))
                return;

            nightbotAccess.Checked = true;
            updateSettings(tokenResponse);
        }

        private void trackSaves_OnClick(object sender, EventArgs e)
        {
            var trackSaves = (MenuItem)sender;
            trackSaves.Checked = !trackSaves.Checked;
            tracker.IsTracking = trackSaves.Checked;
        }

        private void updateSettings(string newPath)
        {
            settings.SavegamePath = newPath;
            updateSettings();
        }

        private void updateSettings()
        {
            File.WriteAllText("settings.json", JsonConvert.SerializeObject(settings));
        }

        private void updateSettings(TokenResponse tokenResponse)
        {
            settings.AccessToken = tokenResponse.AccessToken;
            settings.ExpiresAt = tokenResponse.ExpiresAt;
            settings.RefreshToken = tokenResponse.RefreshToken;

            updateSettings();
        }
    }
}