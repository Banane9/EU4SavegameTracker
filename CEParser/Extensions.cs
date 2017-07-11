using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CEParser
{
    internal static class Extensions
    {
        public static void CreateDepth(this StringBuilder sb, int depth)
        {
            for (int i = 0; i < depth - 1 && i < 20; i++)
                sb.Append("\t");
        }

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