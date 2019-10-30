using Speare.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Speare.Compiler
{
    public class Compiler
    {
        public OpBuilder Compile(Token[] tokens)
        {
            var builder = new OpBuilder();
            var index = 0;

            while (true)
            {
                if (tokens[index].Type == TokenType.EOF)
                    break;

                switch (tokens[index].Type)
                {
                    case TokenType.Method:
                        break;
                }
            }

            return builder;
        }
    }
}
