using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EU4Savegames.Objects;

namespace EU4Savegames
{
    public sealed class EU4Save : IDisposable
    {
        private readonly Stream savegame;
        private readonly Dictionary<Type, List<SavegameObject>> savegameObjects = new Dictionary<Type, List<SavegameObject>>();
        private readonly Stream underlayingFile;

        public IEnumerable<Type> SavegameObjectTypes
        {
            get { return savegameObjects.Keys.ToArray(); }
        }

        public StreamReader SaveReader { get; }

        public EU4Save(string path, bool manualRead = false)
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

            SaveReader = new StreamReader(savegame);

            if (manualRead)
                return;

            Read();
            Close();
        }

        public void Close()
        {
            savegame.Close();
            underlayingFile.Close();
        }

        public TSavegame[] GetSavegameObjects<TSavegame>() where TSavegame : SavegameObject
        {
            return GetSavegameObjects(typeof(TSavegame)).Cast<TSavegame>().ToArray();
        }

        public SavegameObject[] GetSavegameObjects(Type type)
        {
            return savegameObjects.ContainsKey(type) ? savegameObjects[type].ToArray() : new SavegameObject[0];
        }

        public void Read()
        {
            string line;
            while ((line = SaveReader.ReadLine()) != null)
            {
                if (!SavegameObject.TryGetReaderForTag(getTag(line), out var readTagToObject))
                {
                    if (line.Contains('{'))
                        while (!SaveReader.ReadLine().Contains('}')) ;

                    continue;
                }

                var savegameObject = readTagToObject(SaveReader);
                var savegameObjectType = savegameObject.GetType();

                if (!savegameObjects.ContainsKey(savegameObjectType))
                    savegameObjects.Add(savegameObjectType, new List<SavegameObject>());

                savegameObjects[savegameObjectType].Add(savegameObject);
            }
        }

        private static string getTag(string line)
        {
            return line.Trim().Split('=')[0];
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