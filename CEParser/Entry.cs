using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CEParser
{
    public class Entry : Entity
    {
        private bool quoted;
        private string value;

        public Entry(Node parent, string value, bool quoted)
        {
            this.value = value;
            this.quoted = quoted;
            parent.children.Add(this);
        }

        public override void Export(StringBuilder sb, int depth, ref bool endline)
        {
            if (endline)
                CreateDepth(sb, depth);
            else
                sb.Append(" ");
            if (quoted) sb.Append("\"");
            sb.Append(value);
            if (quoted) sb.Append("\"");
            endline = false;
        }

        public override string ToString()
        {
            return value;
        }

        internal override string GetName()
        {
            return "";
        }

        internal override string GetValue()
        {
            return value;
        }
    }
}