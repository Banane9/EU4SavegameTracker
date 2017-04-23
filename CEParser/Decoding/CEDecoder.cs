using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CEParser.Decoding;

namespace CEParser.Decoding
{
    internal abstract class CEDecoder
    {
        protected Stack<Node> hierarchy = new Stack<Node>();

        protected Nodesets nodesets = new Nodesets();

        public List<ParseError> Errors
        {
            get; private set;
        }

        public Game Game { get; }

        public Node Root
        {
            get; protected set;
        }

        public Dictionary<ushort, Node> this[string name]
        {
            get { return nodesets.Get(name); }
        }

        internal CEDecoder(Game game)
        {
            Errors = new List<ParseError>();

            Root = new Node();
            hierarchy.Push(Root);
        }

        public void CleanUp(string mask, bool caseSensitive)
        {
            Root.CleanUp(mask, caseSensitive);
        }

        public string Export()
        {
            StringBuilder sb = new StringBuilder(" ");
            bool endline = false;
            Root.Export(sb, 0, ref endline);
            sb.Remove(0, 2);
            return sb.ToString();
        }

        public abstract void Parse();

        public Task ParseAsync()
        {
            return Task.Run(() =>
            {
                Parse();
            });
        }

        internal static CEDecoder GetDecoder(Stream data)
        {
            var buffer = new byte[Game.MaxHeaderLength];

            data.Position = 0;
            data.Read(buffer, 0, buffer.Length);

            foreach (var game in Game.Games)
            {
                var txtHeader = game.Encoding.GetString(buffer, 0, game.TextHeaderLength);
                var binHeader = game.Encoding.GetString(buffer, 0, game.BinHeaderLength);

                if (game.TextHeader.Equals(txtHeader, StringComparison.InvariantCultureIgnoreCase))
                    return new CETextDecoder(data, game);

                if (game.BinHeader.Equals(binHeader, StringComparison.InvariantCultureIgnoreCase))
                    return new CEBinaryDecoder(data, game);
            }

            return null;
        }

        protected static StreamReader GetStreamReader(string path)
        {
            return new StreamReader(System.IO.File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), Encoding.GetEncoding(1250), true, 1024, true);
        }

        #region Nodeset methods

        public void AddNodeset(string name, Dictionary<ushort, Node> nodes)
        {
            nodesets.Add(name, nodes);
        }

        public Dictionary<ushort, Node> GetNodeset(string name)
        {
            return nodesets.Get(name);
        }

        public void RemoveNodeset(string name)
        {
            nodesets.Remove(name);
        }

        #endregion Nodeset methods

        #region Accessor methods

        public string GetAttributeName(Node parent, string value)
        {
            if (parent == null) return "";
            return parent.GetAttributeName(value);
        }

        public string GetAttributeName(Node parent, int index)
        {
            if (parent == null) return "";
            return parent.GetAttributeName(index);
        }

        public string[] GetAttributeNames(Node parent, string value)
        {
            if (parent == null) return new string[0];
            return parent.GetAttributeNames(value);
        }

        public string[] GetAttributeNames(Node parent)
        {
            if (parent == null) return new string[0];
            return parent.GetAttributeNames();
        }

        public KeyValuePair<string, string>[] GetAttributes(Node parent)
        {
            if (parent == null) return new KeyValuePair<string, string>[0];
            return parent.GetAttributes();
        }

        public string GetAttributeValue(Node parent, string name)
        {
            if (parent == null) return "";
            return parent.GetAttributeValue(name);
        }

        public string GetAttributeValue(Node parent, int index)
        {
            if (parent == null) return "";
            return parent.GetAttributeValue(index);
        }

        public string[] GetAttributeValues(Node parent, string name)
        {
            if (parent == null) return new string[0];
            return parent.GetAttributeValues(name);
        }

        public string[] GetAttributeValues(Node parent)
        {
            if (parent == null) return new string[0];
            return parent.GetAttributeValues();
        }

        public string[] GetEntries(Node parent)
        {
            if (parent == null) return new string[0];
            return parent.GetEntries();
        }

        public string GetEntry(Node parent, int index)
        {
            if (parent == null) return "";
            return parent.GetEntry(index);
        }

        public Node GetSubnode(Node parent, params string[] path)
        {
            if (parent == null) return null;
            Node n = parent;
            for (int i = 0; i < path.Length; i++)
            {
                if (n == null) return null;
                n = n.GetSubnode(path[i].ToLowerInvariant());
            }
            return n;
        }

        public Node GetSubnode(Node parent, Predicate<Node> match)
        {
            if (parent == null) return null;
            List<Node> container = parent.GetSubnodes();
            return container.Find(match);
        }

        public Node GetSubnode(Node parent, int index)
        {
            if (parent == null) return null;
            return parent.GetSubnode(index);
        }

        public List<Node> GetSubnodes(Node parent)
        {
            if (parent == null) return new List<Node>();
            return parent.GetSubnodes();
        }

        public List<Node> GetSubnodes(Node parent, params string[] path)
        {
            if (parent == null) return new List<Node>();
            Node n = parent;
            for (int i = 0; i < path.Length - 1; i++)
            {
                if (n == null) return new List<Node>();
                n = n.GetSubnode(path[i].ToLowerInvariant());
            }
            if (n == null) return new List<Node>();

            if (path[path.Length - 1] == "*")
                return n.GetSubnodes();
            else
                return n.GetSubnodes(path[path.Length - 1].ToLowerInvariant());
        }

