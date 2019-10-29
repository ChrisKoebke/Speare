using Speare.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Speare.Nodes
{
    public abstract class Node
    {
        public List<Node> Children { get; private set; } = new List<Node>();
    }
}
