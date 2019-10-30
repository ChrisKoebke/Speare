using Speare.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Speare.Runtime
{
    public unsafe delegate void ArithmeticOperator(byte* scope, Register a, Register b);

    public static unsafe class Arithmetics
    {
        public static Dictionary<int, ArithmeticOperator> Table = new Dictionary<int, ArithmeticOperator>
        { 
            [Hash(DataType.Int, DataType.Int, Arithmetic.Equals)] = Equals.IntInt,
            [Hash(DataType.Int, DataType.Float, Arithmetic.Equals)] = Equals.IntFloat,
            [Hash(DataType.Float, DataType.Int, Arithmetic.Equals)] = Equals.FloatInt,
            [Hash(DataType.Float, DataType.Float, Arithmetic.Equals)] = Equals.FloatFloat,
            [Hash(DataType.Bool, DataType.Bool, Arithmetic.Equals)] = Equals.BoolBool,

            [Hash(DataType.Int, DataType.Int, Arithmetic.LessThan)] = LessThan.IntInt,

            [Hash(DataType.Int, DataType.Int, Arithmetic.Add)] = Add.IntInt,
            [Hash(DataType.Int, DataType.Float, Arithmetic.Add)] = Add.IntFloat,
            [Hash(DataType.Float, DataType.Int, Arithmetic.Add)] = Add.FloatInt,
            [Hash(DataType.Float, DataType.Float, Arithmetic.Add)] = Add.FloatFloat,

            [Hash(DataType.Int, DataType.Int, Arithmetic.Subtract)] = Subtract.IntInt,
            [Hash(DataType.Int, DataType.Float, Arithmetic.Subtract)] = Subtract.IntFloat,
            [Hash(DataType.Float, DataType.Int, Arithmetic.Subtract)] = Subtract.FloatInt,
            [Hash(DataType.Float, DataType.Float, Arithmetic.Subtract)] = Subtract.FloatFloat,

            [Hash(DataType.Int, DataType.Int, Arithmetic.Multiply)] = Multiply.IntInt,
            [Hash(DataType.Int, DataType.Float, Arithmetic.Multiply)] = Multiply.IntFloat,
            [Hash(DataType.Float, DataType.Int, Arithmetic.Multiply)] = Multiply.FloatInt,
            [Hash(DataType.Float, DataType.Float, Arithmetic.Multiply)] = Multiply.FloatFloat,

            [Hash(DataType.Int, DataType.Int, Arithmetic.Divide)] = Divide.IntInt,
            [Hash(DataType.Int, DataType.Float, Arithmetic.Divide)] = Divide.IntFloat,
            [Hash(DataType.Float, DataType.Int, Arithmetic.Divide)] = Divide.FloatInt,
            [Hash(DataType.Float, DataType.Float, Arithmetic.Divide)] = Divide.FloatFloat
        };
        
        public static int Hash(DataType a, DataType b, Arithmetic arithmetic)
        {
            var hash = 0;
            hash |= (int)a;
            hash |= (int)b << 8;
            hash |= (int)arithmetic << 16;
            return hash;
        }

        public static ArithmeticOperator Get(DataType a, DataType b, Arithmetic arithmetic)
        {
            ArithmeticOperator result;
            Table.TryGetValue(Hash(a, b, arithmetic), out result);
            return result;
        }

        public static new class Equals
        {
            public static void IntInt(byte* scope, Register a, Register b)
            {
                *P.DataType(scope, Register.LastResult) = DataType.Bool;
                *P.BoolValue(scope, Register.LastResult) = *P.IntValue(scope, a) == *P.IntValue(scope, b);
            }

            public static void IntFloat(byte* scope, Register a, Register b)
            {
                *P.DataType(scope, Register.LastResult) = DataType.Bool;
                *P.BoolValue(scope, Register.LastResult) = *P.IntValue(scope, a) == *P.FloatValue(scope, b);
            }

            public static void FloatInt(byte* scope, Register a, Register b)
            {
                *P.DataType(scope, Register.LastResult) = DataType.Bool;
                *P.BoolValue(scope, Register.LastResult) = *P.FloatValue(scope, a) == *P.IntValue(scope, b);
            }

            public static void FloatFloat(byte* scope, Register a, Register b)
            {
                *P.DataType(scope, Register.LastResult) = DataType.Bool;
                *P.BoolValue(scope, Register.LastResult) = *P.FloatValue(scope, a) == *P.FloatValue(scope, b);
            }

            public static void BoolBool(byte* scope, Register a, Register b)
            {
                *P.DataType(scope, Register.LastResult) = DataType.Bool;
                *P.BoolValue(scope, Register.LastResult) = *P.BoolValue(scope, a) == *P.BoolValue(scope, b);
            }
        }

        public static class LessThan
        {
            public static void IntInt(byte* scope, Register a, Register b)
            {
                *P.DataType(scope, Register.LastResult) = DataType.Bool;
                *P.BoolValue(scope, Register.LastResult) = *P.IntValue(scope, a) < *P.IntValue(scope, b);
            }
        }

        public static class Add
        {
            public static void IntInt(byte* scope, Register a, Register b)
            {
                *P.DataType(scope, Register.LastResult) = DataType.Int;
                *P.IntValue(scope, Register.LastResult) = *P.IntValue(scope, a) + *P.IntValue(scope, b);
            }

            public static void IntFloat(byte* scope, Register a, Register b)
            {
                *P.DataType(scope, Register.LastResult) = DataType.Float;
                *P.FloatValue(scope, Register.LastResult) = *P.IntValue(scope, a) + *P.FloatValue(scope, b);
            }

            public static void FloatInt(byte* scope, Register a, Register b)
            {
                *P.DataType(scope, Register.LastResult) = DataType.Float;
                *P.FloatValue(scope, Register.LastResult) = *P.FloatValue(scope, a) + *P.IntValue(scope, b);
            }

            public static void FloatFloat(byte* scope, Register a, Register b)
            {
                *P.DataType(scope, Register.LastResult) = DataType.Float;
                *P.FloatValue(scope, Register.LastResult) = *P.FloatValue(scope, a) + *P.FloatValue(scope, b);
            }
        }

        public static class Subtract
        {
            public static void IntInt(byte* scope, Register a, Register b)
            {
                *P.DataType(scope, Register.LastResult) = DataType.Int;
                *P.IntValue(scope, Register.LastResult) = *P.IntValue(scope, a) - *P.IntValue(scope, b);
            }

            public static void IntFloat(byte* scope, Register a, Register b)
            {
                *P.DataType(scope, Register.LastResult) = DataType.Float;
                *P.FloatValue(scope, Register.LastResult) = *P.IntValue(scope, a) - *P.FloatValue(scope, b);
            }

            public static void FloatInt(byte* scope, Register a, Register b)
            {
                *P.DataType(scope, Register.LastResult) = DataType.Float;
                *P.FloatValue(scope, Register.LastResult) = *P.FloatValue(scope, a) - *P.IntValue(scope, b);
            }

            public static void FloatFloat(byte* scope, Register a, Register b)
            {
                *P.DataType(scope, Register.LastResult) = DataType.Float;
                *P.FloatValue(scope, Register.LastResult) = *P.FloatValue(scope, a) - *P.FloatValue(scope, b);
            }
        }

        public static class Divide
        {
            public static void IntInt(byte* scope, Register a, Register b)
            {
                *P.DataType(scope, Register.LastResult) = DataType.Int;
                *P.IntValue(scope, Register.LastResult) = *P.IntValue(scope, a) / *P.IntValue(scope, b);
            }

            public static void IntFloat(byte* scope, Register a, Register b)
            {
                *P.DataType(scope, Register.LastResult) = DataType.Float;
                *P.FloatValue(scope, Register.LastResult) = *P.IntValue(scope, a) / *P.FloatValue(scope, b);
            }

            public static void FloatInt(byte* scope, Register a, Register b)
            {
                *P.DataType(scope, Register.LastResult) = DataType.Float;
                *P.FloatValue(scope, Register.LastResult) = *P.FloatValue(scope, a) / *P.IntValue(scope, b);
            }

            public static void FloatFloat(byte* scope, Register a, Register b)
            {
                *P.DataType(scope, Register.LastResult) = DataType.Float;
                *P.FloatValue(scope, Register.LastResult) = *P.FloatValue(scope, a) / *P.FloatValue(scope, b);
            }
        }

        public static class Multiply
        {
            public static void IntInt(byte* scope, Register a, Register b)
            {
                *P.DataType(scope, Register.LastResult) = DataType.Int;
                *P.IntValue(scope, Register.LastResult) = *P.IntValue(scope, a) * *P.IntValue(scope, b);
            }

            public static void IntFloat(byte* scope, Register a, Register b)
            {
                *P.DataType(scope, Register.LastResult) = DataType.Float;
                *P.FloatValue(scope, Register.LastResult) = *P.IntValue(scope, a) * *P.FloatValue(scope, b);
            }

            public static void FloatInt(byte* scope, Register a, Register b)
            {
                *P.DataType(scope, Register.LastResult) = DataType.Float;
                *P.FloatValue(scope, Register.LastResult) = *P.FloatValue(scope, a) * *P.IntValue(scope, b);
            }

            public static void FloatFloat(byte* scope, Register a, Register b)
            {
                *P.DataType(scope, Register.LastResult) = DataType.Float;
                *P.FloatValue(scope, Register.LastResult) = *P.FloatValue(scope, a) * *P.FloatValue(scope, b);
            }
        }
    }
}
