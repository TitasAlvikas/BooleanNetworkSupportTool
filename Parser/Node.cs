using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BooleanNetworkSupportTool
{
    public class Node
    {
        private string value;

        public Node parent;

        public List<Node> children = new List<Node>();

        public Node() { }

        public Node(string newValue)
        {
            this.value = newValue;
        }

        public void AddChild(Node node)
        {
            this.children.Add(node);
            node.parent = this;
        }

        public void SetParent(Node node)
        {
            this.parent = node;
            node.AddChild(this);
        }

        public override string ToString()
        {
            return value;
        }
    }
}
