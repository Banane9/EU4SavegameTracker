using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using CEParser;
using CEParser.Tokenization;
using Microsoft.Win32;

namespace CEParser
{
    /// <summary>
    /// Represents infos about a supported game.
    /// </summary>
    public abstract class Game
    {
        private static readonly string steamPath;
        private static readonly string steamUserId;
        private static Encoding ansi = Encoding.GetEncoding("iso-8859-2");
        private static string localBase = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Paradox Interactive");

        /// <summary>
        /// Gets Infos for Crusader Kings II.
        /// </summary>
        public static Game CK2 { get; } = new CK2Game();

        /// <summary>
        /// Gets Infos for Europa Universalis IV.
        /// </summary>
        public static Game EU4 { get; } = new EU4Game();

        /// <summary>
        /// Gets all supported games.
        /// </summary>
        public static IEnumerable<Game> Games
        {
            get
            {
                yield return CK2;
                yield return EU4;
                yield return HoI4;
            }
        }

        /// <summary>
        /// Gets Infos for Hearts of Iron IV.
        /// </summary>
        public static Game HoI4 { get; } = new HoI4Game();

        /// <summary>
        /// Gets whether the Steam cloud folder for the game exists.
        /// </summary>
        public bool CloudFolderAvailable => steamPath != null && steamUserId != null && Directory.Exists(CloudFolderPath);

        /// <summary>
        /// Gets the possibly incomplete path to the Steam cloud save game folder of the game.
        /// Check <see cref="CloudFolderAvailable"/> before using.
        /// </summary>
        public string CloudFolderPath => Path.Combine(steamPath, "userdata", steamUserId, SteamId, "remote", "save games");

        /// <summary>
        /// Gets the <see cref="System.Text.Encoding"/> used for the save games.
        /// </summary>
        public abstract Encoding Encoding { get; }

        /// <summary>
        /// Gets the file extension of the save games.
        /// </summary>
        public abstract string Extension { get; }

        /// <summary>
        /// Gets whether the local save folder for the game exists.
        /// </summary>
        public bool LocalFolderAvailable => Directory.Exists(LocalFolderPath);

        /// <summary>
        /// Gets the path to the local save game folder of the game.
        /// </summary>
        public string LocalFolderPath => Path.Combine(localBase, Name, "save games");

        /// <summary>
        /// Gets the full name of the game.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Gets the Steam id of the game.
        /// </summary>
        public abstract string SteamId { get; }

        /// <summary>
        /// Gets the <see cref="CEParser.BinaryTokens"/> used by the game.
        /// </summary>
        internal BinaryTokens BinaryTokens { get; }

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
            public override string SteamId => "203770";
        }

        private sealed class EU4Game : Game
        {
            public override Encoding Encoding => ansi;
            public override string Extension => "eu4";
            public override string Name => "Europa Universalis IV";
            public override string SteamId => "236850";
        }

        private sealed class HoI4Game : Game
        {
            public override Encoding Encoding => new UTF8Encoding(false);
            public override string Extension => "hoi4";
            public override string Name => "Hearts of Iron IV";
            public override string SteamId => "394360";
        }
    }
}