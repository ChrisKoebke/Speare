using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
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

    public unsafe class StringSpan
    {
        internal StringSpan()
        {
        }

        public StringSpan(char* pointer, int startIndex, int length)
        {
            Pointer = pointer;
            StartIndex = startIndex;
            Length = length;
        }

        public char* Pointer;
        public int StartIndex;
        public int Length;
        
        public char this[int index]
        {
            get { return *(Pointer + index + StartIndex); }
        }

        public int EndIndex
        {
            get { return StartIndex + Length; }
            set { Length = value - StartIndex; }
        }

        public List<StringSpan> Split(char character)
        {
            var list = new List<StringSpan>();
            var startIndex = 0;

            for (int i = 0; i < Length; i++)
            {
                if (this[i] == character)
                {
                    list.Add(new StringSpan(Pointer, StartIndex + startIndex, i - startIndex));
                    startIndex = i + 1;
                }
            }

            list.Add(new StringSpan(Pointer, StartIndex + startIndex, Length - startIndex));
            return list;
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

            return new StringSpan(Pointer, StartIndex + delta, Length - delta);
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

            return new StringSpan(Pointer, StartIndex, Length - delta);
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

            return new StringSpan(Pointer, StartIndex + startDelta, Length - startDelta - endDelta);
        }

        public StringSpan Substring(int startIndex)
        {
            return new StringSpan(Pointer, StartIndex + startIndex, Length - startIndex);
        }

        public StringSpan Substring(int startIndex, int length)
        {
            return new StringSpan(Pointer, StartIndex + startIndex, length);
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

        private static readonly int[] _decimals = new[]
        {
            1, 10, 100, 1000, 10000, 100000, 1000000, 10000000, 100000000, 1000000000
        };

        public int? ToInt32()
        {
            var result = 0;

            for (int i = 0; i < Length; i++)
            {
                if (!Chars.IsInteger(this[i]))
                    return null;

                result += (this[i] - '0') * _decimals[Length - i - 1];
            }

            return result;
        }

        private static readonly float[] _fractions = new[]
        {
            0.1f, 0.01f, 0.001f, 0.0001f, 0.00001f, 0.000001f, 0.0000001f, 0.00000001f, 0.000000001f, 0.000000001f, 0.0000000001f, 0.00000000001f, 0.000000000001f,
            0.0000000000001f, 0.00000000000001f, 0.000000000000001f, 0.0000000000000001f, 0.00000000000000001f, 0.000000000000000001f, 0.0000000000000000001f
        };

        public float? ToFloat()
        {
            var result = 0.0f;
            var fractionIndex = IndexOf('.');

            for (int i = 0; i < Length; i++)
            {
                if (!Chars.IsFloat(this[i]))
                    return null;

                if (i == fractionIndex)
                    continue;

                if (i < fractionIndex)
                {
                    result += (this[i] - '0') * _decimals[fractionIndex - i - 1];
                }
                else
                {
                    result += (this[i] - '0') * _fractions[i - fractionIndex - 1];
                }
            }

            return result;
        }

        public override string ToString()
        {
            return new string(Pointer, StartIndex, Length);
        }
    }

    public static unsafe class StringSpanExtensions
    {
        public static StringSpan ToSpan(this string value)
        {
            fixed (char* pointer = value)
            {
                return new StringSpan(pointer, 0, value.Length);
            }
        }
    }
}
