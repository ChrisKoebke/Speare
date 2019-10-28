using Speare.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Speare.Tokens
{
    public class Token : StringSpan
    {
        public TokenType Type { get; set; }
        
        public string ToFormattedString()
        {
            if (Type == TokenType.EOF)
                return "EOF";

            return string.Format("{0} '{1}'", Type.ToString(), base.ToString());
        }
    }
}
