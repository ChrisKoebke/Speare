using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Speare.Ops
{
    public enum OpCode
    {
        None,
        PushScope,
        PopScope,
        Constant,
        Store,
        Load,
        Call,
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
