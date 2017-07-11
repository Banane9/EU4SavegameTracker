using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClausewitzParser.Tokens
{
    /// <summary>
    /// The abstract base for tokens.
    /// </summary>
    internal abstract class Token
    {
        /// <summary>
        /// Gets the binary (two byte) representation of the token.
        /// </summary>
        public abstract ushort BinaryToken { get; }

        /// <summary>
        /// Gets the text representation of the token.
        /// </summary>
        public abstract string TextToken { get; }
    }
}