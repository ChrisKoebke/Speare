using Speare.Compiler;
using Speare.Parser;
using Speare.Runtime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Speare.Testing
{
    class Program
    {
        public static void PrintVector(string x)
        {
            Console.WriteLine(x);
        }

        static OpBuilder Test1()
        {
            return new OpBuilder()

            .Method()
                .Constant(Register.R0, 0)
                .Constant(Register.R1, 5)
                .Label(":loop")
                .Constant(Register.R2, 1)
                .Arithmetic(Register.R0, Register.R2, Arithmetic.Add)
                .Set(Register.R0, Register.LastResult)
                .DebugPrint(Register.R0)
                .Arithmetic(Register.R0, Register.R1, Arithmetic.LessThan)
                .JumpIf(":loop")
                .Constant(Register.R3, "We made it through the loop!")
                .DebugPrint(Register.R3)
                .Constant(Register.Param0, "Another test")
                .Call(methodIndex: 1)
                .DebugPrint(Register.LastResult)
                .GlobalRead(Register.Param0, "TestVar")
                .Interop("PrintVector")
            .Method(parameterCount: 1)
                .GlobalRead(Register.Local0, "TestVar")
                .DebugPrint(Register.Param0)
                .Constant(Register.LastResult, 199)
                .Return();
        }

        static OpBuilder TestGlobalReadWrite()
        {
            return new OpBuilder()

            .Method()
                .GlobalRead(Register.Local0, "TestVar")
                .Constant(Register.Local1, 100)
                .Arithmetic(Register.Local0, Register.Local1, Arithmetic.Multiply)
                .GlobalWrite("TestVar", Register.LastResult);
        }


        static void TokenizerTest()
        {
            var code = "{\n Tobi: \"Hello this is a test\"\nTestMethod(1, 2)\n}\nTestMethod(a, b)\n{\n}";

            var tokens = Lexer.Tokenize(code);
            foreach (var token in tokens)
            {
                if (token.Type == TokenType.EOF)
                    break;

                Console.WriteLine(token.ToFormattedString());
            }

            Console.ReadLine();
        }

        static void VMTest()
        {
            Interop.RegisterMethodsOf<Program>();

            var machine = VM.FromByteCode(Test1().Build());
            machine["TestVar"] = "From C#";
            machine.Run(methodIndex: 0);

            Console.ReadLine();
        }

        static void Main(string[] args)
        {
            VMTest();
        }
    }
}
