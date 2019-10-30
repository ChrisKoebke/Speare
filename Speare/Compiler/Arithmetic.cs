using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Speare.Compiler
{
    public enum Arithmetic : byte
    {
        Equals,
        NotEquals,
        GreaterThan,
        LessThan,
        GreaterThanOrEqual,
        LessThanOrEqual,
        Add,
        Subtract,
        Divide,
        Multiply,
        Modulo
    }
}
