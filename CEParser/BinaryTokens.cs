using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

namespace CEParser
{
    /// <summary>
    /// Represents the set of binary tokens.
    /// </summary>
    public class BinaryTokens
    {
        private readonly Dictionary<ushort, BinaryToken> codes;

        /// <summary>
        /// Creates a set of binary tokens from an external CSV file.
        /// </summary>
        /// <param name="path">input tokens</param>
        public BinaryTokens(string[] tokens)
        {
            codes = readCodes(tokens);
        }

        public BinaryTokens(string filename)
        {
            // Read tokens file
            using (var reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("CEParser." + filename)))
                codes = readCodes(reader.GetAllLines().Skip(1).Where(line => line.Length > 14));
        }

        /// <summary>
        /// Returns a token corresponding to the given two bytes.
        /// </summary>
        /// <param name="b1">Left-hand byte</param>
        /// <param name="b2">Right-hand byte</param>
        /// <returns>Corresponding token or null if not found</returns>
        public BinaryToken Get(byte b1, byte b2)
        {
            return Get((ushort)(b1 << 8 | b2));
        }

        /// <summary>
        /// Returns a token corresponding to the given two bytes converted to ushort value.
        /// </summary>
        /// <param name="code">Bytes converted to ushort</param>
        /// <returns>Corresponding token or null if not found</returns>
        public BinaryToken Get(ushort code)
        {
            BinaryToken output;
            codes.TryGetValue(code, out output);
            return output;
        }

        /// <summary>
        /// Returns true if there is corresponding binary token for the given code, otherwise false.
        /// </summary>
        /// <param name="code">Bytes converted to ushort</param>
        /// <returns>True if a token is found, otherwise false</returns>
        public bool IsProperToken(ushort code)
        {
            return codes.ContainsKey(code);
        }

        private static Dictionary<ushort, BinaryToken> readCodes(IEnumerable<string> tokens)
        {
            var codes = new Dictionary<ushort, BinaryToken>();

            foreach (var token in tokens)
            {
                string hexCode = token.Substring(2, 4);

                if (UInt16.TryParse(hexCode, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out ushort intCode) && !codes.ContainsKey(intCode))
                    codes.Add(intCode, new BinaryToken(token));
            }

            return codes;
        }
    }
}