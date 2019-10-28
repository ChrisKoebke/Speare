using Speare.Parsing;
using Speare.Tokens;
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

            Tokenizer.PreAllocate();
            var tokens = Tokenizer.Parse(code);

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

            Console.ReadLine();
        }
    }
}
