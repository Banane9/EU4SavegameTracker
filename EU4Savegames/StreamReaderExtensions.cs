using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EU4Savegames
{
    public static class StreamReaderExtensions
    {
        public static IEnumerable<string> GetAllLines(this StreamReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader), "StreamReader can't be null!");

            string line;
            while ((line = reader.ReadLine()) != null)
                yield return line;
        }
    }
}