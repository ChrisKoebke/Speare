using Speare.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Speare.Compiler
{
    public struct CompilerError
    {
        public TokenType TokenType;
        public StringSpan Span;
        public int LineNumber;

        public string Message;
    }
}
