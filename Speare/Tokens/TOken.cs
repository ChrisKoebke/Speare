using Speare.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Speare.Tokens
{
    public class Token
    {
        public StringSpan Code;
        public TokenType Type;
        public int StartIndex;
        public int EndIndex;

        public StringSpan Content
        {
            get { return Code.Substring(StartIndex, EndIndex - StartIndex); }
        }

        public override string ToString()
        {
            if (Type == TokenType.EOF)
                return "EOF";

            var type = Type.ToString();

            return string.Format("{0}{1}'{2}'", type, "".PadLeft(24 - type.Length), Content.ToString());
        }
    }
}
