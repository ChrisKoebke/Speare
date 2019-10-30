using Speare.Compilation;
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
            return (DataType*)(scope + (int)register * 5); // Registers are [byte:type] [int:data]
        }

        public static DataType* DataType(byte* scope, Register startRegister, int index)
        {
            return (DataType*)(scope + ((int)startRegister + index) * 5); // Registers are [byte:type] [int:data]
        }

        public static byte* Value(byte* scope, Register register)
        {
            return scope + (int)register * 5 + 1; // Registers are [byte:type] [int:data]
        }

        public static byte* Value(byte* scope, Register register, int index)
        {
            return scope + ((int)register + index) * 5 + 1; // Registers are [byte:type] [int:data]
        }

        public static int* IntValue(byte* scope, Register register)
        {
            return (int*)(scope + (int)register * 5 + 1); // Registers are [byte:type] [int:data]
        }

        public static int* IntValue(byte* scope, Register register, int index)
        {
            return (int*)(scope + ((int)register + index) * 5 + 1); // Registers are [byte:type] [int:data]
        }

        public static short* ShortValue(byte* scope, Register register)
        {
            return (short*)(scope + (int)register * 5 + 1); // Registers are [byte:type] [int:data]
        }

        public static short* ShortValue(byte* scope, Register register, int index)
        {
            return (short*)(scope + ((int)register + index) * 5 + 1); // Registers are [byte:type] [int:data]
        }

        public static float* FloatValue(byte* scope, Register register)
        {
            return (float*)(scope + (int)register * 5 + 1); // Registers are [byte:type] [int:data]
        }

        public static float* FloatValue(byte* scope, Register register, int index)
        {
            return (float*)(scope + ((int)register + index) * 5 + 1); // Registers are [byte:type] [int:data]
        }

        public static bool* BoolValue(byte* scope, Register register)
        {
            return (bool*)(scope + (int)register * 5 + 1); // Registers are [byte:type] [int:data]
        }

        public static bool* BoolValue(byte* scope, Register register, int index)
        {
            return (bool*)(scope + ((int)register + index) * 5 + 1); // Registers are [byte:type] [int:data]
        }

        public static short* MethodAddress(byte* mth, int methodIndex)
        {
            return (short*)(mth + methodIndex * 3); // Methods are [short:address] [byte:parameterCount]
        }

        public static byte* MethodParameterCount(byte* mth, int methodIndex)
        {
            return mth + methodIndex * 3 + 2; // Methods are [short:address] [byte:parameterCount]
        }

        public static int* StringStartIndex(byte* chrh, int stringIndex)
        {
            return (int*)(chrh + stringIndex * 8); // String header is [int:startIndex] [int:length]
        }

        public static int* StringLength(byte* chrh, int stringIndex)
        {
            return (int*)(chrh + stringIndex * 8 + 4); // String header is [int:startIndex] [int:length]
        }
    }
}
