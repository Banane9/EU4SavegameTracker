using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClausewitzParser.Tokens
{
    /// <summary>
    /// Represents and contains the symbol tokens used.
    /// </summary>
    internal abstract class SymbolToken : Token
    {
        /// <summary>
        /// The token used to close groups.
        /// </summary>
        public static SymbolToken CloseGroup { get; } = new CloseGroupToken();

        /// <summary>
        /// The token used between a key and its value.
        /// </summary>
        public static SymbolToken Equal { get; } = new EqualToken();

        /// <summary>
        /// The token used to open groups.
        /// </summary>
        public static SymbolToken OpenGroup { get; } = new OpenGroupToken();

        private SymbolToken()
        { }

        private sealed class CloseGroupToken : SymbolToken
        {
            public override ushort BinaryToken { get; } = 0x0400;
            public override string TextToken { get; } = "}";
        }

        private sealed class EqualToken : SymbolToken
        {
            public override ushort BinaryToken { get; } = 0x0100;
            public override string TextToken { get; } = "=";
        }

        private sealed class OpenGroupToken : SymbolToken
        {
            public override ushort BinaryToken { get; } = 0x0300;
            public override string TextToken { get; } = "{";
        }
    }
}