﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CEParser.Decoding.Model
{
    public abstract class Entity
    {
        public abstract void Export(StringBuilder sb, int depth, ref bool endline);

        internal int GetChildrenCount()
        {
            int count = 0;
            if (this is Node)
            {
                foreach (var n in (this as Node).children)
                {
                    count += n.GetChildrenCount();
                }
            }
            return count;
        }

        internal abstract string GetName();

        internal abstract string GetValue();

        protected static void CreateDepth(StringBuilder sb, int depth)
        {
            for (int i = 0; i < depth - 1 && i < 20; i++) sb.Append("\t");
        }
    }
}