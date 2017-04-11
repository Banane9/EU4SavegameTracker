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
using Newtonsoft.Json;

namespace EU4SavegameInfo.NightbotUpdater
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        private MenuItem nightbotAccess;
        private NightbotUpdater nightbotUpdater;
        private NotifyIcon notifyIcon;
        private Settings settings;
        private SavegameTracker tracker;
        private MenuItem trackPath;
        private MenuItem trackSaves;

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            MainWindow = new Window() { ShowInTaskbar = false };
            MainWindow.Hide();

            settings = File.Exists("settings.json") ? JsonConvert.DeserializeObject<Settings>(File.ReadAllText("settings.json")) : new Settings();
            nightbotUpdater = new NightbotUpdater(settings);

            nightbotAccess = new MenuItem("Nightbot Access", nightbotAccess_OnClick) { Checked = await nightbotUpdater.GetIsConnected() };

            tracker = new SavegameTracker(settings) { IsTracking = true };
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

        private async void nightbotAccess_OnClick(object sender, EventArgs e)
        {
            if (!await nightbotUpdater.GetIsConnected())
            {
                nightbotAccess.Checked = false;
                nightbotAccess.Checked = await nightbotUpdater.Connect();
                return;
            }

            if (System.Windows.Forms.MessageBox.Show("Do you really want to revoke Nightbot access?", "Confirm Nightbot Access Revocation", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.Cancel)
                return;

            if (await nightbotUpdater.RevokeAccess())
                nightbotAccess.Checked = false;
        }

        private void trackSaves_OnClick(object sender, EventArgs e)
        {
            var trackSaves = (MenuItem)sender;
            trackSaves.Checked = !trackSaves.Checked;
            tracker.IsTracking = trackSaves.Checked;
        }
    }
}