using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CEParser
{
    internal static class Bytes
    {
        public static readonly Encoding ANSI = Encoding.GetEncoding(1250);
        public static readonly byte CarriageReturn = ANSI.GetBytes(new char[] { '\r' })[0];
        public static readonly byte Comment = ANSI.GetBytes(new char[] { '#' })[0];
        public static readonly byte EqualsSign = ANSI.GetBytes(new char[] { '=' })[0];
        public static readonly byte LeftBrace = ANSI.GetBytes(new char[] { '{' })[0];
        public static readonly byte Minus = ANSI.GetBytes(new char[] { '-' })[0];
        public static readonly byte NewLine = ANSI.GetBytes(new char[] { '\n' })[0];
        public static readonly byte Plus = ANSI.GetBytes(new char[] { '+' })[0];
        public static readonly byte Quote = ANSI.GetBytes(new char[] { '\"' })[0];
        public static readonly byte RightBrace = ANSI.GetBytes(new char[] { '}' })[0];
        public static readonly byte Space = ANSI.GetBytes(new char[] { ' ' })[0];
        public static readonly byte Tab = ANSI.GetBytes(new char[] { '\t' })[0];
        public static readonly byte Underscore = ANSI.GetBytes(new char[] { '_' })[0];
        private static readonly bool[] lettersAndDigits = new bool[256];

        static Bytes()
        {
            for (int i = 48; i < 58; i++)
                lettersAndDigits[i] = true;

            for (int i = 65; i < 91; i++)
                lettersAndDigits[i] = true;

            for (int i = 97; i < 123; i++)
                lettersAndDigits[i] = true;
        }

        public static bool IsLetterOrDigit(byte b)
        {
            return lettersAndDigits[b];
        }
    }
}