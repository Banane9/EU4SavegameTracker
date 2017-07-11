using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClausewitzParser.Tokens
{
    /// <summary>
    /// Represents a key token.
    /// </summary>
    internal sealed class KeyToken : Token
    {
        /// <summary>
        /// Gets the binary (two byte) representation of the token.
        /// </summary>
        public override ushort BinaryToken { get; }

        /// <summary>
        /// Gets the text representation of the token.
        /// </summary>
        public override string TextToken { get; }

        /// <summary>
        /// Creates a new instance of the <see cref="KeyToken"/> class with the given binary and text tokens.
        /// </summary>
        /// <param name="binaryToken">The binary (two byte) representation of the token.</param>
        /// <param name="textToken">The text representation of the token.</param>
        public KeyToken(ushort binaryToken, string textToken)
        {
            if (string.IsNullOrWhiteSpace(textToken))
                throw new ArgumentException("Text Token must not be null or whitespace!", nameof(textToken));

            BinaryToken = binaryToken;
            TextToken = textToken;
        }
    }
}