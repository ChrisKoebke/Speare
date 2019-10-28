using Speare.Parsing;
using Speare.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Speare.Testing
{
    class Program
    {
        static void Main(string[] args)
        {
            var code = "{\n\tTobi: Hey Doc, how are you?\n\tDoc: I'm good!\n\tTestMethod(  test , param% )\n\t{\n\t}\n}";

            var tokens = Tokenizer.Parse(code);

            Console.WriteLine(code);
            Console.WriteLine();
            Console.WriteLine("=== PARSER RESULT ===");

            foreach (var token in tokens)
            {
                if (token.Type == TokenType.EOF)
                    break;

                Console.WriteLine(token.ToString());
            }

            Console.ReadLine();
        }
    }
}
