using Speare.Compilation;
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
        public static void PrintVector(string x, int y, int z)
        {
            Console.WriteLine("Vector " + x + "x" + y + "x" + z);
        }

        static void Main(string[] args)
        {
            Interop.RegisterMethodsOf<Program>();

            var builder = new OpBuilder();
            builder.Method()
                       .Constant(Register.R0, 0)
                       .Constant(Register.R1, 5)
                       .Label(":loop")
                       .Constant(Register.R2, 1)
                       .Add(Register.R0, Register.R2)
                       .Move(Register.LastResult, Register.R0)
                       .DebugPrint(Register.R0)
                       .Compare(Register.R0, Register.R1, Comparison.S)
                       .JumpIf(":loop")
                       .Constant(Register.R3, "We made it through the loop!")
                       .DebugPrint(Register.R3)
                       .Constant(Register.Param0, "Another test")
                       .Call(methodIndex: 1)
                       .DebugPrint(Register.LastResult)
                       .Constant(Register.Param0, "hi")
                       .Interop("PrintVector")
                   .Method(parameterCount: 1)
                       .DebugPrint(Register.Param0)
                       .Constant(Register.LastResult, 199)
                       .Return();

            var runtime = VM.FromBuilder(builder);
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
