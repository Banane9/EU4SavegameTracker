using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EU4Savegames
{
    public sealed class EU4Save : IDisposable
    {
        private readonly Stream savegame;
        private readonly Stream underlayingFile;

        public EU4Save(string path)
        {
            underlayingFile = File.OpenRead(path);

            try
            {
                var zipArchive = new ZipArchive(underlayingFile);

                savegame = zipArchive.GetEntry(Path.GetFileName(path)).Open();
            }
            catch (InvalidDataException)
            {
                underlayingFile.Position = 0;
                savegame = underlayingFile;
            }
        }

        public void Close()
        {
            savegame.Close();
            underlayingFile.Close();
        }

        public StreamReader GetReader()
        {
            return new StreamReader(savegame);
        }

        #region IDisposable Support

        private bool disposedValue = false;

        public void Dispose()
        {
            dispose(true);
        }

        private void dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    savegame.Dispose();
                    underlayingFile.Dispose();
                }

                disposedValue = true;
            }
        }

        #endregion IDisposable Support
    }
}