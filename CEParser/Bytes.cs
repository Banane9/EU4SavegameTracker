using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CEParser
{
    public static class Bytes
    {
        public static Encoding ANSI = Encoding.GetEncoding(1250);
        public static byte CarriageReturn;
        public static byte Comment;
        public static byte EqualsSign;
        public static byte LeftBrace;
        public static byte Minus;
        public static byte NewLine;
        public static byte Plus;
        public static byte Quote;
        public static byte RightBrace;
        public static byte Space;
        public static byte Tab;
        public static byte Underscore;
        private static bool[] lettersAndDigits = new bool[256];

        public static void Initialize()
        {
            CarriageReturn = ANSI.GetBytes(new char[] { '\r' })[0];
            NewLine = ANSI.GetBytes(new char[] { '\n' })[0];
            Comment = ANSI.GetBytes(new char[] { '#' })[0];
            LeftBrace = ANSI.GetBytes(new char[] { '{' })[0];
            RightBrace = ANSI.GetBytes(new char[] { '}' })[0];
            EqualsSign = ANSI.GetBytes(new char[] { '=' })[0];
            Quote = ANSI.GetBytes(new char[] { '\"' })[0];
            Underscore = ANSI.GetBytes(new char[] { '_' })[0];
            Minus = ANSI.GetBytes(new char[] { '-' })[0];
            Plus = ANSI.GetBytes(new char[] { '+' })[0];
            Tab = ANSI.GetBytes(new char[] { '\t' })[0];
            Space = ANSI.GetBytes(new char[] { ' ' })[0];

            for (int i = 48; i < 58; i++)
            {
                lettersAndDigits[i] = true;
            }
            for (int i = 65; i < 91; i++)
            {
                lettersAndDigits[i] = true;
            }
            for (int i = 97; i < 123; i++)
            {
                lettersAndDigits[i] = true;
            }
        }

        public static bool IsLetterOrDigit(byte b)
        {
            return lettersAndDigits[b];
        }
    }
}