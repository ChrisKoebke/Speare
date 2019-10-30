using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Speare.Compilation
{
    public enum Op : short
    {
        Null,
        PushScope,
        PopScope,
        Constant,
        Move,
        Compare,
        Jump,
        JumpIf,
        Call,
        Method,
        Return,
        Interop,
        Add,
        Subtract,
        Divide,
        Multiply,
        Modulo,
        Equal,
        Not,
        Exit,
        DebugPrint
    }
}
