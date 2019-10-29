using Speare.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Speare.Parsing
{
    public static unsafe class Lexer
    {
        private static Token[] _tokens = new Token[64 * 1024];

        public static void Allocate()
        {
            for (int i = 0; i < _tokens.Length; i++)
            {
                _tokens[i] = new Token();
            }
        }

        public static void SkipWhitespaceForwards(string code, ref int index)
        {
            while (index < code.Length && Chars.IsWhitespace(code[index]))
                index++;
        }

        public static void SkipWhitespaceBackwards(string code, ref int index)
        {
            while (index > 0 && Chars.IsWhitespace(code[index]))
                index--;
        }

        public static void AddToken(string code, ref int tokenIndex, ref int startIndex, int endIndex, TokenType type)
        {
            var token = _tokens[tokenIndex] ?? (_tokens[tokenIndex] = new Token());

            fixed (char* stringPointer = code)
            {
                token.Type = type;
                token.Span.StringPointer = stringPointer;
                token.Span.StartIndex = startIndex;
                token.Span.Length = endIndex - startIndex;
            }

            tokenIndex++;
            startIndex = endIndex;
        }

        public static void AddError(string code, ref int tokenIndex, ref int startIndex, int endIndex)
        {
            if (tokenIndex > 0)
            {
                var previous = _tokens[tokenIndex - 1];
                if (previous != null && previous.Type == TokenType.Error && previous.Span.EndIndex == startIndex)
                {
                    var span = previous.Span;
                    span.EndIndex = endIndex;
                    startIndex = endIndex;
                    return;
                }
            }

            AddToken(code, ref tokenIndex, ref startIndex, endIndex, TokenType.Error);
        }

        public static Token GetLastToken(int tokenIndex)
        {
            if (tokenIndex == 0)
                return null;

            while (tokenIndex > 0 && _tokens[--tokenIndex].Type == TokenType.Error)
                ;

            return _tokens[tokenIndex];
        }

        public static TokenType GetLastTokenType(int tokenIndex)
        {
            var token = GetLastToken(tokenIndex);
            return token != null ? token.Type : TokenType.None;
        }

        public static bool TokenizeBeginBlock(string code, ref int tokenIndex, ref int startIndex, ref int depth)
        {
            if (code[startIndex] != Chars.BeginBlock)
                return false;

            AddToken(code, ref tokenIndex, ref startIndex, startIndex + 1, TokenType.BeginBlock);
            depth++;

            return true;
        }

        public static bool TokenizeEndBlock(string code, ref int tokenIndex, ref int startIndex, ref int depth)
        {
            if (code[startIndex] != Chars.EndBlock)
                return false;

            AddToken(code, ref tokenIndex, ref startIndex, startIndex + 1, TokenType.EndBlock);
            depth--;

            return true;
        }

        public static bool TokenizeSpeaker(string code, ref int tokenIndex, ref int startIndex, ref int depth)
        {
            var index = startIndex;

            while (index < code.Length)
            {
                if (!Chars.IsIdentifier(code[startIndex]) || code[index] == Chars.NewLine)
                    return false;

                if (code[index] == Chars.SpeakerSeparator)
                {
                    AddToken(code, ref tokenIndex, ref startIndex, index, TokenType.Speaker);
                    startIndex++;

                    return true;
                }

                index++;
            }

            return false;
        }

        public static bool TokenizeSentence(string code, ref int tokenIndex, ref int startIndex, ref int depth)
        {
            if (GetLastTokenType(tokenIndex) != TokenType.Speaker)
                return false;

            var character = code[startIndex];
            if (Chars.IsWhitespace(character) || character == Chars.SpeakerSeparator)
                return false;

            var index = startIndex;

            while (index < code.Length)
            {
                if (code[index] == Chars.NewLine)
                {
                    AddToken(code, ref tokenIndex, ref startIndex, index, TokenType.Sentence);
                    return true;
                }
                if (code[index] == Chars.EndBlock)
                {
                    index--;

                    SkipWhitespaceBackwards(code, ref index);
                    AddToken(code, ref tokenIndex, ref startIndex, index, TokenType.Sentence);

                    return true;
                }

                index++;
            }

            return false;
        }

        public static bool TokenizeMethod(string code, ref int tokenIndex, ref int startIndex, ref int depth)
        {
            var index = startIndex;

            while (index < code.Length)
            {
                if (code[index] == Chars.BeginParameters && index > startIndex)
                {
                    AddToken(code, ref tokenIndex, ref startIndex, index, depth == 0 ? TokenType.MethodDefinition : TokenType.MethodCall);
                    return true;
                }

                if (!Chars.IsIdentifier(code[index]))
                    return false;

                index++;
            }

            return false;
        }

        public static bool TokenizeBeginParameters(string code, ref int tokenIndex, ref int startIndex, ref int depth)
        {
            if (code[startIndex] != Chars.BeginParameters)
                return false;

            AddToken(code, ref tokenIndex, ref startIndex, startIndex + 1, TokenType.BeginParameters);
            return true;
        }

        public static bool TokenizeEndParameters(string code, ref int tokenIndex, ref int startIndex, ref int depth)
        {
            if (code[startIndex] != Chars.EndParameters)
                return false;

            AddToken(code, ref tokenIndex, ref startIndex, startIndex + 1, TokenType.EndParameters);
            return true;
        }

        public static bool TokenizeParameterSeparator(string code, ref int tokenIndex, ref int startIndex, ref int depth)
        {
            if (code[startIndex] != Chars.ParameterSeparator)
                return false;

            AddToken(code, ref tokenIndex, ref startIndex, startIndex + 1, TokenType.ParameterSeparator);
            return true;
        }

        public static bool TokenizeBeginGameEvent(string code, ref int tokenIndex, ref int startIndex, ref int depth)
        {
            if (code[startIndex] != Chars.BeginGameEvent ||
                code[startIndex + 1] != Chars.BeginGameEvent)
                return false;

            AddToken(code, ref tokenIndex, ref startIndex, startIndex + 2, TokenType.BeginGameEvent);
            return true;
        }

        public static bool TokenizeEndGameEvent(string code, ref int tokenIndex, ref int startIndex, ref int depth)
        {
            if (code[startIndex] != Chars.EndGameEvent ||
                code[startIndex + 1] != Chars.EndGameEvent)
                return false;

            AddToken(code, ref tokenIndex, ref startIndex, startIndex + 2, TokenType.EndGameEvent);
            return true;
        }

        public static bool TokenizeIdentifier(string code, ref int tokenIndex, ref int startIndex, ref int depth)
        {
            if (!Chars.IsIdentifier(code[startIndex]))
                return false;

            var index = startIndex;
            while (Chars.IsIdentifier(code[index]))
                index++;

            AddToken(code, ref tokenIndex, ref startIndex, index, TokenType.Identifier);
            return true;
        }

        public static bool TokenizeInteger(string code, ref int tokenIndex, ref int startIndex, ref int depth)
        {
            if (!Chars.IsInteger(code[startIndex]))
                return false;

            var index = startIndex;
            while (Chars.IsInteger(code[index]))
                index++;

            AddToken(code, ref tokenIndex, ref startIndex, index, TokenType.Integer);
            return true;
        }

        public static Token[] Tokenize(string code)
        {
            int startIndex = 0,
                tokenIndex = 0,
                depth = 0;

            while (startIndex < code.Length)
            {
                var result = TokenizeBeginBlock(code, ref tokenIndex, ref startIndex, ref depth) ||
                             TokenizeEndBlock(code, ref tokenIndex, ref startIndex, ref depth) ||
                             TokenizeSpeaker(code, ref tokenIndex, ref startIndex, ref depth) ||
                             TokenizeSentence(code, ref tokenIndex, ref startIndex, ref depth) ||
                             TokenizeMethod(code, ref tokenIndex, ref startIndex, ref depth) ||
                             TokenizeBeginParameters(code, ref tokenIndex, ref startIndex, ref depth) ||
                             TokenizeEndParameters(code, ref tokenIndex, ref startIndex, ref depth) ||
                             TokenizeParameterSeparator(code, ref tokenIndex, ref startIndex, ref depth) ||
                             TokenizeBeginGameEvent(code, ref tokenIndex, ref startIndex, ref depth) ||
                             TokenizeEndGameEvent(code, ref tokenIndex, ref startIndex, ref depth) ||
                             TokenizeInteger(code, ref tokenIndex, ref startIndex, ref depth) ||
                             TokenizeIdentifier(code, ref tokenIndex, ref startIndex, ref depth);

                if (!result && !char.IsWhiteSpace(code[startIndex]))
                {
                    AddError(code, ref tokenIndex, ref startIndex, startIndex + 1);
                }
                else if (!result)
                {
                    startIndex++;
                }
            }

            AddToken(code, ref tokenIndex, ref startIndex, startIndex, TokenType.EOF);
            return _tokens;
        }
    }
}
