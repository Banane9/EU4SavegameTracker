using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CEParser.Decoding;

using CEParser.Decoding;

using Ionic.Zip;

namespace CEParser
{
    /// <summary>
    /// Represents a Clausewitz Engine savegame.
    /// </summary>
    public sealed class CESavegame
    {
        private readonly Stream data;

        private readonly CEDecoder decoder;

        private readonly ZipEntry rnw;

        private bool wasDecoded = false;

        /// <summary>
        /// Gets whether the savegame can be decoded.
        /// </summary>
        public bool CanBeDecoded => decoder != null;

        /// <summary>
        /// Gets whether the savegame has been decoded.
        /// </summary>
        public bool Decoded => Root != null && wasDecoded;

        /// <summary>
        /// Gets the <see cref="CEParser.Game"/> that the savegame is from.
        /// <para/>
        /// Null if the data header didn't match any supported game.
        /// </summary>
        public Game Game => decoder?.Game;

        /// <summary>
        /// Gets whether the savegame has a Random New World associated with it. Only for <see cref="CEParser.Game.EU4"/>.
        /// </summary>
        public bool HasRandomNewWorld
        {
            get { return rnw != null; }
        }

        /// <summary>
        /// Gets a stream containing the Random New World zip file.
        /// Null if <see cref="HasRandomNewWorld"/> is false.
        /// </summary>
        public Stream RandomNewWorld
        {
            get { return rnw?.OpenReader(); }
        }

        /// <summary>
        /// Gets the root <see cref="Node"/> of the savegame.
        /// <para/>
        /// Null if <see cref="Decoded"/> is false.
        /// </summary>
        public Node Root => wasDecoded ? decoder?.Root : null;

        public CESavegame(string path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path), "Stream can't be null!");

            if (!File.Exists(path))
                throw new ArgumentException("No savegame exists in that location!", nameof(path));

            using (var file = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var first = (char)file.ReadByte();
                var second = (char)file.ReadByte();
                file.Position = 0;

                // Check for magic header of zip files
                if (first == 'P' && second == 'K')
                    using (var zip = ZipFile.Read(file))
                    {
                        data = getDataFromZip(zip, path);
                        rnw = zip["rnw.zip"];
                    }
                else
                {
                    data = new MemoryStream((int)file.Length);
                    file.CopyTo(data);
                }
            }

            decoder = CEDecoder.GetDecoder(data);
        }

        public void Decode()
        {
            decoder?.Parse();

            wasDecoded = decoder != null;
        }

        public async Task DecodeAsync()
        {
            if (decoder == null)
                return;

            await decoder.ParseAsync();
            wasDecoded = true;
        }

        private static Stream getDataFromZip(ZipFile zip, string path)
        {
            var entry = zip[Path.GetFileName(path)] ?? zip[zip.EntryFileNames.SingleOrDefault(fName => fName.StartsWith("game"))]
                ?? throw new InvalidDataException("No game data found in the zip file!");

            var entryStream = entry.OpenReader();
            var ms = new MemoryStream((int)entryStream.Length);

            entryStream.CopyTo(ms);

            return ms;
        }
    }
}