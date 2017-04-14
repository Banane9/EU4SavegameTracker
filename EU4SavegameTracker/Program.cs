using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EU4Savegames;
using EU4Savegames.Localisation;
using EU4Savegames.Objects;
using Newtonsoft.Json;

namespace EU4SavegameTracker
{
    internal class Program
    {
        private static Dictionary<string, Timer> timers = new Dictionary<string, Timer>();
        private static UdpClient udpClient;

        private static void alarm(object state)
        {
            var file = (string)state;
            Console.WriteLine($"{file} didn't get written to!");
            Console.Beep();
        }

        private static void fsw_Changed(object sender, FileSystemEventArgs e)
        {
            if (new FileInfo(e.FullPath).Length == 0)
                return;

            if (timers.ContainsKey(e.Name))
                timers[e.Name].Dispose();

            timers.Remove(e.Name);

            notifyListener(e.FullPath);
        }

        private static void fsw_Created(object sender, FileSystemEventArgs e)
        {
            if (!timers.ContainsKey(e.Name))
            {
                Console.WriteLine($"{e.Name} got created!");
                timers.Add(e.Name, new Timer(alarm, e.Name, 10 * 1000, 1000));
            }
        }

        private static void Main(string[] args)
        {
            var port = -1;
            string hostname = "";
            var savegamePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "Paradox Interactive", "Europa Universalis IV", "save games");

            if (args.Length == 1 && args[0].Split(':').Length == 2 && int.TryParse(args[0].Split(':')[1], out int pPort))
            {
                port = pPort;
                hostname = args[0].Split(':')[0];
            }
            else if (args.Length > 0)
            {
                usage();
                return;
            }

            if (!Directory.Exists(savegamePath))
            {
                Console.WriteLine($"The savegame path [{savegamePath}] doesn't exist!");
                Console.ReadLine();
                return;
            }

            Console.WriteLine($"Tracking saves at {savegamePath}");

            if (!string.IsNullOrWhiteSpace(hostname) && port > 0)
                udpClient = new UdpClient(hostname, port);

            var fsw = new FileSystemWatcher(savegamePath, "*.eu4") { EnableRaisingEvents = true };
            fsw.Created += fsw_Created;
            fsw.Changed += fsw_Changed;

            Console.WriteLine("Press any key to quit...");
            Console.ReadLine();
        }

        private static async void notifyListener(string name)
        {
            //if (udpClient == null)
            //    return;

            await Task.Delay(TimeSpan.FromSeconds(2));

            EU4Save save;

            try
            {
                save = new EU4Save(name);
            }
            catch (IOException)
            {
                Console.WriteLine($"Failed to read savegame {Path.GetFileName(name)}");
                return;
            }

            var gpList = save?.GetSavegameObjects<GreatPowersObject>().SingleOrDefault();
            if (gpList == null)
                return;

            var json = JsonConvert.SerializeObject(gpList);
            Console.WriteLine(json);

            //var datagram = Encoding.UTF8.GetBytes(json);

            //try
            //{
            //    await udpClient.SendAsync(datagram, datagram.Length);
            //}
            //catch (IOException)
            //{
            //    Console.WriteLine("Failed to send data");
            //}
        }

        private static void usage()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("EU4SavegameTracker.exe [hostname:port]");
            Console.ReadLine();
        }
    }
}