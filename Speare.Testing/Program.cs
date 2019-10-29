using Speare.Parsing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Speare.Testing
{
    class Program
    {
        static void Main(string[] args)
        {
            var code = "{\n\tTobi: Hey Doc, how are you?\n\tDoc: I'm good!\n\tTestMethod(  3, 4 )\n}\n\nTestMethod(a, b)\n{\n}";
            for (int i = 0; i < 11; i++)
            {
                code += code;
            }

            Lexer.Allocate();

            var a = "123.013".ToSpan().ToFloat();
            var b = "3.000513".ToSpan().ToFloat();
            var c = "184641".ToSpan().ToInt32();

            var sw = Stopwatch.StartNew();
            var tokens = Lexer.Tokenize(code);
            sw.Stop();

            var node = Parser.Parse(tokens);

            Console.WriteLine(code);
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

            Console.WriteLine(sw.Elapsed.TotalMilliseconds + "ms for " + code.Split('\n').Count() + " lines");
            Console.ReadLine();
        }
    }
}
