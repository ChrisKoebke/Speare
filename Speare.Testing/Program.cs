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
            builder.Constant(Var.A, 0)
                   .Constant(Var.B, 5)
                   .Label("AfterInitVarA")
                   .Constant(Var.C, 1)
                   .Add(Var.A, Var.C)
                   .Move(Var.A)
                   .DebugPrint(Var.A)
                   .Compare(Var.A, Var.B, Comparison.SmallerThan)
                   .JumpIf("AfterInitVarA")
                   .Constant(Var.D, "We made it through the loop!")
                   .DebugPrint(Var.D);

            var runtime = Runtime.FromBuilder(builder);
            runtime.Run().MoveNext();

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
