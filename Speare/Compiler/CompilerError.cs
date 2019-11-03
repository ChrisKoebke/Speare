using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Speare.Compiler
{
    public struct CompilerError
    {
        public int StartIndex;
        public int Length;
        public int LineNumber;

        public string Message;
    }
}