        public List<Node> GetSubnodes(Node parent, Predicate<Node> match)
        {
            if (parent == null) return new List<Node>();
            List<Node> container = parent.children.FindAll(x => x is Node).Cast<Node>().ToList();
            return container.FindAll(match);
        }

        public List<Node> GetSubnodes(Node parent, bool recursive)
        {
            if (!recursive) GetSubnodes(parent);
            List<Node> output = new List<Node>();

            parent.RecurseSubnodes(output);

            return output;
        }

        #endregion Accessor methods

        #region Helper methods

        public int GetDepth(Node n)
        {
            return (n?.Depth) ?? -1;
        }

        public string GetNodeName(Node n)
        {
            return (n?.GetName()) ?? "";
        }

        public Node GetParent(Node n)
        {
            return n?.Parent;
        }

        public int GetSubnodesCount(Node n)
        {
            return (n?.GetSubnodes().Count) ?? 0;
        }

        public int GetSubnodesCount(Node n, Func<Node, bool> match)
        {
            return (n?.GetSubnodes().Count(match)) ?? 0;
        }

        // A top entity in the hierarchical tree.
        public Node GetTopParent(Node n)
        {
            while (HasAParent(n))
            {
                n = n.Parent;
            }
            return n;
        }

        /// <summary>
        /// Returns true if a given entity has any child (any other node beneath in hierarchy)
        /// </summary>
        /// <returns>True if the entity has a child, otherwise false</returns>
        public bool HasAContainer(Node node)
        {
            if (node == null) return false;
            return node.children.Exists(x => x is Node);
        }

        /// <summary>
        /// Returns true if a given entity has a child with a given name (a node beneath in hierarchy)
        /// </summary>
        /// <returns>True if the entity has a child with a given name, otherwise false</returns>
        public bool HasAContainer(Node node, string name)
        {
            if (node == null) return false;
            return node.children.Exists(x => x is Node && x.GetName().ToLowerInvariant() == name.ToLowerInvariant());
        }

        public bool HasAnAttribute(Node node, string name, string value)
        {
            if (node == null) return false;
            return node.HasAnAttribute(name, value);
        }

        public bool HasAnAttributeWithName(Node node, string name)
        {
            if (node == null) return false;
            return node.HasAnAttributeWithName(name);
        }

        public bool HasAnAttributeWithValue(Node node, string value)
        {
            if (node == null) return false;
            return node.HasAnAttributeWithValue(value);
        }

        public bool HasAnEntry(Node node, string value)
        {
            if (node == null) return false;
            return node.HasAnEntry(value);
        }

        /// <summary>
        /// Returns true if the entity has a parent, otherwise false (it is the highest-order entity).
        /// </summary>
        /// <returns>True if the entity has a parent, otherwise false</returns>
        public bool HasAParent(Node node)
        {
            if (node == null) return false;
            return node.Depth > 1;
        }

        /// <summary>
        /// Returns true if the entity has a parent and this parent has a given name, otherwise false.
        /// </summary>
        /// <returns>True if the entity has a parent of a given name, otherwise false</returns>
        public bool HasAParent(Node node, string name)
        {
            if (node == null) return false;
            return node.Depth > 1 && node.Parent.GetName().ToLowerInvariant() == name.ToLowerInvariant();
        }

        /// <summary>
        /// Returns true if the entity has a parent and this parent has a given name, counting in the
        /// hierarchy above by the given number of levels, otherwise false.
        /// </summary>
        /// <returns>True if the entity has a parent of a given name up in the hierarchy, otherwise false</returns>
        public bool HasAParent(Node node, string name, int levels)
        {
            if (levels == 1)
                return HasAParent(node, name);
            else if (node.Depth > 1)
                return HasAParent(node.Parent, name, levels - 1);
            else
                return false;
        }

        /// <summary>
        /// Returns true if the entity has a parent, counting in the hierarchy above by the given number of levels, otherwise false.
        /// </summary>
        /// <returns>True if the entity has a parent up in the hierarchy, otherwise false</returns>
        public bool HasAParent(Node node, int levels)
        {
            if (node == null) return false;
            return node.Depth - 1 >= levels;
        }

        public bool PathExists(Node fromNode, bool lastNodeIsAttribute, params string[] subnodes)
        {
            Node parent = null;
            for (int i = 0; i < subnodes.Length; i++)
            {
                parent = fromNode;

                if (fromNode == null)
                {
                    if (lastNodeIsAttribute && i == subnodes.Length - 1)
                        if (!HasAnAttributeWithName(parent, subnodes[i]))
                            return false;
                        else
                            return true;
                    return false;
                }
                fromNode = GetSubnode(fromNode, subnodes[i]);
            }
            return true;
        }

        public bool PathExists(bool lastNodeIsAttribute, params string[] subnodes)
        {
            return PathExists(Root, lastNodeIsAttribute, subnodes);
        }

        #endregion Helper methods

        protected void AddError(int position, string error, string details, int score)
        {
            Errors.Add(new ParseError(position, error, details, score));
        }

        protected void AddError(int position, string error, string details, int score, Entity entity)
        {
            Errors.Add(new ParseError(position, error, details, score, entity));
        }

        #region Event handling

        // File parse progress

        internal void OnFileParseProgress(double progress)
        {
            FileParseProgress?.Invoke(this, new SaveDecodingProgressEventArgs(progress));
        }

        public event FileParseProgressHandler FileParseProgress;

        public delegate void FileParseProgressHandler(object source, SaveDecodingProgressEventArgs e);

        #endregion Event handling
    }
}