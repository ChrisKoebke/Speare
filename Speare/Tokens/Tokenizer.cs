using Speare.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Speare.Tokens
{
    public static class Tokenizer
    {
        private static Token[] _tokens = new Token[32 * 1024];

        public static bool IsAlphabetLowercase(char character)
        {
            return character >= 'a' && character <= 'z';
        }

        public static bool IsAlphabetUppercase(char character)
        {
            return character >= 'A' && character <= 'Z';
        }

        public static bool IsInteger(char character)
        {
            return character >= '0' && character <= '9';
        }

        public static bool IsDouble(char character)
        {
            return (character >= '0' && character <= '9') || character == '.';
        }

        public static bool IsIdentifier(char character)
        {
            return IsAlphabetUppercase(character) || IsAlphabetLowercase(character) || IsInteger(character) || character == '_';
        }

        public static bool IsWhitespace(char character)
        {
            return character == ' ' || character == '\t';
        }

        public static void SkipWhitespaceForwards(StringSpan code, ref int index)
        {
            while (index < code.Length && IsWhitespace(code[index]))
                index++;
        }

        public static void SkipWhitespaceBackwards(StringSpan code, ref int index)
        {
            while (index > 0 && IsWhitespace(code[index]))
                index--;
        }

        public static void Token(StringSpan code, ref int tokenIndex, ref int startIndex, int endIndex, TokenType type)
        {
            var token = _tokens[tokenIndex] ?? (_tokens[tokenIndex] = new Token());

            token.Code = code;
            token.Type = type;
            token.StartIndex = startIndex;
            token.EndIndex = endIndex;

            tokenIndex++;
            startIndex = endIndex;
        }

        public static Token LastToken(int tokenIndex)
        {
            if (tokenIndex == 0)
                return null;

            return _tokens[tokenIndex - 1];
        }

        public static TokenType LastTokenType(int tokenIndex)
        {
            var token = LastToken(tokenIndex);
            return token != null ? token.Type : TokenType.None;
        }

        public static bool ParseBeginBlock(StringSpan code, ref int tokenIndex, ref int startIndex)
        {
            if (code[startIndex] != '{')
            {
                return false;
            }

            Token(code, ref tokenIndex, ref startIndex, startIndex + 1, TokenType.BeginBlock);
            return true;
        }

        public static bool ParseEndBlock(StringSpan code, ref int tokenIndex, ref int startIndex)
        {
            if (code[startIndex] != '}')
            {
                return false;
            }

            Token(code, ref tokenIndex, ref startIndex, startIndex + 1, TokenType.EndBlock);
            return true;
        }

        public static bool ParseSpeaker(StringSpan code, ref int tokenIndex, ref int startIndex)
        {
            var index = startIndex;

            while (index < code.Length)
            {
                if (!IsIdentifier(code[startIndex]) || code[index] == '\n')
                    return false;

                if (code[index] == ':')
                {
                    Token(code, ref tokenIndex, ref startIndex, index, TokenType.Speaker);
                    return true;
                }

                index++;
            }

            return false;
        }

        public static bool ParseSentence(StringSpan code, ref int tokenIndex, ref int startIndex)
        {
            if (LastTokenType(tokenIndex) != TokenType.Speaker)
                return false;

            var character = code[startIndex];
            if (character == ' ' || character == ':' || character == '\t')
                return false;

            var index = startIndex;

            while (index < code.Length)
            {
                if (code[index] == '\n')
                {
                    Token(code, ref tokenIndex, ref startIndex, index, TokenType.Sentence);
                    return true;
                }
                if (code[index] == '}')
                {
                    index--;

                    SkipWhitespaceBackwards(code, ref index);
                    Token(code, ref tokenIndex, ref startIndex, index, TokenType.Sentence);

                    return true;
                }

                index++;
            }

            return false;
        }

        public static bool ParseMethodName(StringSpan code, ref int tokenIndex, ref int startIndex)
        {
            var index = startIndex;
            while (index < code.Length)
            {
                if (code[index] == '(' && index > startIndex)
                {
                    Token(code, ref tokenIndex, ref startIndex, index, TokenType.MethodName);
                    return true;
                }

                if (!IsIdentifier(code[index]))
                    return false;

                index++;
            }

            return false;
        }

        public static bool ParseMethodParameters(StringSpan code, ref int tokenIndex, ref int startIndex)
        {
            if (LastTokenType(tokenIndex) != TokenType.MethodName || code[startIndex] != '(')
                return false;

            // Skip open bracket and white space
            startIndex++;
            SkipWhitespaceForwards(code, ref startIndex);

            var index = startIndex;
            var tokenCount = 0;

            while (index < code.Length)
            {
                if (code[index] == ')' || code[index] == ',')
                {
                    var endIndex = index - 1;
                    SkipWhitespaceBackwards(code, ref endIndex);

                    Token(code, ref tokenIndex, ref startIndex, endIndex + 1, TokenType.MethodParameter);
                    tokenCount++;

                    if (code[index] != ')')
                    {
                        // Skip semicolon and white spaces
                        index++;
                        SkipWhitespaceForwards(code, ref index);
                    }
                    else
                    {
                        return true;
                    }

                    startIndex = index;
                }
                else if (!IsWhitespace(code[index]))
                {
                    return false;
                }

                index++;
            }

            return false;
        }

        public static Token[] Parse(string code)
        {
            var span = code.ToSpan();

            int startIndex = 0,
                tokenIndex = 0;

            while (startIndex < span.Length)
            {
                var result = ParseBeginBlock(span, ref tokenIndex, ref startIndex) ||
                             ParseEndBlock(span, ref tokenIndex, ref startIndex) ||
                             ParseSpeaker(span, ref tokenIndex, ref startIndex) ||
                             ParseSentence(span, ref tokenIndex, ref startIndex) ||
                             ParseMethodName(span, ref tokenIndex, ref startIndex) ||
                             ParseMethodParameters(span, ref tokenIndex, ref startIndex);

                if (!result)
                {
                    startIndex++;
                }
            }

            Token(span, ref tokenIndex, ref startIndex, startIndex, TokenType.EOF);
            return _tokens;
        }
    }
}
