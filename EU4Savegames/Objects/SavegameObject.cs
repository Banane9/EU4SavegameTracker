using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EU4Savegames.Objects
{
    /// <summary>
    /// Base class for savegame objects. Extending classes must have a constructor that takes a <see cref="StreamReader"/>.
    /// </summary>
    public abstract partial class SavegameObject
    {
        private static readonly Dictionary<string, ReadTagToObjectFunc> tagToObjects = new Dictionary<string, ReadTagToObjectFunc>();

        protected SavegameObject(IEnumerator reader)
        { }

        public static void AddTagToObjectReader(string tag, ReadTagToObjectFunc readTagToObject)
        {
            if (tagToObjects.ContainsKey(tag))
                return;

            tagToObjects.Add(tag, readTagToObject);
        }

        public static bool TryGetReaderForTag(string tag, out ReadTagToObjectFunc readToTagObject)
        {
            readToTagObject = null;

            if (!tagToObjects.ContainsKey(tag))
                return false;

            readToTagObject = tagToObjects[tag];
            return true;
        }

        public delegate SavegameObject ReadTagToObjectFunc(IEnumerator reader);
    }
}