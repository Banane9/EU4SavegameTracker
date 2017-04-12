using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using EU4Savegames;
using Microsoft.Win32;

namespace EU4SavegameInfo.NightbotUpdater
{
    internal class SavegameTracker
    {
        private readonly FileSystemWatcher fsw;
        private readonly NightbotUpdater nightbotUpdater;
        private readonly Dictionary<string, System.Threading.Timer> timers = new Dictionary<string, System.Threading.Timer>();

        public static string DefaultPath { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "Paradox Interactive", "Europa Universalis IV", "save games");

        public bool IsTracking
        {
            get { return fsw.EnableRaisingEvents; }
            set { fsw.EnableRaisingEvents = value; }
        }

        public string SavegamePath
        {
            get { return fsw.Path; }
            set { fsw.Path = value; }
        }

        public SavegameTracker(Settings settings, NightbotUpdater nightbotUpdater)
        {
            this.nightbotUpdater = nightbotUpdater;

            var savegamePath = DefaultPath;

            if (settings.SavegamePath != null)
                savegamePath = settings.SavegamePath;

            if (!Directory.Exists(savegamePath))
            {
                var folderBrowser = new FolderBrowserDialog()
                {
                    ShowNewFolderButton = false,
                    Description = "Pick Savegame Location",
                    RootFolder = Environment.SpecialFolder.MyDocuments,
                };

                while (folderBrowser.ShowDialog() != DialogResult.OK)
                    if (MessageBox.Show("You must pick your savegame location!", "Savegame Error", MessageBoxButtons.RetryCancel, MessageBoxIcon.Warning) == DialogResult.Cancel)
                        throw new InvalidOperationException();

                savegamePath = folderBrowser.SelectedPath;
                settings.Update(savegamePath);
            }

            fsw = new FileSystemWatcher(savegamePath, "*.eu4");

            fsw.Created += fsw_Created;
            fsw.Changed += fsw_Changed;
        }

        private void alarm(object state)
        {
            var file = (string)state;

            if (timers.ContainsKey(file))
                timers[file].Dispose();

            timers.Remove(file);

            MessageBox.Show($"{file} didn't get written to!", "Corrupted Savegame", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void fsw_Changed(object sender, FileSystemEventArgs e)
        {
            if (new FileInfo(e.FullPath).Length == 0)
                return;

            if (timers.ContainsKey(e.Name))
                timers[e.Name].Dispose();

            timers.Remove(e.Name);

            notifyNightbot(e.FullPath);
        }

        private void fsw_Created(object sender, FileSystemEventArgs e)
        {
            if (!timers.ContainsKey(e.Name))
                timers.Add(e.Name, new System.Threading.Timer(alarm, e.Name, 10 * 1000, 1000));
        }

        private async void notifyNightbot(string savegame)
        {
            await Task.Delay(TimeSpan.FromSeconds(2));

            EU4Save save;
            try
            {
                save = new EU4Save(savegame);
            }
            catch (IOException)
            {
                Console.WriteLine($"Failed to read savegame {Path.GetFileName(savegame)}");
                return;
            }

            try
            {
                nightbotUpdater.Update(save);
            }
            catch (IOException)
            {
                Console.WriteLine("Failed to send data");
            }
        }
    }
}