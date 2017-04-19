using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ionic.Zip;

namespace CEParser.Files
{
    public sealed class CESavegame
    {
        private readonly Stream data;
        private readonly ZipEntry rnw;

        public bool HasRandomNewWorld
        {
            get { return rnw != null; }
        }

        public Stream RandomNewWorld
        {
            get { return rnw?.OpenReader(); }
        }

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
                    data.Position = 0;
                }
            }

            CEDecoder.GetDecoder(data);
        }

        private static Stream getDataFromZip(ZipFile zip, string path)
        {
            var entry = zip[Path.GetFileName(path)] ?? zip[zip.EntryFileNames.SingleOrDefault(fName => fName.StartsWith("game"))]
                ?? throw new InvalidDataException("No game data found in the zip file!");

            var entryStream = entry.OpenReader();
            var ms = new MemoryStream((int)entryStream.Length);

            entryStream.CopyTo(ms);
            ms.Position = 0;

            return ms;
        }
    }
}