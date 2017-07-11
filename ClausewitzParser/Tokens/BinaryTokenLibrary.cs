using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace ClausewitzParser.Tokens
{
    /// <summary>
    /// Contains information for converting the 2 byte binary codes to their text equivalent.
    /// </summary>
    internal static class BinaryTokenLibrary
    {
        private readonly static Dictionary<Game, Dictionary<ushort, string>> gameTokens = new Dictionary<Game, Dictionary<ushort, string>>();

        /// <summary>
        /// Gets the text version of a binary token for a specific game or null if either isn't available.
        /// </summary>
        /// <param name="game">The game whose dictionary to look in.</param>
        /// <param name="binaryToken">The token to look for.</param>
        /// <returns>The text version of the token or null if it couldn't be found.</returns>
        public static string GetToken(Game game, ushort binaryToken)
        {
            var tokenDict = GetTokenDictionary(game);

            if (tokenDict != null && tokenDict.ContainsKey(binaryToken))
                return tokenDict[binaryToken];

            return null;
        }

        /// <summary>
        /// Gets a Dictionary of tokens for a game or null if it's not available.
        /// </summary>
        /// <param name="game">The game that the dictionary is wanted for.</param>
        /// <returns>The dictionary of tokens for the game or null if it's not available.</returns>
        public static Dictionary<ushort, string> GetTokenDictionary(Game game)
        {
            if (!gameTokens.ContainsKey(game))
                gameTokens.Add(game, loadTokenDictionary(game));

            return gameTokens[game];
        }

        public static string GetTokenDictionaryFilename(Game game)
        {
            return $"{game.Extension}tokens.csv";
        }

        private static Dictionary<ushort, string> loadTokenDictionary(Game game)
        {
            Stream tokenDictStream;
            var filename = GetTokenDictionaryFilename(game);

            if (!File.Exists(filename))
            {
                tokenDictStream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"ClausewitzParser.{filename}");
                tryWriteToFile(game, tokenDictStream);
            }
            else
                tokenDictStream = File.OpenRead(filename);

            var dictReader = new StreamReader(tokenDictStream);
        }

        private static void tryWriteToFile(Game game, Stream tokenDictStream)
        {
            try
            {
                using (var fileStream = File.OpenWrite(GetTokenDictionaryFilename(game)))
                {
                    fileStream.Position = 0;
                    tokenDictStream.CopyTo(fileStream);
                }
            }
            catch (IOException e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                tokenDictStream.Position = 0;
            }
        }
    }
}