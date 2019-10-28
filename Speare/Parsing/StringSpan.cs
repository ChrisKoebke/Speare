using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Speare.Parsing
{
    public enum StringSpanCase
    {
        Default,
        Uppercase,
        Lowercase
    }

    public class StringSpan
    {
        static MemberInfo StringBuilderValue = typeof(StringBuilder).GetMember("m_StringValue").FirstOrDefault();

        internal StringSpan(string value, int startIndex, int length) : this(new StringBuilder(value), startIndex, length)
        {
        }

        internal StringSpan(StringBuilder buffer, int startIndex, int length)
        {
            _buffer = buffer;
            StartIndex = startIndex;
            Length = length;
        }

        private StringBuilder _buffer;

        public int StartIndex { get; private set; }
        public int Length { get; private set; }
        public StringSpanCase Case { get; private set; }

        public char this[int index]
        {
            get
            {
                var value = _buffer[index + StartIndex];

                if (Case == StringSpanCase.Uppercase && value >= 'a' && value <= 'z')
                {
                    return (char)(value & ~0x20);
                }
                else if (Case == StringSpanCase.Lowercase && value >= 'A' && value <= 'Z')
                {
                    return (char)(value | 0x20);
                }

                return value;
            }
        }

        public IEnumerable<StringSpan> Split(char character)
        {
            var startIndex = 0;
            for (int i = 0; i < Length; i++)
            {
                if (this[i] == character)
                {
                    yield return new StringSpan(_buffer, StartIndex + startIndex, i - startIndex);
                    startIndex = i + 1;
                }
            }

            yield return new StringSpan(_buffer, StartIndex + startIndex, Length - startIndex);
        }

        public StringSpan TrimStart()
        {
            return TrimStart(' ', '\t', '\r');
        }

        public StringSpan TrimStart(params char[] characters)
        {
            var delta = 0;

            for (int j = 0; j < characters.Length; j++)
            {
                for (int i = 0; i < Length; i++)
                {
                    if (this[i] != characters[j])
                        break;

                    delta++;
                }
            }

            return new StringSpan(_buffer, StartIndex + delta, Length - delta);
        }

        public StringSpan TrimEnd()
        {
            return TrimEnd(' ', '\t', '\r');
        }

        public StringSpan TrimEnd(params char[] characters)
        {
            var delta = 0;
            for (int j = 0; j < characters.Length; j++)
            {
                for (int i = Length - 1; i >= 0; i--)
                {
                    if (this[i] != characters[j])
                        break;

                    delta++;
                }
            }

            return new StringSpan(_buffer, StartIndex, Length - delta);
        }

        public StringSpan Trim()
        {
            return Trim(' ', '\t', '\r');
        }

        public StringSpan Trim(params char[] characters)
        {
            var startDelta = 0;
            var endDelta = 0;

            for (int j = 0; j < characters.Length; j++)
            {
                for (int i = 0; i < Length; i++)
                {
                    if (this[i] != characters[j])
                        break;

                    startDelta++;
                }
            }
            for (int j = 0; j < characters.Length; j++)
            {
                for (int i = Length - 1; i >= 0; i--)
                {
                    if (this[i] != characters[j])
                        break;

                    endDelta++;
                }
            }

            return new StringSpan(_buffer, StartIndex + startDelta, Length - startDelta - endDelta);
        }

        public StringSpan Substring(int startIndex)
        {
            var delta = Math.Min(startIndex, _buffer.Length - StartIndex);
            return new StringSpan(_buffer, StartIndex + delta, Length - delta);
        }

        public StringSpan Substring(int startIndex, int length)
        {
            var delta = Math.Min(startIndex, _buffer.Length - StartIndex);
            return new StringSpan(_buffer, StartIndex + delta, length);
        }

        public int IndexOf(char character, int startIndex = 0)
        {
            var endIndex = StartIndex + startIndex + Length;
            for (int i = StartIndex + startIndex; i < endIndex; i++)
            {
                if (this[i] == character)
                    return i - StartIndex;
            }
            
            return -1;
        }

        public StringSpan ToUpper()
        {
            return new StringSpan(_buffer, StartIndex, Length)
            {
                Case = StringSpanCase.Uppercase
            };
        }

        public StringSpan ToLower()
        {
            return new StringSpan(_buffer, StartIndex, Length)
            {
                Case = StringSpanCase.Lowercase
            };
        }

        public bool Contains(string value)
        {
            var endIndex = Length - value.Length;
            for (int i = 0; i < endIndex; i++)
            {
                for (int j = 0; j < value.Length; j++)
                {
                    if (this[i + j] != value[j])
                        break;

                    if (j == value.Length - 1)
                        return true;
                }
            }

            return false;
        }

        public bool Contains(StringSpan value)
        {
            var endIndex = Length - value.Length;
            for (int i = 0; i < endIndex; i++)
            {
                for (int j = 0; j < value.Length; j++)
                {
                    if (this[i + j] != value[j])
                        break;

                    if (j == value.Length - 1)
                        return true;
                }
            }

            return false;
        }

        public bool StartsWith(string value)
        {
            for (int i = 0; i < value.Length; i++)
            {
                if (this[i] != value[i])
                    return false;
            }

            return true;
        }

        public bool StartsWith(StringSpan value)
        {
            for (int i = 0; i < value.Length; i++)
            {
                if (this[i] != value[i])
                    return false;
            }

            return true;
        }

        public bool EndsWith(string value)
        {
            for (int i = Length - value.Length; i < Length; i++)
            {
                if (this[i] != value[i])
                    return false;
            }

            return true;
        }

        public bool EndsWith(StringSpan value)
        {
            for (int i = Length - value.Length; i < Length; i++)
            {
                if (this[i] != value[i])
                    return false;
            }

            return true;
        }

        public bool IsEmpty()
        {
            return IsEmpty(0, Length);
        }

        public bool IsEmpty(int startIndex, int length)
        {
            for (int i = startIndex; i > 0 && i < length && i < Length; i++)
            {
                if (!char.IsWhiteSpace(this[i]))
                    return false;
            }

            return true;
        }

        public StringBuilder GetBuffer()
        {
            return _buffer;
        }

        public override string ToString()
        {
            switch (Case)
            {
                case StringSpanCase.Uppercase:
                    return _buffer.ToString(StartIndex, Length).ToUpper();
                case StringSpanCase.Lowercase:
                    return _buffer.ToString(StartIndex, Length).ToLower();

            }

            return _buffer.ToString(StartIndex, Length);
        }
    }

    public static class StringSpanExtensions
    {
        public static StringSpan ToSpan(this string value)
        {
            return new StringSpan(value, 0, value.Length);
        }
    }
}
