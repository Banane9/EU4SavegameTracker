using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CEParser
{
    public class Node : Entity
    {
        protected internal List<Entity> children = new List<Entity>();

        /// <summary>
        /// Specifies how deep in the file structure the current entity lies.
        /// </summary>
        protected internal byte Depth;

        protected internal Node Parent;
        private string name;
        private Dictionary<string, int> subnodeCache;

        /// <summary>
        /// Creates a new entity.
        /// </summary>
        /// <param name="file">Reference to a file that holds the entity contents.</param>
        public Node(Node parent, string name)
        {
            this.name = name;
            this.Parent = parent;
            this.Depth = (byte)(Parent.Depth + 1);
            parent.children.Add(this);
            //if (!parent.subnodes.ContainsKey(name.ToLowerInvariant()))
            //    parent.subnodes.Add(name.ToLowerInvariant(), (ushort)(parent.children.Count - 1));
        }

        public Node()
        {
            this.name = "";
            this.Parent = null;
            this.Depth = 0;
        }

        public void CleanUp(string mask, bool caseSensitive)
        {
            string m = caseSensitive ? mask : mask.ToLowerInvariant();
            if (m.StartsWith("*") && !m.EndsWith("*"))
            {
                m = mask.Replace("*", "");
                for (int i = children.Count - 1; i >= 0; i--)
                {
                    string name = caseSensitive ? children[i].GetName() : children[i].GetName().ToLowerInvariant();
                    if (name.EndsWith(m))
                    {
                        children.RemoveAt(i);
                    }
                    else
                    {
                        string value = caseSensitive ? children[i].GetValue() : children[i].GetValue().ToLowerInvariant();
                        if (value.EndsWith(m)) children.RemoveAt(i);
                    }
                }
            }
            else if (m.EndsWith("*") && !m.StartsWith("*"))
            {
                m = mask.Replace("*", "");
                for (int i = children.Count - 1; i >= 0; i--)
                {
                    string name = caseSensitive ? children[i].GetName() : children[i].GetName().ToLowerInvariant();
                    if (name.StartsWith(m))
                    {
                        children.RemoveAt(i);
                    }
                    else
                    {
                        string value = caseSensitive ? children[i].GetValue() : children[i].GetValue().ToLowerInvariant();
                        if (value.StartsWith(m)) children.RemoveAt(i);
                    }
                }
            }
            else
            {
                m = mask.Replace("*", "");
                for (int i = children.Count - 1; i >= 0; i--)
                {
                    string name = caseSensitive ? children[i].GetName() : children[i].GetName().ToLowerInvariant();
                    if (name.Contains(m))
                    {
                        children.RemoveAt(i);
                    }
                    else
                    {
                        string value = caseSensitive ? children[i].GetValue() : children[i].GetValue().ToLowerInvariant();
                        if (value.Contains(m)) children.RemoveAt(i);
                    }
                }
            }

            for (int i = 0; i < children.Count; i++)
            {
                if (children[i] is Node) (children[i] as Node).CleanUp(mask, caseSensitive);
            }
        }

        public override void Export(StringBuilder sb, int depth, ref bool endline)
        {
            if (name != "")
            {
                if (!endline)
                {
                    sb.AppendLine();
                }
                CreateDepth(sb, Depth);
                sb.Append(name);
                sb.Append("={");
                sb.AppendLine();
                endline = true;
            }
            else if (Depth > 0)
            {
                if (!endline)
                {
                    sb.AppendLine();
                }
                CreateDepth(sb, Depth);
                sb.Append("{");
                sb.AppendLine();
                endline = true;
            }

            foreach (var n in children)
            {
                n.Export(sb, Depth + 1, ref endline);
            }

            if (Depth > 0)
            {
                sb.AppendLine();
                CreateDepth(sb, Depth);
                sb.Append("}");
                endline = false;
            }
        }

        public override string ToString()
        {
            return name + "={";
        }

        internal KeyValuePair<string, string> GetAttribute(int index)
        {
            int idx = 0;
            for (int i = 0; i < children.Count; i++)
            {
                if (children[i] is Attribute)
                {
                    if (idx++ == index) return new KeyValuePair<string, string>(children[i].GetName(), children[i].GetValue());
                }
            }
            return new KeyValuePair<string, string>();
        }

        internal string GetAttributeName(string value)
        {
            if (value == null || value == "") return "";
            string v = value.ToLowerInvariant();
            return (children.Find(x => x is Attribute && String.Compare(x.GetValue(), v, true) == 0)?.GetName()) ?? "";
        }

        internal string GetAttributeName(int index)
        {
            int idx = 0;
            for (int i = 0; i < children.Count; i++)
            {
                if (children[i] is Attribute)
                {
                    if (idx++ == index) return children[i].GetName();
                }
            }
            return "";
        }

        internal string[] GetAttributeNames(string value)
        {
            List<string> output = new List<string>();
            string v = value.ToLowerInvariant();
            for (int i = 0; i < children.Count; i++)
            {
                if (children[i] is Attribute && String.Compare(children[i].GetValue(), v, true) == 0)
                {
                    output.Add(children[i].GetValue());
                }
            }
            return output.ToArray();
        }

        internal string[] GetAttributeNames()
        {
            List<string> output = new List<string>();
            for (int i = 0; i < children.Count; i++)
            {
                if (children[i] is Attribute)
                {
                    output.Add(children[i].GetName());
                }
            }
            return output.ToArray();
        }

        internal KeyValuePair<string, string>[] GetAttributes()
        {
            List<KeyValuePair<string, string>> output = new List<KeyValuePair<string, string>>();
            for (int i = 0; i < children.Count; i++)
            {
                if (children[i] is Attribute)
                {
                    output.Add(new KeyValuePair<string, string>(children[i].GetName(), children[i].GetValue()));
                }
            }
            return output.ToArray();
        }

        internal string GetAttributeValue(string name)
        {
            if (name == null || name == "") return "";
            string n = name.ToLowerInvariant();
            return (children.Find(x => x is Attribute && String.Compare(x.GetName(), n, true) == 0)?.GetValue()) ?? "";
        }

        internal string GetAttributeValue(int index)
        {
            int idx = 0;
            for (int i = 0; i < children.Count; i++)
            {
                if (children[i] is Attribute)
                {
                    if (idx++ == index) return children[i].GetValue();
                }
            }
            return "";
        }

        internal string[] GetAttributeValues(string name)
        {
            List<string> output = new List<string>();
            string n = name.ToLowerInvariant();
            for (int i = 0; i < children.Count; i++)
            {
                if (children[i] is Attribute && String.Compare(children[i].GetName(), n, true) == 0)
                {
                    output.Add(children[i].GetValue());
                }
            }
            return output.ToArray();
        }

        internal string[] GetAttributeValues()
        {
            List<string> output = new List<string>();
            for (int i = 0; i < children.Count; i++)
            {
                if (children[i] is Attribute)
                {
                    output.Add(children[i].GetValue());
                }
            }
            return output.ToArray();
        }

        internal string[] GetEntries()
        {
            List<string> output = new List<string>();
            for (int i = 0; i < children.Count; i++)
            {
                if (children[i] is Entry)
                {
                    output.Add(children[i].GetValue());
                }
            }
            return output.ToArray();
        }

        internal string GetEntry(int index)
        {
            int idx = 0;
            for (int i = 0; i < children.Count; i++)
            {
                if (children[i] is Entry)
                {
                    if (idx++ == index) return children[i].GetValue();
                }
            }
            return "";
        }

        internal override string GetName()
        {
            return name;
        }

        internal Node GetSubnode(int index)
        {
            //index = subnodes.Values.ElementAt(index);
            //return children[index] as Node;

            int idx = 0;
            for (int i = 0; i < children.Count; i++)
            {
                if (children[i] is Node)
                {
                    if (idx++ == index) return children[i] as Node;
                }
            }
            return null;
        }

        internal Node GetSubnode(string name)
        {
            if (subnodeCache != null)
            {
                if (!subnodeCache.ContainsKey(name)) return null;
                return children[subnodeCache[name]] as Node;
            }
            else
            {
                return children.Find(x => String.Compare(x.GetName(), name, StringComparison.OrdinalIgnoreCase) == 0) as Node;
            }
        }

        internal List<Node> GetSubnodes()
        {
            return children.FindAll(x => x is Node).Cast<Node>().ToList();
        }

        internal List<Node> GetSubnodes(string name)
        {
            return children.FindAll(x => x is Node && String.Compare(x.GetName(), name, StringComparison.OrdinalIgnoreCase) == 0).Cast<Node>().ToList();
        }

        internal override string GetValue()
        {
            return "";
        }

        internal bool HasAnAttribute(string name, string value)
        {
            return children.Exists(x => x is Attribute && x.GetName().ToLowerInvariant() == name.ToLowerInvariant() && x.GetValue().ToLowerInvariant() == value.ToLowerInvariant());
        }

        internal bool HasAnAttributeWithName(string name)
        {
            return children.Exists(x => x is Attribute && x.GetName().ToLowerInvariant() == name.ToLowerInvariant());
        }

        internal bool HasAnAttributeWithValue(string value)
        {
            return children.Exists(x => x is Attribute && x.GetValue().ToLowerInvariant() == value.ToLowerInvariant());
        }

        internal bool HasAnEntry(string value)
        {
            return children.Exists(x => x is Entry && x.GetValue().ToLowerInvariant() == value.ToLowerInvariant());
        }

        internal void PrepareCache()
        {
            if (children.Count > 256)
            {
                subnodeCache = new Dictionary<string, int>();
                for (int i = 0; i < children.Count; i++)
                {
                    if (!subnodeCache.ContainsKey(children[i].GetName().ToLowerInvariant()))
                        subnodeCache.Add(children[i].GetName().ToLowerInvariant(), i);
                }
            }
        }

        internal void RecurseSubnodes(List<Node> output)
        {
            var subnodes = GetSubnodes();
            output.AddRange(subnodes);
            foreach (var n in subnodes)
            {
                n.RecurseSubnodes(output);
            }
        }
    }
}