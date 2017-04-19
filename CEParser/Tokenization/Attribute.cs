using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CEParser.Tokenization
{
    public class Attribute : Entity
    {
        private string name;
        private bool quoted;
        private string value;

        public Attribute(Node parent, string name, string value, bool quoted)
        {
            this.name = name;
            this.value = value;
            this.quoted = quoted;
            parent.children.Add(this);
        }

        public override void Export(StringBuilder sb, int depth, ref bool endline)
        {
            if (!endline)
            {
                sb.AppendLine();
            }
            CreateDepth(sb, depth);
            sb.Append(name);
            sb.Append("=");
            if (quoted) sb.Append("\"");
            sb.Append(value);
            if (quoted) sb.Append("\"");
            endline = false;
        }

        public override string ToString()
        {
            return String.Format(quoted ? "{0} = \"{1}\"" : "{0} = {1}", name, value);
        }

        internal override string GetName()
        {
            return name;
        }

        internal override string GetValue()
        {
            return value;
        }
    }
}