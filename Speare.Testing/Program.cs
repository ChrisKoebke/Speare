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
        static void Main(string[] args)
        {
            var runtime = new Runtime();
            
            var builder = new OpBuilder();
            builder.Constant("This is a test")
                   .DebugPrint()
                   .Store(0)
                   .Constant(1024)
                   .DebugPrint()
                   .Load(0)
                   .DebugPrint();

            runtime.Load(builder);
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
