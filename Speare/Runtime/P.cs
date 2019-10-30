using Speare.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Speare.Runtime
{
    public static unsafe class P
    {
        public static DataType* DataType(byte* scope, Register register)
        {
            return (DataType*)(scope + (int)register * Constants.SizeOfRegister + Constants.OffsetRegisterType);
        }

        public static DataType* DataType(byte* scope, Register startRegister, int index)
        {
            return (DataType*)(scope + ((int)startRegister + index) * Constants.SizeOfRegister + Constants.OffsetRegisterType);
        }

        public static byte* Value(byte* scope, Register register)
        {
            return scope + (int)register * Constants.SizeOfRegister + Constants.OffsetRegisterData;
        }

        public static byte* Value(byte* scope, Register register, int index)
        {
            return scope + ((int)register + index) * Constants.SizeOfRegister + Constants.OffsetRegisterData;
        }

        public static int* IntValue(byte* scope, Register register)
        {
            return (int*)(scope + (int)register * Constants.SizeOfRegister + Constants.OffsetRegisterData);
        }

        public static int* IntValue(byte* scope, Register register, int index)
        {
            return (int*)(scope + ((int)register + index) * Constants.SizeOfRegister + Constants.OffsetRegisterData); 
        }

        public static short* ShortValue(byte* scope, Register register)
        {
            return (short*)(scope + (int)register * Constants.SizeOfRegister + Constants.OffsetRegisterData);
        }

        public static short* ShortValue(byte* scope, Register register, int index)
        {
            return (short*)(scope + ((int)register + index) * Constants.SizeOfRegister + Constants.OffsetRegisterData);
        }

        public static float* FloatValue(byte* scope, Register register)
        {
            return (float*)(scope + (int)register * Constants.SizeOfRegister + Constants.OffsetRegisterData);
        }

        public static float* FloatValue(byte* scope, Register register, int index)
        {
            return (float*)(scope + ((int)register + index) * Constants.SizeOfRegister + Constants.OffsetRegisterData);
        }

        public static bool* BoolValue(byte* scope, Register register)
        {
            return (bool*)(scope + (int)register * Constants.SizeOfRegister + Constants.OffsetRegisterData);
        }

        public static bool* BoolValue(byte* scope, Register register, int index)
        {
            return (bool*)(scope + ((int)register + index) * Constants.SizeOfRegister + Constants.OffsetRegisterData);
        }

        public static short* MethodAddress(byte* mth, int methodIndex)
        {
            return (short*)(mth + methodIndex * Constants.SizeOfMethod + Constants.OffsetMethodAddress);
        }

        public static byte* MethodParameterCount(byte* mth, int methodIndex)
        {
            return mth + methodIndex * Constants.SizeOfMethod + Constants.OffsetMethodParameterCount;
        }

        public static int* StringStartIndex(byte* chrh, int stringIndex)
        {
            return (int*)(chrh + stringIndex * Constants.SizeOfString + Constants.OffsetStringStartIndex);
        }

        public static int* StringLength(byte* chrh, int stringIndex)
        {
            return (int*)(chrh + stringIndex * Constants.SizeOfString + Constants.OffsetStringLength);
        }
    }
}
