using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Speare.Ops
{
    public enum OpCode : short
    {
        None,
        PushScope,
        PopScope,
        Constant,
        Move,
        Load,
        Call,
        Interop,
        Jump,
        Add,
        Subtract,
        Divide,
        Multiply,
        Modulo,
        Equals,
        LargerThan,
        SmallerThan,
        LargerOrEqualThan,
        SmallerOrEqualThan,
        NotEqual,
        Not,
        DebugPrint
    }
}
