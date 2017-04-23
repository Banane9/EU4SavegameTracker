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
    public sealed class EU4Save
    {
        private readonly Dictionary<Type, List<SavegameObject>> savegameObjects = new Dictionary<Type, List<SavegameObject>>();
        private string savegame;

        public IEnumerable<Type> SavegameObjectTypes
        {
            get { return savegameObjects.Keys.ToArray(); }
        }

        public EU4Save(string path)
        {
            savegame = SaveLoader.Decode(path, "temp.eu4").Result;
            Read();
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
            var openedBraces = 0;
            var split = savegame.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var enumerator = split.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var line = (string)enumerator.Current;

                if (openedBraces > 0)
                {
                    if (line.Contains("}"))
                        --openedBraces;

                    continue;
                }

                if (line.Contains('{'))
                    ++openedBraces;

                if (!SavegameObject.TryGetReaderForTag(getTag(line), out var readTagToObject))
                    continue;

                var savegameObject = readTagToObject(enumerator);
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
    }
}