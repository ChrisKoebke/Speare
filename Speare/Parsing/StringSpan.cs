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
        public StringSpan()
        {
        }
        public StringSpan(string value, int startIndex, int length)
        {
            Value = value;
            StartIndex = startIndex;
            Length = length;
        }

        public string Value { get; set; }
        public int StartIndex { get; set; }
        public int Length { get; set; }
        public StringSpanCase Case { get; set; }
        public int EndIndex
        {
            get { return StartIndex + Length; }
            set { Length = value - StartIndex; }
        }

        public char this[int index]
        {
            get
            {
                var value = Value[index + StartIndex];

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
                    yield return new StringSpan(Value, StartIndex + startIndex, i - startIndex);
                    startIndex = i + 1;
                }
            }

            yield return new StringSpan(Value, StartIndex + startIndex, Length - startIndex);
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

            return new StringSpan(Value, StartIndex + delta, Length - delta);
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

            return new StringSpan(Value, StartIndex, Length - delta);
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

            return new StringSpan(Value, StartIndex + startDelta, Length - startDelta - endDelta);
        }

        public StringSpan Substring(int startIndex)
        {
            var delta = Math.Min(startIndex, Value.Length - StartIndex);
            return new StringSpan(Value, StartIndex + delta, Length - delta);
        }

        public StringSpan Substring(int startIndex, int length)
        {
            var delta = Math.Min(startIndex, Value.Length - StartIndex);
            return new StringSpan(Value, StartIndex + delta, length);
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
            return new StringSpan(Value, StartIndex, Length)
            {
                Case = StringSpanCase.Uppercase
            };
        }

        public StringSpan ToLower()
        {
            return new StringSpan(Value, StartIndex, Length)
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

        public override string ToString()
        {
            switch (Case)
            {
                case StringSpanCase.Uppercase:
                    return Value.Substring(StartIndex, Length).ToUpper();
                case StringSpanCase.Lowercase:
                    return Value.Substring(StartIndex, Length).ToLower();

            }

            return Value.Substring(StartIndex, Length);
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
