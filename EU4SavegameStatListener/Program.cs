using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using EU4Savegames;
using Newtonsoft.Json;

namespace EU4SavegameStatListener
{
    internal class Program
    {
        private static UdpClient udpClient;

        private static IEnumerable<string> formatGreatPowers(IEnumerable<GreatPower> greatPowers)
        {
            foreach (var gp in greatPowers)
                yield return $"{gp.Rank}. {TagNames.GetEntry("english", gp.Tag)}";
        }

        private static void Main(string[] args)
        {
            if (args.Length < 1 || !int.TryParse(args[0], out int port) || port < 0)
            {
                usage();
                return;
            }

            udpClient = new UdpClient(port, AddressFamily.InterNetwork);

            Console.WriteLine($"Listening on port {port}");
            Console.WriteLine();

            receive();

            Console.ReadLine();
        }

        private static async void receive()
        {
            while (true)
            {
                var received = await udpClient.ReceiveAsync();
                var json = Encoding.UTF8.GetString(received.Buffer);

                Console.WriteLine(json);
                Console.WriteLine();

                var greatPowers = JsonConvert.DeserializeObject<GreatPower[]>(json);

                File.WriteAllLines("greatPowerRanking.txt", formatGreatPowers(greatPowers));
            }
        }

        private static void usage()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("EU4SavegameStatListener.exe port");
            Console.ReadLine();
        }
    }
}