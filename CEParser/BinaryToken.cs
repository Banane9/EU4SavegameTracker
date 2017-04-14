using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CEParser
{
    /// <summary>
    /// Represents a binary token, i.e. 2-byte portion of the file.
    /// </summary>
    public class BinaryToken
    {
        /// <summary>
        /// Byte array (in ANSI coding) representation of the token.
        /// </summary>
        public byte[] Bytes;

        /// <summary>
        /// If other than None, specifies data type that is expected after this token.
        /// </summary>
        public DataType DataType = DataType.Unspecified;

        /// <summary>
        /// If true and the token defines a container name, data inside the container will inherit this token's quoted property.
        /// </summary>
        public bool InheritQuoted = false;

        /// <summary>
        /// If true and the token defines a container name, data inside the container will inherit this token's type.
        /// </summary>
        public bool InheritType = false;

        /// <summary>
        /// If true, strings and dates after this token will be treated as quoted.
        /// </summary>
        public bool Quoted = false;

        /// <summary>
        /// Text representation of the token.
        /// </summary>
        public string Text = "";

        private static Encoding ANSI = Encoding.GetEncoding(1250);

        /// <summary>
        /// Creates a new binary token definition.
        /// </summary>
        /// <param name="input">Line from CSV definition file.</param>
        public BinaryToken(string input)
        {
            string[] fields = input.Split(';');
            if (fields.Length < 3) return;
            Text = fields[1];
            DataType = GetSpecialCode(fields[2].ToLowerInvariant(), ref InheritType);
            switch (fields[3].Trim())
            {
                case "yes*": Quoted = true; InheritQuoted = true; break;
                case "yes": Quoted = true; InheritQuoted = false; break;
                case "no*": Quoted = false; InheritQuoted = true; break;
                default: Quoted = false; InheritQuoted = false; break;
            }

            if (Text == "")
                Bytes = new byte[0];
            else
                Bytes = ANSI.GetBytes(Text);
        }

        /// <summary>
        /// Recognizes special code from string input.
        /// </summary>
        /// <param name="input">String representation</param>
        /// <returns>Special code</returns>
        public DataType GetSpecialCode(string input)
        {
            switch (input)
            {
                case "string": return DataType.String;
                case "integer": return DataType.Integer;
                case "float": return DataType.Float;
                case "float5": return DataType.Float5;
                case "date": return DataType.Date;
                case "boolean": return DataType.Boolean;
                case "variable": return DataType.Variable;
                default: return DataType.Unspecified;
            }
        }

        public DataType GetSpecialCode(string input, ref bool inheritType)
        {
            if (input.EndsWith("*"))
            {
                input = input.Remove(input.Length - 1, 1);
                inheritType = true;
            }

            switch (input)
            {
                case "string": return DataType.String;
                case "integer": return DataType.Integer;
                case "float": return DataType.Float;
                case "float5": return DataType.Float5;
                case "date": return DataType.Date;
                case "boolean": return DataType.Boolean;
                case "variable": return DataType.Variable;
                default: return DataType.Unspecified;
            }
        }

        public override string ToString()
        {
            return Text;
        }
    }
}