using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Speare.Parsing
{
    public static class Chars
    {
        public static char BeginBlock = '{';
        public static char EndBlock = '}';
        public static char SpeakerSeparator = ':';
        public static char NewLine = '\n';
        public static char BeginParameters = '(';
        public static char EndParameters = ')';
        public static char ParameterSeparator = ',';
        public static char BeginGameEvent = '<';
        public static char EndGameEvent = '>';

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

        public static bool IsFloat(char character)
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
    }
}
