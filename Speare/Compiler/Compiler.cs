using Speare.Parser;
using Speare.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Speare.Compiler
{
    public static class Compiler
    {
        private static Token[] _tokens;
        private static int _tokenIndex;

        private static Token _previous;
        private static Token _current;

        private static OpBuilder _ops = new OpBuilder();
        private static Dictionary<string, int> _methods = new Dictionary<string, int>();
        private static Stack<Token> _stack = new Stack<Token>();
        private static List<CompilerError> _errors = new List<CompilerError>();

        private static void Clear()
        {
            _ops.Clear();
            _errors.Clear();
            _methods.Clear();
            _stack.Clear();
        }

        private static void MoveNext()
        {
            _previous = _current;
            _current = _tokens[_tokenIndex++];
        }

        private static void Error(Token token, string message)
        {
            _errors.Add(new CompilerError
            {
                StartIndex = token.Span.StartIndex,
                Length = token.Span.Length,
                LineNumber = token.LineNumber,
                Message = string.Format("Invalid '{0}' in line {1}: {2}", token.ToString(), token.LineNumber, message)
            });
        }

        private static bool Assert(TokenType type, string message)
        {
            if (_current.Type != type)
            {
                Error(_current, message);
                return false;
            }

            MoveNext();
            return true;
        }

        private static bool AssertAny(TokenType[] types, string message)
        {
            if (!types.Contains(_current.Type))
            {
                Error(_current, message);
                return false;
            }

            MoveNext();
            return true;
        }

        private static void FindMethods()
        {
            int depth = 0;

            for (int i = 0; i < _tokens.Length; i++)
            {
                switch (_tokens[i].Type)
                {
                    case TokenType.BeginBlock:
                        depth++;
                        break;
                    case TokenType.EndBlock:
                        depth--;
                        break;
                    case TokenType.MethodName:
                        if (depth == 0)
                        {
                            _methods[_tokens[i].Span.ToString()] = _methods.Count;
                        }
                        break;
                    case TokenType.EndOfFile:
                        return;
                }
            }
        }

        private static void CompileInteger()
        {

        }

        private static void CompileMethodCall()
        {
            Assert(TokenType.MethodName, "Method expected.");
            Assert(TokenType.OpenParenthesis, "'(' expected.");

            while (_current.Type != TokenType.CloseParenthesis && _current.Type != TokenType.EndOfFile)
            {

            }

            Assert(TokenType.CloseParenthesis, "')' expected.");
        }

        private static void CompileMethodDefinition()
        {
            Assert(TokenType.BeginBlock, "'{' expected.");

            while (_current.Type != TokenType.EndBlock && _current.Type != TokenType.EndOfFile)
            {
                switch (_current.Type)
                {
                    case TokenType.MethodName:
                        CompileMethodCall();
                        break;
                }

                MoveNext();
            }

            Assert(TokenType.EndBlock, "'}' expected.");
        }

        public static void Compile(Token[] tokens, out OpBuilder ops, out List<CompilerError> errors)
        {
            _tokens = tokens;

            Clear();
            FindMethods();
            
            while (true)
            {
                MoveNext();
                if (_current.Type == TokenType.EndOfFile)
                    break;

                if (Assert(TokenType.MethodName, "Only methods are allowed on root level."))
                {
                    CompileMethodDefinition();
                }
                else
                {
                    _tokenIndex++;
                }
            }
            
            ops = _ops;
            errors = _errors;
        }
    }
}
