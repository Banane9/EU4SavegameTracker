using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EU4Savegames;
using Microsoft.Win32;

namespace EU4SavegameInfo.NightbotUpdater
{
    internal class SavegameTracker
    {
        private readonly FileSystemWatcher fsw = new FileSystemWatcher(DefaultPath, "*.eu4");
        private readonly Dictionary<string, Timer> timers = new Dictionary<string, Timer>();

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

        public SavegameTracker(Action<string> onNewPath, string path = null)
        {
            if (path != null)
                SavegamePath = path;

            if (!Directory.Exists(SavegamePath))
            {
                var folderBrowser = new System.Windows.Forms.FolderBrowserDialog()
                {
                    ShowNewFolderButton = false,
                    Description = "Pick Savegame Location",
                    RootFolder = Environment.SpecialFolder.MyDocuments,
                };

                if (folderBrowser.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                    throw new InvalidOperationException("No savegame folder selected.");

                SavegamePath = folderBrowser.SelectedPath;
                onNewPath(SavegamePath);
            }

            var fsw = new FileSystemWatcher(SavegamePath, "*.eu4");

            fsw.Created += fsw_Created;
            fsw.Changed += fsw_Changed;
        }

        private void alarm(object state)
        {
            var file = (string)state;
            Console.WriteLine($"{file} didn't get written to!");
            Console.Beep();
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
            {
                Console.WriteLine($"{e.Name} got created!");
                timers.Add(e.Name, new Timer(alarm, e.Name, 10 * 1000, 1000));
            }
        }

        private async void notifyNightbot(string savegame)
        {
            await Task.Delay(TimeSpan.FromSeconds(2));

            GreatPower[] greatPowers = null;

            try
            {
                var save = new EU4Save(savegame);
                greatPowers = GreatPower.ReadGreatPowersFromFile(save.GetReader()).ToArray();
                save.Close();
            }
            catch (IOException)
            {
                Console.WriteLine($"Failed to read savegame {Path.GetFileName(savegame)}");
                return;
            }

            try
            {
            }
            catch (IOException)
            {
                Console.WriteLine("Failed to send data");
            }
        }
    }
}