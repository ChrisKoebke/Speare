using Speare.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Speare.Parser
{
    public class Token
    {
        public TokenType Type;
        public int LineNumber;

        public StringSpan Span = new StringSpan();

        public string ToFormattedString()
        {
            if (Type == TokenType.EndOfFile)
                return "EOF";

            return string.Format("{0} '{1}'", Type.ToString(), Span.ToString());
        }

        public override string ToString()
        {
            return Span.ToString();
        }
    }
}
