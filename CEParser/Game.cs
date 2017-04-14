using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using CEParser;
using Microsoft.Win32;

namespace CEParser
{
    public abstract class Game
    {
        private static readonly string steamPath;
        private static readonly string steamUserId;
        private static Encoding ansi = Encoding.GetEncoding("iso-8859-2");
        private static string localBase = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Paradox Interactive");

        public static Game CK2 { get; } = new CK2Game();
        public static Game EU4 { get; } = new EU4Game();
        public static Game HoI4 { get; } = new HoI4Game();

        public BinaryTokens BinaryTokens { get; }
        public bool CloudFolderAvailable => steamPath != null && steamUserId != null && Directory.Exists(CloudFolderPath);
        public string CloudFolderPath => Path.Combine(steamPath, "userdata", steamUserId, SteamId, "remote", "save games");
        public abstract Encoding Encoding { get; }
        public abstract string Extension { get; }
        public bool LocalFolderAvailable => Directory.Exists(LocalFolderPath);
        public string LocalFolderPath => Path.Combine(localBase, Name, "save games");
        public abstract string Name { get; }
        protected abstract string SteamId { get; }

        static Game()
        {
            try
            {
                var steam = Registry.CurrentUser?.OpenSubKey("Software")?.OpenSubKey("Valve")?.OpenSubKey("Steam");

                steamPath = steam?.GetValue("SteamPath", "")?.ToString().Replace('/', Path.DirectorySeparatorChar);

                steamUserId = steam?.OpenSubKey("Users")?.GetSubKeyNames()[0];
            }
            catch (SecurityException)
            { }
        }

        private Game()
        {
            BinaryTokens = new BinaryTokens(Extension + "bin.csv");
        }

        private sealed class CK2Game : Game
        {
            public override Encoding Encoding => ansi;
            public override string Extension => "ck2";
            public override string Name => "Crusader Kings II";
            protected override string SteamId => "203770";
        }

        private sealed class EU4Game : Game
        {
            public override Encoding Encoding => ansi;
            public override string Extension => "eu4";
            public override string Name => "Europa Universalis IV";
            protected override string SteamId => "236850";
        }

        private sealed class HoI4Game : Game
        {
            public override Encoding Encoding => new UTF8Encoding(false);
            public override string Extension => "hoi4";
            public override string Name => "Hearts of Iron IV";
            protected override string SteamId => "394360";
        }
    }
}