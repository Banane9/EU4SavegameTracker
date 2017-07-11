using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CEParser.Decoding.Model
{
    public class Field<TValue> : Entry
    {
        public TValue Value { get; }

        public Field(string name, TValue value, bool quoted)
            : base(value, quoted)
        {
            Name = name;
            //parent.children.Add(this);
        }

        public override void Export(StringBuilder sb, int depth, ref bool endline)
        {
            if (!endline)
                sb.AppendLine();

            sb.CreateDepth(depth);
            sb.Append(Name);
            sb.Append("=");

            if (Quoted)
                sb.Append("\"");

            sb.Append(Value);

            if (Quoted)
                sb.Append("\"");

            endline = false;
        }

        public override string ToString()
        {
            return $"{Name}={(Quoted ? $"\"{Value}\"" : Value.ToString())}";
        }
    }
}