using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Speare.Compiler
{
    public enum Op : short
    {
        Null,
        PushScope,
        PopScope,
        Constant,
        Set,
        GlobalRead,
        GlobalWrite,
        Compare,
        Jump,
        JumpIf,
        Call,
        Interop,
        Return,
        MethodDefinition,
        Arithmetic,
        Exit,
        DebugPrint
    }
}
