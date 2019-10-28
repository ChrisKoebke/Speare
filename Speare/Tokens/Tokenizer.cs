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

        public static void PreAllocate()
        {
            for (int i = 0; i < _tokens.Length; i++)
            {
                _tokens[i] = new Token();
            }
        }

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

        public static void SkipWhitespaceForwards(string code, ref int index)
        {
            while (index < code.Length && IsWhitespace(code[index]))
                index++;
        }

        public static void SkipWhitespaceBackwards(string code, ref int index)
        {
            while (index > 0 && IsWhitespace(code[index]))
                index--;
        }

        public static void Token(string code, ref int tokenIndex, ref int startIndex, int endIndex, TokenType type)
        {
            var token = _tokens[tokenIndex] ?? (_tokens[tokenIndex] = new Token());

            token.Value = code;
            token.Type = type;
            token.StartIndex = startIndex;
            token.EndIndex = endIndex;

            tokenIndex++;
            startIndex = endIndex;
        }

        public static void Error(string code, ref int tokenIndex, ref int startIndex, int endIndex)
        {
            if (tokenIndex > 0)
            {
                var previous = _tokens[tokenIndex - 1];
                if (previous != null && previous.Type == TokenType.Error && previous.EndIndex == startIndex)
                {
                    previous.EndIndex = endIndex;
                    startIndex = endIndex;
                    return;
                }
            }

            Token(code, ref tokenIndex, ref startIndex, endIndex, TokenType.Error);
        }

        public static Token LastToken(int tokenIndex)
        {
            if (tokenIndex == 0)
                return null;

            while (tokenIndex > 0 && _tokens[--tokenIndex].Type == TokenType.Error)
                ;

            return _tokens[tokenIndex];
        }

        public static TokenType LastTokenType(int tokenIndex)
        {
            var token = LastToken(tokenIndex);
            return token != null ? token.Type : TokenType.None;
        }

        public static bool ParseBeginBlock(string code, ref int tokenIndex, ref int startIndex, ref int depth)
        {
            if (code[startIndex] != Chars.BeginBlock)
            {
                return false;
            }

            Token(code, ref tokenIndex, ref startIndex, startIndex + 1, TokenType.BeginBlock);
            depth++;

            return true;
        }

        public static bool ParseEndBlock(string code, ref int tokenIndex, ref int startIndex, ref int depth)
        {
            if (code[startIndex] != Chars.EndBlock)
            {
                return false;
            }

            Token(code, ref tokenIndex, ref startIndex, startIndex + 1, TokenType.EndBlock);
            depth--;

            return true;
        }

        public static bool ParseSpeaker(string code, ref int tokenIndex, ref int startIndex, ref int depth)
        {
            var index = startIndex;

            while (index < code.Length)
            {
                if (!IsIdentifier(code[startIndex]) || code[index] == Chars.NewLine)
                    return false;

                if (code[index] == Chars.SpeakerSeparator)
                {
                    Token(code, ref tokenIndex, ref startIndex, index, TokenType.Speaker);
                    startIndex++;

                    return true;
                }

                index++;
            }

            return false;
        }

        public static bool ParseSentence(string code, ref int tokenIndex, ref int startIndex, ref int depth)
        {
            if (LastTokenType(tokenIndex) != TokenType.Speaker)
                return false;

            var character = code[startIndex];
            if (character == ' ' || character == '\t' || character == Chars.SpeakerSeparator)
                return false;

            var index = startIndex;

            while (index < code.Length)
            {
                if (code[index] == Chars.NewLine)
                {
                    Token(code, ref tokenIndex, ref startIndex, index, TokenType.Sentence);
                    return true;
                }
                if (code[index] == Chars.EndBlock)
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

        public static bool ParseMethod(string code, ref int tokenIndex, ref int startIndex, ref int depth)
        {
            var index = startIndex;
            while (index < code.Length)
            {
                if (code[index] == Chars.BeginParameters && index > startIndex)
                {
                    Token(code, ref tokenIndex, ref startIndex, index, depth == 0 ? TokenType.MethodDefinition : TokenType.MethodCall);
                    return true;
                }

                if (!IsIdentifier(code[index]))
                    return false;

                index++;
            }

            return false;
        }

        public static bool ParseBeginParameters(string code, ref int tokenIndex, ref int startIndex, ref int depth)
        {
            if (code[startIndex] != Chars.BeginParameters)
                return false;

            Token(code, ref tokenIndex, ref startIndex, startIndex + 1, TokenType.BeginParameters);
            return true;
        }

        public static bool ParseEndParameters(string code, ref int tokenIndex, ref int startIndex, ref int depth)
        {
            if (code[startIndex] != Chars.EndParameters)
                return false;

            Token(code, ref tokenIndex, ref startIndex, startIndex + 1, TokenType.EndParameters);
            return true;
        }

        public static bool ParseParameterSeparator(string code, ref int tokenIndex, ref int startIndex, ref int depth)
        {
            if (code[startIndex] != Chars.ParameterSeparator)
                return false;

            Token(code, ref tokenIndex, ref startIndex, startIndex + 1, TokenType.ParameterSeparator);
            return true;
        }

        public static bool ParseBeginGameEvent(string code, ref int tokenIndex, ref int startIndex, ref int depth)
        {
            if (code[startIndex] != Chars.BeginGameEvent ||
                code[startIndex + 1] != Chars.BeginGameEvent)
                return false;

            Token(code, ref tokenIndex, ref startIndex, startIndex + 2, TokenType.BeginGameEvent);
            return true;
        }

        public static bool ParseEndGameEvent(string code, ref int tokenIndex, ref int startIndex, ref int depth)
        {
            if (code[startIndex] != Chars.EndGameEvent ||
                code[startIndex + 1] != Chars.EndGameEvent)
                return false;

            Token(code, ref tokenIndex, ref startIndex, startIndex + 2, TokenType.EndGameEvent);
            return true;
        }

        public static bool ParseIdentifier(string code, ref int tokenIndex, ref int startIndex, ref int depth)
        {
            if (!IsIdentifier(code[startIndex]))
                return false;

            var index = startIndex;
            while (IsIdentifier(code[index]))
                index++;

            Token(code, ref tokenIndex, ref startIndex, index, TokenType.Identifier);
            return true;
        }

        public static bool ParseInteger(string code, ref int tokenIndex, ref int startIndex, ref int depth)
        {
            if (!IsInteger(code[startIndex]))
                return false;

            var index = startIndex;
            while (IsInteger(code[index]))
                index++;

            Token(code, ref tokenIndex, ref startIndex, index, TokenType.Integer);
            return true;
        }

        public static Token[] Parse(string code)
        {
            int startIndex = 0,
                tokenIndex = 0,
                depth = 0;

            while (startIndex < code.Length)
            {
                var result = ParseBeginBlock(code, ref tokenIndex, ref startIndex, ref depth) ||
                             ParseEndBlock(code, ref tokenIndex, ref startIndex, ref depth) ||
                             ParseSpeaker(code, ref tokenIndex, ref startIndex, ref depth) ||
                             ParseSentence(code, ref tokenIndex, ref startIndex, ref depth) ||
                             ParseMethod(code, ref tokenIndex, ref startIndex, ref depth) ||
                             ParseBeginParameters(code, ref tokenIndex, ref startIndex, ref depth) ||
                             ParseEndParameters(code, ref tokenIndex, ref startIndex, ref depth) ||
                             ParseParameterSeparator(code, ref tokenIndex, ref startIndex, ref depth) ||
                             ParseBeginGameEvent(code, ref tokenIndex, ref startIndex, ref depth) ||
                             ParseEndGameEvent(code, ref tokenIndex, ref startIndex, ref depth) ||
                             ParseInteger(code, ref tokenIndex, ref startIndex, ref depth) ||
                             ParseIdentifier(code, ref tokenIndex, ref startIndex, ref depth);

                if (!result && !char.IsWhiteSpace(code[startIndex]))
                {
                    Error(code, ref tokenIndex, ref startIndex, startIndex + 1);
                }
                else if (!result)
                {
                    startIndex++;
                }
            }

            Token(code, ref tokenIndex, ref startIndex, startIndex, TokenType.EOF);
            return _tokens;
        }
    }
}
