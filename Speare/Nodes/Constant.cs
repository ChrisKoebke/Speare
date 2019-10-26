using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Speare.Nodes
{
    public class Constant : Node
    {
        public Constant(object value)
        {
            Value = value;
        }

        public object Value { get; private set; }
    }
}
