using Speare.Compilation;
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
        public static void PrintVector(int x, int y, int z)
        {
            Console.WriteLine("Vector " + x + "x" + y + "x" + z);
        }

        static void Main(string[] args)
        {
            Interop.RegisterMethodsOf<Program>();

            var builder = new OpBuilder();
            builder.PushScope()
                   .Constant(64)
                   .Move(Register.A)
                   .Constant(1024)
                   .Move(Register.B)
                   .Constant(11)
                   .Move(Register.C)
                   .Interop("PrintVector")
                   .PopScope()
                   .Interop("PrintVector")
                   .Jump(0);

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
