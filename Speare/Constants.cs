using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Speare
{
    public static class Constants
    {
        public const int RegisterCount = 18;

        public const int SizeOfHeaderElement = 4;
        public const int SizeOfHeader = SizeOfHeaderElement * 4;

        public const int SizeOfRegister = 5;
        public const int SizeOfScope = RegisterCount * SizeOfRegister;
        public const int SizeOfMethod = 3;
        public const int SizeOfString = 8;

        public const int OffsetRegisterType = 0;
        public const int OffsetRegisterData = 1;

        public const int OffsetMethodAddress = 0;
        public const int OffsetMethodParameterCount = 2;

        public const int OffsetStringStartIndex = 0;
        public const int OffsetStringLength = 4;
    }
}
