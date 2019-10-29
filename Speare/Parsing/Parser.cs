using Speare.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Speare.Parsing
{
    public static class Parser
    {
        public static Node Parse(Token[] tokens)
        {
            var root = new Root();
            var stack = new Stack<Node>();
            stack.Push(root);

            for (int i = 0; i < tokens.Length; i++)
            {
                var current = stack.Peek();

                switch (tokens[i].Type)
                {
                    case TokenType.None:
                        break;
                    case TokenType.BeginBlock:
                        break;
                    case TokenType.EndBlock:
                        break;
                    case TokenType.Speaker:
                        break;
                    case TokenType.Sentence:
                        break;
                    case TokenType.MethodDefinition:
                        break;
                    case TokenType.MethodCall:
                        break;
                    case TokenType.Identifier:
                        break;
                    case TokenType.ParameterSeparator:
                        break;
                    case TokenType.BeginParameters:
                        break;
                    case TokenType.EndParameters:
                        break;
                    case TokenType.BeginGameEvent:
                        break;
                    case TokenType.EndGameEvent:
                        break;
                    case TokenType.GameEvent:
                        break;
                    case TokenType.Integer:
                        current.Children.Add(new Constant(tokens[i].Span.ToInt32()));
                        break;
                    case TokenType.Float:
                        current.Children.Add(new Constant(tokens[i].Span.ToFloat()));
                        break;
                    case TokenType.String:
                        break;
                    case TokenType.Error:
                        break;
                    case TokenType.EOF:
                        break;
                }
            }
            return root;
        }
    }
}
