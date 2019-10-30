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
        public StringSpan Span = new StringSpan();
        public TokenType Type;
        
        public string ToFormattedString()
        {
            if (Type == TokenType.EOF)
                return "EOF";

            return string.Format("{0} '{1}'", Type.ToString(), base.ToString());
        }

        public override string ToString()
        {
            return Span.ToString();
        }
    }
}
