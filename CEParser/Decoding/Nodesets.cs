using System;
using System.Collections.Generic;
using System.Linq;

namespace CEParser.Decoding
{
    public class Nodesets
    {
        private Dictionary<string, Dictionary<ushort, Node>> nodesets = new Dictionary<string, Dictionary<ushort, Node>>();

        public void Add(string name, Dictionary<ushort, Node> nodeset)
        {
            if (nodesets.ContainsKey(name))
                nodesets.Remove(name);

            nodesets.Add(name, nodeset);
        }

        public Dictionary<ushort, Node> Get(string name)
        {
            Dictionary<ushort, Node> output;
            nodesets.TryGetValue(name, out output);
            return output ?? new Dictionary<ushort, Node>();
        }

        public void Remove(string name)
        {
            if (nodesets.ContainsKey(name))
                nodesets.Remove(name);
        }
    }
}