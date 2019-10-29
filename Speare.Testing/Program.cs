using Speare.Ops;
using Speare.Parsing;
using Speare.Runtimes;
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
        public static void PrintVector(string x, int y, int z)
        {
            Console.WriteLine("Vector " + x + "x" + y + "x" + z);
        }

        static void Main(string[] args)
        {
            Interop.RegisterMethodsOf<Program>();

            var builder = new OpBuilder();
            builder.Method()
                       .Constant(Register.A, 0)
                       .Constant(Register.B, 5)
                       .Label(":loop")
                       .Constant(Register.C, 1)
                       .Add(Register.A, Register.C)
                       .Move(Register.LastResult, Register.A)
                       .DebugPrint(Register.A)
                       .Compare(Register.A, Register.B, Comparison.SmallerThan)
                       .JumpIf(":loop")
                       .Constant(Register.D, "We made it through the loop!")
                       .DebugPrint(Register.D)
                       .Constant(Register.A, "Another test")
                       .Call(1)
                       .DebugPrint(Register.LastResult)
                   .Method(parameterCount: 1)
                       .DebugPrint(Register.A)
                       .Constant(Register.LastResult, 199)
                       .Return();

            var runtime = Runtime.FromBuilder(builder);
            runtime.Run(methodIndex: 0).MoveNext();

            /*Console.WriteLine(code);
            Console.WriteLine();
            Console.WriteLine("=== TOKENIZER RESULT ===");

            foreach (var token in tokens)
            {
                if (token.Type == TokenType.EOF)
                    break;

                var tokenType = token.Type.ToString();
                Console.WriteLine(
                    "{0}{1}'{2}'",
                    tokenType, 
                    string.Empty.PadLeft(24 - tokenType.Length), 
                    token.ToString()
                );
            }
            */
            Console.ReadLine();
        }
    }
}
