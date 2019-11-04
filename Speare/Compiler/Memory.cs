using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Speare.Compiler
{
    public unsafe class Memory
    {
        public byte[] Data = new byte[256];
        public int Position = 0;

        private void BoundsCheck(int numberOfBytesAdded)
        {
            var newSize = Data.Length;
            while (Position + numberOfBytesAdded > newSize)
            {
                newSize <<= 2;
            }

            if (newSize == Data.Length)
                return;

            Array.Resize(ref Data, newSize);
        }

        public void Clear()
        {
            Array.Clear(Data, 0, Data.Length);
            Position = 0;
        }

        public void Write(int value)
        {
            BoundsCheck(4);

            fixed (byte* pointer = Data)
            {
                *(int*)(pointer + Position) = value;
                Position += 4;
            }
        }

        public void Write(short value)
        {
            BoundsCheck(2);

            fixed (byte* pointer = Data)
            {
                *(short*)(pointer + Position) = value;
                Position += 2;
            }
        }

        public void Write(byte value)
        {
            BoundsCheck(1);

            fixed (byte* pointer = Data)
            {
                *(pointer + Position) = value;
                Position += 1;
            }
        }

        public void Write(float value)
        {
            BoundsCheck(4);

            fixed (byte* pointer = Data)
            {
                *(float*)(pointer + Position) = value;
                Position += 4;
            }
        }

        public void Write(bool value)
        {
            BoundsCheck(1);

            fixed (byte* pointer = Data)
            {
                *(bool*)(pointer + Position) = value;
                Position += 1;
            }
        }

        public static unsafe void Copy11_MoreOptimized(byte* src, byte* dst)
        {
            *(long*)dst = *(long*)src;
            *(long*)(dst + 5) = *(long*)(src + 5);
        }

        public void Write(string value)
        {
            fixed (char* source = value)
            {
                Write(source, value.Length);
            }
        }

        public void Write(char* source, int length)
        {
            BoundsCheck(length);

            fixed (byte* destination = Data)
            {
                // TODO: optimize
                for (int i = 0; i < length; i++)
                {
                    *(destination + Position + i) = *(byte*)(source + i);
                }

                Position += length;
            }
        }
    }
}
